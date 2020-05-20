﻿using System;
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
using Controller.Variables.Compare;
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
        //look
        public static Window visualizationWindow;
        //look
        private static bool isVisualizationWindowOpen = false;



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
                    Math.Round((_buffer.Duration * TimesToReplicateOutput + waitTime) * OutputHandler.NumberOfIterations);
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



        //look
        public OutputVisualizationWindowController OutputVisulizationWindowController
        {
            get
            {
                RootController root = GetRootController();
                CTVViewModel treeView = ModelBasedCTVBuilder.BuildCheckableTree(root);
                OutputVisualizationWindowController outputVisualizationController = new OutputVisualizationWindowController(root, treeView);

                return outputVisualizationController;
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

        #endregion

        // ******************** Child Controllers (View Models) ********************
        #region Child Controllers
        /// <summary>
        /// The controller for the variables window
        /// </summary>
        private readonly VariablesController _variables;

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
                return "(" + waitTime + ") + " + Math.Round(_buffer.Duration * TimesToReplicateOutput).ToString();
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
            set { _outputHandler.alwaysIncrease = value; }
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
            get { return _variables.numberOfIterations; }
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
            //SimpleSequenceSelectedCommand = new RelayCommand(RunModeChangedHandler);

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

        //private void RunModeChangedHandler(object parameter)
        //{
        //    if (((string)parameter) == "Simple")
        //    {
        //        CurrentModeController = SimpleSequenceController;
        //    }
        //    else
        //    {
        //        CurrentModeController = MeasurementRoutineController;
        //    }

        //    OnPropertyChanged("CurrentModeController");
        //}

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

        //private void LoadFile(object parameter)
        //{
        //    var fileDialog = new OpenFileDialog { DefaultExt = ".xml.gz", Filter = "Sequence (.xml.gz)|*.xml.gz" };
        //    bool? result = fileDialog.ShowDialog();
        //    if (result == true)
        //    {
        //        string fileName = fileDialog.FileName;

        //        LoadFileByFileName(fileName, true);
        //    }
        //}

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

        public void OpenVisualizeWindow(object parameter)
        {
            if (!DoubleBuffer.ModelIsWrong)
            {
                VisualizationWindowManager vwm = VisualizationWindowManager.GetInstance(GetRootController());
                vwm.OpenWindow();
            }
            else
            {
                MessageBoxResult errorBox = new MessageBoxResult();
                String error = "Due to errors in the current model, the ouput visualizer can not show the output";
                errorBox = MessageBox.Show(error);
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

        private string StringOrDefault(string str, string defaultStr)
        {
            if (string.IsNullOrEmpty(str))
            {
                return defaultStr;
            }

            return str;
        }

        public void LoadModel(RootModel model, int timesToReplicate, GenerationFinishedCallback callback = null)
        {
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

        //A hack that ensures the "Variables" window and the "Errors" window are not closed automatically
        private void CloseWindows()
        {
            foreach (KeyValuePair<string, Window> window in _windowList.Windows())
            {
                if (window.Key != "Variables" && window.Key != "Errors")
                    window.Value.Close();
            }
        }

        //RECO this method references a class in the View project that maps views to controllers, a task that a datatemplate should be doing!
        private void CreateWindows()
        {
            RootController controller = GetRootController();
            Dictionary<string, IWindowController> windowControllers = GenerateWindowControllerCollection(controller);
            windowControllers.Add("Variables", _variables);//adding these two ensures the "generator" creates the corresponding windows if necessary
            windowControllers.Add("Errors", null);

            Dictionary<string, Window> newWindows = _windowsGenerator.Generate(windowControllers);

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
                VariableComparison.ShowNewWindow(_variables, newPrimaryModel.Data.variablesModel);
            }//end variable comparison

            CheckModelChanges(newPrimaryModel);

        }
        private void CheckModelChanges(RootModel newPrimaryModel)
        {
            if (_model == null)
                return;
            StringBuilder LogString = new StringBuilder("");
            //Step 1: Compare single Steps.


            SequenceGroupModel group = ((RootModel)_model).Data.group;
            SequenceGroupModel groupNew = newPrimaryModel.Data.group;

            if (group.Cards.Count != groupNew.Cards.Count)
            {
                //System.Console.WriteLine("Number of cards not equal! In group {0}", group.Name);
                LogString.Append("Number of cards not equal");
            }
            else
            {
                for (int j = 0; j < group.Cards.Count; j++)
                {
                    Model.Data.Cards.CardBasicModel card = group.Cards[j];
                    Model.Data.Cards.CardBasicModel cardNew = groupNew.Cards[j];
                    if (card.Name != cardNew.Name)
                    {
                        //System.Console.WriteLine("Card names not equal! old: {0} new: {1}", card.Name, cardNew.Name);
                        LogString.Append("Card names not equal \t" + card.Name +
                                         " --> " + cardNew.Name + "\n");
                    }
                    if (card.Sequences.Count != cardNew.Sequences.Count)
                    {
                        //System.Console.WriteLine("Number of sequences not equal! In card {0}", card.Name);
                        LogString.Append("Number of sequences not in card \"" +
                                         card.Name + "\"\n");
                    }
                    else
                    {
                        for (int k = 0; k < card.Sequences.Count; k++)
                        {
                            Model.Data.Sequences.SequenceModel sequence = card.Sequences[k];
                            Model.Data.Sequences.SequenceModel sequenceNew = cardNew.Sequences[k];
                            if (sequence.Name != sequenceNew.Name)
                            {
                                //System.Console.WriteLine("Sequence names not equal! old: {0} new: {1}", sequence.Name, sequenceNew.Name);
                                LogString.Append("Sequence names not equal in \"" + card.Name + "\"\t" +
                                                 sequence.Name + " --> " +
                                                 sequenceNew.Name + "\n");
                            }
                            if (sequence.Channels.Count != sequenceNew.Channels.Count)
                            {
                                //System.Console.WriteLine("Number of channels not equal! In sequence {0}", sequence.Name);
                                LogString.Append("Number of channels not equal in \"" + card.Name + "\", \"" + sequence.Name + "\"\n");
                            }
                            else
                            {
                                for (int l = 0; l < sequence.Channels.Count; l++)
                                {
                                    Model.Data.Channels.ChannelBasicModel channel = sequence.Channels[l];
                                    Model.Data.Channels.ChannelBasicModel channelNew =
                                        sequenceNew.Channels[l];
                                    if (k == 0)
                                    {
                                        if (channel.Setting.Name != channelNew.Setting.Name)
                                        {
                                            //System.Console.WriteLine("Channel names not equal! old: {0} new: {1}", channel.Setting.Name, channelNew.Setting.Name);
                                            LogString.Append("Channel names not equal in \"" + card.Name + "\"\t" + channel.Setting.Name +
                                                             " --> " + channelNew.Setting.Name + "\n");
                                        }



                                        if (channel.Setting.LowerLimit !=
                                            channelNew.Setting.LowerLimit)
                                        {
                                            LogString.Append("Channel lower limit not equal in \"" + card.Name + "\", \"" + channel.Setting.Name +
                                                             "\"\t" +
                                                             channel.Setting.LowerLimit +
                                                             " --> " +
                                                             channelNew.Setting.LowerLimit +
                                                             "\n");
                                        }
                                        if (channel.Setting.UpperLimit !=
                                            channelNew.Setting.UpperLimit)
                                        {
                                            LogString.Append("Channel lower limit not equal in \"" + card.Name + "\", \"" + channel.Setting.Name +
                                                             "\"\t" +
                                                             channel.Setting.UpperLimit +
                                                             " --> " +
                                                             channelNew.Setting.UpperLimit +
                                                             "\n");
                                        }
                                        if (channel.Setting.Invert !=
                                            channelNew.Setting.Invert)
                                        {
                                            LogString.Append("Channel inverting not equal in \"" + card.Name + "\", \"" + channel.Setting.Name +
                                                             "\"\t" +
                                                             channel.Setting.Invert +
                                                             " --> " +
                                                             channelNew.Setting.Invert +
                                                             "\n");
                                        }

                                        if (channel.Setting.UseCalibration != channelNew.Setting.UseCalibration)
                                        {
                                            LogString.Append(
                                                    "Channel settings usage of \"use calibration\" in \"" + card.Name + "\", \"" +
                                                    channel.Setting.Name + "\"\t" + channel.Setting.UseCalibration +
                                                    " --> " + channelNew.Setting.UseCalibration + "\n");
                                        }
                                        else
                                        {
                                            if (channel.Setting.UseCalibration)
                                            {

                                                if (channel.Setting.InputUnit != channelNew.Setting.InputUnit)
                                                {
                                                    LogString.Append("Channel input unit not equal in \"" + card.Name + "\", \"" +
                                                        channel.Setting.Name + "\"\t" +
                                                        channel.Setting.InputUnit +
                                                        " --> " +
                                                        channelNew.Setting.InputUnit +
                                                        "\n");
                                                }
                                            }
                                        }

                                        //Compare the scripts even if the calibration is disabled! (Ask Felix)
                                        if (channel.Setting.CalibrationScript != channelNew.Setting.CalibrationScript)
                                        {
                                            LogString.Append("Channel calibration Script not equal in \"" + card.Name + "\", \"" +
                                                channel.Setting.Name + "\"\t" +
                                                channel.Setting.CalibrationScript +
                                                " --> " +
                                                channelNew.Setting.CalibrationScript +
                                                "\n");
                                        }



                                    }
                                    if (channel.Steps.Count != channelNew.Steps.Count)
                                    {
                                        //System.Console.WriteLine("Number of steps not equal! In channel {0}", channel.Setting.Name);
                                        LogString.Append("Number of steps not equal in \"" + card.Name + "\",  \"" + sequence.Name +
                                                         "\", \"" + channel.Setting.Name + "\"\n");
                                    }
                                    else
                                    {
                                        for (int m = 0; m < channel.Steps.Count; m++)
                                        {
                                            //System.Console.WriteLine("m: {0}", m);
                                            Model.Data.Steps.StepBasicModel step = channel.Steps[m];
                                            Model.Data.Steps.StepBasicModel stepNew = channelNew.Steps[m];
                                            if (step.GetType() != stepNew.GetType())
                                            {
                                                //System.Console.WriteLine("Step type changed in channel {2} at step {3}! old: {0} new: {1}",step.GetType(), stepNew.GetType(),channel.Setting.Name, m);
                                                LogString.Append("Step types not equal in \"" + card.Name + "\",  \"" +
                                                                 sequence.Name + "\", \"" +
                                                                 channel.Setting.Name + "\", step \"" +
                                                                 (m + 1) + "\"\t" + step.GetType() + " --> " +
                                                                 stepNew.GetType() + "\n");
                                            }
                                            else
                                            {
                                                //System.Console.WriteLine("Else!");
                                                if (step.GetType() ==
                                                    typeof(Model.Data.Steps.StepFileModel))
                                                {
                                                    if (((StepFileModel)step).FileName !=
                                                        ((StepFileModel)step).FileName)
                                                    {
                                                        //System.Console.WriteLine("Step filename changed in channel {2} at step {3}! old: {0} new: {1}",((StepFileModel)step).FileName, ((StepFileModel)step).FileName,channel.Setting.Name, m);
                                                        LogString.Append("Step filenames not equal in \"" + card.Name +
                                                                         "\",  \"" + sequence.Name +
                                                                         "\", \"" + channel.Setting.Name +
                                                                         "\", step \"" + (m + 1) + "\"\t" +
                                                                         ((StepFileModel)step).FileName +
                                                                         " --> " +
                                                                         ((StepFileModel)stepNew).FileName +
                                                                         "\n");
                                                    }
                                                }
                                                if (step.GetType() == typeof(StepRampModel))
                                                {
                                                    //System.Console.WriteLine("Ramp {0} {1}\n{2} - {3}!", step.Duration.Value, stepNew.Duration.Value, step.DurationVariableName, stepNew.DurationVariableName);
                                                    if (step.DurationVariableName !=
                                                        stepNew.DurationVariableName)
                                                    {
                                                        //System.Console.WriteLine("Duration variable changed in channel {2} at step {3}! old: {0} new: {1}",step.DurationVariableName,stepNew.DurationVariableName,channel.Setting.Name, m);
                                                        LogString.Append(
                                                            "Duration Variables not equal in \"" + card.Name + "\",  \"" +
                                                            sequence.Name + "\", \"" + channel.Setting.Name +
                                                            "\", step \"" + (m + 1) + "\"\t" +
                                                            StringOrDefault(step.DurationVariableName,
                                                                "user input") + " --> " +
                                                            StringOrDefault(stepNew.DurationVariableName,
                                                                "user input") + "\n");
                                                    }
                                                    if (step.DurationVariableName == "" ||
                                                        step.DurationVariableName == null)
                                                    {
                                                        if (step.Duration.Value != stepNew.Duration.Value)
                                                        {
                                                            // System.Console.WriteLine("Duration changed in channel {2} at step {3}! old: {0} new: {1}",step.Duration.Value,stepNew.Duration.Value,channel.Setting.Name, m);
                                                            LogString.Append(
                                                                "Duration values not equal in \"" +
                                                                card.Name +
                                                                "\",  \"" + sequence.Name + "\", \"" +
                                                                channel.Setting.Name + "\", step \"" +
                                                                (m + 1) + "\"\t" + step.Duration.Value +
                                                                " --> " + stepNew.Duration.Value + "\n");
                                                        }
                                                    }
                                                    if (step.ValueVariableName != stepNew.ValueVariableName)
                                                    {
                                                        //System.Console.WriteLine("Value variable changed in channel {2} at step {3}! old: {0} new: {1}",step.ValueVariableName,stepNew.ValueVariableName,channel.Setting.Name, m);
                                                        LogString.Append("Value Variables not equal in \"" + card.Name +
                                                                         "\",  \"" + sequence.Name +
                                                                         "\", \"" + channel.Setting.Name +
                                                                         "\", step \"" + (m + 1) + "\"\t" +
                                                                         StringOrDefault(
                                                                             step.ValueVariableName,
                                                                             "user input") + " --> " +
                                                                         StringOrDefault(
                                                                             stepNew.ValueVariableName,
                                                                             "user input") + "\n");
                                                    }
                                                    if (step.ValueVariableName == "" ||
                                                        step.ValueVariableName == null)
                                                    {
                                                        if (step.Value.Value != stepNew.Value.Value)
                                                        {
                                                            // System.Console.WriteLine("Value changed in channel {2} at step {3}! old: {0} new: {1}",step.Value.Value,stepNew.Value.Value,channel.Setting.Name, m);
                                                            LogString.Append(
                                                                "Value values not equal in \"" + card.Name + "\",  \"" +
                                                                sequence.Name + "\", \"" +
                                                                channel.Setting.Name + "\", step \"" +
                                                                (m + 1) + "\"\t" + step.Value.Value +
                                                                " --> " + stepNew.Value.Value + "\n");
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (LogString.Length != 0)
            {
                LogString.Insert(0, "Detected changes:\n");
                SimpleStringOkWindow.ShowNewSimpleStringOkWindow("Detected changes", LogString.ToString());
            }
            else
            {
                LogString.Append("No changed detected!");
            }

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
                IterationManagerController.IsPauseEnabled = false;
                IterationManagerController.IsAlwaysIncreaseEnabled = false;


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
                IterationManagerController.IsPauseEnabled = true;
                IterationManagerController.IsAlwaysIncreaseEnabled = true;
            }
        }
        #endregion

    }


}
