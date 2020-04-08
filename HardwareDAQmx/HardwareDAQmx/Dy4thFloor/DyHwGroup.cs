using Communication.Interfaces.Generator;
using Generator.Generator.Concatenator;

namespace HardwareDAQmx.Dy4thFloor
{
    /// <summary>
    /// The hardware group for a PCI-Cards-based experiment
    /// </summary>
    /// <seealso cref="HardwareDAQmx.NIBasicHWGroup" />
    public class DyHwGroup : NIBasicHWGroup
    {
        /// <summary>
        /// The name of the master card ("AO1")
        /// </summary>
        private const string MASTER_CARD_NAME = ANALOG_CARD_BASE_NAME + "1";

        /// <summary>
        /// Creates all digital and analog cards (including the synchronization master)
        /// </summary>
        /// <param name="numAnalogCards">The number of analog cards.</param>
        /// <param name="numDigitalCards">The number of digital cards.</param>
        protected override void CreateCards(int numAnalogCards, int numDigitalCards)
        {
            _syncMaster = new AnalogCard();
            //The sync master is one of the regular analog cards, that's why the following loop starts from 1

            for (int i = 1; i < numAnalogCards; i++)
                _analogCards.Add(ANALOG_CARD_BASE_NAME + (i + 1).ToString(), new AnalogCard());

            for (int i = 0; i < numDigitalCards; i++)
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
            double[,] samplesForMaster = firstAnalogCardOutput.Output;
            double sampleRate = firstAnalogCardOutput.SampleRate;

            _syncMaster.Initialize(MASTER_CARD_NAME + "/ao0:7", MIN_VOLTAGE, MAX_VOLTAGE, totalAmountOfSamples, sampleRate);
            _syncMaster.Synchronize();
            _syncMaster.Data(samplesForMaster);
        }



    }
}