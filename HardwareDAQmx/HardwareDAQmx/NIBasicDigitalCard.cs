using NationalInstruments.DAQmx;
using System;

namespace HardwareDAQmx
{
    public abstract class NIBasicDigitalCard:NIBasicCard
    {
        private readonly DigitalSingleChannelWriter _writer;


        /// <summary>
        /// Constructor for a digital output card
        /// </summary>
        /// <param name="name">needs the name of the card</param>
        public NIBasicDigitalCard()
            :base()
        {
            _writer = new DigitalSingleChannelWriter(_myTask.Stream);
        }

        /// <summary>
        /// Configures the initialized card
        /// </summary>
        /// <param name="physicalChannelName">Physical channel name e.g. DO1/ ... be careful analog cards differ</param>
        /// <param name="samplesPerChannel">Total amount of samples to output</param>
        /// <param name="rate">Samplerate in Hz</param>
        public abstract void Initialize(string physicalChannelName, int samplesPerChannel, double rate);


        /// <summary>
        /// Writes timestep (samples) array to the hardware device
        /// </summary>
        /// <param name="output">Array of timesteps</param>
        public void Data(UInt32[] output)
        {
            _writer.WriteMultiSamplePort(false, output);
        }


    }
}
