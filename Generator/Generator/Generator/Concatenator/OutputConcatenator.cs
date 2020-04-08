using Communication.Interfaces.Generator;
using System;
using System.Collections.Generic;

namespace Generator.Generator.Concatenator
{
    /// <summary>
    /// Manages the list of all <see cref=" AnalogCardOutput"/>'s and the list of all <see cref=" DigitalCardOutput"/>.
    /// Allows adding cards and time-steps, and filling the missing time-steps from sequences.
    /// </summary>
    /// <seealso cref="Communication.Interfaces.Generator.IConcatenator" />
    public class OutputConcatenator : IConcatenator
    {
        /// <summary>
        /// A dictionary that stores all <see cref=" AnalogCardOutput"/>'s as values and the card names as keys
        /// </summary>
        private readonly Dictionary<string, AnalogCardOutput> _analogCards = new Dictionary<string, AnalogCardOutput>();
        /// <summary>
        /// A dictionary that stores all <see cref=" DigitalCardOutput"/>'s as values and the card names as keys
        /// </summary>
        private readonly Dictionary<string, DigitalCardOutput> _digitalCards = new Dictionary<string, DigitalCardOutput>();
        /// <summary>
        /// The name of the currently treated card
        /// </summary>
        public string ActualCardName;
        /// <summary>
        /// The index of the currently treated channel
        /// </summary>
        public uint ActualChannelNumber;

        /// <summary>
        /// Gets the digital cards dictionary.
        /// </summary>
        /// <value>
        /// The digital cards dictionary.
        /// </value>
        public Dictionary<string, DigitalCardOutput> DigitalCards
        {
            get { return _digitalCards; }
        }

        /// <summary>
        /// Gets the analog cards dictionary.
        /// </summary>
        /// <value>
        /// The analog cards dictionary.
        /// </value>
        public Dictionary<string, AnalogCardOutput> AnalogCards
        {
            get { return _analogCards; }
        }


        #region IConcatenator Members


        /// <summary>
        /// Generate the final output of the concatenator.
        /// </summary>
        /// <returns>
        /// A dictionary that maps each card's name (as a <c>string</c>) to an object of a card type which represents the final raw output.
        /// </returns>
        public IModelOutput Output()
        {
            var output = new Dictionary<string, ICardOutput>();
            ModelOutput modelOutput = new ModelOutput(output);

            foreach (var analogCard in _analogCards)
            {
                output.Add(analogCard.Key, analogCard.Value);
            }

            foreach (var digitalCard in _digitalCards)
            {
                output.Add(digitalCard.Key, digitalCard.Value);
            }


            return modelOutput;
        }

        #endregion

        /// <summary>
        /// Adds an analog card to the collection of all cards.
        /// </summary>
        /// <param name="name">The name of the card to add.</param>
        /// <param name="card">The card.</param>
        public void AddCard(string name, AnalogCardOutput card)
        {
            _analogCards.Add(name, card);
        }

        /// <summary>
        /// Adds a digital card to the collection of all cards.
        /// </summary>
        /// <param name="name">The name of the card to add.</param>
        /// <param name="card">The card.</param>
        public void AddCard(string name, DigitalCardOutput card)
        {
            _digitalCards.Add(name, card);
        }

        /// <summary>
        /// Adds an array of digital time-steps to the currently active card. 
        /// Throws an exception if the list of all digital cards doesn't contain the currently active card.
        /// </summary>
        /// <param name="timeSteps">The time steps to add.</param>
        /// <exception cref="System.Exception"></exception>
        public void AddSteps(uint[] timeSteps)
        {
            if (!_digitalCards.ContainsKey(ActualCardName))
                throw new Exception("Could not find concatenator for " + ActualCardName);

            _digitalCards[ActualCardName].Add(timeSteps, ActualChannelNumber);
        }

        /// <summary>
        /// Adds an array of analog time-steps to the currently active card. 
        /// Throws an exception if the list of all analog cards doesn't contain the currently active card.
        /// </summary>
        /// <param name="timeSteps">The time steps to add.</param>
        /// <exception cref="System.Exception"></exception>
        public void AddSteps(double[] timeSteps)
        {
            if (!_analogCards.ContainsKey(ActualCardName))
                throw new Exception("Could not find concatenator for " + ActualCardName);

            _analogCards[ActualCardName].Add(timeSteps, ActualChannelNumber);
        }

        /// <summary>
        /// Fills the missing time-steps in specified sequence.
        /// </summary>
        /// <param name="totalDurationOfSequence">The total duration of sequence measured in time-steps.</param>
        /// <param name="sequenceIndex">Index of the sequence to fill.</param>
        public void FillSequence(uint totalDurationOfSequence, int sequenceIndex)
        {
            if (_analogCards.ContainsKey(ActualCardName))
                _analogCards[ActualCardName].FillSequence(totalDurationOfSequence, sequenceIndex);

            if (_digitalCards.ContainsKey(ActualCardName))
                _digitalCards[ActualCardName].FillSequence(totalDurationOfSequence);
        }

        


    }
}