using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using Buffer.Basic;
using Communication.Commands;
using Communication.Interfaces.Controller;
using Communication.Interfaces.Model;
using Communication.Interfaces.Windows;
using Controller.Data.WindowGroups;
using Controller.Data.Windows;
using Controller.Root;
using Controller.Variables;
using Microsoft.Win32;
using Model.Data.Cards;
using Model.Data.Channels;
using Model.Data.SequenceGroups;
using Model.Data.Sequences;
using Model.Data.Steps;
using Model.Root;
using Controller.Variables.Compare;

namespace Controller.Control
{
    public class ControlWindowController : INotifyPropertyChanged
    {
        // ******************** variables ********************
        private readonly DoubleBuffer _buffer;
        private readonly OutputHandler _outputHandler;

        private readonly IControllerRecipe _controllerRecipe;
        private IModel _model;
        private static WindowList _windowList;
        private readonly IWindowGenerator _windows;
        private bool _iterateAndSave;
        private bool _once = false;
        private readonly VariablesController _variables;

        private readonly IWindowController _variablesController;

        // ******************** properties ********************
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
        public OutputHandler OutputHandler
        {
            get
            {
                return _outputHandler;
            }
        }

        public string Duration
        {
            get { return "(" + Math.Round(OutputHandler._settings.getDoubleValue("constantTimeWaitOffset")) + ") + " + Math.Round(_buffer.Duration).ToString(); }
        }

        public string DurationTotal
        {
            get
            {
                //System.Console.WriteLine(Math.Floor((DurationTotalInSeconds / 3600)).ToString("00") + ":" + Math.Round(((DurationTotalInSeconds / 60) % 60)).ToString("00"));
                return Math.Floor((DurationTotalInSeconds / 3600)).ToString("00") + ":" + Math.Round(((DurationTotalInSeconds / 60) % 60)).ToString("00");
            }
        }

        public double DurationTotalInSeconds
        {
            get
            {
                return
                    Math.Round((_buffer.Duration + OutputHandler._settings.getDoubleValue("constantTimeWaitOffset")
                    /*Zwischenwartezeit*/) * OutputHandler.NumberOfIterations);
            }
        }

