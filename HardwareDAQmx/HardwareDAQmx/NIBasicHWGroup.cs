using Communication.Interfaces.Generator;
using Communication.Interfaces.Hardware;
using Generator.Generator.Concatenator;
using System;
using System.Collections.Generic;

namespace HardwareDAQmx
{
    /// <summary>
    /// Manages the output of a collection of NationalInstruments cards
    /// </summary>
    /// <seealso cref="Communication.Interfaces.Hardware.IHardwareGroup" />
    public abstract class NIBasicHWGroup : IHardwareGroup
    {
        /// <summary>
        /// The collection of slave analog cards
        /// </summary>
        protected Dictionary<string, NIBasicAnalogCard> _analogCards = new Dictionary<string, NIBasicAnalogCard>();
        /// <summary>
        /// The collection of slave digital cards
        /// </summary>
        protected Dictionary<string, NIBasicDigitalCard> _digitalCards = new Dictionary<string, NIBasicDigitalCard>();
        /// <summary>
        /// The master analog card (used for synchronization)
        /// </summary>
        protected NIBasicAnalogCard _syncMaster;

        //Event Handling        
        /// <summary>
        /// A lock used to thread-safely increment the <see cref=" _finished"/> counter.
        /// </summary>
        private object _finishPadlock = new object();
        /// <summary>
        /// A counter for the finished cards
        /// </summary>
        private int _finished;
        /// <summary>
        /// A lock used to thread-safely increment the <see cref=" _initPadlock"/> counter.
        /// </summary>
        private object _initPadlock = new object();
        /// <summary>
        /// A counter for the initialized cards
        /// </summary>
        private int _initialized;
        /// <summary>
        /// Occurs when all cards have been initialized.
        /// </summary>
        public event EventHandler Initialized;
        /// <summary>
        /// Occurs when all cards have finished outputting.
        /// </summary>
        public event EventHandler Finished;
        /// <summary>
        /// Indicates whether all cards have finished
        /// </summary>
        private bool hasFinishedEventBeenThrown = false;

        /// <summary>
        /// The base name for digital cards
        /// </summary>
        protected const string DIGITAL_CARD_BASE_NAME = "DO";
        /// <summary>
        /// The base name for analog cards
        /// </summary>
        protected const string ANALOG_CARD_BASE_NAME = "AO";
        /// <summary>
        /// The minimum output voltage
        /// </summary>
        protected const int MIN_VOLTAGE = -10;
        /// <summary>
        /// The maximum output voltage
        /// </summary>
        protected const int MAX_VOLTAGE = +10;

        /// <summary>
        /// Creates all digital and analog cards (including the synchronization master)
        /// </summary>
        /// <param name="numAnalogCards">The number of analog cards.</param>
        /// <param name="numDigitalCards">The number of digital cards.</param>
        protected abstract void CreateCards(int numAnalogCards, int numDigitalCards);

        /// <summary>
        /// Initializes the master card.
        /// </summary>
        /// <param name="data">The otuput data.</param>
        protected abstract void InitializeMasterCard(IModelOutput data);

        #region IHardwareGroup Members

        /// <summary>
        /// Initializes all cards and triggers sending the converted data to the system. (output does not start)
        /// </summary>
        /// <param name="data">The output data.</param>
        /// <exception cref="System.Exception">
        /// Data for a card is missing.
        /// </exception>
        public void Initialize(IModelOutput data)
        {
            System.Diagnostics.Debug.Write("\n\nA new cycle started at " + DateTime.Now.ToString());
            //Check for the existence of the output data
            foreach (string card in _analogCards.Keys)
            {
                if (!data.Output.ContainsKey(card))
                    throw new Exception("Data for card " + card + " is missing.");
            }

            foreach (string card in _digitalCards.Keys)
            {
                if (!data.Output.ContainsKey(card))
                    throw new Exception("Data for card " + card + " is missing.");
            }

            int analogs = Global.GetNumAnalogCards();
            int digitals = Global.GetNumDigitalCards();

            CreateCards(analogs, digitals);

            _initialized = 0;
            _finished = 0;

            _syncMaster.Finished += OnFinishing;
            _syncMaster.Initialized += OnInitialization;

            InitializeMasterCard(data);

            foreach (var card in _analogCards)
            {
                AnalogCardOutput concatenator = ((AnalogCardOutput)data.Output[card.Key]);
                card.Value.Finished += OnFinishing;
                card.Value.Initialized += OnInitialization;
                card.Value.Initialize(card.Key + "/ao0:7", MIN_VOLTAGE, MAX_VOLTAGE, concatenator.Output.GetLength(1), concatenator.SampleRate);
                card.Value.Synchronize(_syncMaster);
                double[,] output = concatenator.Output;
                card.Value.Data(output);
            }

            foreach (var card in _digitalCards)
            {
                DigitalCardOutput concatenator = ((DigitalCardOutput)data.Output[card.Key]);
                card.Value.Finished += OnFinishing;
                card.Value.Initialized += OnInitialization;
                card.Value.Initialize(card.Key + "/", concatenator.Output.Length, concatenator.SampleRate);
                card.Value.Synchronize(_syncMaster);
                uint[] output = concatenator.Output;
                card.Value.Data(output);
            }

            if (null != Initialized)
                Initialized(this, new EventArgs());
        }

        /// <summary>
        /// Determines whether sending the data to the hardware system has finished.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if sending the data to the hardware system has finished, <c>false</c> otherwise.
        /// </returns>
        public bool HasFinished()
        {
            return hasFinishedEventBeenThrown;
        }

        /// <summary>
        /// Orders the hardware system to start putting the data on its outputs.
        /// </summary>
        public void Start()
        {          
            hasFinishedEventBeenThrown = false;

            foreach (NIBasicDigitalCard card in _digitalCards.Values)
            {
                card.Start();
            }
            foreach (NIBasicAnalogCard card in _analogCards.Values)
            {
                card.Start();
            }
            _syncMaster.Start();
        }
        #endregion

        /// <summary>
        /// Called when a card has finished its initialized.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnInitialization(object sender, EventArgs e)
        {
            lock (_initPadlock)
            {
                _initialized++;
            }
        }

        /// <summary>
        /// Called when a card finishes output.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void OnFinishing(object sender, EventArgs e)
        {
            lock (_finishPadlock)
            {
                _finished++;
            }

            if (_finished == _initialized)
            {
                DisposeCards();
                hasFinishedEventBeenThrown = true;
                _analogCards = new Dictionary<string, NIBasicAnalogCard>();
                _digitalCards = new Dictionary<string, NIBasicDigitalCard>();

                if (null != Finished)
                    Finished(this, new EventArgs());
            }
        }

        /// <summary>
        /// Disposes the cards.
        /// </summary>
        private void DisposeCards()
        {
            foreach (NIBasicAnalogCard aCard in _analogCards.Values)
            {
                aCard.Dispose();
            }

            foreach (NIBasicDigitalCard dCard in _digitalCards.Values)
            {
                dCard.Dispose();
            }

            _syncMaster.Dispose();

        }

    }
}