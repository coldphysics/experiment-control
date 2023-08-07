using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Buffer.Basic;
using Communication.Commands;
using Communication.Interfaces.Controller;
using Communication.Interfaces.Model;
using Communication.Interfaces.Windows;
using Controller.Control;
using Controller.Data.Windows;
using Controller.Root;
using Controller.Settings;
using Controller.Variables;
using Model.Data.Cards;
using Model.Data.Channels;
using Model.Data.SequenceGroups;
using Model.Data.Steps;
using Model.Root;
using Model.Data.Sequences;
using Model.Settings;
using Model.Settings.Settings;
using System.Threading;
using System.ComponentModel;
using Model.Builder;
using System.Windows.Media.Imaging;
using Gat.Controls;
using Controller.Control.StepBatchAddition;
using CustomElements.CheckableTreeView;
using Controller.Options;
using Model.Options;
using Controller.AdWin.Simulator;
using HardwareAdWin.Simulator;
using Controller.AdWin.Debug;
using Communication.Interfaces.Hardware;
using HardwareAdWin.Debug;
using System.Windows.Threading;
using Controller.MainWindow.MeasurementRoutine;
using Controller.OutputVisualizer;
using Model.MeasurementRoutine;
using Controller.Common;
using Controller.Control.Compare;
using System.Linq;
using Model.Utilities;
using Controller.Helper;
using Controller.Error;
using Errors.Error;

namespace Controller.MainWindow
{
    /// <summary>
    /// The controller for the main window
    /// </summary>
    /// <seealso cref="Prototyping.Controller.BaseController" />
    public class MainWindowController : BaseController
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowController"/> class.
        /// </summary>
        public MainWindowController(IModel model, DoubleBuffer buffer, OutputHandler outputHandler, IControllerRecipe controllerRecipe,
                                       IWindowGenerator windows, VariablesController variables)
        {
            _variables = variables;
            _errorsController = new ErrorsWindowController(this);
            _variables.RefreshWindows += ControlWindowController_RefreshWindows;
            _model = model;
            _buffer = buffer;
            _outputHandler = outputHandler;
            _controllerRecipe = controllerRecipe;
            _windowsGenerator = windows;
            InitializeCommands();

            ReadOptions();
            LoadSavedProperties();

            _controllerRecipe.Cook(_model, _variables);

            CreateWindows();

            WindowsListChanged += ControlWindowController_DoWindowsListChanged;
            TimesToReplicateOutput = 1;//this copies the output to the buffer.
            // GetRootController().CopyToBuffer();
            MeasurementRoutineController = new MeasurementRoutineManagerController(this, _model);
            IterationManagerController = new IterationManagerController(this);

            CurrentModeController = MeasurementRoutineController;
            _incrementIteratorsIsEnabled = true;
        }

        public MainWindowController()
        {

        }

        // ******************** Fields ******************************************
        #region Fields
        private readonly DoubleBuffer _buffer;
        private readonly OutputHandler _outputHandler;
        private readonly IControllerRecipe _controllerRecipe;
        private IModel _model;
        private readonly IWindowGenerator _windowsGenerator;
        /// <summary>
        /// Indicates whether the simulator window is open
        /// </summary>
        private bool isSimulatorWindowOpen = false;
        /// <summary>
        /// Indicates whether the fifo debug window is open
        /// </summary>
        private bool isFifoDebugWindowOpen;
        private bool _isAlwaysIncreaseEnabled = true;
        private bool _isPauseEnabled = true;
        #endregion

        // ********************* Properties ****************************************
        #region Properties
        public OutputHandler OutputHandler
        {
            get
            {
                return _outputHandler;
            }
        }

        private double DurationTotalInSeconds
        {
            get
            {
                double waitTime = (double)((DecimalSetting)ProfilesManager.GetInstance().ActiveProfile.GetSettingByName(SettingNames.CONSTANT_WAIT_TIME)).Value;

                return
                    Math.Round((_buffer.DurationSeconds + waitTime) * OutputHandler.NumberOfIterations);
            }
        }

        public bool IsStarted
        {
            set;
            get;
        }
        #endregion

        //********************* Child Windows (Views) ******************************
        #region Child Windows

        private static WindowList _windowList;

        /// <summary>
        /// Gets the windows list.
        /// </summary>
        /// <value>
        /// The windows list.
        /// </value>
        /// <remarks>This static property serves as a singleton for the windows list</remarks>
        public static ObservableCollection<ShowableWindow> WindowsList
        {
            get
            {
                ObservableCollection<ShowableWindow> windows = new ObservableCollection<ShowableWindow>();
                if (_windowList == null)
                {
                    return null;
                }
                windows.Clear();
                foreach (KeyValuePair<string, Window> window in _windowList.Windows())
                {
                    //System.Console.Write("N: {0}\n",window.Key );
                    ShowableWindow sWindow = new ShowableWindow();
                    sWindow.window = window.Value;
                    sWindow.Name = window.Key;
                    windows.Add(sWindow);
                }
                return windows;
            }
        }

        /// <summary>
        /// The simulator window
        /// </summary>
        private Window simulatorWindow;

        /// <summary>
        /// The fifo debug window
        /// </summary>
        private Window fifoDebugWindow;

        private Window variablesComparisonWindow;


        #endregion

        // ******************** Child Controllers (View Models) ********************
        #region Child Controllers
        /// <summary>
        /// The controller for the variables window
        /// </summary>
        private readonly VariablesController _variables;

        private readonly ErrorsWindowController _errorsController;

        /// <summary>
        /// Gets the step batch adder controller.
        /// </summary>
        /// <value>
        /// The step batch adder controller.
        /// </value>
        public StepBatchAdderController StepBatchAdderController
        {
            get
            {
                RootController root = GetRootController();
                CTVViewModel treeView = ModelBasedCTVBuilder.Build(root, false);//Load AnalogCards initially
                StepBatchAdderController stepBatchAdderController = new StepBatchAdderController(root, treeView);

                return stepBatchAdderController;
            }

        }


        /// <summary>
        /// Gets the profile manager controller.
        /// </summary>
        /// <value>
        /// The profile manager controller.
        /// </value>
        /// <remarks>The controller is built whenever this property accessed.</remarks>
        public ProfileManagerController ProfileManagerController
        {
            get
            {
                //The controller(s) are built each time, because we want them to have the latest snapshot of the profiles manager as the working copy.
                //if (profileManagerController == null)
                //{
                ControllersBuilder builder = new ControllersBuilder();
                builder.Build();
                ProfileManagerController profileManagerController = builder.GetResult();
                //}

                return profileManagerController;
            }

        }

