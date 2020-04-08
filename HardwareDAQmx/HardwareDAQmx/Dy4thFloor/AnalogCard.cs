using NationalInstruments.DAQmx;
using System;
using System.Windows;

namespace HardwareDAQmx.Dy4thFloor
{
    /// <summary>
    /// Implementation of an analog card for NI hardware
    /// </summary>
    public class AnalogCard : NIBasicAnalogCard
    {
        /// <summary>
        /// Configures the initialized card
        /// </summary>
        /// <param name="physicalChannelName">Physical channel name e.g. IN/ao0 ... be careful digital cards differ</param>
        /// <param name="samplesPerChannel">Total amount of samples to output</param>
        /// <param name="rate">Sample rate in Hz</param>
        public override void Initialize(string physicalChannelName, double minimumValue, double maximumValue,
                              int samplesPerChannel, double rate)
        {
            try
            {
                _myTask.AOChannels.CreateVoltageChannel(physicalChannelName,
                                                        "",
                                                        minimumValue,
                                                        maximumValue,
                                                        AOVoltageUnits.Volts);
            }
            catch (Exception)
            {
                MessageBox.Show("One of the NI analog cards could not be found. Is everything connected?");
                throw;
            }

            _cardname = physicalChannelName;

            // slave uses the masters clock
            _myTask.Timing.ConfigureSampleClock("",
                rate, SampleClockActiveEdge.Falling, SampleQuantityMode.FiniteSamples, samplesPerChannel);

            OnInitialized(new EventArgs());
        }

        /// <summary>
        /// Initialize one card as master device for time synchronization
        /// </summary>
        public override void Synchronize()
        {
            // First, verify the master task so we can query its properties.
            _myTask.Control(TaskAction.Verify);

            string deviceName = _myTask.AOChannels[0].PhysicalName;

            System.Diagnostics.Debug.Write("\nmaster: " + deviceName +" synchronized!");
            /* due to a bug the master needs to write more samples than the slaves, search "DAQmx bug" in wiki */
            _myTask.Timing.SamplesPerChannel += 3;

            // check if this makes sense
            _myTask.Control(TaskAction.Verify);
        }

        /// <summary>
        /// Synchronizes the current card with the specified master card (which should be an analog card)
        /// </summary>
        /// <param name="master">The master card.</param>
        public override void Synchronize(NIBasicAnalogCard master)
        {
            System.Diagnostics.Debug.Write("\nslave: " + _cardname + " synchronized");
            string masterTerminalNameBase = GetMasterTerminalBaseName(master);

            _myTask.Timing.SampleClockSource = masterTerminalNameBase + "ao/SampleClock";
            _myTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger( masterTerminalNameBase + "ao/StartTrigger", DigitalEdgeStartTriggerEdge.Rising);
            _myTask.Control(TaskAction.Verify);
        }

    }
}