        public string FileName
        {
            get { return "file name: " + _FileName; }
            set
            {
                _FileName = value;
                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("FileName"));
                }
            }
        }

        private string _FileName;




        private DateTime _startTime = DateTime.Now;

        public string StartTime
        {
            get { return _startTime.ToString("ddd, dd.MM., HH:mm:ss"); }
        }
        public string EndTime
        {
            get { return _startTime.AddSeconds((CompletedScans + 1) * DurationTotalInSeconds).ToString("ddd, dd.MM., HH:mm:ss"); }
        }

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


        //This part of the code is used to exclude settings for projects which do not need them.
        public ObservableCollection<Setting> SettingsList
        {
            get
            {
                ObservableCollection<Setting> retList = new ObservableCollection<Setting>();
                foreach (Setting setting in _settingsList)
                {
                    if (setting.Name.Equals("RqoAdWinPath") && Global.Experiment != Global.ExperimentTypes.AdWin5thFloor)
                    {
                        continue;
                    }
                    if (setting.Name.Equals("lecroyVBScript") && Global.Experiment != Global.ExperimentTypes.Superatom)
                    {
                        continue;
                    }

                    retList.Add(setting);
                }
                return retList;
            }
        }
        private ObservableCollection<Setting> _settingsList = new ObservableCollection<Setting>();

        private readonly LocalSettings _settings = new LocalSettings();
        public bool StopAfterScan
        {
            get { return _outputHandler.StopAfterScan; }
            set { _outputHandler.StopAfterScan = value; }
        }

        public bool ControlLecroy
        {
            get { return _outputHandler.ControlLecroy; }
            set { _outputHandler.ControlLecroy = value; }
        }

        public bool shuffleIterations
        {
            get { return _outputHandler.shuffleIterations; }
            set { _outputHandler.shuffleIterations = value; }
        }

        public bool pause
        {
            get { return _outputHandler.pause; }
            set { _outputHandler.pause = value; }
        }

        public bool alwaysIncrease
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
            get { return _iterateAndSave; }
        }

        private Brush _generatorBrushColor = Brushes.GreenYellow;
        public Brush GeneratorStateColor
        {
            get
            {
                return _generatorBrushColor;
            }
            private set
            {
                _generatorBrushColor = value;

                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("GeneratorStateColor"));
            }
        }

        private Brush _outputCycleColor = Brushes.Gainsboro;
        public Brush OutputCycleColor
        {
            get
            {
                return _outputCycleColor;
            }
            private set
            {
                _outputCycleColor = value;
                PropertyChanged(this, new PropertyChangedEventArgs("OutputCycleColor"));
            }
        }

        public string OutputCycleState
        {
            get { return _outputCycleState; }
            set
            {
                _outputCycleState = value;
                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("OutputCycleState"));
                }
            }
        }
        private string _outputCycleState = "no output";

        public string GeneratorState
        {
            get { return _generatorState; }
            set
            {
                _generatorState = value;
                if (null != PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("GeneratorState"));
                }
            }
        }
        private string _generatorState = "waiting for changes";

        public int GlobalCounter
        {
            get { return _outputHandler.GlobalCounter; }
        }

        public int StartCounterOfScans
        {
            get { return _outputHandler.StartCounterOfScans; }
        }

        public String LastStartCounterOfScans
        {
            get { return "( last " + _outputHandler.LastStartCounterOfScans + " )"; }
        }

        public int NumberOfIterations
        {
            get { return _variables.numberOfIterations; }
        }

        public int IterationOfScan
        {
            get { return _outputHandler.IterationOfScan; }
        }

        public int CompletedScans
        {
            get { return _outputHandler.CompletedScans; }
        }

        private Dictionary<string, IWindowController> GetWindowController(RootController controllerTree)
        {
            var output = new Dictionary<string, IWindowController>();
            foreach (WindowGroupController windowGroupController in controllerTree.DataController.SequenceGroups)
            {
                foreach (WindowBasicController windowController in windowGroupController.Windows)
                {
                    output.Add(windowController.Name, windowController);
                }
            }
            return output;
        }

        // ******************** events ********************
        public ICommand StartCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand IterateAndSaveCommand { get; private set; }
        public ICommand OnlyOnceCommand { get; private set; }
        public ICommand LoadCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand SaveSettingsCommand { get; private set; }

        public ICommand ShowSwitchWindowCommand { get; private set; }

        public ICommand ClearAllDurationZeroSteps { get; private set; }

        public ICommand RefreshWindows { get; private set; }

        public static event WindowsListChanged DoWindowsListChanged;
        public delegate void WindowsListChanged(object sender, EventArgs e);

        private void ControlWindowController_RefreshWindows(object sender, EventArgs arg)
        {
            DoRefreshWindows(null);
        }


        // ******************** constructor ********************
        public ControlWindowController(IModel model, DoubleBuffer buffer, OutputHandler outputHandler, IControllerRecipe controllerRecipe,
                                       IWindowGenerator windows, VariablesController variables)
        {
            _variablesController = variables;
            (_variablesController as VariablesController).RefreshWindows += ControlWindowController_RefreshWindows;
            _variables = variables;
            _model = model;
            _buffer = buffer;
            _outputHandler = outputHandler;
            _controllerRecipe = controllerRecipe;
            _windows = windows;
            StartCommand = new RelayCommand(StartOutput);
            StopCommand = new RelayCommand(StopOutput);
            IterateAndSaveCommand = new RelayCommand(IterateAndSaveClick);
            OnlyOnceCommand = new RelayCommand(Once);
            LoadCommand = new RelayCommand(LoadFile);
            SaveCommand = new RelayCommand(SaveFile);
            RefreshCommand = new RelayCommand(RefreshSettings);
            SaveSettingsCommand = new RelayCommand(DoSaveSettings);
            ShowSwitchWindowCommand = new RelayCommand(DoShowSwitchWindow);
            ClearAllDurationZeroSteps = new RelayCommand(DoClearAllDurationZeroSteps);
            RefreshWindows = new RelayCommand(DoRefreshWindows);

            //SelectWindowCommand = new RelayCommand(DoSelectWindow);
            //StartRun = new RelayCommand(StartRunOutput);

            CreateWindows();
            LoadSettings();

            DoWindowsListChanged += ControlWindowController_DoWindowsListChanged;
        }

        void ControlWindowController_DoWindowsListChanged(object sender, EventArgs e)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("WindowsList"));
            }
        }

        /*public void DoSelectWindow(object parameter)
        {
            Console.WriteLine("DoSelectWindow!");
        }*/

        public void DoSaveSettings(object parameter)
        {
            _settings.saveSettings();
        }



        public void DoShowSwitchWindow(object parameter)
        {
            var rawOutput = _outputHandler.getModel();
            if (rawOutput != null)
            {
                Window switchWindowContainer = WindowsHelper.CreateWindowHostingUserControl(new SwitchWindowController(rawOutput), false);
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

        public void DoClearAllDurationZeroSteps(object parameter)
        {
            MessageBoxResult res = MessageBox.Show(
                "Do you really want to exterminate all steps that have a duration of zero? This could fuck up you whole program! Be careful with doing this! You really want this?",
                "Delete all Steps with duration 0", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.No)
            {
                return;
            }
            res = MessageBox.Show(
    "Ok but you could also just delete them by hand, do this now! Or do you still want to delete all of them automatically?",
    "Delete all Steps with duration 0", MessageBoxButton.YesNo);
            if (res == MessageBoxResult.No)
            {
                return;
            }
            foreach (KeyValuePair<string, SequenceGroupModel> group in (_model as Model.Root.RootModel).Data.Groups)
            {
                foreach (CardBasicModel card in group.Value.Cards)
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
            }
            _outputHandler.OutputCycleState = OutputHandler.CycleStates.Stopped;
            ((VariablesController)_variablesController).GetRootController().DisableCopyToBuffer(); // FIXME is there a better access to the root model or a better way to stop updates ?
            _windowList.CloseAll();
            CreateWindows();
            _buffer.CopyData(_model);
        }

        public void LoadSettings()
        {
            _settings.reloadSettings();
            _settingsList = new ObservableCollection<Setting>(_settings.Settings);
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("SettingsList"));
            }
        }

        public void RefreshSettings(object parameter)
        {
            LoadSettings();
        }

        public void OnGeneratorStateChange(object sender, EventArgs e)
        {
            switch (_buffer.CurrentGeneratorState)
            {
                case DoubleBuffer.GeneratorState.Waiting:
                    GeneratorState = "waiting for changes";
                    GeneratorStateColor = Brushes.GreenYellow;
                    break;
                case DoubleBuffer.GeneratorState.Generating:
                    GeneratorState = "generating";
                    GeneratorStateColor = Brushes.Gold;
                    break;
                case DoubleBuffer.GeneratorState.GeneratingPendingChanges:
                    GeneratorState = "pending changes";
                    GeneratorStateColor = Brushes.Red;
                    break;
            }
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("Duration"));
                PropertyChanged(this, new PropertyChangedEventArgs("DurationTotal"));
                PropertyChanged(this, new PropertyChangedEventArgs("EndTime"));
            }
        }

        public void OnScanChange(object sender, EventArgs e)
        {
            PropertyChanged(this, new PropertyChangedEventArgs("globalCounter"));
            PropertyChanged(this, new PropertyChangedEventArgs("StartCounterOfScans"));
            PropertyChanged(this, new PropertyChangedEventArgs("LastStartCounterOfScans"));
            PropertyChanged(this, new PropertyChangedEventArgs("NumberOfIterations"));
            PropertyChanged(this, new PropertyChangedEventArgs("DurationTotal"));
            PropertyChanged(this, new PropertyChangedEventArgs("EndTime"));
            PropertyChanged(this, new PropertyChangedEventArgs("CompletedScans"));
            PropertyChanged(this, new PropertyChangedEventArgs("IterationOfScan"));
            _iterateAndSave = _outputHandler.OutputCycleState == OutputHandler.CycleStates.Scanning || _outputHandler.OutputCycleState == OutputHandler.CycleStates.ScanningOnce;
            PropertyChanged(this, new PropertyChangedEventArgs("IsIterateAndSaveChecked"));


        }

        public void OnOuputLoopStateChange(object sender, EventArgs e)
        {
            switch (_outputHandler.OutputLoopState)
            {
                case OutputHandler.OutputLoopStates.Sleeping:
                    OutputCycleState = "no output";
                    OutputCycleColor = Brushes.Gainsboro;
                    break;
                case OutputHandler.OutputLoopStates.WaitForHardware:
                    OutputCycleState = "waiting for hardware return";
                    OutputCycleColor = Brushes.GreenYellow;
                    break;
                case OutputHandler.OutputLoopStates.Preparing:
                    OutputCycleState = "preparing start";
                    OutputCycleColor = Brushes.Gold;
                    break;
                case OutputHandler.OutputLoopStates.WaitForIteration:
                    OutputCycleState = "waiting for data";
                    OutputCycleColor = Brushes.Red;
                    break;
                case OutputHandler.OutputLoopStates.WaitForStart:
                    OutputCycleState = "scheduled start";
                    OutputCycleColor = Brushes.GreenYellow;
                    break;
                case OutputHandler.OutputLoopStates.PostStart:
                    OutputCycleState = "after start";
                    OutputCycleColor = Brushes.GreenYellow;
                    break;
            }
        }

        public RootController GetRootController()
        {
            return ((VariablesController)_variablesController).GetRootController();
        }

        private string stringOrDefault(string str, string defaultStr)
        {
            if (str == null || str == "")
            {
                return defaultStr;
            }
            return str;
        }
        private void SaveDummy()
        {
            Model.V1.Root.RootModel result = new Model.V1.Root.RootModel(Global.Experiment);
            result.Data.Groups.Add("test", null);
            var writer = new FileStream("d:\\tessst.xml", FileMode.Create);
            Type type = result.GetType();
            var serializer = new DataContractSerializer(type);
            serializer.WriteObject(writer, result);
            writer.Close();
        }
        #region testing
        private RootModel ConvertIfNecessary(string fileName)
        {
            //SaveDummy();
            _outputHandler.OutputCycleState = OutputHandler.CycleStates.Stopped;
            ((VariablesController)_variablesController).GetRootController().DisableCopyToBuffer(); // FIXME is there a better access to the root model or a better way to stop updates ?
            _windowList.CloseAll();
            var stream = new FileStream(fileName, FileMode.Open);
            var gz = new GZipStream(stream, CompressionMode.Decompress, false);

            StreamReader stringReader = new StreamReader(gz);
            string xml = stringReader.ReadToEnd();

            int index = xml.IndexOf("MODEL_VERSION");
            RootModel result = null;
            XmlDictionaryReader reader = null;

            //It is version 1!
            if (index < 0)
            {
                xml = xml.Replace("http://schemas.datacontract.org/2004/07/Model", "http://schemas.datacontract.org/2004/07/Model.V1");

                //RECO find more dynamic way of figuring this out!
                xml = xml.Replace("KeyValueOfstringSequenceGroupModel3tbQ1peO", "KeyValueOfstringSequenceGroupModelEC1jtckR");
                reader = XmlDictionaryReader.CreateTextReader(GenerateStreamFromString(xml), new XmlDictionaryReaderQuotas());

                var deserializer = new DataContractSerializer(typeof(Model.V1.Root.RootModel));
                Model.V1.Root.RootModel model = (Model.V1.Root.RootModel)deserializer.ReadObject(reader, true);
                Model.V1.ModelConverter converter = new Model.V1.ModelConverter();
                result = converter.ConvertToCurrentVersion(model);
            }
            else
            {
                reader = XmlDictionaryReader.CreateTextReader(GenerateStreamFromString(xml), new XmlDictionaryReaderQuotas());
                var deserializer = new DataContractSerializer(typeof(RootModel));
                result = (RootModel)deserializer.ReadObject(reader, true);
            }

            reader.Close();
            stream.Close();

            return result;
        }

        private static MemoryStream GenerateStreamFromString(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        }
        #endregion
        public void LoadFileByFileName(string fileName, bool checkChanges)
        {

            _outputHandler.OutputCycleState = OutputHandler.CycleStates.Stopped;
            ((VariablesController)_variablesController).GetRootController().DisableCopyToBuffer(); // FIXME is there a better access to the root model or a better way to stop updates ?
            _windowList.CloseAll();
            RootModel model = ConvertIfNecessary(fileName);


            if (_variables.Variables.Count != 0)
            {
                var output = new StringBuilder();
                var newVariablesList = model.Data.variablesModel.VariablesList;

                var newVariableNames = new List<string>();
                var oldVariableNames = new List<string>();

                int cntSame = 0;
                foreach (var newVariable in newVariablesList)
                {
                    newVariableNames.Add(newVariable.VariableName);
                }
                foreach (var oldVariable in _variables.Variables)
                {
                    oldVariableNames.Add(oldVariable.VariableName);
                }
                var notInNew = oldVariableNames.Except(newVariableNames);
                output.Append("not in new: " + notInNew.Count() + "\n");
                foreach (var missingNew in notInNew)
                {
                    var missing = _variables.GetByName(missingNew);
                    output.Append(missing.VariableName + "\t" + missing.VariableValue + "\t" +
                                      missing.VariableCode + "\n");
                }
                var notInOld = newVariableNames.Except(oldVariableNames);
                output.Append("not in old: " + notInNew.Count() + "\n");
                foreach (var missingOld in notInOld)
                {
                    var missing = model.Data.variablesModel.GetByName(missingOld);
                    output.Append(missing.VariableName + "\t" + missing.VariableValue + "\t" +
                                      missing.VariableCode + "\n");
                }
                var inBoth = oldVariableNames.Except(notInNew);


                foreach (var newVariable in newVariablesList)
                {
                    bool same = true;
                    if (!inBoth.Contains(newVariable.VariableName))
                    {
                        continue;
                    }

                    var oldVariable = _variables.GetByName(newVariable.VariableName);
                    if (newVariable.VariableCode != oldVariable.VariableCode)
                    {
                        same = false;
                        output.Append(newVariable.VariableName + "\told: " + oldVariable.VariableCode +
                                          "\tnew: " + newVariable.VariableCode + "\n");
                    }
                    if (newVariable.VariableValue != oldVariable.VariableValue)
                    {
                        same = false;
                        output.Append(newVariable.VariableName + "\told: " + oldVariable.VariableValue +
                                          "\tnew: " + newVariable.VariableValue + "\n");
                    }

                    if (same)
                    {
                        cntSame++;
                    }
                }
                output.Append(cntSame + " variables are the same." + "\n");

                /*new Thread(new ThreadStart(delegate
                {
                    MessageBox.Show(output.ToString(), "Variable comparison - " + DateTime.Now.ToString());
                })).Start();*/

                VariableComparison.ShowNewWindow(_variables, model.Data.variablesModel);
            }//end variable comparison




            //new code started 02.12.14
            //now start the comparison of the values of everything. Wiki:
            //Variables, Steps, Channel Names, Limits
            if (checkChanges)
            {

                StringBuilder LogString = new StringBuilder("");
                //Step 1: Compare single Steps.
                if (((RootModel)_model).Data.Groups.Count != ((RootModel)model).Data.Groups.Count)
                {
                    //Number of Groups not equal
                    //System.Console.WriteLine("Number of groups not equal!");
                    LogString.Append("Number of groups not equal\n");

                }
                else
                {
                    for (int i = 0; i < ((RootModel)_model).Data.Groups.Count; i++)
                    {
                        Model.Data.SequenceGroups.SequenceGroupModel group =
                            ((RootModel)_model).Data.Groups[((RootModel)_model).Data.Groups.Keys.ToList()[i]];
                        Model.Data.SequenceGroups.SequenceGroupModel groupNew =
                            ((RootModel)model).Data.Groups[((RootModel)model).Data.Groups.Keys.ToList()[i]];
                        if (group.Name !=
                            groupNew.Name)
                        {
                            //System.Console.WriteLine("Group names not equal! old: {0} new: {1}", group.Name, groupNew.Name);
                            LogString.Append("Group names not equal\t" + group.Name + " --> " + groupNew.Name + "\n");
                        }
                        if (group.Cards.Count != groupNew.Cards.Count)
                        {
                            //System.Console.WriteLine("Number of cards not equal! In group {0}", group.Name);
                            LogString.Append("Number of cards not equal in \"" + group.Name + "\"\n");
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
                                    LogString.Append("Card names not equal in \"" + group.Name + "\"\t" + card.Name +
                                                     " --> " + cardNew.Name + "\n");
                                }
                                if (card.Sequences.Count != cardNew.Sequences.Count)
                                {
                                    //System.Console.WriteLine("Number of sequences not equal! In card {0}", card.Name);
                                    LogString.Append("Number of sequences not equal in \"" + group.Name + "\", \"" +
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
                                            LogString.Append("Sequence names not equal in \"" + group.Name + "\", \"" +
                                                             card.Name + "\"\t" + sequence.Name + " --> " +
                                                             sequenceNew.Name + "\n");
                                        }
                                        if (sequence.Channels.Count != sequenceNew.Channels.Count)
                                        {
                                            //System.Console.WriteLine("Number of channels not equal! In sequence {0}", sequence.Name);
                                            LogString.Append("Number of channels not equal in \"" + group.Name +
                                                             "\", \"" + card.Name + "\", \"" + sequence.Name + "\"\n");
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
                                                        LogString.Append("Channel names not equal in \"" + group.Name +
                                                                         "\", \"" + card.Name + "\", \"" +
                                                                         sequence.Name + "\"\t" + channel.Setting.Name +
                                                                         " --> " + channelNew.Setting.Name + "\n");
                                                    }



                                                    if (channel.Setting.LowerLimit !=
                                                        channelNew.Setting.LowerLimit)
                                                    {
                                                        LogString.Append("Channel lower limit not equal in \"" +
                                                                         group.Name +
                                                                         "\", \"" + card.Name + "\", \"" +
                                                                         sequence.Name + "\", \"" + channel.Setting.Name +
                                                                         "\"\t" +
                                                                         channel.Setting.LowerLimit +
                                                                         " --> " +
                                                                         channelNew.Setting.LowerLimit +
                                                                         "\n");
                                                    }
                                                    if (channel.Setting.UpperLimit !=
                                                        channelNew.Setting.UpperLimit)
                                                    {
                                                        LogString.Append("Channel lower limit not equal in \"" +
                                                                         group.Name +
                                                                         "\", \"" + card.Name + "\", \"" +
                                                                         sequence.Name + "\", \"" + channel.Setting.Name +
                                                                         "\"\t" +
                                                                         channel.Setting.UpperLimit +
                                                                         " --> " +
                                                                         channelNew.Setting.UpperLimit +
                                                                         "\n");
                                                    }
                                                    if (channel.Setting.InitValue !=
                                                        channelNew.Setting.InitValue)
                                                    {
                                                        LogString.Append("Channel lower limit not equal in \"" +
                                                                         group.Name +
                                                                         "\", \"" + card.Name + "\", \"" +
                                                                         sequence.Name + "\", \"" + channel.Setting.Name +
                                                                         "\"\t" +
                                                                         channel.Setting.InitValue +
                                                                         " --> " +
                                                                         channelNew.Setting.InitValue +
                                                                         "\n");
                                                    }








                                                    if (channel.Setting.Invert !=
                                                        channelNew.Setting.Invert)
                                                    {
                                                        LogString.Append("Channel inverting not equal in \"" +
                                                                         group.Name +
                                                                         "\", \"" + card.Name + "\", \"" +
                                                                         sequence.Name + "\", \"" + channel.Setting.Name +
                                                                         "\"\t" +
                                                                         channel.Setting.Invert +
                                                                         " --> " +
                                                                         channelNew.Setting.Invert +
                                                                         "\n");
                                                    }
 
                                                    if (channel.Setting.UseCalibration != channelNew.Setting.UseCalibration)
                                                    {
                                                        LogString.Append(
                                                                "Channel settings usage of \"use calibration\" in \"" +
                                                                group.Name +
                                                                "\", \"" + card.Name + "\", \"" +
                                                                sequence.Name + "\", \"" + channel.Setting.Name + "\"\t" + channel.Setting.UseCalibration +
                                                                " --> " + channelNew.Setting.UseCalibration + "\n");
                                                    }
                                                    else
                                                    {
                                                        if (channel.Setting.UseCalibration)
                                                        {
                                                            if (channel.Setting.CalibrationScript != channelNew.Setting.CalibrationScript)
                                                            {
                                                                LogString.Append("Channel calibration script not equal in \"" +
                                                                    group.Name +
                                                                    "\", \"" + card.Name + "\", \"" +
                                                                    sequence.Name + "\", \"" + channel.Setting.Name + "\"\t" +
                                                                    channel.Setting.CalibrationScript +
                                                                    " --> " +
                                                                    channelNew.Setting.CalibrationScript +
                                                                    "\n");
                                                            }

                                                            if (channel.Setting.InputUnit != channelNew.Setting.InputUnit)
                                                            {
                                                                LogString.Append("Channel input unit not equal in \"" +
                                                                    group.Name +
                                                                    "\", \"" + card.Name + "\", \"" +
                                                                    sequence.Name + "\", \"" + channel.Setting.Name + "\"\t" +
                                                                    channel.Setting.InputUnit +
                                                                    " --> " +
                                                                    channelNew.Setting.InputUnit +
                                                                    "\n");
                                                            }
                                                        }
                                                    }

                                                    

                                                }
                                                if (channel.Steps.Count != channelNew.Steps.Count)
                                                {
                                                    //System.Console.WriteLine("Number of steps not equal! In channel {0}", channel.Setting.Name);
                                                    LogString.Append("Number of steps not equal in \"" + group.Name +
                                                                     "\", \"" + card.Name + "\",  \"" + sequence.Name +
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
                                                            LogString.Append("Step types not equal in \"" + group.Name +
                                                                             "\", \"" + card.Name + "\",  \"" +
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
                                                                    LogString.Append("Step filenames not equal in \"" +
                                                                                     group.Name + "\", \"" + card.Name +
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
                                                                        "Duration variables not equal in \"" +
                                                                        group.Name + "\", \"" + card.Name + "\",  \"" +
                                                                        sequence.Name + "\", \"" + channel.Setting.Name +
                                                                        "\", step \"" + (m + 1) + "\"\t" +
                                                                        stringOrDefault(step.DurationVariableName,
                                                                            "user input") + " --> " +
                                                                        stringOrDefault(stepNew.DurationVariableName,
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
                                                                            group.Name + "\", \"" + card.Name +
                                                                            "\",  \"" + sequence.Name + "\", \"" +
                                                                            channel.Setting.Name + "\", step \"" +
                                                                            (m + 1) + "\"\t" + step.Duration.Value +
                                                                            " --> " + stepNew.Duration.Value + "\n");
                                                                    }
                                                                }
                                                                if (step.ValueVariableName != stepNew.ValueVariableName)
                                                                {
                                                                    //System.Console.WriteLine("Value variable changed in channel {2} at step {3}! old: {0} new: {1}",step.ValueVariableName,stepNew.ValueVariableName,channel.Setting.Name, m);
                                                                    LogString.Append("Value variables not equal in \"" +
                                                                                     group.Name + "\", \"" + card.Name +
                                                                                     "\",  \"" + sequence.Name +
                                                                                     "\", \"" + channel.Setting.Name +
                                                                                     "\", step \"" + (m + 1) + "\"\t" +
                                                                                     stringOrDefault(
                                                                                         step.ValueVariableName,
                                                                                         "user input") + " --> " +
                                                                                     stringOrDefault(
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
                                                                            "Value values not equal in \"" + group.Name +
                                                                            "\", \"" + card.Name + "\",  \"" +
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
                    }
                }
                //LogString.Append(SettingsValidation(ref model));
                if (LogString.Length != 0)
                {
                    LogString.Insert(0, "Detected changes:\n");
                    SimpleStringOkWindow.ShowNewSimpleStringOkWindow("Detected changes", LogString.ToString());
                }
                else
                {
                    LogString.Append("No changed detected!");
                }

                //end of new code 02.12.14
            }

            _model = model;
            _variables.SetNewVariablesModel(model.Data.variablesModel);
            _variables.SetNewRootModel(model);
            CreateWindows();
            _buffer.CopyData(_model);

            FileName = fileName;
        }

        public void DoRefreshWindows(object parameter)
        {
            createNewWindowsForNewModel(_model);
        }

        public void createNewWindowsForNewModel(IModel newModel)
        {
            _outputHandler.OutputCycleState = OutputHandler.CycleStates.Stopped;
            ((VariablesController)_variablesController).GetRootController().DisableCopyToBuffer(); // FIXME is there a better access to the root model or a better way to stop updates ?
            _windowList.CloseAll();
            _model = newModel;
            _variables.SetNewVariablesModel((newModel as RootModel).Data.variablesModel);
            _variables.SetNewRootModel((newModel as RootModel));
            CreateWindows();
            _buffer.CopyData(_model);
        }

        private void LoadFile(object parameter)
        {
            var fileDialog = new OpenFileDialog { DefaultExt = ".xml.gz", Filter = "Sequence (.xml.gz)|*.xml.gz" };
            bool? result = fileDialog.ShowDialog();
            if (result == true)
            {
                string fileName = fileDialog.FileName;

                LoadFileByFileName(fileName, true);
            }
        }

        private void SaveFile(object parameter)
        {
            var fileDialog = new SaveFileDialog { DefaultExt = ".xml.gz", Filter = "Sequence (.xml.gz)|*.xml.gz" };

            bool? result = fileDialog.ShowDialog();
            if (result == true)
            {
                var writer = new FileStream(fileDialog.FileName, FileMode.Create);
                var gz = new GZipStream(writer, CompressionMode.Compress, false);
                Type type = _model.GetType();
                var serializer = new DataContractSerializer(type);
                serializer.WriteObject(gz, _model);
                gz.Close();
                writer.Close();
                FileName = fileDialog.FileName;
            }
        }

        private void IterateAndSaveClick(object parameter)
        {
            bool newIterateAndSaveState = !_iterateAndSave;
            if (newIterateAndSaveState)
            {
                _startTime = DateTime.Now;
                if (this.PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("EndTime"));
                    PropertyChanged(this, new PropertyChangedEventArgs("StartTime"));
                }
                if (_once)
                {
                    _outputHandler.OutputCycleState = OutputHandler.CycleStates.ScanningOnce;
                }
                else
                {
                    _outputHandler.OutputCycleState = OutputHandler.CycleStates.Scanning;
                }
            }
            else
            {
                _outputHandler.OutputCycleState = OutputHandler.CycleStates.Running;
            }
            _iterateAndSave = newIterateAndSaveState;
        }

        private void Once(object parameter)
        {
            _once = !_once;
            OutputHandler.CycleStates cycleState = _outputHandler.OutputCycleState;
            if (cycleState == OutputHandler.CycleStates.Scanning || cycleState == OutputHandler.CycleStates.ScanningOnce)
            {
                if (_once)
                {
                    _outputHandler.OutputCycleState = OutputHandler.CycleStates.ScanningOnce;
                }
                else
                {
                    _outputHandler.OutputCycleState = OutputHandler.CycleStates.Scanning;
                }
            }
        }

        private void StartOutput(object parameter)
        {
            _outputHandler.OutputCycleState = OutputHandler.CycleStates.Running;
        }

        private void StopOutput(object parameter)
        {
            _outputHandler.OutputCycleState = OutputHandler.CycleStates.Stopped;
        }

        private void CreateWindows()
        {
            var controller = (RootController)_controllerRecipe.Cook(_model, _variables);
            Dictionary<string, IWindowController> windowControllers = GetWindowController(controller);
            windowControllers.Add("Variables", _variablesController);
            windowControllers.Add("Errors", null);

            Dictionary<string, Window> newWindows = _windows.Generate(windowControllers);
            //newWindows.Add("Variables", _variablesWindow);
            //newWindows.Add("Errors", _errorWindow);
            Random rnd = new Random();

            Dictionary<string, Window> snewWindows = new Dictionary<string, Window>(newWindows);
            newWindows.Clear();

            foreach (KeyValuePair<string, Window> w in snewWindows)
            {
                newWindows.Add(w.Key, w.Value);

            }

            _windowList = new WindowList(newWindows);
            _windowList.ShowAll();
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("WindowsList"));
            }
            if (null != DoWindowsListChanged)
            {
                DoWindowsListChanged(null, null);
            }
        }
    }
}