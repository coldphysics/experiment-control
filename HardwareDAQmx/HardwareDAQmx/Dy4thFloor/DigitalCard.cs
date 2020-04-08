using NationalInstruments.DAQmx;
using System;

namespace HardwareDAQmx.Dy4thFloor
{
    /// <summary>
    /// Implementation of a digital card for NI hardware
    /// </summary>
    public class DigitalCard:NIBasicDigitalCard
    {


        /// <summary>
        /// Configures the initialized card
        /// </summary>
        /// <param name="physicalChannelName">Physical channel name e.g. DO1/ ... becareful analog cards differ</param>
        /// <param name="samplesPerChannel">Total amount of samples to output</param>
        /// <param name="rate">Sample rate in Hz</param>
        public override void Initialize(string physicalChannelName, int samplesPerChannel, double rate)
        {
            // initialize channels
            _myTask.DOChannels.CreateChannel(physicalChannelName + "port0_32",
                                             "",
                                             ChannelLineGrouping.OneChannelForAllLines);
            _cardname = physicalChannelName + "port0_32";

            // configure slave sample clock
            _myTask.Timing.ConfigureSampleClock("",
                rate, SampleClockActiveEdge.Falling, SampleQuantityMode.FiniteSamples, samplesPerChannel);
            
            OnInitialized(new EventArgs());
        }

        /// <summary>
        /// Synchronizes card to a master card
        /// </summary>
        /// <param name="master">To which master shall I synchronize</param>
        public override void Synchronize(NIBasicAnalogCard master)
        {
            string baseName = GetMasterTerminalBaseName(master);
            _myTask.Timing.SampleClockSource = baseName + "ao/SampleClock";
            // and listen to start trigger
            _myTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(baseName + "ao/StartTrigger", DigitalEdgeStartTriggerEdge.Rising);
            System.Diagnostics.Debug.Write("\nslave: " + _cardname);
            _myTask.Control(TaskAction.Verify);

        }
    }
}