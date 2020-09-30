using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Buffer.Basic;
using Communication.Commands;
using Communication.Interfaces.Model;
using Microsoft.Win32;
using Model.MeasurementRoutine;
using Model.Root;
using Model.Settings;
using System.Linq;
using System.Threading.Tasks;
using Controller.Helper;
using Errors.Error;

namespace Controller.MainWindow.MeasurementRoutine
{
    public class MeasurementRoutineManagerController : ChildController
    {
        private RoutineModelController selectedSecondaryModel;
        private MeasurementRoutineManager manager;
        private object bufferUpdatesLock = null;
        private string currentStartStopButtonLabel;
        private bool isAdvancedMode = false;
        private RoutineScripts scripts;

        public string CurrentStartStopButtonLabel
        {
            get { return currentStartStopButtonLabel; }
            set
            {
                currentStartStopButtonLabel = value;
                OnPropertyChanged("CurrentStartStopButtonLabel");
            }
        }
        private MainWindowController Parent
        {
            get
            {
                return (MainWindowController)parent;
            }
        }
        public string Script
        {
            set
            {
                scripts.RoutineScript = value;
            }
            get
            {
                return scripts.RoutineScript;
            }

        }
        public string InitializationScript
        {
            set
            {
                scripts.InitializationScript = value;
            }

            get
            {
                return scripts.InitializationScript;
            }
        }
        public string CycleState
        {
            get
            {
                return Parent.CycleState;
            }
        }
        public RoutineModelController PrimaryModel
        {
            set;
            get;
        }
        public RoutineModelController SelectedSecondaryModel
        {
            get { return selectedSecondaryModel; }
            set
            {
                selectedSecondaryModel = value;
                OnPropertyChanged("SelectedSecondaryModel");
            }
        }
        public ObservableCollection<RoutineModelController> SecondaryModels
        {
            set;
            get;
        }
        public RoutineModelController CurrentRoutineModel { set; get; }
        public int CurrentRoutineModelIndex
        {
            get
            {
                if (CurrentRoutineModel != null)
                {
                    if (CurrentRoutineModel == PrimaryModel)
                    {
                        return 0;
                    }
                    else
                    {
                        return SecondaryModels.IndexOf(CurrentRoutineModel) + 1;
                    }
                }

                return -1;
            }
        }
        public RoutineModelController NextRoutineModel { set; get; }
        public string LoadedModel
        {
            get
            {
                return Parent.FileName;
            }
        }
        public bool IsAdvancedMode
        {
            get
            {
                return this.isAdvancedMode;
            }

            set
            {
                this.isAdvancedMode = value;
                Parent.NotifyMeasurementRoutineModeChanged();
                OnPropertyChanged("IsAdvancedMode");
                OnPropertyChanged("AdvancedModeVisibility");
            }
        }
        public bool IsAdvancedModeAllowed
        {
            get
            {
                if (ProfilesManager.GetInstance().ActiveProfile.GetSettingValueByName<bool>(SettingNames.ALLOW_ACCESS_DATABASE))
                {
                    if (ProfilesManager.GetInstance().ActiveProfile.GetSettingValueByName<bool>(SettingNames.USE_LEGACY_DATABASE))
                        return false;
                }

                return true;
            }

        }
        public int PrimaryModelReplicationCount
        {
            set
            {
                PrimaryModel.TimesToReplicate = value;

                if (!IsAdvancedMode)//otherwise, this will happen automatically next time it is loaded
                    Parent.TimesToReplicateOutput = value;//this issues a copy to buffer
            }

            get
            {
                return PrimaryModel.TimesToReplicate;
            }
        }
        public Visibility AdvancedModeVisibility
        {
            get
            {
                if (IsAdvancedMode)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }


        public ICommand SetPrimaryModelCommand { private set; get; }
        public ICommand SetPythonScriptsCommand { private set; get; }
        public ICommand AddSecondaryModelCommand { private set; get; }
        public ICommand RemoveSecondaryModelCommand { private set; get; }
        public ICommand MoveSecondaryModelUpCommand { private set; get; }
        public ICommand MoveSecondaryModelDownCommand { private set; get; }
        public ICommand SetScriptCommand { private set; get; }
        public ICommand StartStopCommand { private set; get; }
        public ICommand SaveActiveModelCommand { get { return Parent.SaveCommand; } }
        public ICommand SaveMeasurementRoutineCommand { private set; get; }
        public ICommand LoadMeasurementRoutineCommand { private set; get; }
        public ICommand OpenVisualizeWindowCommand { get; private set; }

        public MeasurementRoutineManagerController(MainWindowController parent, IModel initialModel)
            : base(parent)
        {
            scripts = new RoutineScripts();
            CreateControllers((RootModel)initialModel);
            manager = new MeasurementRoutineManager();
            CurrentStartStopButtonLabel = "Start";
            InitializeCommands();
        }

        private void CreateControllers(RootModel initialModel)
        {
            PrimaryModel = new RoutineModelController(initialModel);
            CurrentRoutineModel = PrimaryModel;
            SecondaryModels = new ObservableCollection<RoutineModelController>();
        }

        private void InitializeCommands()
        {
            SetPrimaryModelCommand = new RelayCommand(LoadPrimaryModel, CanLoadPrimaryModel);
            SetPythonScriptsCommand = new RelayCommand(LoadPythonScripts, CanLoadPythonScripts);
            AddSecondaryModelCommand = new RelayCommand(LoadSecondaryModel, CanAddSecondaryModel);
            RemoveSecondaryModelCommand = new RelayCommand(RemoveSecondaryModel, CanRemoveSelectedSecondaryModel);
            MoveSecondaryModelDownCommand = new RelayCommand(MoveSecondaryModelDown, CanMoveSelectedSecondaryModelDown);
            MoveSecondaryModelUpCommand = new RelayCommand(MoveSecondaryModelUp, CanMoveSelectedSecondaryModelUp);
            SetScriptCommand = new RelayCommand(SetScript);
            StartStopCommand = new RelayCommand(StartStop, CanClickOnStartStop);
            SaveMeasurementRoutineCommand = new RelayCommand(SaveMesurementRoutine);
            LoadMeasurementRoutineCommand = new RelayCommand(LoadMeasurementRoutine, CanLoadMeasurementRoutine);
        }

        private bool IsModelAlreadyInUseAsPrimary(string fileName)
        {
            if (PrimaryModel != null && fileName.Equals(PrimaryModel.FilePath))
                return true;

            return false;
        }

        private bool IsModelAlreadyInUseAsSecondary(string fileName)
        {
            if (SecondaryModels != null)
                foreach (var model in SecondaryModels)
                    if (fileName.Equals(model.FilePath))
                        return true;

            return false;
        }

        private async void LoadPrimaryModel(object parameters)
        {
            var fileDialog = new OpenFileDialog { DefaultExt = ".xml.gz", Filter = "Sequence (.xml.gz)|*.xml.gz" };
            bool? result = fileDialog.ShowDialog();

            if (result == true)
            {
                string fileName = fileDialog.FileName;

                if (IsModelAlreadyInUseAsSecondary(fileName))
                {
                    MessageBox.Show("The model you are trying to load as a primary model is already loaded as a secondary model. You cannot load the same model more than once.", "Model already loaded", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    await LoadModelAsync(fileName, true);
                }

            }
        }

        private void LoadPythonScripts(object parameter)
        {
            PrimaryModel.LoadPythonScripts(null);
        }

        private async void LoadSecondaryModel(object parameters)
        {
            var fileDialog = new OpenFileDialog { DefaultExt = ".xml.gz", Filter = "Sequence (.xml.gz)|*.xml.gz" };
            bool? result = fileDialog.ShowDialog();

            if (result == true)
            {
                string fileName = fileDialog.FileName;

                if (IsModelAlreadyInUseAsSecondary(fileName))
                    MessageBox.Show("The model you are trying to load is already loaded. You cannot load the same model more than once.", "Model already loaded", MessageBoxButton.OK, MessageBoxImage.Error);
                else if (IsModelAlreadyInUseAsPrimary(fileName))
                {
                    MessageBoxResult r = MessageBox.Show("The model you are trying to load as a secondary is already loaded as a primary. If you click on Yes the primary model will be unloaded, and all unsaved changes to it will be lost.\nDo you want to continue?", "Model already loaded", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (r == MessageBoxResult.Yes)
                    {
                        Parent.CreateNewModel();
                        await LoadModelAsync(fileName, false);
                    }
                }
                else
                {
                    await LoadModelAsync(fileName, false);
                }
            }
        }

        public void SetPrimaryModel(RootModel newModel, string filePath = "", bool isSilent = false)
        {
            // creating a new routine based root model ensures resetting the counters object
            RoutineBasedRootModel rbrm = new RoutineBasedRootModel();
            rbrm.ActualModel = newModel;
            rbrm.FilePath = filePath;
            rbrm.TimesToReplicate = PrimaryModel.TimesToReplicate;
            PrimaryModel.RoutineModel = rbrm;

            if (CurrentRoutineModel == null)
                CurrentRoutineModel = PrimaryModel;

            if (!isSilent)
            {
                //Check changed structure or variables
                Parent.PrimaryModelPostLoadChecks(newModel);
            }

            //Make this the primary model
            Parent.LoadModel(CurrentRoutineModel.ActualModel, this.PrimaryModel.TimesToReplicate, null, isSilent);
            Parent.FileName = filePath;
        }

        public async Task<RootModel> LoadModelAsync(string filePath, bool isPrimaryModel, bool isSilent = false)
        {
            Parent.BlockUI("Loading Model...");

            try
            {
                RootModel model = await DoLoadModel(filePath, isSilent);
                Parent.UnblockUI();

                if (isPrimaryModel)
                {
                    SetPrimaryModel(model, filePath, isSilent);
                }
                else
                {
                    ErrorCollector.Instance.NotificationsEnabled = false;
                    bool verificationResult = model.Verify();
                    ErrorCollector.Instance.NotificationsEnabled = true;

                    //RECO: a hard check for errors might be necessary by creating a generator and trying to generate the sequence and verifying it afterwards.
                    if (verificationResult)//'Soft' check for errors
                    {
                        RoutineBasedRootModel toAdd = new RoutineBasedRootModel() { ActualModel = model, FilePath = filePath, TimesToReplicate = 1 };
                        RoutineModelController controllerToAdd = new RoutineModelController(toAdd);
                        this.SecondaryModels.Add(controllerToAdd);
                        //OnPropertyChanged("SecondaryModels");
                        OnPropertyChanged("CanRemoveSelectedSecondaryModel");
                        OnPropertyChanged("CanMoveSelectedSecondaryModelUp");
                        OnPropertyChanged("CanMoveSelectedSecondaryModelDown");
                    }
                    else
                    {
                        if (!isSilent)
                        {
                            MessageBox.Show("This secondary model has errors. Please load it as a primary model to be able to fix the errors, and then try to load it again as a secondary.", "Model Has Errors", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }

                }
                return model;

            }
            catch (Exception e)
            {
                Parent.UnblockUI();
                if (!isSilent)
                {
                    string reason = e.Message;
                    MessageBox.Show("Failed to load the specified model, a conversion error might exist. Reason: " + reason, "Model Has Errors", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                return null;
            }

        }

        private async Task<RootModel> DoLoadModel(string filePath, bool isSilent)
        {
            return await Task.Run(() =>
            {
                ModelLoader loader = new ModelLoader();

                if (!isSilent)
                {
                    loader.ModelStructureMismatchDetected += Parent.loader_ModelStructureMismatchDetected;
                    loader.ModelVersionMismatchDetected += Parent.loader_ModelVersionMismatchDetected;
                }

                return loader.LoadModel(filePath);
            });
        }

        public void ConnectControllerToModel(MeasurementRoutineModel model, bool isSilent)
        {
            PrimaryModel.RoutineModel = model.PrimaryModel;
            OnPropertyChanged("PrimaryModel");
            SecondaryModels.Clear();

            foreach (RoutineBasedRootModel sModel in model.SecondaryModels)
            {
                SecondaryModels.Add(new RoutineModelController(sModel));
            }

            CurrentRoutineModel = PrimaryModel;

            if (!isSilent)
            {
                //Check changed structure or variables
                Parent.PrimaryModelPostLoadChecks(CurrentRoutineModel.ActualModel);
            }

            //Make this the primary model
            Parent.LoadModel(CurrentRoutineModel.ActualModel, CurrentRoutineModel.TimesToReplicate, null, isSilent);
            Parent.FileName = CurrentRoutineModel.FilePath;
            scripts.InitializationScript = model.RoutineInitializationScript;
            scripts.RoutineScript = model.RoutineControlScript;

            OnPropertyChanged("CanRemoveSelectedSecondaryModel");
            OnPropertyChanged("CanMoveSelectedSecondaryModelUp");
            OnPropertyChanged("CanMoveSelectedSecondaryModelDown");
        }

        public async Task<MeasurementRoutineModel> LoadMeasurementRoutineAsync(string filePath, bool isSilent = false)
        {
            Parent.BlockUI("Loading Model...");

            MeasurementRoutineModel model = await Task.Run(() =>
            {
                MeasurementRoutineLoader loader = new MeasurementRoutineLoader();

                if (!isSilent)
                {
                    loader.ModelStructureMismatchDetected += Parent.loader_ModelStructureMismatchDetected;
                    loader.ModelVersionMismatchDetected += Parent.loader_ModelVersionMismatchDetected;
                    loader.ModelFileNotFound += loader_ModelFileNotFound;
                }

                return loader.LoadMeasurementRoutine(filePath);
            });

            Parent.UnblockUI();

            if (model != null)
            {
                ConnectControllerToModel(model, isSilent);
            }

            return model;

        }

        private void loader_ModelFileNotFound(object sender, ModelFileNotFoundEventArgs e)
        {
            e.Cancel = true;
            string message = string.Format("Cannot open the model: {0}. Please make sure the file exists, and that it is not being used elsewhere and then try again.", e.ModelFileName);
            MessageBox.Show(message, "Failed to Load Measurement Routine", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void RemoveSecondaryModel(object parameter)
        {
            RoutineBasedRootModel model = SelectedSecondaryModel.RoutineModel;
            SecondaryModels.Remove(SelectedSecondaryModel);
            //OnPropertyChanged("SecondaryModels");
            OnPropertyChanged("CanRemoveSelectedSecondaryModel");
            OnPropertyChanged("CanMoveSelectedSecondaryModelUp");
            OnPropertyChanged("CanMoveSelectedSecondaryModelDown");
        }

        private void SetScript(object parameter)
        {
            bool isReadOnly = Parent.IsStarted;
            MeasurementRoutineScriptController scriptController = new MeasurementRoutineScriptController(InitializationScript, Script, isReadOnly);
            Window scriptWindow = WindowsHelper.CreateWindowToHostViewModel(scriptController, false);
            scriptWindow.MinHeight = 400;
            scriptWindow.MinWidth = 400;
            scriptWindow.Height = 700;
            scriptWindow.Width = 1000;
            scriptWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            scriptWindow.Title = "Measurement Routine Script";

            scriptWindow.ShowDialog();

            if (scriptController.Result == MeasurementRoutineScriptController.SetScriptResult.SAVE)
            {
                InitializationScript = scriptController.InitializationScript;
                Script = scriptController.Script;
                //This helps in initializing global variables when necessary so that they can be referenced by scripts
                if (manager.RequiresInitialization())
                    manager.RunInitializationScript(scripts.InitializationScript);
            }
        }

        private void MoveSecondaryModel(RoutineModelController model, bool isUp)
        {
            int index = SecondaryModels.IndexOf(model);

            if (index >= 0)
            {
                if (isUp && index > 0)
                {
                    SecondaryModels.Move(index, index - 1);
                    OnPropertyChanged("CanMoveSelectedSecondaryModelUp");
                    OnPropertyChanged("CanMoveSelectedSecondaryModelDown");
                }
                else if (!isUp && index < SecondaryModels.Count - 1)
                {
                    SecondaryModels.Move(index, index + 1);
                    OnPropertyChanged("CanMoveSelectedSecondaryModelUp");
                    OnPropertyChanged("CanMoveSelectedSecondaryModelDown");
                }
            }
        }

        private void MoveSecondaryModelUp(object parameter)
        {
            MoveSecondaryModel(SelectedSecondaryModel, true);
        }

        private void MoveSecondaryModelDown(object parameter)
        {
            MoveSecondaryModel(SelectedSecondaryModel, false);
        }


        /// <summary>
        /// The action to be performed when clicking the StartStop button. 
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void StartStop(object parameter)
        {
            if (!Parent.IsStarted)//if it is stopped, we start it
            {
                // disable (ok) button in the profile manager window to prevent changing settings while running sequences.
                if (Parent.MeasurementRoutineController.isAdvancedMode)
                    Parent.ProfileManagerController.IsSaveButtonEnabled = false;
                NextRoutineModelInfo info = GetNextRoutineModel();
                CurrentRoutineModel = info.Model;
                Parent.OutputHandler.ModelIndex = info.ModelIndex;
                Parent.LoadModel(CurrentRoutineModel.ActualModel, CurrentRoutineModel.TimesToReplicate,

                //this callback is called once when the model gets generated
                (args) =>
                {
                    if (args.IsSuccessful)
                    {
                        Parent.FileName = CurrentRoutineModel.FilePath;
                        Parent.StartOutput(parameter);
                        CurrentStartStopButtonLabel = "Stop";
                    }
                });
            }
            else
            {
                Parent.ProfileManagerController.IsSaveButtonEnabled = true;// re-enable (ok) button in the profile manager window to allow altering settings.
                Parent.StopOutput(parameter);
                manager.Reset();

                //Reset all iterators and model-specific counters in all models.

                foreach (RoutineModelController rmc in SecondaryModels)
                {
                    rmc.RoutineModel.Reset();
                }

                PrimaryModel.RoutineModel.Reset();

                CurrentStartStopButtonLabel = "Start";
            }
        }

        private bool CanRemoveSelectedSecondaryModel(object paramenter)
        {
            return SelectedSecondaryModel != null && !Parent.IsStarted;
        }

        private bool CanMoveSelectedSecondaryModelUp(object paramenter)
        {

            if (SelectedSecondaryModel != null)
            {
                int index = SecondaryModels.IndexOf(SelectedSecondaryModel);

                if (index > 0)
                    return true;
            }

            return false;

        }
        private bool CanMoveSelectedSecondaryModelDown(object paramenter)
        {

            if (SelectedSecondaryModel != null)
            {
                int index = SecondaryModels.IndexOf(SelectedSecondaryModel);

                if (index < SecondaryModels.Count - 1)
                    return true;
            }

            return false;

        }
        private bool CanClickOnStartStop(object parameter)
        {
            if (Parent.IsStarted)
                return true;//You can always click on stop

            return (PrimaryModel != null && PrimaryModel.ActualModel != null);//You can click on start if there is a primarymodel loaded

        }
        private bool CanLoadPrimaryModel(object parameter)
        {
            return !IsAdvancedMode || !Parent.IsStarted;
        }
        private bool CanLoadPythonScripts(object parameter)
        {
            return !Parent.IsStarted;
        }
        private bool CanAddSecondaryModel(object parameter)
        {
            return !Parent.IsStarted;
        }
        private bool CanLoadMeasurementRoutine(object parameter)
        {
            return !Parent.IsStarted;
        }

        private void SaveMesurementRoutine(object parameter)
        {
            MeasurementRoutineModel routineData = ConstructMeasurementRoutineModel();
            FileHelper.SaveFile(routineData, ".routine.gz", "Measurement Routine (.routine.gz)|*.routine.gz");
        }
        public MeasurementRoutineModel ConstructMeasurementRoutineModel()
        {
            MeasurementRoutineModel routineData = new MeasurementRoutineModel();
            routineData.RoutineInitializationScript = scripts.InitializationScript;
            routineData.RoutineControlScript = scripts.RoutineScript;
            routineData.PrimaryModel = PrimaryModel.RoutineModel;

            foreach (RoutineModelController controller in SecondaryModels)
            {
                routineData.SecondaryModels.Add(controller.RoutineModel);
            }

            return routineData;
        }

        private async void LoadMeasurementRoutine(object parameter)
        {
            var fileDialog = new OpenFileDialog { DefaultExt = ".routine.gz", Filter = "Measurement Routine (.routine.gz)|*.routine.gz" };
            bool? result = fileDialog.ShowDialog();
            if (result == true)
            {
                string fileName = fileDialog.FileName;
                await LoadMeasurementRoutineAsync(fileName);
            }
        }


        /// <summary>
        /// Determines the next routine model.
        /// </summary>
        /// <returns>The controller of the next routine model to run.</returns>
        /// <remarks>If the mode is basic, the method does not run the routine script, but rather returns the primary model.</remarks>
        private NextRoutineModelInfo GetNextRoutineModel()
        {
            int newIndex = 0;

            if (IsAdvancedMode) //Only run routine script if in advanced mode
                newIndex = manager.GetNextModelIndex(scripts.InitializationScript, scripts.RoutineScript, PrimaryModel.RoutineModel,
                    SecondaryModels.Select(controller => controller.RoutineModel).ToList(), Parent);

            RoutineModelController newRoutineModel = null;

            if (newIndex == 0)
            {
                newRoutineModel = PrimaryModel;
            }
            else
            {
                newIndex--;

                if (newIndex >= 0 && newIndex < SecondaryModels.Count)
                {
                    newRoutineModel = SecondaryModels[newIndex];
                    newIndex++;
                }
                else
                {
                    //We have an error in the script!
                    string errorMessage = string.Format("The measurement routine script is trying to set the next model to the index {0} which does not exist! The primary model is loaded instead.", newIndex + 1);
                    ErrorCollector.Instance.AddError(errorMessage, ErrorCategory.MainHardware, true, ErrorTypes.DynamicCompileError);
                    newRoutineModel = PrimaryModel;
                }
            }

            return new NextRoutineModelInfo { Model = newRoutineModel, ModelIndex = newIndex };
        }

        /// <summary>
        /// Determines the next model.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="BeforeIteratingVariablesEventArgs"/> instance containing the event data.</param>
        public void DetermineNextModel(object sender, BeforeIteratingVariablesEventArgs args)
        {
            NextRoutineModelInfo newRoutine = GetNextRoutineModel();

            if (newRoutine.Model == null)
            {
                //fatal error no model
                System.Diagnostics.Debug.Fail("No model is found!");
            }

            args.NextModelIndex = newRoutine.ModelIndex;

            if (newRoutine.Model != CurrentRoutineModel)
            {
                bufferUpdatesLock = Parent.GetRootController().BulkUpdateStart(); // prevent multiple threads from acessing the buffer.
                NextRoutineModel = newRoutine.Model;
                args.ModelWillChange = true;
            }
            else
            {
                NextRoutineModel = null;
                args.ModelWillChange = false;
            }
        }

        public void ChangeCurrentModelIfNecessary(object sender, EventArgs args)
        {
            if (NextRoutineModel != null)
            {
                CurrentRoutineModel = NextRoutineModel;
                Parent.LoadModel(NextRoutineModel.ActualModel, NextRoutineModel.TimesToReplicate);
                Parent.FileName = NextRoutineModel.FilePath;
                Parent.GetRootController().BulkUpdateEnd(bufferUpdatesLock);
            }

        }

        public void UpdateLoadedModel()
        {
            OnPropertyChanged("LoadedModel");
        }

        private class RoutineScripts
        {
            public string InitializationScript { set; get; }
            public string RoutineScript { set; get; }
        }

        private class NextRoutineModelInfo
        {
            public RoutineModelController Model { set; get; }
            public int ModelIndex { set; get; }
        }
    }
}
