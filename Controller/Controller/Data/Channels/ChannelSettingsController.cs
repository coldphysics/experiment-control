using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using Communication.Commands;
using Controller.Data.Steps;
using Controller.Data.Tabs;
using Model.Data.Channels;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Buffer.OutputProcessors.CalibrationUnit;
using Model.Settings;
using System.Windows.Controls;

namespace Controller.Data.Channels
{
    public class ChannelSettingsController : INotifyPropertyChanged, IDataErrorInfo
    {
        public const string DEFAULT_INPUT_UNIT = "V";
        // ******************** properties ********************
        /*public double InitValue
        {
            get
            {
                if (IndexOfTab() == 0)
                {
                    return _model.InitValue;
                }
                var previousTab = (TabController)_parent.PreviousTab();
                var predecessorChannel = (ChannelBasicController)previousTab.Channels[_model.Index()];
                object lastStep = predecessorChannel.Steps.Last();
                if (lastStep.GetType() == typeof(ChannelSettingsController))
                {
                    return ((ChannelSettingsController)lastStep).InitValue;
                }
                else
                {
                    return ((StepBasicController)lastStep).Value;
                }
            }
            set
            {
                if (IndexOfTab() == 0)
                {
                    _model.InitValue = value;
                    _parent.CopyToBuffer();
                    if (null != this.PropertyChanged)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("InitValue"));
                    }
                }
            }
        }
        public bool InitValueReadOnly
        {
            get
            {
                if (_parent.Index() == 0)
                {
                    return false;
                }
                return true;
            }
        }

        public Brush InitValueBackgroundColor
        {
            get
            {
                if (_parent.Index() == 0)
                {
                    return Brushes.White;
                }
                return Brushes.LightGray;
            }
        }*/

        public double StartTime { get { return _parent.ActualStartTime(); } }

        public bool Invert
        {
            get { return originalModel.Invert; }
            set
            {
                originalModel.Invert = value;
                OnPropertyChanged("Invert");
                _parent.CopyToBuffer();
            }
        }

        //InputUnit
        public string InputUnit
        {
            get 
            { 
                return currentModel.InputUnit; 
            }
            set
            {
                currentModel.InputUnit = value;
                OnPropertyChanged("InputUnit");
                OnPropertyChanged("ActiveInputUnit");
            }
        }


        public string TimeUnit
        {
            get
            {
                return TimeSettingsInfo.GetInstance().TimeUnit;
            }
        }

        public double Max//SETTINGS
        {
            get
            {
                return currentModel.UpperLimit;
            }
            set
            {
                currentModel.UpperLimit = value;
                OnPropertyChanged("Min");
                OnPropertyChanged("Max");
            }
        }

        public double Min//SETTINGS
        {
            get
            {

                return currentModel.LowerLimit;
            }
            set
            {
                currentModel.LowerLimit = value;
                OnPropertyChanged("Min");
                OnPropertyChanged("Max");

            }
        }

        public bool UseCalibration
        {
            set
            {
                currentModel.UseCalibration = value;
                OnPropertyChanged("CalibrationScript");
                OnPropertyChanged("InputUnit");
                OnPropertyChanged("ActiveInputUnit");
            }
            get
            {
                return currentModel.UseCalibration;
            }
        }

        public string CalibrationScript
        {
            get
            {
                return currentModel.CalibrationScript;
            }

            set
            {
                currentModel.CalibrationScript = value;
                OnPropertyChanged("CalibrationScript");
            }
        }

        public string Name//SETTINGS
        {
            get
            {
                return currentModel.Name;
            }
            set
            {
                currentModel.Name = value;

                string stringIdentifier = _parent._parent.Name + "-" + this.NumberOfChannel;
                Dictionary<string, ChannelSettingsSaveValues> defaultValues = GetDefaultValues();
                defaultValues[stringIdentifier].Name = currentModel.Name;
                SaveDefaultValues(defaultValues);
            }
        }

        public int NumberOfChannel
        {
            get { return _numOfChannel; }
        }

        private readonly int _numOfChannel;

