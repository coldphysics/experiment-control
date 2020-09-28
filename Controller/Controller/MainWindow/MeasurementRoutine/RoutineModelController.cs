using Model.MeasurementRoutine;
using Model.Root;
using System.Windows.Input;
using Communication.Commands;
using System.Windows;
using Controller.Helper;

namespace Controller.MainWindow.MeasurementRoutine
{
    public class RoutineModelController : BaseController
    {

        private ICommand setPythonScriptsCommand;

        public RoutineBasedRootModel RoutineModel
        {
            get;
            set;
        }

        public string FilePath
        {
            get
            {
                return RoutineModel.FilePath;
            }

            set
            {
                RoutineModel.FilePath = value;
                OnPropertyChanged("FilePath");
            }
        }

        public int TimesToReplicate
        {
            get
            {
                return RoutineModel.TimesToReplicate;
            }

            set
            {
                RoutineModel.TimesToReplicate = value;
                OnPropertyChanged("TimesToReplicate");
            }
        }

        public RootModel ActualModel
        {
            set { RoutineModel.ActualModel = value; }

            get
            {
                return RoutineModel.ActualModel;
            }
        }

        public ICommand SetPythonScriptsCommand
        {
            get
            {
                if (this.setPythonScriptsCommand == null)
                    this.setPythonScriptsCommand = new RelayCommand(LoadPythonScripts);

                return this.setPythonScriptsCommand;
            }
        }

        public RoutineModelController(RoutineBasedRootModel rbrm)
        {
            this.RoutineModel = rbrm;
        }

        public RoutineModelController(RootModel model)
            : this(new RoutineBasedRootModel() { TimesToReplicate = 1, ActualModel = model })
        {

        }

        public void LoadPythonScripts(object parameter)
        {
            PythonScriptsController pyhtonScriptsController = new PythonScriptsController(this.RoutineModel);
            Window pyhtonScriptsWindow = WindowsHelper.CreateWindowToHostViewModel(pyhtonScriptsController, false, false, false, true);

            pyhtonScriptsWindow.MinHeight = 360;
            pyhtonScriptsWindow.MinWidth = 550;
            pyhtonScriptsWindow.Height = 450;
            pyhtonScriptsWindow.Width = pyhtonScriptsWindow.MinWidth;
            pyhtonScriptsWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            pyhtonScriptsWindow.Title = "Python Scripts";
            pyhtonScriptsWindow.ShowDialog();
        }
    }
}
