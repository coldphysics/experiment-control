using System.Diagnostics;
using System.Globalization;
using Communication.Interfaces.Hardware;
using Model.Root;
using Model.Variables;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Errors.Error;
using MySql.Data.MySqlClient;
using Communication.Interfaces.Generator;
using Model.Settings;
using Model.Settings.Settings;
using System.Text;
using Model.MeasurementRoutine.GlobalVariables;
using Model.MeasurementRoutine;
using PythonUtils.FileExecution;
using Buffer.DatabaseAccess;

namespace Buffer.Basic
{
    public class BeforeIteratingVariablesEventArgs : EventArgs
    {
        public bool ModelWillChange { set; get; }
        public int NextModelIndex { set; get; }
    }
    //RECO hide the public functionalities of the OutputHandler within the DoubleBuffer    
    /// <summary>
    /// Handles the output loop 
    /// </summary>
    /// <remarks>
    /// This class handles the primary loop that controls and interacts with most other components
    /// in the software,
    /// </remarks>
    public class OutputHandler
    {

        #region Enums
        //  **************** enums *************************        
        /// <summary>
        /// The running mode that the cycle currently follows.
        /// </summary>
        public enum CycleStates
        {
            /// <summary>
            /// The cycle is stopped
            /// </summary>
            Stopped,
            /// <summary>
            /// The cycle is running without incrementing iterator variables.
            /// </summary>
            Running,
            /// <summary>
            /// The cycle is running while incrementing iterator variables. 
            /// When iterator variables reach their final values, they start from the beginning.
            /// </summary>
            Scanning,
            /// <summary>
            /// The cycle is running while incrementing iterator variables. 
            /// When iterator variables reach their final values, they retain these values.
            /// </summary>
            ScanningOnce
        };

        /// <summary>
        /// The state of the output loop thread.
        /// </summary>
        public enum OutputLoopStates
        {
            /// <summary>
            /// The output loop is waiting for the hardware to finish the previous cycle.
            /// </summary>
            WaitForHardware,
            /// <summary>
            /// The output loop is waiting for the user to initiate a start.
            /// </summary>
            Sleeping,
            /// <summary>
            /// The output loop is in the preparation phase before starting the output.
            /// </summary>
            Preparing,
            /// <summary>
            /// The output loop is in the phase after initiating a start
            /// </summary>
            PostStart,
            /// <summary>
            /// The output loop is waiting before initiating a start if the previous cycle finished before the (cycletime + gaptime) are over.
            /// </summary>
            WaitForStart,
            /// <summary>
            /// The output loop is waiting for the data to be generated.
            /// </summary>
            WaitForIteration
        }

        /// <summary>
        /// Describes the potential states of a cycle, intended to be used for database storage
        /// </summary>
        public enum OperatingMode
        {
            /// <summary>
            /// No measurement routine and not iterating
            /// </summary>
            STATIC,
            /// <summary>
            /// No measurement routine and iterating
            /// </summary>
            ITERATION,
            /// <summary>
            /// A measurement routine (might be also iterating)
            /// </summary>
            MEASUREMENT_ROUTINE
        }
        #endregion

        #region Attributes, Properties, Events, and Constants
        // ********************** CONSTANTS ****************************        
        /// <summary>
        /// The sleep period which is done in the busy-waiting loop that waits for the hardware to finish the output.
        /// </summary>
        private const int WAIT_FOR_HARDWARE_TO_RETURN_BUSY_WAITING_SLEEP_PERIOD = 10; //ms        
        /// <summary>
        /// The sleep period which is done in the busy-waiting loop that waits for the data to be generated.
        /// </summary>
        private const int WAIT_FOR_ITERATED_DATA_BUSY_WAITING_SLEEP_PERIOD = 10; //ms        
        /// <summary>
        /// The number of milliseconds one second has
        /// </summary>
        private const int SECOND = 1000;//ms        
        /// <summary>
        /// The extra time to wait for the first cycle. This accounts for possible one-time initializations or operations.
        /// </summary>
        private const int EXTRA_WAIT_FOR_FIRST_CYCLE = 2 * SECOND;//ms

        //  **************** variables/objects *************************        
        /// <summary>
        /// Indicates whether iterations should be shuffled.
        /// </summary>
        public bool shuffleIterations = false;
        /// <summary>
        /// Indicates whether iteration should be paused keeping the last generated model looping.
        /// </summary>
        public bool pause = false;
        /// <summary>
        /// Indicates that the global counter should always increase even if the iterators are not incrementing (unless pause is also pressed)
        /// </summary>
        public bool alwaysIncrease = false;
        /// <summary>
        /// Indicates that the output loop should stop after the first whole scan of iterators is over.
        /// </summary>
        public bool stopAfterScan;


        /// <summary>
        /// Gets or sets the model-specific counters.
        /// </summary>
        /// <value>
        /// The model counters.
        /// </value>
        public ModelSpecificCounters ModelCounters { set; get; }

        /// <summary>
        /// Gets or sets a value indicating whether a measurement routine is currently running
        /// </summary>
        /// <value>
        /// <c>true</c> if the program is in the measurement routine mode; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This value is used for DB storage
        /// </remarks>
        public bool IsMeasurementRoutineMode
        { set; get; }

        /// <summary>
        /// Gets or sets the index of the model within the current measurement routine.
        /// </summary>
        /// <value>
        /// The index of the current model if a measurement routine is running, otherwise -1.
        /// </value>
        public int ModelIndex
        { set; get; }


