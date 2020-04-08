using Communication.Interfaces.Generator;
using Generator.Generator.Concatenator;

namespace HardwareDAQmx.SuperAtoms5thFloor
{
    /// <summary>
    /// The hardware group for a Chassis-based NI experiment.
    /// </summary>
    /// <seealso cref="HardwareDAQmx.NIBasicHWGroup" />
    public class SuperAtomsHwGroup : NIBasicHWGroup
    {
        /// <summary>
        /// The name of the master card.
        /// </summary>
        private const string MASTER_CARD_NAME = "IN";

        /// <summary>
        /// Creates all digital and analog cards (including the synchronization master)
        /// </summary>
        /// <param name="numAnalogCards">The number of analog cards.</param>
        /// <param name="numDigitalCards">The number of digital cards.</param>
        protected override void CreateCards(int numAnalogCards, int numDigitalCards)
        {
            _syncMaster = new AnalogCard();
            //The sync master is not one of the regular analog cards

            for (int i = 0; i < numAnalogCards; i++)
                _analogCards.Add(ANALOG_CARD_BASE_NAME + (i + 1).ToString(), new AnalogCard());

            for(int i = 0; i < numDigitalCards; i++)
                _digitalCards.Add(DIGITAL_CARD_BASE_NAME + (i + 1).ToString(), new DigitalCard());

        }
        /// <summary>
        /// Initializes the master card.
        /// </summary>
        /// <param name="data">The otuput data.</param>
        protected override void InitializeMasterCard(IModelOutput data)
        {
            AnalogCardOutput firstAnalogCardOutput = ((AnalogCardOutput)data.Output[ANALOG_CARD_BASE_NAME + "1"]);
            int totalAmountOfSamples = firstAnalogCardOutput.Output.GetLength(1);
            double[,] samplesForMaster = new double[1, totalAmountOfSamples];
            double sampleRate = firstAnalogCardOutput.SampleRate;

            _syncMaster.Initialize(MASTER_CARD_NAME + "/ao0", MIN_VOLTAGE, MAX_VOLTAGE, totalAmountOfSamples, sampleRate);
            _syncMaster.Synchronize();
            _syncMaster.Data(samplesForMaster);
        }
    }
}