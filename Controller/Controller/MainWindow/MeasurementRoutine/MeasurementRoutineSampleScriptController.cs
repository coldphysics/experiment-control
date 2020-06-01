using System.Windows;
using System.Windows.Controls;
using Communication.Commands;

namespace Controller.MainWindow.MeasurementRoutine
{
    public class MeasurementRoutineSampleScriptController:BaseController
    {
        /// <summary>
        /// The possible outcomes of this dialog window.
        /// </summary>
        public enum SampleScriptResult
        {
            CLOSE,
            COPY
        }

        /// <summary>
        /// The command that is triggered when the save button is clicked.
        /// </summary>
        private RelayCommand _saveCommand;
        /// <summary>
        /// The command that is triggered when the close button is clicked.
        /// </summary>
        private RelayCommand _closeCommand;


        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public SampleScriptResult Result
        {
            set;
            get;
        }

        /// <summary>
        /// Gets the save command.
        /// </summary>
        /// <value>
        /// The save command.
        /// </value>
        public RelayCommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(Save);
                }
                return _saveCommand;
            }
        }
        /// <summary>
        /// Gets the close command.
        /// </summary>
        /// <value>
        /// The close command.
        /// </value>
        public RelayCommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                    _closeCommand = new RelayCommand(CloseWindow);

                return _closeCommand;
            }
        }

        public string SampleInitializationScript
        {
            get
            {
                //return "n = 0 # initialize the counter\n" +
                //       "# create some cells in the routine array\n" +
                //       "routine_array.Add(0)\n" +
                //       "routine_array.Add(0)\n" +
                //       "routine_array.Add(0)";
                return "import sys\n"+
                       "sys.path.append(r'C:\\Program Files\\IronPython 2.7\\Lib') # to import external libraries\n" +
                       "import os\n" +
                       "n = -1 # initialize the counter\n" + // this is intialized by -1 because in the sample script we want to run the primary model 5 times before switching to the secondary mode.
                      "# create some cells in the routine array\n" +
                      "routine_array.Add(0)\n" +
                      "routine_array.Add(0)\n" +
                      "routine_array.Add(0)";
            }
        }
        //Ebaa 29.05.2018 The name of next iteration should be last iteration
        public string SampleRepetitiveScript
        {
            get
            {
                return "if currentMode == 0: #corresponds to primary mode\n" +
                       "    n = n + 1 #increment only for primary cycles\n" +
                       "elif currentMode == 1: #corresponds to secondary mode\n" +
                       "    if lastIteration == numberOfIterations: #switch back to primary mode, if calibration scan is finished\n" +
                       "         currentMode = 0\n" +
                       "         n = 0\n" +
                       "if n == 5: #switch to calibration scan after 5 primary cycles\n" +
                       "     currentMode = 1";
            }
        }

        public MeasurementRoutineSampleScriptController()
        {
            Result = SampleScriptResult.CLOSE;
        }

        /// <summary>
        /// Executed when the save command is triggered
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void Save(object parameter)
        {
            Result = SampleScriptResult.COPY;
            CloseWindow(parameter);
        }

        /// <summary>
        /// Executed when the cancel command is executed.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void CloseWindow(object parameter)
        {
            if (parameter != null)
            {
                UserControl uc = (UserControl)parameter;
                Window w = Window.GetWindow(uc);
                w.Close();
            }
        }
    }
}
