using Model.MeasurementRoutine;
using Model.Root;
using System.Windows.Input;
using Communication.Commands;
using System.Windows;

namespace Controller.MainWindow.MeasurementRoutine
{
    public class RoutineModelController : BaseController
    {
        private RoutineBasedRootModel routineModel;

        private ICommand setPythonScriptsCommand;

        public RoutineBasedRootModel RoutineModel
        {
            get { return routineModel; }
            set { routineModel = value; }
        }


        public string FilePath
        {
            get
            {
                return routineModel.FilePath;
            }

            set
            {
                routineModel.FilePath = value;
                OnPropertyChanged("FilePath");
            }
        }

        public int TimesToReplicate
        {
            get
            {
                return routineModel.TimesToReplicate;
            }

            set
            {
                routineModel.TimesToReplicate = value;
                OnPropertyChanged("TimesToReplicate");
            }
        }

        public RootModel ActualModel
        {
            set { routineModel.ActualModel = value; }

            get
            {
                return routineModel.ActualModel;
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

        public RoutineModelController(RoutineBasedRootModel model)
        {
            this.routineModel = model;

        }

        public void LoadPythonScripts(object parameter)
        {
            PythonScriptsController pyhtonScriptsController = new PythonScriptsController(this.routineModel);
            Window pyhtonScriptsWindow = WindowsHelper.CreateCustomWindowToHostViewModel(pyhtonScriptsController, false);

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