        //Ebaa 11.06
        private int _startGlobalCounterOfMeasurementRoutine;
        /// <summary>
        /// Gets or sets the start global counter of the current measurement routine.
        /// </summary>
        /// <value>
        /// The start global counter of the measurement routine.
        /// </value>
        public int StartGlobalCounterOfMeasurementRoutine
        {
            get
            {
                if (IsMeasurementRoutineMode)
                    return _startGlobalCounterOfMeasurementRoutine;
                else return 0;
            }

            set
            {
                _startGlobalCounterOfMeasurementRoutine = value;
            }
        }

        /// <summary>
        /// The python file executor used to execute time-critical python files.
        /// </summary>
        private readonly BasicPythonFileExecutor pyExecutor;
        /// <summary>
        /// The hardware manager that is used to communicate with the hardware.
        /// </summary>
        private readonly IHardwareManager _hardwareManager;

        /// <summary>
        /// Provides access to the underlying database.
        /// </summary>
        private readonly DatabaseManager _databaseManager;
        /// <summary>
        /// The root to the data model hierarchy
        /// </summary>
        private RootModel _rootModel;
        /// <summary>
        /// The duration of the current cycle measured in milliseconds.
        /// </summary>
        private double _cycleDuration;
        /// <summary>
        /// The variable that controls the busy-waiting loop that waits for the data to be generated.
        /// </summary>
        private bool _waitForGeneratedData;
        /// <summary>
        /// Indicates whether new data was generated in the last cycle.
        /// </summary>
        private bool iteratedInLastCycle = true;
        /// <summary>
        /// A local copy of the global counter. This value could get out-of-sync with 
        /// the value stored in the database if the settings does not order the program to save on the database.
        /// </summary>
        private int _globalCounter;
        /// <summary>
        /// The number of iterations before a single scan finishes.
        /// </summary>
        private int _numberOfIterations;
        /// <summary>
        /// The holds the current state (running mode) of the cycle
        /// </summary>
        private CycleStates _cycleState;

        private int _timesToReplicateOutput;

        //  ********************** events *************************      

        public event BeforeIteratingVariablesEventHandler BeforeIteratingVariables;
        public delegate void BeforeIteratingVariablesEventHandler(object sender, BeforeIteratingVariablesEventArgs e);

        /// <summary>
        /// Occurs when [after iterating variables].
        /// </summary>
        public event EventHandler AfterIteratingVariables;

        /// <summary>
        /// Occurs when the state of the iterators changes.
        /// </summary>
        public event EventHandler OnScanChange;
        /// <summary>
        /// Occurs when current state of the output loop changes.
        /// </summary>
        public event EventHandler OnOuputLoopStateChange;

        /// <summary>
        /// Is connected to the VariablesController 
        /// </summary>
        public event IterateVariablesEventHandler IterateVariables;
        /// <summary>
        /// The delegate that the <see cref=" IterateVariables"/> event uses.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public delegate void IterateVariablesEventHandler(object sender, EventArgs e);


        /// <summary>
        /// Is connected to the VariablesController 
        /// </summary>
        public event ResetIteratorValuesEventHandler ResetIteratorValues;
        /// <summary>
        /// The delegate that the <see cref=" ResetIteratorValues"/> event uses.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public delegate void ResetIteratorValuesEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Occurs when iterator variables have to be locked (become readonly).
        /// </summary>
        public event LockVariablesEventHandler LockIterators;
        /// <summary>
        /// The delegate that the <see cref=" LockIterators"/> event uses.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public delegate void LockVariablesEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Occurs when iterator variables have to be unlocked (become editable).
        /// </summary>
        public event UnlockVariablesEventHandler UnlockIterators;
        /// <summary>
        /// The delegate that the <see cref=" UnlockIterators"/> event uses.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public delegate void UnlockVariablesEventHandler(object sender, EventArgs e);

        //  **************** lock variables *************************

        /// <summary>
        /// main loop lock object during the output (this disallows other components from altering the CycleState when the output is running)
        /// </summary>
        private static readonly object outputLoopLock = new object();

        /// <summary>
        /// Allows the OutputThread to sleep waiting for new input, and wake up when it "comes"  
        /// </summary>
        private static readonly object outputLoopSleepLock = new object();

        //  **************** generated data ready for hardware *************************        
        /// <summary>
        /// The data resulting from the generator.
        /// </summary>
        private IModelOutput _rawOutput; // binary data for hardware

        // ********************* Threads *************************        
        /// <summary>
        /// The output thread
        /// </summary>
        private Thread outputThread;
        /// <summary>
        /// The data saver thread
        /// </summary>
        private Thread dataSaverThread;
        /// <summary>
        /// The thread that initiates a separate process to run the non-time-critical python script for the start of a new scan.
        /// </summary>
        private Thread startScanPythonScriptThread;
        /// <summary>
        /// The thread that initiates a separate process to run the non-time-critical python script for each iteration.
        /// </summary>
        private Thread everyIterationPythonScriptThread;
        /// <summary>
        /// The thread that runs the script that controls Lecroy.
        /// </summary>
        private Thread lecroyControllerThread;

        //  **************** Python Processes *************************        
        /// <summary>
        /// The process to run the non-time-critical python script for the start of a new scan.
        /// </summary>
        private Process pStartScan;
        /// <summary>
        /// The process to run the non-time-critical python script for each iteration.
        /// </summary>
        private Process pEverySingleIteration;