        /// <summary>
        /// Gets the options manager controller.
        /// </summary>
        /// <value>
        /// The options manager controller.
        /// </value>
        /// <remarks>The controller is built whenever this property accessed.</remarks>
        public OptionsManagerController OptionsManagerController
        {
            get
            {
                OptionsManagerBuilder builder = new OptionsManagerBuilder();
                builder.Build();
                OptionsManagerController optionsManagerController = builder.GetResult();

                return optionsManagerController;
            }
        }

        /// <summary>
        /// Creates the controller for the adwin simulator window and returns it.
        /// </summary>
        /// <value>
        /// The simulator controller.
        /// </value>
        public SimulatorController SimulatorController
        {
            get
            {
                if (Global.GetHardwareType() == HW_TYPES.AdWin_Simulator)
                {
                    SimulatorController controller = new SimulatorController(DummyAdWinHW.GetInstance());

                    return controller;
                }
                else
                    return null;
            }
        }

        /// <summary>
        /// Creates the controller for the FIFO debug window and returns it.
        /// </summary>
        /// <value>
        /// The FIFO debug controller.
        /// </value>
        public FifoDebugController FifoDebugController
        {
            get
            {
                if (Global.GetHardwareType() == HW_TYPES.AdWin_Simulator ||
                    Global.GetHardwareType() == HW_TYPES.AdWin_T11 ||
                    Global.GetHardwareType() == HW_TYPES.AdWin_T12)
                {
                    List<IAdWinDebug> fifos = new List<IAdWinDebug>();
                    int fifosCount = Global.GetNumOfBuffers();

                    for (int i = 0; i < fifosCount; i++)
                        fifos.Add(new FifoStatus(i));

                    FifoDebugController controller = new FifoDebugController(fifos);

                    return controller;
                }
                else
                    return null;
            }
        }

        //public SimpleSequenceManagerController SimpleSequenceController { private set; get; }

        public MeasurementRoutineManagerController MeasurementRoutineController { private set; get; }

        public IterationManagerController IterationManagerController { private set; get; }

        public BaseController CurrentModeController { private set; get; }


        #endregion

        // ********************* UI-Bound Properties and Fields*********************

        #region UI Bound Properties

        #region Fields
        /*********** Fields ************************/

        /// <summary>
        /// The UI is blocked
        /// </summary>
        private bool isUIBlocked;

        /// <summary>
        /// The message that will be shown while the UI is blocked
        /// </summary>
        private string uiBlockedMessage;

        private Brush _generatorBrushColor = Brushes.GreenYellow;

        private Brush _outputCycleColor = Brushes.Gainsboro;

        private string _outputCycleState = "No Output";

        private string _generatorState = "Waiting for Changes";

        private bool _iterateAndSave = true;

        private bool _once = false;

        private bool isLoadingModel = false;

        private string _fileName;

        private DateTime _startTime = DateTime.Now;

        /// <summary>
        /// The path to the main window icon
        /// </summary>
        private string icon;
        #endregion

        #region Properties
        /********** Bound Properties ***************/

