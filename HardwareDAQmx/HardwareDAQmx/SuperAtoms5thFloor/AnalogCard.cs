using NationalInstruments.DAQmx;
using System;
using System.Windows;

namespace HardwareDAQmx.SuperAtoms5thFloor
{
    /// <summary>
    /// Implementation of a analog card for NI hardware
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
                MessageBox.Show("Is the NI-PCIE-Bus-Extender box turned on? It is behind the analog channels and you have to reboot the computer afterwards!");
                throw;
            }

            _cardname = physicalChannelName;

            _myTask.Timing.ConfigureSampleClock("", //onboard clock
                                                rate,
                                                SampleClockActiveEdge.Rising,
                                                SampleQuantityMode.FiniteSamples,
                                                samplesPerChannel);
            OnInitialized(new EventArgs());
        }

        /// <summary>
        /// Initialize one card as master device for time synchronization
        /// </summary>
        public override void Synchronize()
        {
            // First, verify the master task so we can query its properties.
            _myTask.Control(TaskAction.Verify);

            _myTask.Timing.SamplesPerChannel += 3;
            _myTask.Timing.ReferenceClockSource = "PXIe_Clk100";
            _myTask.ExportSignals.ExportHardwareSignal(ExportSignal.SampleClock, "PXI_Trig7");
        }

        /// <summary>
        /// Synchronize the card to a master device
        /// </summary>
        /// <param name="master">To which master shall i synchronize</param>
        public override void Synchronize(NIBasicAnalogCard master)
        {
            // First, verify the master task so we can query its properties.
            master.Task.Control(TaskAction.Verify);

            string masterTerminalNameBase = GetMasterTerminalBaseName(master);

            _myTask.Timing.SampleClockSource = masterTerminalNameBase + "ao/SampleClock";

            _myTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(masterTerminalNameBase + "ao/StartTrigger",
                                                                      DigitalEdgeStartTriggerEdge.Falling);
        }


    }
}