        //  ***************** properties *************************        
        /// <summary>
        /// Gets the global counter.
        /// </summary>
        /// <value>
        /// The global counter.
        /// </value>
        public int GlobalCounter
        {
            get { return _globalCounter; }
            private set
            {
                _globalCounter = value;
                ErrorCollector ec = ErrorCollector.Instance;
                ec.GlobalCounter = GlobalCounter;
            }
        }

        //Ebaa 11.06

        /// <summary>
        /// Gets the value of the global counter when the current "set" of scans has been initiated.
        /// </summary>
        /// <value>
        /// The value of the global counter when the current "set" of scans has been initiated.
        /// </value>
        public int StartCounterOfScansOfCurrentModel
        {
            get
            {
                return ModelCounters.StartCounterOfScansOfCurrentModel;
            }
            set
            {
                // This is done when stopping the whole experiment or stopping the iteration only
                if (value == 0 && ModelCounters.StartCounterOfScansOfCurrentModel != 0)
                {
                    LastStartCounterOfScans = ModelCounters.StartCounterOfScansOfCurrentModel;
                }
                ModelCounters.StartCounterOfScansOfCurrentModel = value;
            }
        }

        /// <summary>
        /// Gets the last start counter of scans.
        /// </summary>
        /// <value>
        /// The last start counter of scans.
        /// </value>
        public int LastStartCounterOfScans { get; private set; }
        /// <summary>
        /// Gets the state of the output loop.
        /// </summary>
        /// <value>
        /// The state of the output loop.
        /// </value>
        public OutputLoopStates OutputLoopState { get; private set; }
        /// <summary>
        /// Gets the number of completed scans.
        /// </summary>
        /// <value>
        /// The number of completed scans.
        /// </value>
        //public int CompletedScans { get; private set; }
        public int CompletedScans
        {
            get
            {
                return ModelCounters.CompletedScans;

            }
            set
            {
                ModelCounters.CompletedScans = value;
            }
        }
        /// <summary>
        /// Gets the number of the current iteration within the current scan
        /// </summary>
        /// <value>
        /// The number of the current iteration within the current scan
        /// </value>
        public int IterationOfScan
        {
            get
            {
                return ModelCounters.IterationOfScan;
            }
            set
            {
                ModelCounters.IterationOfScan = value;
            }
        }
        /// <summary>
        /// Gets or sets the number of iterations a single scan holds.
        /// </summary>
        /// <value>
        /// The number of iterations a single scan holds.
        /// </value>
        public int NumberOfIterations
        {
            get { return _numberOfIterations; }
            set
            {
                _numberOfIterations = value;
                UpdateIteratorState();
            }
        }

