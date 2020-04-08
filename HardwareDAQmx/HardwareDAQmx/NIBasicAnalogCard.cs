using NationalInstruments.DAQmx;

namespace HardwareDAQmx
{
    /// <summary>
    /// Provides the basic functionality of all NationalInstrumets-based analog cards.
    /// </summary>
    /// <seealso cref="HardwareDAQmx.NIBasicCard" />
    public abstract class NIBasicAnalogCard : NIBasicCard
    {
        /// <summary>
        /// The writer that is used to write samples to the card.
        /// </summary>
        private readonly AnalogMultiChannelWriter _writer;


        /// <summary>
        /// Initializes a new instance of the <see cref="NIBasicAnalogCard"/> class.
        /// </summary>
        public NIBasicAnalogCard()
            : base()
        {
            _writer = new AnalogMultiChannelWriter(_myTask.Stream);
        }


        /// <summary>
        /// Configures the initialized card
        /// </summary>
        /// <param name="physicalChannelName">Physical channel name e.g. IN/ao0 ... be careful digital cards differ</param>
        /// <param name="samplesPerChannel">Total amount of samples to output</param>
        /// <param name="rate">Sample Rate in Hz</param>
        public abstract void Initialize(string physicalChannelName, double minimumValue, double maximumValue,
                              int samplesPerChannel, double rate);
        

        /// <summary>
        /// Makes the current card a master card.
        /// </summary>
        public abstract void Synchronize();



        /// <summary>
        /// Writes timestep (samples) array to the hardware device
        /// </summary>
        /// <param name="output">Array of timesteps</param>        
        public void Data(double[,] output)
        {
            //The constructor of the class NIBasicCard ensures cleanup when the task is done

            if (true)
                _writer.WriteMultiSample(false, output);
        }
    }
}
