using Communication.Commands;
using Controller.Helper;
using Controller.Settings.Settings;
using Model.MeasurementRoutine;
using Model.Settings.Settings;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Controller.MainWindow.MeasurementRoutine
{
    public class PythonScriptsController : BaseController
    {
        RoutineBasedRootModel model;
        FileSettingController everyCycleTimeCritical;

        public FileSettingController EveryCycleTimeCritical
        {
            get { return everyCycleTimeCritical; }
            set { everyCycleTimeCritical = value; }
        }
        FileSettingController everyCycleNonTimeCritical;

        public FileSettingController EveryCycleNonTimeCritical
        {
            get { return everyCycleNonTimeCritical; }
            set { everyCycleNonTimeCritical = value; }
        }
        FileSettingController startofScanTimeCritical;

        public FileSettingController StartofScanTimeCritical
        {
            get { return startofScanTimeCritical; }
            set { startofScanTimeCritical = value; }
        }
        FileSettingController startofScanNonTimeCritical;

        public FileSettingController StartofScanNonTimeCritical
        {
            get { return startofScanNonTimeCritical; }
            set { startofScanNonTimeCritical = value; }
        }
        FileSettingController controlLecroyVB;

        public FileSettingController ControlLecroyVB
        {
            get { return controlLecroyVB; }
            set { controlLecroyVB = value; }
        }
        public ICommand SavePythonScriptsCommand { private set; get; }

        //public FileSettingController EveryCycleTimeCritical { get => everyCycleTimeCritical; set => everyCycleTimeCritical = value; }
        //public FileSettingController EveryCycleNonTimeCritical { get => everyCycleNonTimeCritical; set => everyCycleNonTimeCritical = value; }
        //public FileSettingController StartofScanTimeCritical { get => startofScanTimeCritical; set => startofScanTimeCritical = value; }
        //public FileSettingController StartofScanNonTimeCritical { get => startofScanNonTimeCritical; set => startofScanNonTimeCritical = value; }
        //public FileSettingController ControlLecroyVB { get => controlLecroyVB; set => controlLecroyVB = value; }

        public PythonScriptsController(RoutineBasedRootModel model)
        {
            SavePythonScriptsCommand = new RelayCommand(Save);
            this.model = model;
            FileSetting ectc = new FileSetting("Every Cycle Time-Critical Python File Path", model.ActualModel.EveryCycleTimeCriticalPythonFilePath, new List<string> { "Python Files (*.py)|*.py" });
            EveryCycleTimeCritical = new FileSettingController(ectc);
            FileSetting ecntc = new FileSetting("Every Cycle Non-Time-Critical Python File Path", model.ActualModel.EveryCycleNonTimeCriticalPythonPath, new List<string> { "Python Files (*.py)|*.py" });
            EveryCycleNonTimeCritical = new FileSettingController(ecntc);
            FileSetting sostc = new FileSetting("Start of Scan Time-Critical Python File Path", model.ActualModel.StartofScanTimeCriticalPythonFilePath, new List<string> { "Python Files (*.py)|*.py" });
            StartofScanTimeCritical = new FileSettingController(sostc);
            FileSetting sosntc = new FileSetting("Start of Scan Non-Time-Critical Python File Path", model.ActualModel.StartofScanNonTimeCriticalPythonFilePath, new List<string> { "Python Files (*.py)|*.py" });
            StartofScanNonTimeCritical = new FileSettingController(sosntc);
            FileSetting clvb = new FileSetting("Control Lecroy VB Script Path", model.ActualModel.ControlLecroyVBScriptPath, new List<string> { "Visual Basic Files (*.vbs)|*.vbs" });
            ControlLecroyVB = new FileSettingController(clvb);
        }

        private void Save(object parameter)
        {
            this.model.ActualModel.EveryCycleTimeCriticalPythonFilePath = EveryCycleTimeCritical.Value;
            this.model.ActualModel.EveryCycleNonTimeCriticalPythonPath = EveryCycleNonTimeCritical.Value;
            this.model.ActualModel.StartofScanTimeCriticalPythonFilePath = StartofScanTimeCritical.Value;
            this.model.ActualModel.StartofScanNonTimeCriticalPythonFilePath = StartofScanNonTimeCritical.Value;
            this.model.ActualModel.ControlLecroyVBScriptPath = ControlLecroyVB.Value;
            if (this.model.FilePath != null && this.model.FilePath.Length > 0)
                FileHelper.SaveFile(this.model.FilePath, this.model.ActualModel);
            CloseWindow(parameter);
        }
        private void CloseWindow(object parameter)
        {
            if (parameter != null)
            {
                UserControl uc = (UserControl)parameter;
                Window w = Window.GetWindow(uc);
                w.Close();
            }
        }
        //    /// <summary>
        //    /// Initializes a new instance of the <see cref="FileSettingController"/> class.
        //    /// </summary>
        //    /// <param name="setting">The setting.</param>
        //    /// <summary>
        //    /// The action to be performed to choose a value
        //    /// </summary>
        //    /// <param name="obj">A parameter to pass to the action</param>
        //    protected override void OpenCommandAction(object obj)
        //    {
        //        StringBuilder filterBuilder = new StringBuilder();
        //        int counter = 0;

        //        foreach (string fileType in Setting.AcceptedFileExtensions)
        //        {
        //            filterBuilder.Append(fileType);

        //            if (counter < Setting.AcceptedFileExtensions.Count - 1)
        //                filterBuilder.Append("|");

        //            counter++;
        //        }

        //        OpenFileDialog openFileDialog = new OpenFileDialog();
        //        openFileDialog.Filter = filterBuilder.ToString();
        //        openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        //        if (openFileDialog.ShowDialog() == true)
        //            Value = openFileDialog.FileName;
        //    }
    }
}