        /// <summary>
        /// Gets or sets the state of the output cycle.
        /// </summary>
        /// <value>
        /// The state of the output cycle.
        /// </value>
        /// <remarks>Setting the cycle state is treated as an "order" for the output loop, thus it has side-effects.</remarks>
        public CycleStates OutputCycleState
        {
            get { return _cycleState; }
            set
            {
                lock (outputLoopSleepLock)
                {
                    lock (outputLoopLock)
                    {
                        CycleStates oldstate = _cycleState;

                        if (_cycleState != value)
                        {
                            _cycleState = value;

                            if (_cycleState == CycleStates.Running || _cycleState == CycleStates.Stopped)
                            {
                                ResetIteratorValues(this, null); //TODO check if this is necessary
                                ModelCounters.Reset();
                                StartGlobalCounterOfMeasurementRoutine = 0;
                                StartCounterOfScansOfCurrentModel = 0;
                                UnlockIterators(this, null);
                            }

                            if (_cycleState == CycleStates.Scanning || _cycleState == CycleStates.ScanningOnce)
                            {
                                if (oldstate == CycleStates.Running || oldstate == CycleStates.Stopped)
                                {
                                    ModelCounters.Reset();
                                    StartGlobalCounterOfMeasurementRoutine = GlobalCounter;
                                    StartCounterOfScansOfCurrentModel = GlobalCounter;
                                    ModelCounters.GCIsSet = true;
                                    LockIterators(this, null);//This disables the UI of the iterators so the user cannot change them
                                }
                            }

                            UpdateIteratorState();

                            if (oldstate == CycleStates.Stopped)
                            {
                                //This wakes the OutputHandler thread when it is "Sleeping" (i.e., waiting for a Start command from the user)
                                Monitor.PulseAll(outputLoopSleepLock);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether to control the Lecroy device.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the Lecroy is to be controlled; otherwise, <c>false</c>.
        /// </value>
        public bool ControlLecroy { get; set; }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <returns>The model.</returns>
        public RootModel Model
        {
            get
            {
                return _rootModel;
            }
        }

        /// <summary>
        /// Gets the gap timespan from the settings.
        /// </summary>
        /// <value>
        /// The gap timespan measured in milliseconds.
        /// </value>
        private decimal GapTimespan
        {
            get
            {
                return ProfilesManager.GetInstance().ActiveProfile.GetSettingValueByName<decimal>(SettingNames.CONSTANT_WAIT_TIME) * SECOND;
            }
        }


        #endregion

        #region Methods

        //  **********************************************************************************************************
        //  *********************************** constructor **********************************************************        
        /// <summary>
        /// Initializes a new instance of the <see cref="OutputHandler"/> class.
        /// </summary>
        /// <param name="hardwareManager">The hardware manager.</param>
        public OutputHandler(IHardwareManager hardwareManager)
        {
            BasicPythonFileExecutorBuilder builder = new BasicPythonFileExecutorBuilder();
            this.pyExecutor = (BasicPythonFileExecutor)builder.Build();
            _hardwareManager = hardwareManager;
            _databaseManager = new DatabaseManager();
            
            // increase global counter by one so nothing will be overwritten
            GlobalCounter = _databaseManager.LoadGlobalCounter() + 1;
            ModelCounters = new ModelSpecificCounters();
            // the output thread is created once and runs 'till the program stops
            outputThread = new Thread(OutputLoop)
            {
                Name = "OutputThread",
                IsBackground = true,
                Priority = ThreadPriority.Highest
            };

            outputThread.Start();
        }



        //  *********************************** work loop **********************************************************
        /// <summary>
        /// The functionality of the OutputHandler is here
        /// </summary>
        private void OutputLoop()
        {
            //initialization
            bool first = true;
            DateTime nextStartTime = DateTime.Now;
            CycleStates cycleState;
            RootModel workLoopCopy;


            // main loop
            while (true)
            {

                //In the first loop no data has been sent to the hardware system yet, so do not wait for the hardware.
                if (!first)
                {
                    SetOutputLoopThreadState(OutputLoopStates.WaitForHardware);
                    /***/
                    //Wait until all output data has been transferred to the hardware system
                    WaitForHardware();
                }

                lock (outputLoopSleepLock)
                {
                    cycleState = _cycleState;//TODO why?

                    //If this is the first loop, or after being stopped (user clicked on 'Stop')
                    if (first || cycleState == CycleStates.Stopped)
                    {
                        //If the user clicked on 'stop', iterator variables are set to their starting values.
                        if (!first)
                            //TODO check if this is necassary.
                            ResetIteratorValues(this, null);//RECO try not to fire an event while locked


                        first = false;
                        SetOutputLoopThreadState(OutputLoopStates.Sleeping);
                        //Wait until the state of the cycle changes from "Stopped" to something else.
                        Monitor.Wait(outputLoopSleepLock);
                        //For this cycle, do not wait for the data from the buffer
                        _waitForGeneratedData = false;



                        //gapTimespan = ProfilesManager.GetInstance().ActiveProfile.GetSettingValueByName<int>(SettingNames.CONSTANT_WAIT_TIME) * SECOND;
                        // add additional 2000 ms for the startime after it is stopped to avoid delay errors
                        nextStartTime = DateTime.Now.AddMilliseconds((double)GapTimespan + EXTRA_WAIT_FOR_FIRST_CYCLE);
                    }
                }

                //busy waiting (until the Buffer finishes preparing the rawData and calls the SetNewCycleData()
                //if this is the first cycle after being stopped, do not wait for data (_waitForIteratedData is already set to false in the block above)
                while (_waitForGeneratedData)
                {
                    //TODO this might fire the event several times!
                    SetOutputLoopThreadState(OutputLoopStates.WaitForIteration);
                    Thread.Sleep(WAIT_FOR_ITERATED_DATA_BUSY_WAITING_SLEEP_PERIOD);
                }


                lock (outputLoopLock)//#
                {
                    //The data from the buffer is there
                    SetOutputLoopThreadState(OutputLoopStates.Preparing);
                    cycleState = _cycleState;
                    bool controlLecroy = ControlLecroy;

                    //This thread might be stuck at (#) because the user has clicked on Stop and SetCycleState was called (which locks the outputLoopLock)
                    if (cycleState == CycleStates.Stopped)
                    {
                        continue;
                    }

                    //initial value
                    iteratedInLastCycle = false;


                    //Make a local copy from the Model (received from the Buffer)
                    workLoopCopy = (RootModel)DeepClone(_rootModel);
                    workLoopCopy.EstimatedStartTime = nextStartTime;
                    workLoopCopy.GlobalCounter = GlobalCounter;
                    workLoopCopy.IsItererating = cycleState == CycleStates.Scanning ||
                                                 cycleState == CycleStates.ScanningOnce;


                    // everything has to wait until the variables and so on are stored in the database
                    /***/
                    EntryPOCO databaseEntry = GenerateDBEntryPOCO(workLoopCopy);
                    _databaseManager.CreateDatabaseEntry(databaseEntry);


                    // start python scripts as a background thread
                    // the first cycle when we have 'scanning' or 'scanning_once' states
                    if (IterationOfScan == 1)
                    {
                        /***/
                        RunPhythonScriptStartScanTimeCritical(workLoopCopy);

                        startScanPythonScriptThread = new Thread(RunPhythonScriptStartScan);
                        startScanPythonScriptThread.Name = "startScanPythonScriptThread";
                        startScanPythonScriptThread.Start();
                    }

                    /***/
                    RunPhythonScriptEverySingleIterationTimeCritical(workLoopCopy);


                    /***/
                    everyIterationPythonScriptThread = new Thread(RunPhythonScriptEverySingleIteration);
                    everyIterationPythonScriptThread.Name = "everyIterationPythonScriptThread";
                    everyIterationPythonScriptThread.Start();
                    _hardwareManager.Initialize(_rawOutput, _cycleDuration);

                    // the total cycle time consists of the cycle  and a gap to prepare the next cycle. Since the total time should be always the same a defined gap time is between every cycle
                    SetOutputLoopThreadState(OutputLoopStates.WaitForStart);
                    DateTime startTime = nextStartTime;
                    // timeToWait indicates how much time is remaining until (cycleduration + gap) to finish
                    double timeToWait = (startTime - DateTime.Now).TotalMilliseconds;
                    //TODO seems tricky! could have no other solution
                    if (timeToWait > 0)
                    {
                        if (timeToWait > 20)
                        {
                            Thread.Sleep((int)Math.Floor(timeToWait - 10));
                        }
                        // busy wait - between 10 and 20 ms
                        while (DateTime.Now < startTime)
                        {
                        }
                    }

                    /***/
                    // start the hardware (set param_1 to 2) 
                    // hardware takes the responsibility of outputting the cycle
                    _hardwareManager.Start();

                    // calculate next start time
                    nextStartTime = startTime.AddMilliseconds(_cycleDuration * _timesToReplicateOutput + (double)GapTimespan);

                    // we miss the start time of next cycle 
                    if (timeToWait < 0)
                    {
                        // correct for the overtime of the last cycle
                        nextStartTime = nextStartTime.AddMilliseconds(-timeToWait);
                        ErrorCollector errorCollector = ErrorCollector.Instance;
                        errorCollector.AddError(
                            "The preparation time in front of a cycles is too short!\nTime passed (ms): " +
                            ((double)GapTimespan + (-timeToWait)) +
                            "\nWanted preparation time (ms): " + GapTimespan,
                            ErrorCategory.Basic, true, ErrorTypes.ProgramError);
                    }

                    SetOutputLoopThreadState(OutputLoopStates.PostStart);


                    ProfilesManager profilesManager = ProfilesManager.GetInstance();
                    bool saveFirstModeltoXmlFile = false;// Indicates whether the (save only the first model file of iterating sequences) checkbox is checked.
                    if (profilesManager.ActiveProfile.DoesSettingExist(SettingNames.SAVE_MODEL_TO_XML))
                    {
                        bool saveToXMLFile = profilesManager.ActiveProfile.GetSettingValueByName<bool>(SettingNames.SAVE_MODEL_TO_XML);

                        if (saveToXMLFile)
                        {
                            saveFirstModeltoXmlFile = profilesManager.ActiveProfile.GetSettingValueByName<bool>(SettingNames.SAVE_FIRST_MODEL_TO_XML);
                        }

                    }



                    if (IterationOfScan == 1 && CompletedScans == 0 && saveFirstModeltoXmlFile) // saves only the first model file of iterating sequences.
                    {
                        /***/
                        // saves here the current Model as XML.gz file in parallel
                        dataSaverThread = new Thread(SaveAllData);
                        dataSaverThread.Name = "dataSaverThread";
                        dataSaverThread.Start(workLoopCopy);

                    }
                    else if (!saveFirstModeltoXmlFile)
                    {
                        /***/
                        // saves here the current Model as XML.gz file in parallel
                        dataSaverThread = new Thread(SaveAllData);
                        dataSaverThread.Name = "dataSaverThread";
                        dataSaverThread.Start(workLoopCopy);
                    }
                    else
                    { //No other option
                    }



                    // using a new thread set the value of 'ionizationPulses' taken from variableslist and start 'lecroyVBScript'
                    if (controlLecroy)
                    {
                        /***/
                        lecroyControllerThread = new Thread(SetLecroy);
                        lecroyControllerThread.Name = "lecroyControllerThread";
                        lecroyControllerThread.Start(workLoopCopy);
                    }

                    //
                    BeforeIteratingVariablesEventArgs eventArgs = new BeforeIteratingVariablesEventArgs();
                    DoBeforeIteratingVariables(eventArgs);

                    _waitForGeneratedData = false;
                    // if in iterating mode some actions have to take place
                    if (workLoopCopy.IsItererating && !pause)
                    {
                        /***/
                        // increase Globalcounter in case of 'scanning' or 'scanning once' and pause check-box hasn't been checked in UI
                        GlobalCounter++;
                        iteratedInLastCycle = true;

                        // check for overflow - start over again
                        if (IterationOfScan + 1 > NumberOfIterations)
                        {
                            // handle scan only once (no further use of iterators values and turns to normal running )
                            if (cycleState == CycleStates.ScanningOnce)
                            {
                                if (_cycleState != CycleStates.Stopped)
                                {
                                    OutputCycleState = CycleStates.Running;
                                }
                            }
                            else
                            {

                                CompletedScans++; // indicates the number of completed scans so far
                                IterationOfScan = 1; // reset the iteration value to 1 of the new scan

                                if (shuffleIterations)
                                {
                                    /***/
                                    IterateVariables(this, null);
                                }
                                else
                                {
                                    /***/
                                    ResetIteratorValues(this, null);
                                }

                                _waitForGeneratedData = true;
                            }
                            // handle stop after scan
                            if (stopAfterScan)
                            {
                                OutputCycleState = CycleStates.Stopped;
                            }
                        }
                        else
                        {
                            // no overflow so just go to next iteration
                            IterationOfScan++;
                            /***/
                            IterateVariables(this, null);


                            _waitForGeneratedData = true;
                        }
                        /***/
                        UpdateIteratorState();
                    }
                    else
                    {
                        if (alwaysIncrease && !pause)
                        {
                            /***/
                            GlobalCounter++;
                            iteratedInLastCycle = true;
                            UpdateIteratorState();
                        }
                    }

                    ModelIndex = eventArgs.NextModelIndex; // this is used to save the model number in the database.
                    if (eventArgs.ModelWillChange)//A new model will be loaded
                    {
                        _waitForGeneratedData = true;
                        DoAfterIteratingVariables();
                    }
                    else//we might have an iterated model
                    {
                        // if no real iterator is present don't wait - since you would have to wait forever
                        if (NumberOfIterations <= 1)
                            _waitForGeneratedData = false;
                    }

                    Console.WriteLine("End of current OutputLoop, NumberOfIterations ={0}", NumberOfIterations);




                }



                // run the garbage collector here since it is not in the time critical path
                //                GC.Collect();
                //                GC.WaitForPendingFinalizers();
                //watch1.Stop();
                //var elapsedMs1 = watch1.ElapsedMilliseconds;
                //Console.WriteLine("elapsed time:" + elapsedMs1.ToString());
            }

            // ReSharper disable once FunctionNeverReturns

        }

        /// <summary>
        /// Starts a separate process to control the Lecroy.
        /// </summary>
        /// <param name="rootModel">The root model.</param>
        /// <remarks>This method assumes the existence of a variable called "IonizationPulses" and will show an error in the error window if it is missing.</remarks>
        private void SetLecroy(object rootModel)
        {
            Thread.Sleep(10 * 1000);

            RootModel model = (RootModel)rootModel;
            DateTime estimatedStartTime = model.EstimatedStartTime;
            string relativePath = string.Format("{0:yyyy\\\\MM\\\\dd}", estimatedStartTime) + "\\";
            bool foundVariable = false;
            int ionizationPulses = -1;

            foreach (VariableModel variable in model.Data.variablesModel.VariablesList)
            {
                if (variable.VariableName.Trim() != "IonizationPulses")
                {
                    continue;
                }
                foundVariable = true;
                ionizationPulses = (int)Math.Round(variable.VariableValue);
            }
            if (!foundVariable)
            {
                var errorCollector = ErrorCollector.Instance;
                errorCollector.AddError("IonizationPuleses variable not found (outputHandler, Lecroy)!", ErrorCategory.Variables, false,
                    ErrorTypes.Other);
                return;
            }

            try
            {
                string controlLecroyScriptPath = ProfilesManager.GetInstance().ActiveProfile.GetSettingValueByName<string>(SettingNames.LECROY_VB_SCRIPT);
                Process.Start(controlLecroyScriptPath, (model.IsItererating ? "1" : "0") + " " + model.GlobalCounter + " " + ionizationPulses + " \"" + relativePath + "\"");
            }
            catch (Exception e)
            {
                var errorCollector = ErrorCollector.Instance;
                errorCollector.AddError(e.ToString(), ErrorCategory.Python, false, ErrorTypes.DynamicCompileError);
                // FIXME - display Error
            }

        }

        /// <summary>
        /// Polls every 10 ms whether the hardware system has received
        /// </summary>
        private void WaitForHardware()
        {
            while (!_hardwareManager.HasFinished())
            {
                Thread.Sleep(WAIT_FOR_HARDWARE_TO_RETURN_BUSY_WAITING_SLEEP_PERIOD);
            }
        }

        /// <summary>
        /// Clones a serializable object.
        /// </summary>
        /// <param name="original">The object to clone</param>
        /// <returns>A clone of the object.</returns>
        private Object DeepClone(Object original)
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter { Context = new StreamingContext(StreamingContextStates.Clone) };
            formatter.Serialize(stream, original);
            stream.Position = 0;
            return formatter.Deserialize(stream);
        }

        //  *********************************** set / get / update **********************************************************
        private void DoBeforeIteratingVariables(BeforeIteratingVariablesEventArgs e)
        {
            BeforeIteratingVariables?.Invoke(this, e);
        }

        private void DoAfterIteratingVariables()
        {
            AfterIteratingVariables?.Invoke(this, null);
        }

        /// <summary>
        /// Fires the OnScanChange event
        /// </summary>
        public void UpdateIteratorState()
        {
            OnScanChange?.Invoke(this, null);
        }


        /// <summary>
        /// Changes the OutputLoopState and fires the OnOuputLoopStateChange event
        /// </summary>
        /// <param name="outputLoopState">the new state of the OutputLoopState</param>
        private void SetOutputLoopThreadState(OutputLoopStates outputLoopState)
        {
            OutputLoopState = outputLoopState;
            UpdateOutputLoopState();
        }

        /// <summary>
        /// Fires the OnOutputLoopStateChange event
        /// </summary>
        private void UpdateOutputLoopState()
        {
            OnOuputLoopStateChange?.Invoke(this, null);
        }

        /// <summary>
        /// Called by the thread in the Buffer (DoubleBuffer) to inform the OutputHandler that the batch of raw output is prepared
        /// </summary>
        /// <param name="rootModel">a reference to the copy of the rootModel that was made by the Buffer and manipulated by it in order to generate the rawOutput</param>
        /// <param name="cycleDuration">the cycle duration (ms) calculated by the buffer</param>
        /// <param name="rawOutput">the output generated by the Buffer in case the HardwareManager it uses decided the rawOutput doesn't need to be converted</param>
        /// <param name="processedRawOutput">the output generated by the Buffer in case the HardwareManager it uses decided the rawOutput needs to be converted</param>
        internal void SetNewCycleData(RootModel rootModel, double cycleDuration,
            IModelOutput rawOutput, int timesToReplicateOutput)
        {
            bool updateGuiIteratorState = false;

            lock (outputLoopLock)
            {
                _rootModel = rootModel;
                _cycleDuration = cycleDuration * SECOND;
                _rawOutput = rawOutput;
                _timesToReplicateOutput = timesToReplicateOutput;


                _waitForGeneratedData = false;//this stops the busy waiting
                // increase the global counter (since the model has changed) if it has not been increased already
                if (!iteratedInLastCycle)
                {
                    GlobalCounter++;
                    iteratedInLastCycle = true;
                    updateGuiIteratorState = true;
                }
            }
            if (updateGuiIteratorState)
            {
                UpdateIteratorState();
            }
        }

        //  *********************************** run external python scripts **********************************************************

        //    "PythonScriptEverySingleIteration",
        //    "PythonScriptEverySingleIterationTimeCritical",
        //    "PythonScriptStartScan",
        //    "PythonScriptStartScanTimeCritical",

        /// <summary>
        /// Runs a Python script on a separate process
        /// </summary>
        /// <param name="scriptLocationSettingsEntry">the entry in the settings that contains the path to the Python script file</param>
        /// <param name="pythonProcess">the process that should run the script. It could be null</param>
        /// <returns>the process that will run the python script</returns>
        private Process RunNonTimeCriticalPythonScript(string scriptPath, Process pythonProcess)
        {
            string pythonEngine = ProfilesManager.GetInstance().ActiveProfile.GetSettingValueByName<string>(SettingNames.PYTHON_INTERPRETER);

            //if the Python engine is not specified, or if the script file itself is not specified
            if (pythonEngine.Trim().Equals("") || scriptPath.Trim().Equals(""))
            {
                return null;
            }

            //the process might already be running from a previous execution of the loop
            if (pythonProcess != null)
            {
                try
                {
                    pythonProcess.Kill();
                    //Process KILLED - error Message
                    var errorCollector = ErrorCollector.Instance;
                    errorCollector.AddError("Python (" + scriptPath + ") not finished!",
                        ErrorCategory.Python, true, ErrorTypes.DynamicCompileError);
                }
                catch (Exception)
                {
                    //Process already DEAD. This is the normal case
                }
            }

            //Start the process and run the Python engine and the specified Python script on it
            pythonProcess = new Process();
            pythonProcess.StartInfo.FileName = "\"" + pythonEngine + "\"";
            pythonProcess.StartInfo.Arguments = "\"" + scriptPath + "\"";
            pythonProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            pythonProcess.Start();

            //Return the started process
            return pythonProcess;
        }

        /// <summary>
        /// Runs a Python script on the current process
        /// </summary>
        /// <param name="scriptLocationSettingsEntry">the entry in the settings that contains the path to the Python script file</param>
        private void RunTimeCriticalPythonScript(string pyhtonScript)
        {
            try
            {
                if (!pyhtonScript.Trim().Equals(""))
                {
                    pyExecutor.Execute(pyhtonScript);
                }
            }
            catch (Exception e)
            {
                var errorCollector = ErrorCollector.Instance;
                errorCollector.AddError(e.ToString(), ErrorCategory.Python, false, ErrorTypes.DynamicCompileError);
                string error = pyExecutor.ExplainException(e);
                Console.Error.WriteLine(error);
            }
        }

        /// <summary>
        /// Runs the non-time-critical Python script which should run at the start of the scan in a separate process
        /// </summary>
        private void RunPhythonScriptStartScan()
        {
            //string scriptFilePath = ProfilesManager.GetInstance().ActiveProfile.GetSettingValueByName<string>(SettingNames.NON_TIME_CRITICAL_PYTHON_START);

            // string scriptFilePath = model.StartofScanNonTimeCriticalPythonFilePath;
            string scriptFilePath = _rootModel.StartofScanNonTimeCriticalPythonFilePath;
            pStartScan = RunNonTimeCriticalPythonScript(scriptFilePath, pStartScan);
        }

        /// <summary>
        /// Runs the time-critical Python script which should run at the start of the scan in the current process  
        /// </summary>
        private void RunPhythonScriptStartScanTimeCritical(RootModel model)
        {
            // string scriptFilePath = ProfilesManager.GetInstance().ActiveProfile.GetSettingValueByName<string>(SettingNames.TIME_CRITICAL_PYTHON_START);
            string scriptFilePath = model.StartofScanTimeCriticalPythonFilePath;
            RunTimeCriticalPythonScript(scriptFilePath);
        }

        //A single step in the variables

        /// <summary>
        /// Runs the non-time-critical Python script which should run every single iteration in a separate process
        /// </summary>
        private void RunPhythonScriptEverySingleIteration()
        {
            //string scriptFilePath = ProfilesManager.GetInstance().ActiveProfile.GetSettingValueByName<string>(SettingNames.NON_TIME_CRITICAL_PYTHON_CYCLE);
            // string scriptFilePath = model.EveryCycleNonTimeCriticalPythonPath;
            string scriptFilePath = _rootModel.EveryCycleNonTimeCriticalPythonPath;
            pEverySingleIteration = RunNonTimeCriticalPythonScript(scriptFilePath, pEverySingleIteration);
        }

        /// <summary>
        /// Runs the time-critical Python script which should run every single iteration in the current process
        /// </summary>
        private void RunPhythonScriptEverySingleIterationTimeCritical(RootModel model)
        {
            // string scriptFilePath = ProfilesManager.GetInstance().ActiveProfile.GetSettingValueByName<string>(SettingNames.TIME_CRITICAL_PYTHON_CYCLE);
            string scriptFilePath = model.EveryCycleTimeCriticalPythonFilePath;
            RunTimeCriticalPythonScript(scriptFilePath);
        }


        //  *********************************** Database Access **********************************************************
        private EntryPOCO GenerateDBEntryPOCO(RootModel rootModel)
        {
            return new EntryPOCO {
                CompletedScans = CompletedScans,
                CycleDuration = _cycleDuration,
                EstimatedStartTime = rootModel.EstimatedStartTime,
                GlobalCounter = rootModel.GlobalCounter,
                IsIterating = rootModel.IsItererating,
                IsMeasurementRoutineMode = IsMeasurementRoutineMode,
                IterationOfScan = IterationOfScan,
                ModelIndex = ModelIndex,
                NumberOfIterations = NumberOfIterations,
                StartCounterOfScansOfCurrentModel = StartCounterOfScansOfCurrentModel,
                StartGlobalCounterOfMeasurementRoutine = StartGlobalCounterOfMeasurementRoutine,
                VariableList = rootModel.Data.variablesModel.VariablesList 
            };
        }


        //  *********************************** save data to file **********************************************************
        /// <summary>
        /// Saves the variables to a text file and optionally the binary run itself - all this here is not time critical and runs in a seperate thread
        /// </summary>
        /// <param name="rootModel"></param>
        //Ebaa 8.05.2018
        private void SaveAllData(object rootModel)
        {
            ProfilesManager profilesManager = ProfilesManager.GetInstance();

            if (profilesManager.ActiveProfile.DoesSettingExist(SettingNames.SAVE_MODEL_TO_XML))
            {
                bool saveToXMLFile = profilesManager.ActiveProfile.GetSettingValueByName<bool>(SettingNames.SAVE_MODEL_TO_XML);

                if (saveToXMLFile)
                {
                    var model = (RootModel)rootModel;
                    DateTime estimatedStartTime = model.EstimatedStartTime;
                    string mainSaveFolder = profilesManager.ActiveProfile.GetSettingValueByName<string>(SettingNames.SAVE_MODEL_FOLDER_PATH);
                    String relativePath = String.Format("\\{0:yyyy\\\\MM\\\\dd}\\", estimatedStartTime);
                    String saveFolder = mainSaveFolder + relativePath;

                    // save variables
                    if (!Directory.Exists(saveFolder))
                    {
                        Directory.CreateDirectory(saveFolder);
                    }

                    bool saveToTxtFile = profilesManager.ActiveProfile.GetSettingValueByName<bool>(SettingNames.SAVE_VARIABLES_TO_TXT);

                    if (saveToTxtFile)
                    {
                        List<string> variablesList = new List<string>();

                        // create list 
                        foreach (VariableModel variable in model.Data.variablesModel.VariablesList)
                        {
                            if (variable.VariableName.Trim() != "")
                            {
                                variablesList.Add($"{variable.VariableName}\t{variable.VariableValue.ToString(CultureInfo.InvariantCulture)}");
                            }
                        }

                        string varOut = string.Join("\r\n", variablesList);
                        File.WriteAllText(saveFolder + model.GlobalCounter + "_variables.txt", varOut.Replace(',', '.'));
                    }


                    //  xml data
                    using (var writer = new FileStream(saveFolder + model.GlobalCounter + ".xml.gz", FileMode.Create))
                    {
                        using (var gz = new GZipStream(writer, CompressionMode.Compress, false))
                        {
                            Type type = model.GetType();
                            var serializer = new DataContractSerializer(type);
                            serializer.WriteObject(gz, model);
                        }
                    }

                }
            }
        }

        //ebaa 8.05.2018
        //private void SaveXmlFirstModel(object rootModel)
        //{
        //    ProfilesManager profilesManager = ProfilesManager.GetInstance();
        //    var model = (RootModel)rootModel;
        //    DateTime estimatedStartTime = model.EstimatedStartTime;
        //    string mainSaveFolder = profilesManager.ActiveProfile.GetSettingValueByName<string>(SettingNames.SAVE_MODEL_FOLDER_PATH);
        //    String relativePath = String.Format("\\{0:yyyy\\\\MM\\\\dd}\\", estimatedStartTime);
        //    String saveFolder = mainSaveFolder + relativePath;

        //    // save variables
        //    if (!Directory.Exists(saveFolder))
        //    {
        //        Directory.CreateDirectory(saveFolder);
        //    }

        //    //string varOut = String.Join("\r\n", model.VariablesList);
        //    using (var writer = new FileStream(saveFolder + model.GlobalCounter + ".xml.gz", FileMode.Create))
        //    {
        //        using (var gz = new GZipStream(writer, CompressionMode.Compress, false))
        //        {
        //            Type type = model.GetType();
        //            var serializer = new DataContractSerializer(type);
        //            serializer.WriteObject(gz, model);
        //        }
        //    }

        //}


        #endregion
    }
}