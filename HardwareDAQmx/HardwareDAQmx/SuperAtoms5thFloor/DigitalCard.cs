using NationalInstruments.DAQmx;
using System;

namespace HardwareDAQmx.SuperAtoms5thFloor
{
    /// <summary>
    /// Implementation of a digital card for NI hardware
    /// </summary>
    public class DigitalCard:NIBasicDigitalCard
    {
         
        /// <summary>
        /// Configures the initialized card
        /// </summary>
        /// <param name="physicalChannelName">Physical channel name e.g. DO1/ ... be careful analog cards differ</param>
        /// <param name="samplesPerChannel">Total amount of samples to output</param>
        /// <param name="rate">Samplerate in Hz</param>
        public override void Initialize(string physicalChannelName, int samplesPerChannel, double rate)
        {
            _myTask.DOChannels.CreateChannel(physicalChannelName + "port0_32",
                                             "",
                                             ChannelLineGrouping.OneChannelForAllLines);
            _cardname = physicalChannelName + "port0_32";

            //configure SampleClock
            _myTask.Timing.ConfigureSampleClock("/" + physicalChannelName + "PXI_Trig7", //onboard clock
                                                rate,
                                                SampleClockActiveEdge.Rising,
                                                SampleQuantityMode.FiniteSamples,
                                                samplesPerChannel);
            OnInitialized(new EventArgs());
        }

        /// <summary>
        /// Synchronizes card to a master card
        /// </summary>
        /// <param name="master">To which master shall i synchronize</param>
        public override void Synchronize(NIBasicAnalogCard master)
        {
            _myTask.Control(TaskAction.Verify);
            string masterTerminalNameBase = GetMasterTerminalBaseName(master);
            _myTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(masterTerminalNameBase + "ao/StartTrigger",
                                                                      DigitalEdgeStartTriggerEdge.Falling);
           
        }

    }
}