        // ******************** events ********************
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnTabUpdate(object sender, EventArgs e)
        {
            OnPropertyChanged("StartTime");
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        // ******************** variables ********************

        private readonly ChannelSettingsModel originalModel;

        private ChannelSettingsModel currentModel;

        private ChannelSettingsModel uiModel;

        public readonly TabController _parent;

        // ******************** constructor ********************
        public ChannelSettingsController(ChannelSettingsModel model, TabController parent, int numOfChannel)
        {
            _parent = parent;
            originalModel = model;
            currentModel = originalModel;
            _numOfChannel = numOfChannel;
            parent.TabUpdate += OnTabUpdate;
            CloseWindowCommand = new RelayCommand(CloseWindow);
            OpenSettingsCommand = new RelayCommand(OpenCommand);
        }


        // ******************** Commands ***********************
        public ICommand OpenSettingsCommand { get; private set; }

        private ICommand _saveCommand;

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(
                        param => this.SaveAnalogSettingsCommand(param)
                        , param => this.IsValid
                        );
                }
                return _saveCommand;
            }
        }

        public RelayCommand CloseWindowCommand { get; private set; }

        private void SaveAnalogSettingsCommand(object parameter)
        {
            //Order copy to buffer

            //Saving default values
            string stringIdentifier = _parent._parent.Name + "-" + this.NumberOfChannel;
            Dictionary<string, ChannelSettingsSaveValues> defaultValues = GetDefaultValues();
            defaultValues[stringIdentifier].Min = uiModel.LowerLimit;
            defaultValues[stringIdentifier].Max = uiModel.UpperLimit;
            SaveDefaultValues(defaultValues);

            originalModel.UpperLimit = uiModel.UpperLimit;
            originalModel.LowerLimit = uiModel.LowerLimit;
            originalModel.UseCalibration = uiModel.UseCalibration;
            originalModel.InputUnit = uiModel.InputUnit;
            originalModel.CalibrationScript = uiModel.CalibrationScript;


            _parent.CopyToBuffer();

            foreach (ChannelBasicController channel in _parent.Channels)
            {
                if (channel.Index() == originalModel.Index())
                {
                    for (int i = 0; i < channel.Steps.Count; i++)
                    {
                        if (channel.Steps[i].GetType() == typeof(StepRampController))
                        {
                            ((StepBasicController)channel.Steps[i]).UpdateProperty("InputUnit");
                        }
                    }
                }
            }

            CloseWindow(parameter);
        }

        private void CloseWindow(object uc)
        {
            currentModel = originalModel;
            OnPropertyChanged(null);//To notify the potential elements shown on the ChannelHeader section of the possible changes.

            if (uc != null)
            {
                Window w = Window.GetWindow((UserControl)uc);
                w.Close();
            }
        }

        private void OpenCommand(object parameter)
        {
            uiModel = originalModel.ShallowCopy();
            currentModel = uiModel;
            Window settingsWindow = WindowsHelper.CreateWindowToHostViewModel(this, true, true);
            settingsWindow.Title = "Analog Settings";
            settingsWindow.ShowDialog();
        }


        // ******************* Default Values Handling *


        public Dictionary<string, ChannelSettingsSaveValues> GetDefaultValues()
        {
            //System.Console.Write("loading defaultValues\n");
            Dictionary<string, ChannelSettingsSaveValues> defaultValues = new Dictionary<string, ChannelSettingsSaveValues>();
            if (File.Exists("defaultValues.xml"))
            {
                var stream = new FileStream("defaultValues.xml", FileMode.Open);
                var reader = XmlDictionaryReader.CreateTextReader(stream, new XmlDictionaryReaderQuotas());
                var deserializer = new DataContractSerializer(typeof(Dictionary<string, ChannelSettingsSaveValues>));
                //System.Console.Write("a\n");
                defaultValues = (Dictionary<string, ChannelSettingsSaveValues>)deserializer.ReadObject(reader, true);
                stream.Close();
            }
            else
            {
                var writer = new FileStream("defaultValues.xml", FileMode.Create);
                Type type = defaultValues.GetType();
                var serializer = new DataContractSerializer(type);
                serializer.WriteObject(writer, defaultValues);
                writer.Close();
            }
            return defaultValues;
        }


        private void SaveDefaultValues(Dictionary<string, ChannelSettingsSaveValues> defaultValues)
        {
            var writer = new FileStream("defaultValues.xml", FileMode.Create);
            Type type = defaultValues.GetType();
            var serializer = new DataContractSerializer(type);
            serializer.WriteObject(writer, defaultValues);
            writer.Close();
        }



        #region IDataErrorInfo Members

        private static readonly string[] ValidatedProperties = 
        { 
            "Min", 
            "Max", 
            "CalibrationScript",
            "InputUnit"
        };

        private bool IsValid
        {
            get
            {
                foreach (string property in ValidatedProperties)
                {
                    if (this[property].Length != 0)
                        return false;
                }

                return true;
            }
        }


        public string this[string columnName]
        {
            get
            {
                string result = "";

                switch (columnName)
                {
                    case "Min":
                        if (Min > Max)
                        {
                            result = "The minimal Value should not be larger than the maximal Value.";
                        }

                        if (Min < -10.0)
                        {
                            result += "The minimal Value should not be smaller than -10 (the lowest Value allowed by the hardware).";
                        }

                        break;

                    case "Max":
                        if (Min > Max)
                        {
                            result = "The maximal Value should not be smaller than the minimal Value.";
                        }
                        if (Max > 10.0)
                        {
                            result += "The maximal Value should not be greater than +10 (the largest Value allowed by the hardware).";
                        }
                        break;

                    case "CalibrationScript":
                        if (UseCalibration)
                        {

                            if (CalibrationScript == null || CalibrationScript.Trim().Length == 0)
                            {
                                result = "You must specify a calibration Script!";
                            }
                            else
                            {
                                string errorMessage;
                                if (!OutputCalibrator.ValidatePythonScript(_parent.Name, uiModel.Channel, CalibrationScript, _parent._rootController.returnModel.Data, out errorMessage, false))
                                {
                                    result = errorMessage;
                                }
                            }
                        }
                        break;
                    case "InputUnit":
                        if (UseCalibration)
                        {
                            if (InputUnit == null || InputUnit.Trim().Length == 0)
                            {
                                result = "You must specify an Input unit!";
                            }
                        }

                        break;
                }


                return result;
            }

        }


        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}