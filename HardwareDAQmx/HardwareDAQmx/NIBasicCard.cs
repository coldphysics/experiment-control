using NationalInstruments.DAQmx;
using System;

namespace HardwareDAQmx
{
    /// <summary>
    /// Provides the basic functionality of all NationalInstrumets-based cards.
    /// </summary>
    public abstract class NIBasicCard
    {
        /// <summary>
        /// The NI-task that will be associated with the current card. A task defines the channels, buffers, time-settings ,etc... of one card of the experiment.
        /// </summary>
        protected readonly NationalInstruments.DAQmx.Task _myTask;

        /// <summary>
        /// The name of this card.
        /// </summary>
        protected string _cardname;

        /// <summary>
        /// Initializes a new instance of the <see cref="NIBasicCard"/> class.
        /// </summary>
        public NIBasicCard()
        {
            //The task is assigned with a unique name
            _myTask = new NationalInstruments.DAQmx.Task();
            _myTask.Done += myTask_Done;
        }

        /// <summary>
        /// Gets a reference to the task.
        /// </summary>
        /// <value>
        /// The task.
        /// </value>
        public NationalInstruments.DAQmx.Task Task
        {
            get { return _myTask; }
        }

        /// <summary>
        /// Occurs when the task signals its completion
        /// </summary>
        public event EventHandler Finished;
        /// <summary>
        /// Occurs when task and output cards are initialized.
        /// </summary>
        public event EventHandler Initialized;

        /// <summary>
        /// Raises the <see cref="E:Initialized" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void OnInitialized(EventArgs e)
        {
            System.Diagnostics.Debug.Write("\nCard " + _cardname + " initialized!");

            if (Initialized != null)
                Initialized(this, e);
        }


        /// <summary>
        /// Actually start output of data
        /// </summary>
        public virtual void Start()
        {
            _myTask.Start();

            System.Diagnostics.Debug.Write("\nCard " + _cardname + " Started!");
        }

        /// <summary>
        /// Free resources
        /// </summary>
        public virtual void Dispose()
        {
            if (_myTask != null)
            {
                _myTask.Dispose();
                System.Diagnostics.Debug.Write("\nCard " + _cardname + " disposed!");
            }
        }

        /// <summary>
        /// This function gets called when the working thread has finished
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void myTask_Done(object sender, TaskDoneEventArgs e)
        {
            System.Diagnostics.Debug.Write("\nCard " + _cardname + " finished!");

            if (Finished != null)
            {
                Finished(this, new EventArgs());
            }
        }

        /// <summary>
        /// Synchronizes the current card with the specified master card (which should be an analog card)
        /// </summary>
        /// <param name="master">The master card.</param>
        public abstract void Synchronize(NIBasicAnalogCard master);

        /// <summary>
        /// Gets a string that represents the trigger source for a slave card based on the master card.
        /// </summary>
        /// <param name="master">The master card.</param>
        /// <returns>The trigger source</returns>
        protected string GetMasterTerminalBaseName(NIBasicAnalogCard master)
        {
            master.Task.Control(TaskAction.Verify);

            string masterFirstPhysChanName = master.Task.AOChannels[0].PhysicalName;
            string masterDeviceName = masterFirstPhysChanName.Split('/')[0];
            string masterTerminalNameBase = "/" + GetDeviceName(masterDeviceName) + "/";

            return masterTerminalNameBase;
        }
        /// <summary>
        /// Figures out the devicename 
        /// </summary>
        /// <param name="deviceName">FIXME devicename wtf?!?!</param>
        /// <returns>Device name</returns>
        protected string GetDeviceName(string deviceName)
        {
            Device device = DaqSystem.Local.LoadDevice(deviceName);
            if (device.BusType != DeviceBusType.CompactDaq)
                return deviceName;
            else
                return device.CompactDaqChassisDeviceName;
        }
    
    }
}