        /// <summary>
        /// Gets or sets the icon path.
        /// </summary>
        /// <value>
        /// The icon path.
        /// </value>
        public string Icon
        {
            set
            {
                this.icon = value;
                OnPropertyChanged("Icon");
            }

            get
            {
                return this.icon;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the UI is blocked.
        /// </summary>
        /// <value>
        /// <c>true</c> if the UI is blocked; otherwise, <c>false</c>.
        /// </value>
        public bool IsUIBlocked
        {
            get
            {
                return isUIBlocked;
            }

            set
            {
                this.isUIBlocked = value;
                OnPropertyChanged("IsUIBlocked");
            }
        }

        /// <summary>
        /// Gets or sets the UI blocked message.
        /// </summary>
        /// <value>
        /// The UI blocked message.
        /// </value>
        public string UIBlockedMessage
        {
            get
            {
                return uiBlockedMessage;
            }

            set
            {
                this.uiBlockedMessage = value;
                OnPropertyChanged("UIBlockedMessage");
            }
        }

        public string Duration
        {
            get
            {
                decimal waitTime = ((DecimalSetting)ProfilesManager.GetInstance().ActiveProfile.GetSettingByName(SettingNames.CONSTANT_WAIT_TIME)).Value;
                return "(" + waitTime + ") + " + Math.Round(_buffer.DurationSeconds);
            }
        }

        public string DurationTotal
        {
            get
            {
                //System.Console.WriteLine(Math.Floor((DurationTotalInSeconds / 3600)).ToString("00") + ":" + Math.Round(((DurationTotalInSeconds / 60) % 60)).ToString("00"));
                return Math.Floor((DurationTotalInSeconds / 3600)).ToString("00") + ":" + Math.Round(((DurationTotalInSeconds / 60) % 60)).ToString("00");
            }
        }

        public string FileName
        {
            get
            {
                if (String.IsNullOrEmpty(_fileName))
                {
                    return "(File: n/a)";
                }
                return "(File: " + _fileName + ")";
            }
            set
            {
                _fileName = value;
                OnPropertyChanged("FileName");
                MeasurementRoutineController.UpdateLoadedModel();

            }
        }

        public string StartTime
        {
            get { return _startTime.ToString("ddd, dd.MM., HH:mm:ss"); }
        }

        public string EndTime
        {
            get
            {
                //The old way does not work well with measurement routines!
                return _startTime.AddSeconds((CompletedScans + 1) * DurationTotalInSeconds).ToString("ddd, dd.MM., HH:mm:ss");
                //return DateTime.Now.AddSeconds(DurationTotalInSeconds).ToString("ddd, dd.MM., HH:mm:ss");
            }
        }

        public int TimesToReplicateOutput
        {
            set
            {
                GetRootController().TimesToReplicateOutput = value;
                //OnPropertyChanged("TimesToReplicateOutput");
                GetRootController().CopyToBuffer();
                GetRootController().EnableCopyToBufferAndCopyChanges();
            }

            get
            {
                return GetRootController().TimesToReplicateOutput;
            }
        }

        /// <summary>
        /// A non-static wrapper around <see cref=" WindowsList"/> that allows binding with (from the View menu)
        /// </summary>
        /// <value>
        /// The window list.
        /// </value>
        public ObservableCollection<ShowableWindow> WindowList
        {
            get
            {
                return MainWindowController.WindowsList;
            }
        }

        public bool StopAfterScan
        {
            get { return _outputHandler.stopAfterScan; }
            set { _outputHandler.stopAfterScan = value; }
        }

        public bool ControlLecroy
        {
            get { return _outputHandler.ControlLecroy; }
            set
            {
                _outputHandler.ControlLecroy = value;
                Model.Properties.Settings.Default.ControlLecroy = value;
                Model.Properties.Settings.Default.Save();
                OnPropertyChanged("ControlLecroy");
            }
        }

        /// <summary>
        /// Indicates whether the control lecroy button should be enabled
        /// </summary>
        public bool IsControlLecroyPossible
        {
            get
            {
                string lecroyScriptPath = ProfilesManager.GetInstance().ActiveProfile.GetSettingValueByName<string>(SettingNames.LECROY_VB_SCRIPT);

                if (!String.IsNullOrEmpty(lecroyScriptPath))
                    return true;
                else
                    return false;
            }
        }

        public bool IsPauseEnabled
        {
            get
            {
                return _isPauseEnabled && (AlwaysIncrease || IsIterateAndSaveChecked);
            }
            set
            {
                _isPauseEnabled = value;
                OnPropertyChanged("IsPauseEnabled");
            }
        }

        public bool ShuffleIterations
        {
            get { return _outputHandler.shuffleIterations; }
            set { _outputHandler.shuffleIterations = value; }
        }

        public bool Pause
        {
            get { return _outputHandler.pause; }
            set { _outputHandler.pause = value; }
        }

        public bool AlwaysIncrease
        {
            get { return _outputHandler.alwaysIncrease; }
            set
            {
                _outputHandler.alwaysIncrease = value;
                OnPropertyChanged("IsPauseEnabled"); 
            }
        }

        public bool IsAlwaysIncreaseEnabled
        {
            get { return _isAlwaysIncreaseEnabled && !IsIterateAndSaveChecked; }
            set
            {
                _isAlwaysIncreaseEnabled = value;
                OnPropertyChanged("IsAlwaysIncreaseEnabled");
            }
        }

        public bool IsOnceChecked
        {
            get { return _once; }

        }




        public bool IsIterateAndSaveChecked
        {
            get
            {
                return _iterateAndSave;
            }
            set
            {
                _iterateAndSave = value;
                OnPropertyChanged("IsIterateAndSaveChecked");
                OnPropertyChanged("IsPauseEnabled");
            }
        }

        public Brush GeneratorStateColor
        {
            get
            {
                return _generatorBrushColor;
            }
            private set
            {
                _generatorBrushColor = value;
                OnPropertyChanged("GeneratorStateColor");

            }
        }

        public Brush OutputCycleColor
        {
            get
            {
                return _outputCycleColor;
            }
            private set
            {
                _outputCycleColor = value;
                OnPropertyChanged("OutputCycleColor");
            }
        }

        public string OutputCycleState
        {
            get { return _outputCycleState; }
            set
            {
                _outputCycleState = value;
                OnPropertyChanged("OutputCycleState");
            }
        }

        public string GeneratorState
        {
            get { return _generatorState; }
            set
            {
                _generatorState = value;
                OnPropertyChanged("GeneratorState");
            }
        }

        public string CycleState
        {
            get
            {
                switch (OutputHandler.OutputCycleState)
                {
                    case Buffer.Basic.OutputHandler.CycleStates.Running:
                        return "Running / Not Iterating";
                    case Buffer.Basic.OutputHandler.CycleStates.Scanning:
                        return "Running / Iterating";
                    case Buffer.Basic.OutputHandler.CycleStates.ScanningOnce:
                        return "Running / Iterating Once";
                    case Buffer.Basic.OutputHandler.CycleStates.Stopped:
                        return "Stopped";

                    default:
                        return "Unknown State";
                }
            }
        }

        public int GlobalCounter
        {
            get { return _outputHandler.GlobalCounter; }
        }


        //Ebaa 11.06

        /// <summary>
        /// Gets the start counter of scans of current model.
        /// </summary>
        /// <value>
        /// The start counter of scans of current model.
        /// </value>
        public int StartCounterOfScansOfCurrentModel
        {
            get { return _outputHandler.StartCounterOfScansOfCurrentModel; }
        }

        public int LastStartCounterOfScans
        {
            get { return _outputHandler.LastStartCounterOfScans; }
        }

        public int NumberOfIterations
        {
            get { return _variables.NumberOfIterations; }
        }

        /// <summary>
        /// Gets the iteration of scan. This is model-specific.
        /// </summary>
        /// <value>
        /// The iteration of scan.
        /// </value>
        public int IterationOfScan
        {
            get { return _outputHandler.IterationOfScan; }
        }

        /// <summary>
        /// Gets the completed scans. This is model-specific.
        /// </summary>
        /// <value>
        /// The completed scans.
        /// </value>
        public int CompletedScans
        {
            get { return _outputHandler.CompletedScans; }
        }


        /// <summary>
        /// indicates whether the increment iterators checkbox is enabled.
        /// This value is set to false when we are working in the advanced mode.
        /// </summary>
        private bool _incrementIteratorsIsEnabled;
        public bool IncrementIteratorsIsEnabled
        {
            get
            {
                return _incrementIteratorsIsEnabled;
            }
            set
            {
                _incrementIteratorsIsEnabled = value;
                OnPropertyChanged("IncrementIteratorsIsEnabled");
            }
        }

        #endregion
        #endregion

        // ******************** Events-Related *************************************
        #region Commands

        private void InitializeCommands()
        {
            IterateAndSaveCommand = new RelayCommand(IterateAndSaveClick);
            OnlyOnceCommand = new RelayCommand(Once);
            //LoadCommand = new RelayCommand(LoadFile);
            SaveCommand = new RelayCommand(SaveFile, CanSaveActiveModel);
            ShowSwitchWindowCommand = new RelayCommand(ShowSwitchWindow);
            ClearAllDurationZeroStepsCommand = new RelayCommand(ClearAllDurationZeroSteps);
            //RefreshWindowsCommand = new RelayCommand(RefreshWindows);
            ShowFifoDebugWindowCommand = new RelayCommand(ShowFifoDebug,
                        (param) =>
                        {
                            return Global.GetHardwareType() == HW_TYPES.AdWin_Simulator
                                || Global.GetHardwareType() == HW_TYPES.AdWin_T11 || Global.GetHardwareType() == HW_TYPES.AdWin_T12;
                        });
            ShowSimulatorCommand = new RelayCommand(ShowSimulator, (param) => { return Global.GetHardwareType() == HW_TYPES.AdWin_Simulator; });

            //Ebaa 
            OpenVisualizeWindowCommand = new RelayCommand(OpenVisualizeWindow);
            ShowStepBatchAdderCommand = new RelayCommand(ShowStepBatchAdder);
            ShowAboutBoxCommand = new RelayCommand(ShowAboutBox);
            NewCommand = new RelayCommand(CreateNewModelWithPromot, CanCreateNewModel);
            ShowOptionsManagerCommand = new RelayCommand(param => ShowOptionsManager());
            ShowProfilesManagerCommand = new RelayCommand(param => ShowProfilesManager());
            WindowLoadedCommand = new RelayCommand(Window_Loaded);

        }

        //public ICommand SimpleSequenceSelectedCommand { get; private set; }
        public ICommand IterateAndSaveCommand { get; private set; }
        public ICommand OnlyOnceCommand { get; private set; }
        public ICommand LoadCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand ShowSwitchWindowCommand { get; private set; }
        public ICommand ClearAllDurationZeroStepsCommand { get; private set; }
        //public ICommand RefreshWindowsCommand { get; private set; }
        /// <summary>
        /// Gets the command that shows the adwin fifo debug window
        /// </summary>
        /// <value>
        /// The command that shows the adwin fifo debug window
        /// </value>
        public ICommand ShowFifoDebugWindowCommand { get; private set; }
        /// <summary>
        /// Gets the show simulator command.
        /// </summary>
        /// <value>
        /// The show simulator command.
        /// </value>
        public ICommand ShowSimulatorCommand { get; private set; }
        /// <summary>
        /// Gets or sets the show step batch adder command.
        /// </summary>
        /// <value>
        /// The show step batch adder command.
        /// </value>
        public ICommand ShowStepBatchAdderCommand { get; private set; }

        //look 
        /// <summary>
        /// Gets or sets the open visualizer window command
        /// </summary>
        /// <value>
        /// The open visualizer window command.
        /// </value>
        public ICommand OpenVisualizeWindowCommand { get; private set; }

        /// <summary>
        /// Gets or sets the show about box command.
        /// </summary>
        /// <value>
        /// The show about box command.
        /// </value>
        public ICommand ShowAboutBoxCommand { get; private set; }
        /// <summary>
        /// Gets the new model command.
        /// </summary>
        /// <value>
        /// The new model command.
        /// </value>
        public ICommand NewCommand { get; private set; }
        /// <summary>
        /// Gets the command to be executed to show the options manager
        /// </summary>
        /// <value>
        /// The command to be executed to show the options manager
        /// </value>
        public ICommand ShowOptionsManagerCommand { get; private set; }
        /// <summary>
        /// Gets the command to be executed to show the profiles manager
        /// </summary>
        /// <value>
        /// The command to be executed to show the profiles manager
        /// </value>
        public ICommand ShowProfilesManagerCommand { get; private set; }
        /// <summary>
        /// This command is triggered when the window finishes loading
        /// </summary>
        public ICommand WindowLoadedCommand { get; private set; }
        #endregion

        #region Events
        //TODO DANGER! static event
        /// <summary>
        /// Occurs when the list of windows is changed.
        /// </summary>
        public static event EventHandler WindowsListChanged;


        public delegate void GenerationFinishedCallback(FinishedModelGenerationEventArgs args);
        #endregion

        #region Event Handling
        public void OnCreatingWindow()
        {
            VisualizationWindowManager.Initialize(this);
            _buffer.FinishedModelGeneration += VisualizationWindowManager.GetInstance().HandleNewGeneratedOutputEvent;
        }

        private void ControlWindowController_RefreshWindows(object sender, EventArgs arg)
        {
            RefreshWindows(null);
        }

        private void ControlWindowController_DoWindowsListChanged(object sender, EventArgs e)
        {
            OnPropertyChanged("WindowsList");
            OnPropertyChanged("WindowList");
            _variables.UpdateWindowsList();//informs the variables window that the set of windows has changed
        }

        public void OnScanChange(object sender, EventArgs e)
        {
            OnPropertyChanged("globalCounter");

            //Ebaa 11.06
            // IterationManagerController.NotifyPropertyChanged("StartCounterOfScans");
            IterationManagerController.NotifyPropertyChanged("StartCounterOfScansOfCurrentModel");
            IterationManagerController.NotifyPropertyChanged("LastStartCounterOfScans");
            IterationManagerController.NotifyPropertyChanged("NumberOfIterations");
            OnPropertyChanged("DurationTotal");
            OnPropertyChanged("EndTime");
            IterationManagerController.NotifyPropertyChanged("CompletedScans");
            IterationManagerController.NotifyPropertyChanged("IterationOfScan");
            //_iterateAndSave = _outputHandler.OutputCycleState == OutputHandler.CycleStates.Scanning || _outputHandler.OutputCycleState == OutputHandler.CycleStates.ScanningOnce;
            //OnPropertyChanged("IsIterateAndSaveChecked");
            //SimpleSequenceController.NotifyPropertyChanged("CycleState");
            //SimpleSequenceController.NotifyPropertyChanged("CurrentStartStopButtonLabel");
            MeasurementRoutineController.NotifyPropertyChanged("CycleState");
            MeasurementRoutineController.NotifyPropertyChanged("CurrentStartStopButtonLabel");
        }

        public void OnGeneratorStateChange(object sender, EventArgs e)
        {
            switch (_buffer.CurrentGeneratorState)
            {
                case DoubleBuffer.GeneratorState.Waiting:
                    GeneratorState = "Waiting for Changes";
                    GeneratorStateColor = Brushes.GreenYellow;
                    break;
                case DoubleBuffer.GeneratorState.Generating:
                    GeneratorState = "Generating";
                    GeneratorStateColor = Brushes.Gold;
                    break;
                case DoubleBuffer.GeneratorState.GeneratingPendingChanges:
                    GeneratorState = "Pending Changes";
                    GeneratorStateColor = Brushes.Red;
                    break;
            }

            OnPropertyChanged("Duration");
            OnPropertyChanged("DurationTotal");
            OnPropertyChanged("EndTime");
        }

        public void OnOuputLoopStateChange(object sender, EventArgs e)
        {
            switch (_outputHandler.OutputLoopState)
            {
                case OutputHandler.OutputLoopStates.Sleeping:
                    OutputCycleState = "No Output";
                    OutputCycleColor = Brushes.Gainsboro;
                    break;
                case OutputHandler.OutputLoopStates.WaitForHardware:
                    OutputCycleState = "Waiting for Hardware Return";
                    OutputCycleColor = Brushes.GreenYellow;
                    break;
                case OutputHandler.OutputLoopStates.Preparing:
                    OutputCycleState = "Preparing Start";
                    OutputCycleColor = Brushes.Gold;
                    break;
                case OutputHandler.OutputLoopStates.WaitForIteration:
                    OutputCycleState = "Waiting for Data";
                    OutputCycleColor = Brushes.Red;
                    break;
                case OutputHandler.OutputLoopStates.WaitForStart:
                    OutputCycleState = "Scheduled Start";
                    OutputCycleColor = Brushes.GreenYellow;
                    break;
                case OutputHandler.OutputLoopStates.PostStart:
                    OutputCycleState = "After Start";
                    OutputCycleColor = Brushes.GreenYellow;
                    break;
            }
        }

        public void loader_ModelStructureMismatchDetected(object sender, ModelStructureMismatchEventArgs e)
        {
            string message = string.Format(
                "A structural mismatch has been detected between the model being loaded and the current profile:" +
                "\nCurrent Profile:\n{0} = {4}\n{1} = {5}\n{2} = {6}\n{3} = {7}\n\n" +
                "Model:\n{0} = {8}\n{1} = {9}\n{2} = {10}\n{3} = {11}\n\n" +
                "Do you want to load the model anyway?",
                "Number of Analog Cards", "Number of Channels per Analog Card", "Number of Digital Cards", "Number of Channels per Digital Card",
                Global.GetNumAnalogCards(), Global.GetNumAnalogChannelsPerCard(), Global.GetNumDigitalCards(), Global.GetNumDigitalChannelsPerCard(),
                e.ModelAnalogCards, e.ModelAnalogChannelsPerCard, e.ModelDigitalCards, e.ModelDigitalChannelsPerCard);

            MessageBoxResult result = MessageBox.Show(message, "Structural Mismatch", MessageBoxButton.YesNo, MessageBoxImage.Question);
            e.Cancel = (result == MessageBoxResult.No);
        }

        public void loader_ModelVersionMismatchDetected(object sender, ModelVersionMismatchEventArgs e)
        {
            // MessageBox.Show("You are using an old version of the model" + "\n" + "In this version python scripts are model-specific\n"  + "Note: In order to modify python scripts, load the model as a primary model", "Warning", MessageBoxButton.OK);
        }


        /// <summary>
        /// Shows the fifo debug window.
        /// </summary>
        /// <param name="paramenter">unused</param>
        private void ShowFifoDebug(object paramenter)
        {
            if (fifoDebugWindow == null || !isFifoDebugWindowOpen)
            {
                fifoDebugWindow = WindowsHelper.CreateWindowToHostViewModel(FifoDebugController, false);
                isFifoDebugWindowOpen = true;
                fifoDebugWindow.Closed += new EventHandler((sender, args) => isFifoDebugWindowOpen = false);
                fifoDebugWindow.MinHeight = 600;
                fifoDebugWindow.MinWidth = 300;
                fifoDebugWindow.Height = 900;
                fifoDebugWindow.Width = 500;
                fifoDebugWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                fifoDebugWindow.Title = "AdWin FIFOs Status";
            }

            fifoDebugWindow.Show();
            fifoDebugWindow.Focus();
        }

        /// <summary>
        /// Shows the AdWin Simulator window.
        /// </summary>
        /// <param name="parameter">not used</param>
        private void ShowSimulator(object parameter)
        {
            if (simulatorWindow == null || !isSimulatorWindowOpen)
            {
                simulatorWindow = WindowsHelper.CreateWindowToHostViewModel(SimulatorController, false);
                isSimulatorWindowOpen = true;
                simulatorWindow.Closed += new EventHandler((sender, args) => isSimulatorWindowOpen = false);
                simulatorWindow.MinHeight = 360;
                simulatorWindow.MinWidth = 500;
                simulatorWindow.Height = 800;
                simulatorWindow.Width = 725;
                simulatorWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                simulatorWindow.Title = "AdWin Hardware Simulator";
            }

            simulatorWindow.Show();
            simulatorWindow.Focus();
        }

        /// <summary>
        /// Shows the step batch adder window.
        /// </summary>
        /// <param name="parameter">Not used</param>
        private void ShowStepBatchAdder(object parameter)
        {

            Window managerWindow = WindowsHelper.CreateWindowToHostViewModel(StepBatchAdderController, false);

            managerWindow.MinHeight = 360;
            managerWindow.MinWidth = 500;
            managerWindow.Height = 400;
            managerWindow.Width = managerWindow.MinWidth;
            managerWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            managerWindow.Title = "Steps Batch-Adder";

            managerWindow.ShowDialog();
        }


        /// <summary>
        /// Shows the options manager.
        /// </summary>
        private void ShowOptionsManager()
        {
            OptionsManagerController controller = OptionsManagerController;
            Window managerWindow = WindowsHelper.CreateWindowToHostViewModel(controller, false);
            managerWindow.MinHeight = 350;
            managerWindow.MinWidth = 650;
            managerWindow.Height = 350;
            managerWindow.Width = managerWindow.MinWidth;
            managerWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            managerWindow.Title = "Options";

            managerWindow.ShowDialog();

            if (controller.Result == OptionsManagerController.OptionsManagerResult.SAVE_NO_RESTART)
            {
                OptionsManager manager = OptionsManager.GetInstance();
                //Immediately save the new settings
                manager.GetValuesFromCopy(controller.OptionsCopy);
                manager.SaveOptions();

                //Read Options
                ReadOptions();

                //Changes might need to be reflected on future models
                //GetRootController().CopyToBuffer();
            }
            else if (controller.Result == Options.OptionsManagerController.OptionsManagerResult.SAVE_RESTART)
            {
                if (OutputHandler.OutputLoopState == Buffer.Basic.OutputHandler.OutputLoopStates.Sleeping)
                {
                    OptionsManager.GetInstance().GetValuesFromCopy(controller.OptionsCopy);
                    OptionsManager.GetInstance().SaveOptions();
                    //We can restart immediately
                    RestartApplication();
                }
                else//Make sure to stop execution of cycles before stopping
                {
                    BackgroundWorker worker = new BackgroundWorker();

                    //this is where the long running process should go
                    worker.DoWork += (o, ea) =>
                    {
                        StopCycling();
                    };

                    worker.RunWorkerCompleted += (o, ea) =>
                    {
                        UnblockUI();
                        OptionsManager.GetInstance().GetValuesFromCopy(controller.OptionsCopy);
                        OptionsManager.GetInstance().SaveOptions();
                    //We can restart immediately
                    RestartApplication();

                    };
                    //set the IsBusy before you start the thread
                    BlockUI("Waiting for the current cycle to finish...");
                    worker.RunWorkerAsync();
                }

            }
        }

        /// <summary>
        /// Shows the profiles manager.
        /// </summary>
        private void ShowProfilesManager()
        {
            ProfileManagerController controller = ProfileManagerController;
            Window managerWindow = WindowsHelper.CreateWindowToHostViewModel(controller, false);
            managerWindow.MinHeight = 360;
            managerWindow.MinWidth = 780;
            managerWindow.Height = 625;
            managerWindow.Width = managerWindow.MinWidth;
            managerWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            managerWindow.Title = "Profiles Management";

            managerWindow.ShowDialog();

            if (controller.Result == ProfileManagerController.ProfileManagerResult.SAVE_NO_RESTART)
            {
                //Immediately save the new settings
                ProfilesManager.GetInstance().GetValuesFromSnapshot(controller.SettingsSnapshot);
                ProfilesManager.GetInstance().SaveAllProfiles();

                //Might Change
                OnPropertyChanged("Duration");
                OnPropertyChanged("DurationTotal");
                OnPropertyChanged("IsControlLecroyPossible");

                //Changes need to be reflected on future models
                GetRootController().CopyToBuffer();
            }
            else if (controller.Result == Settings.ProfileManagerController.ProfileManagerResult.SAVE_RESTART)
            {
                if (OutputHandler.OutputLoopState == Buffer.Basic.OutputHandler.OutputLoopStates.Sleeping)
                {
                    ProfilesManager.GetInstance().GetValuesFromSnapshot(controller.SettingsSnapshot);
                    ProfilesManager.GetInstance().SaveAllProfiles();
                    //We can restart immediately
                    RestartApplication();
                }
                else//Make sure to stop execution of cycles before stopping
                {
                    BackgroundWorker worker = new BackgroundWorker();

                    //this is where the long running process should go
                    worker.DoWork += (o, ea) =>
                    {
                        StopCycling();
                    };

                    worker.RunWorkerCompleted += (o, ea) =>
                    {
                        UnblockUI();
                        ProfilesManager.GetInstance().GetValuesFromSnapshot(controller.SettingsSnapshot);
                        ProfilesManager.GetInstance().SaveAllProfiles();
                    //We can restart immediately
                    RestartApplication();

                    };
                    //set the IsBusy before you start the thread
                    BlockUI("Waiting for the current cycle to finish...");
                    worker.RunWorkerAsync();
                }

            }

        }

        private void ShowSwitchWindow(object parameter)
        {
            var rawOutput = _outputHandler.Model;
            if (rawOutput != null)
            {
                Window switchWindowContainer = WindowsHelper.CreateWindowToHostViewModel(new SwitchWindowController(rawOutput), false);
                switchWindowContainer.Width = 525;
                switchWindowContainer.Height = 350;
                switchWindowContainer.Title = "Switches";
                switchWindowContainer.Show();
                //SwitchWindow.MainWindow window = new SwitchWindow.MainWindow(rawOutput);
                //window.Show();
            }
            else
            {
                MessageBox.Show("RawOutput is empty!", "Error!", MessageBoxButton.OK);
            }
        }

        private void ClearAllDurationZeroSteps(object parameter)
        {
            MessageBoxResult res = MessageBox.Show(
                "Do you really want to exterminate all steps that have a Duration of zero? This could fuck up you whole program! Be careful with doing this! You really want this?",
                "Delete all Steps with Duration 0", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.No)
            {
                return;
            }
            res = MessageBox.Show(
    "Ok but you could also just delete them by hand, do this now! Or do you still want to delete all of them automatically?",
    "Delete all Steps with Duration 0", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.No)
            {
                return;
            }

            foreach (CardBasicModel card in (_model as Model.Root.RootModel).Data.group.Cards)
            {
                foreach (SequenceModel sequence in card.Sequences)
                {
                    foreach (ChannelBasicModel channel in sequence.Channels)
                    {
                        bool restart = true;

                        while (restart)
                        {
                            restart = false;

                            foreach (StepBasicModel step in channel.Steps)
                            {
                                if (step.Duration.Value == 0)
                                {
                                    if (step.DurationVariableName == null || step.DurationVariableName == "")
                                    {
                                        System.Console.WriteLine("DUR0 at card " + card.Name + " and sequence " +
                                                                 sequence.Name + " channel " + channel.Setting.Name + " Dur: " + step.Duration.Value + " Var: " + step.DurationVariableName);
                                        channel.Steps.Remove(step);
                                        restart = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            _outputHandler.OutputCycleState = OutputHandler.CycleStates.Stopped;
            GetRootController().DisableCopyToBuffer(); // FIXME is there a better access to the root model or a better way to stop updates ?
            LoadModel((RootModel)_model, TimesToReplicateOutput);
        }

        private void RefreshWindows(object parameter)
        {
            CreateNewWindowsForNewModel(_model);
        }


        private void SaveFile(object parameter)
        {
            string result = FileHelper.SaveFile(_model);

            if (result != null)
            {

                FileName = result;
            }
        }

        private void IterateAndSaveClick(object parameter)
        {
            _iterateAndSave = !_iterateAndSave;
            OnPropertyChanged("IsIterateAndSaveChecked");
            OnPropertyChanged("IsPauseEnabled");
            OnPropertyChanged("IsAlwaysIncreaseEnabled");
            DetermineCycleState();
        }

        private void Once(object parameter)
        {
            _once = !_once;
            DetermineCycleState();
        }

        public void StartOutput(object parameter)
        {
            IsStarted = true;
            DetermineCycleState();
        }

        public void StopOutput(object parameter)
        {
            IsStarted = false;
            DetermineCycleState();
        }

        private void CreateNewModelWithPromot(object parameter)
        {
            if (MessageBox.Show("Are you sure you want to create a new blank model? All unsaved changes to the current model will be lost.", "New Model Creation Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                CreateNewModel();
            }
        }

        public void CreateNewModel()
        {
            ModelBuilder modelBuilder = new ModelBuilder();
            modelBuilder.Build();
            RootModel model = modelBuilder.GetRootModel();
            MeasurementRoutineController.SetPrimaryModel(model);
            CreateNewWindowsForNewModel(model);
            FileName = "";
        }

        private void ShowAboutBox(object parameter)
        {
            About about = new About();
            about.Title = "Experiment Control";
            about.Hyperlink = new Uri("https://github.com/coldphysics/experiment-control/releases");
            about.HyperlinkText = "Click here to check recent updates of the software!";
            about.Publisher = "Pi5 - University of Stuttgart";
            about.AdditionalNotes = "Development of this code was started by Stephan Jennewein to control the NI hardware of the SuperAtoms experiment, supervised by Michael Schlagmüller. Since then main contributors have been Udo Hermann, Majd Abdo, Ghareeb Falazi , Ebaa Alnazer (ghareeb.falazi@hotmail.com, ebaa.alnazer@hotmail.com).";
            about.Copyright = "© 2015-2020 PI5 - University of Stuttgart\n";
            about.Copyright += "Third party copyrights: ©1994-2018 Xceed Software Inc";

            // setting several properties here
            about.ApplicationLogo = new BitmapImage(new System.Uri("pack://application:,,,/View;component/Resources/cr.png"));
            // ...

            about.Show();
        }

        //Ebaa
        /// <summary>
        /// shows the visulization Window
        /// </summary>
        /// <param name="parameter">Not used</param>

        private void OpenVisualizeWindow(object parameter)
        {
            if (!DoubleBuffer.ModelIsWrong)
            {
                VisualizationWindowManager vwm = VisualizationWindowManager.GetInstance();
                vwm.OpenWindow();
            }
            else
            {
                MessageBoxResult errorBox = new MessageBoxResult();
                string error = "Due to errors in the current model, the ouput visualizer can not show the output";
                errorBox = MessageBox.Show(error);
            }

        }

        public void ShutdownApplication(object parameter)
        {
            CancelEventArgs e = (CancelEventArgs)parameter;

            if (OutputHandler.OutputLoopState != OutputHandler.OutputLoopStates.Sleeping)
            {
                MessageBoxResult result =
                    MessageBox.Show(
                        "The hardware Output is still in progress. Do you really want to Close this application?",
                        "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
                if (result == MessageBoxResult.Yes)
                {
                    HardwareAdWin.HardwareAdWin.ControlAdwinProcess.StopAdwin();
                    Application.Current.Shutdown();
                }
            }
            Application.Current.Shutdown();

        }

        private void Window_Loaded(object parameter)
        {
            // show info when in debugging mode
            if (Global.GetHardwareType() == HW_TYPES.AdWin_Simulator || Global.GetHardwareType() == HW_TYPES.NO_OUTPUT || !Global.CanAccessDatabase())
            {
                string output = "According to the selected profile:";

                if (Global.GetHardwareType() == HW_TYPES.AdWin_Simulator || Global.GetHardwareType() == HW_TYPES.NO_OUTPUT)
                    output += "\n * No hardware output will take place!";
                if (!Global.CanAccessDatabase())
                    output += "\n * No data will be written into the database!";

                MessageBox.Show(output, "Debug Mode", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        #endregion


        /******* Helper Methods *****/
        #region Helper Methods

        private void ReadOptions()
        {
            OptionsManager manager = OptionsManager.GetInstance();
            Icon = manager.GetOptionValueByName<string>(OptionNames.ICON_PATH);
            OnPropertyChanged("Icon");
            _variables.NotifyOptionsChanged();
        }

        private void RestartApplication()
        {
            System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
            Application.Current.Shutdown();
        }

        private void StopCycling()
        {
            OutputHandler.OutputCycleState = Buffer.Basic.OutputHandler.CycleStates.Stopped;

            while (OutputHandler.OutputLoopState != Buffer.Basic.OutputHandler.OutputLoopStates.Sleeping)
            {
                Thread.Sleep(500);
            }
        }

        public void BlockUI(string message)
        {
            UIBlockedMessage = message;
            IsUIBlocked = true;
        }

        public void UnblockUI()
        {

            IsUIBlocked = false;
        }

        private Dictionary<string, IWindowController> GenerateWindowControllerCollection(RootController controllerTree)
        {
            var output = new Dictionary<string, IWindowController>();

            foreach (WindowBasicController windowController in controllerTree.DataController.SequenceGroup.Windows)
            {
                output.Add(windowController.Name, windowController);
            }

            return output;
        }

        public RootController GetRootController()
        {
            return _variables.GetRootController();
        }



        public void LoadModel(RootModel model, int timesToReplicate, GenerationFinishedCallback callback = null, bool isSilent = false)
        {
            object token = ErrorCollector.Instance.StartBulkUpdate();
            _model = model;
            ModelSpecificCounters counters = MeasurementRoutineController.CurrentRoutineModel.RoutineModel.Counters;

            // Set StartCounterOfScansOfCurrentModel if it is not set before.
            if (!counters.GCIsSet)
            {
                counters.GCIsSet = true;
                counters.StartCounterOfScansOfCurrentModel = _outputHandler.GlobalCounter;
                _outputHandler.UpdateIteratorState();
            }
            else
            {
                Console.WriteLine("Global counter is already set, avoiding setting it again!");
            }

            GetRootController().SetModelCounters(counters);
            _variables.SetNewVariablesModel(model.Data.variablesModel);
            _variables.SetNewRootModel(model);
            _controllerRecipe.Cook(_model, _variables);

            //Trigger the callback if needed
            if (callback != null)
            {
                DoubleBuffer.FinishedModelGenerationEventHandler handler = null;

                //When the "FinishedModelGeneration" event occurs in the double buffer:
                handler =
                    new DoubleBuffer.FinishedModelGenerationEventHandler((sender, args) =>
                    {
                    //trigger the callback
                    callback(args);
                    //stop receiving new events
                    _buffer.FinishedModelGeneration -= handler;
                    });

                _buffer.FinishedModelGeneration += handler;
            }

            GetRootController().TimesToReplicateOutput = timesToReplicate;
            // Debug.Assert(GetRootController()._enableCopyToBuffer == false);
            GetRootController().CopyToBuffer(); // to ensure copying to the buffer for the first time after start (this sets pending changes to true for the first time).
            GetRootController().EnableCopyToBufferAndCopyChanges();
            ErrorCollector.Instance.EndBulkUpdate(token);

            if (!isSilent)
            {
                //The following block of code ensures that if a none-UI thread tries to create the windows no exception will occur
                Dispatcher dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
                if (dispatcher != null)
                {
                    // We know the thread have a dispatcher that we can use.
                    HandleWindowsAfterModelLoad();
                }
                else
                {
                    Application.Current.Dispatcher.InvokeAsync(new Action(() =>
                    {
                        HandleWindowsAfterModelLoad();
                    }));
                }
            }

        }

        private void HandleWindowsAfterModelLoad()
        {
            CloseWindows();
            CreateWindows();
        }

        /// <summary>
        /// Creates the new windows for a new model resulting from a click on "new"
        /// </summary>
        /// <param name="newModel">The new model.</param>
        public void CreateNewWindowsForNewModel(IModel newModel)
        {
            _outputHandler.OutputCycleState = OutputHandler.CycleStates.Stopped;
            GetRootController().DisableCopyToBuffer(); // FIXME is there a better access to the root model or a better way to stop updates ?
            LoadModel((RootModel)newModel, MeasurementRoutineController.CurrentRoutineModel.TimesToReplicate);
        }

        //This ensures the "Variables" window and the "Errors" window are not closed automatically
        private void CloseWindows()
        {
            foreach (KeyValuePair<string, Window> window in _windowList.Windows())
            {
                if (window.Key != "Variables" && window.Key != "Errors")
                {
                    window.Value.Closing -= PreventWindowFromClosing;
                    window.Value.Close();
                }
            }
        }

        private void PreventWindowFromClosing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            ((Window)sender).Hide();
        }

        private void CreateWindows()
        {
            RootController controller = GetRootController();
            Dictionary<string, IWindowController> windowControllers = GenerateWindowControllerCollection(controller);
            windowControllers.Add("Variables", _variables);//adding these two ensures the "generator" creates the corresponding windows if necessary
            windowControllers.Add("Errors", _errorsController);

            Dictionary<string, Window> newWindows = _windowsGenerator.Generate(windowControllers);

            // Prevent these windows from closing (hiding them instead). We do this here, ot in the Window generator so that we can
            // unsubscribe the event handler in the CloseWindows method later
            // The Variables and Errors windows are reused. Therefore, we unsubscribe the event handler before subscribing it (otherwise, 
            // the same event handler will be triggered multiple times for these two windows). Trying to unsubscribe a non-existing subscription
            // does not throw an exception.
            foreach (Window window in newWindows.Values)
            {
                window.Closing -= PreventWindowFromClosing;
                window.Closing += PreventWindowFromClosing;
            }


            _windowList = new WindowList(newWindows);


            if (OptionsManager.GetInstance().GetOptionValueByName<bool>(OptionNames.AUTOMATICALLY_OPEN_WINDOWS))
                _windowList.ShowAll();
            else
            {
                _windowList.ShowByName("Variables");
                _windowList.ShowByName("Errors");
            }

            WindowsListChanged?.Invoke(null, null);
        }


        /// <summary>
        /// Loads selected changes made by the user to the UI
        /// </summary>
        private void LoadSavedProperties()
        {
            ControlLecroy = Model.Properties.Settings.Default.ControlLecroy;
        }

        public void DetermineCycleState()
        {
            OutputHandler.CycleStates currentState = OutputHandler.OutputCycleState;

            if (!IsStarted)
                OutputHandler.OutputCycleState = Buffer.Basic.OutputHandler.CycleStates.Stopped;
            else
            {
                if (_iterateAndSave)
                {
                    if (_once)
                        OutputHandler.OutputCycleState = OutputHandler.CycleStates.ScanningOnce;
                    else
                        OutputHandler.OutputCycleState = OutputHandler.CycleStates.Scanning;

                    _startTime = DateTime.Now;

                    OnPropertyChanged("EndTime");
                    OnPropertyChanged("StartTime");
                }
                else
                    OutputHandler.OutputCycleState = OutputHandler.CycleStates.Running;
            }
        }

        /// <summary>
        /// Determines whether we are allowed to save the active model.
        /// </summary>
        /// <param name="pramaeter">The parameter.</param>
        /// <returns></returns>
        private bool CanSaveActiveModel(object pramaeter)
        {
            return !IsStarted || !MeasurementRoutineController.IsAdvancedMode;
        }

        private bool CanCreateNewModel(object parameter)
        {
            return !IsStarted;
        }

        /// <summary>
        /// Performs checks on the loaded model (variable changes and structure changes)
        /// </summary>
        /// <param name="newPrimaryModel">The new primary model.</param>
        public void PrimaryModelPostLoadChecks(RootModel newPrimaryModel)
        {
            if (_variables.Variables.Count != 0)
            {
                if (variablesComparisonWindow != null &&
                    Application.Current.Windows.Cast<Window>().Any(w => w.Name == variablesComparisonWindow.Name))
                {
                    variablesComparisonWindow.Close();
                    variablesComparisonWindow = null;
                }

                VariableComparisonController controller = new VariableComparisonController(_variables, newPrimaryModel.Data.variablesModel);
                variablesComparisonWindow = WindowsHelper.CreateWindowToHostViewModel(controller, true, false);
                variablesComparisonWindow.Name = "VariablesComparisonWindow";
                variablesComparisonWindow.Title = "Variables Comparison";
                variablesComparisonWindow.Show();
            }//end variable comparison

            CheckModelChanges(newPrimaryModel);

        }
        private void CheckModelChanges(RootModel newPrimaryModel)
        {
            StringBuilder LogString = ModelChangeChecker.DescribeModelChanges((RootModel)_model, newPrimaryModel);

            if (LogString.Length != 0)
            {
                LogString.Insert(0, "Detected changes:\n");
            }
            else
            {
                LogString.Append("No structural changes between the old and the new models detected!");
            }

            SimpleStringOkDialogController dialogController = new SimpleStringOkDialogController(LogString.ToString());
            Window change = WindowsHelper.CreateWindowToHostViewModel(dialogController, true, false);
            change.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            change.Title = "Detected Structural Changes";
            change.ShowDialog();
        }

        /// <summary>
        /// Announces that the usage or un-usage of a measurement routine changed.
        /// </summary>
        public void NotifyMeasurementRoutineModeChanged()
        {
            if (MeasurementRoutineController.IsAdvancedMode)
            {
                OutputHandler.IsMeasurementRoutineMode = true;
                OutputHandler.StartGlobalCounterOfMeasurementRoutine = GlobalCounter;
                OutputHandler.ModelIndex = MeasurementRoutineController.CurrentRoutineModelIndex;

                IsIterateAndSaveChecked = true;
                IncrementIteratorsIsEnabled = false;
                //Ebaa 11.06
                if (IsStarted)
                    ProfileManagerController.IsSaveButtonEnabled = false;

                IterationManagerController.IsPreviousStartGCOfScansVisible = Visibility.Hidden;
                IterationManagerController.NameOfTheCurrentStartGCOfScans = "Start counter of routine: ";
                IterationManagerController.IsScanOnlyOnceEnabled = false;
                IterationManagerController.IsStopAfterScanEnabled = false;
                IterationManagerController.IsShuffleIterationsEnabled = false;
                IsPauseEnabled = false;
                IsAlwaysIncreaseEnabled = false;


            }
            else
            {
                //Ebaa 12.06
                // OutputHandler.StartCounterOfScans = 0;
                OutputHandler.IsMeasurementRoutineMode = false;
                OutputHandler.ModelIndex = MeasurementRoutineController.CurrentRoutineModelIndex;
                OutputHandler.StartGlobalCounterOfMeasurementRoutine = 0;
                IncrementIteratorsIsEnabled = true;
                //Ebaa 12.06
                ProfileManagerController.IsSaveButtonEnabled = true;

                IterationManagerController.NameOfTheCurrentStartGCOfScans = "Current Start GC of Scans: ";
                IterationManagerController.IsPreviousStartGCOfScansVisible = Visibility.Visible;
                IterationManagerController.IsScanOnlyOnceEnabled = true;
                IterationManagerController.IsStopAfterScanEnabled = true;
                IterationManagerController.IsShuffleIterationsEnabled = true;
                IsPauseEnabled = true;
                IsAlwaysIncreaseEnabled = true;
            }
        }
        #endregion

    }


}
