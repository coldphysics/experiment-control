using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Communication.Commands;
using Model.MeasurementRoutine.GlobalVariables;
using System.Linq;
using Controller.Common;

namespace Controller.MainWindow.MeasurementRoutine
{
    /// <summary>
    /// Holds data used to describe to users the usage of a special python variable that they can use in a python script
    /// </summary>
    public class VariableUsageDescriptor
    {
        /// <summary>
        /// All data types of variables
        /// </summary>
        public enum VariableTypeEnum { Integer, Boolean, String, StringArray, DoubleArray }
        /// <summary>
        /// All access types of variables
        /// </summary>
        public enum AccessTypeEnum { Read, Write, ReadWrite }

        /// <summary>
        /// Sets the type of the variable.
        /// </summary>
        /// <value>
        /// The type of the variable.
        /// </value>
        public VariableTypeEnum VariableType { set; private get; }

        /// <summary>
        /// Sets the type of the access.
        /// </summary>
        /// <value>
        /// The type of the access.
        /// </value>
        public AccessTypeEnum AccessType { set; private get; }

        /// <summary>
        /// Gets or sets the name of the variable.
        /// </summary>
        /// <value>
        /// The name of the variable.
        /// </value>
        public string VariableName { set; get; }

        /// <summary>
        /// Gets the variable type as string.
        /// </summary>
        /// <value>
        /// The variable type as string.
        /// </value>
        public string VariableTypeAsString
        {
            get
            {
                switch (VariableType)
                {
                    case VariableTypeEnum.Boolean:
                        return "Boolean";
                    case VariableTypeEnum.Integer:
                        return "Integer";
                    case VariableTypeEnum.String:
                        return "String";
                    case VariableTypeEnum.StringArray:
                        return "Array of Strings";
                    case VariableTypeEnum.DoubleArray:
                        return "List of Doubles";
                }

                return "Unknown";
            }
        }

        /// <summary>
        /// Gets the read or write.
        /// </summary>
        /// <value>
        /// The read or write.
        /// </value>
        public string ReadOrWrite
        {
            get
            {
                switch (AccessType)
                {
                    case AccessTypeEnum.Read:
                        return "Read Only";
                    case AccessTypeEnum.Write:
                        return "Write";
                    case AccessTypeEnum.ReadWrite:
                        return "Read/Write";
                }

                return "Unknown";
            }
        }
        public string Remarks { set; get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableUsageDescriptor"/> class.
        /// </summary>
        public VariableUsageDescriptor()
        {
            VariableType = VariableTypeEnum.Integer;
            AccessType = AccessTypeEnum.Read;
        }
    }

    public class MeasurementRoutineScriptController : BaseController
    {
        /// <summary>
        /// The possible outcomes of this dialog window.
        /// </summary>
        public enum SetScriptResult
        {
            CANCEL_OR_CLOSE,
            SAVE
        }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public bool IsReadOnly { set; get; }
        /// <summary>
        /// The initialization script text.
        /// </summary>
        private string initializationScript;
        /// <summary>
        /// The repetitive script text.
        /// </summary>
        private string script;

        private bool requiresCodeCheck;
        /// <summary>
        /// The command that is triggered when the save button is clicked.
        /// </summary>
        private RelayCommand _saveCommand;
        /// <summary>
        /// The command that is triggered when the close button is clicked.
        /// </summary>
        private RelayCommand _closeCommand;
        /// <summary>
        /// The command that shows information about the execution steps in the measurement routine
        /// </summary>
        private RelayCommand _showExecutionStepsInfo;
        /// <summary>
        /// The command that shows a window with sample scripts
        /// </summary>
        private RelayCommand _showSampleScriptCommand;
        /// <summary>
        /// This commands opens the initialization script in the default Python editor
        /// </summary>
        private RelayCommand _openInitializationScriptInExternalEditor;
        /// <summary>
        /// This command opens the control script in the default Python 
        /// </summary>
        private RelayCommand _openControlScriptInExternalEditor;

        private RelayCommand _checkCodeCommand;

        /// <summary>
        /// Gets or sets the initialization script.
        /// </summary>
        /// <value>
        /// The initialization script.
        /// </value>
        public string InitializationScript
        {
            get { return initializationScript; }
            set
            {
                initializationScript = value;
                this.requiresCodeCheck = true;
                OnPropertyChanged("InitializationScript");
            }
        }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        public SetScriptResult Result
        {
            set;
            get;
        }
        /// <summary>
        /// Gets or sets the script.
        /// </summary>
        /// <value>
        /// The script.
        /// </value>
        public string Script
        {
            get { return script; }
            set
            {
                script = value;
                this.requiresCodeCheck = true;
                OnPropertyChanged("Script");
            }
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
                    _saveCommand = new RelayCommand(Save, CanSaveScripts);
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

        public RelayCommand ShowExecutionStepsInfo
        {
            get
            {
                if (_showExecutionStepsInfo == null)
                    _showExecutionStepsInfo = new RelayCommand(ShowExecutionSteps);
                return _showExecutionStepsInfo;
            }


        }
        public RelayCommand ShowSampleScriptCommand
        {
            get
            {
                if (_showSampleScriptCommand == null)
                {
                    _showSampleScriptCommand = new RelayCommand(ShowSampleScript, CanShowSampleScript);
                }

                return _showSampleScriptCommand;
            }
        }
        public RelayCommand CheckCodeCommand
        {
            get
            {
                if (_checkCodeCommand == null)
                    _checkCodeCommand = new RelayCommand(CheckCode);

                return _checkCodeCommand;
            }
        }
        public RelayCommand OpenInitializationScriptInExternalEditor
        {
            get
            {
                if (_openInitializationScriptInExternalEditor == null)
                {
                    _openInitializationScriptInExternalEditor = new RelayCommand((parameter) =>
                    {
                        string newContent = OpenPythonScriptInExternalEditor(InitializationScript);

                        if (newContent!= InitializationScript)
                        {
                            InitializationScript = newContent;
                        }
                    });
                }

                return _openInitializationScriptInExternalEditor;
            }
        }

        public RelayCommand OpenControlScriptInExternalEditor
        {
            get
            {
                if (_openControlScriptInExternalEditor == null)
                {
                    _openControlScriptInExternalEditor = new RelayCommand((parameter) =>
                    {
                        string newContent = OpenPythonScriptInExternalEditor(Script);

                        if (newContent != Script)
                        {
                            Script = newContent;
                        }
                    });
                }

                return _openControlScriptInExternalEditor;
            }
        }

        public VariableUsageDescriptor[] BuiltInVariables { set; get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MeasurementRoutineScriptController"/> class.
        /// </summary>
        /// <param name="initializationScript">The initial script.</param>
        /// <param name="repetitiveScript">The control script</param>
        /// <param name="isReadOnly">Indicates whether the instance is read-only or not</param>
        public MeasurementRoutineScriptController(string initializationScript, string repetitiveScript, bool isReadOnly)
        {
            this.requiresCodeCheck = true;
            this.script = repetitiveScript;
            this.initializationScript = initializationScript;
            this.IsReadOnly = isReadOnly;
            Result = SetScriptResult.CANCEL_OR_CLOSE;
            InitializeBuiltInVariables();
        }

        private void InitializeBuiltInVariables()
        {
            BuiltInVariables = new VariableUsageDescriptor[]{
                new VariableUsageDescriptor(){
                    VariableName = MeasurementRoutineManager.VAR_CURRENT_MODE,
                    AccessType = VariableUsageDescriptor.AccessTypeEnum.ReadWrite,
                    Remarks = "The index of the current model (0 for the primary model, higher for secondary models). You can assign a value to this variable in order to affect the model of the next cycle."
                },
                new VariableUsageDescriptor(){
                    VariableName = GlobalVariableNames.ROUTINE_ARRAY,
                    AccessType = VariableUsageDescriptor.AccessTypeEnum.ReadWrite,
                    Remarks = String.Format(
                    "A list of double values accessible from python scripts outside the measurement routine (e.g., dynamic variables). Use \"{0}.Add(15)\" to add the value \"15\" at the end of the list (increases the size of the list). Use \"{0}[0]\" to read or write an existing element at the position 0 of the list."
                    , GlobalVariableNames.ROUTINE_ARRAY)

                },
                new VariableUsageDescriptor(){
                    VariableName= MeasurementRoutineManager.VAR_PRIMARY_MODEL,
                    VariableType = VariableUsageDescriptor.VariableTypeEnum.String,
                    Remarks = "The path of the primary model."
                },
                new VariableUsageDescriptor(){
                    VariableName = MeasurementRoutineManager.VAR_SECONDARY_MODELS,
                    VariableType = VariableUsageDescriptor.VariableTypeEnum.StringArray,
                    Remarks = "An array of the paths of secondary models"
                },

                new VariableUsageDescriptor(){
                    VariableName = MeasurementRoutineManager.VAR_PREVIOUS_MODE,
                    Remarks = "The index of the previous model (0 for the primary model, higher for secondary models)"
                },
                new VariableUsageDescriptor(){
                    VariableName = MeasurementRoutineManager.VAR_START_ROUTINE,
                    VariableType = VariableUsageDescriptor.VariableTypeEnum.Boolean,
                    Remarks = "A boolean value indicating whether the current cycle is the first cycle in the routine"
                },
                new VariableUsageDescriptor(){
                    VariableName = MeasurementRoutineManager.VAR_GLOBAL_COUNTER,
                    VariableType = VariableUsageDescriptor.VariableTypeEnum.Integer,
                    Remarks = "The current value of the global counter"
                },
                new VariableUsageDescriptor(){
                    VariableName = MeasurementRoutineManager.VAR_NUMBER_OF_ITERATIONS,
                    Remarks = "The total number of iterations of the current model based on the iterator variables."
                },

                //Ebaa 29.05.2018 The name of next iteration should be last iteration
                 new VariableUsageDescriptor(){
                    VariableName = MeasurementRoutineManager.VAR_LAST_ITERATION,
                    Remarks = "The number of the previous iteration within the current scan (1 if not iterating)"
                //new VariableUsageDescriptor(){
                //    VariableName = MeasurementRoutineManager.VAR_NEXT_ITERATION,
                //    Remarks = "The number of the next iteration within the current scan (0 if not iterating)"
                },
                new VariableUsageDescriptor(){
                    VariableName = MeasurementRoutineManager.VAR_COMPLETED_SCANS,
                    Remarks = "The number of scans completed in the current run. (0 if not iterating)"
                },
                new VariableUsageDescriptor(){
                    VariableName = MeasurementRoutineManager.VAR_START_COUNTER_OF_SCANS,
                    Remarks = "The value of the global counter when the current set of scans started"
                },
                new VariableUsageDescriptor(){
                    VariableName = MeasurementRoutineManager.VAR_SCAN_ONLY_ONCE,
                    VariableType = VariableUsageDescriptor.VariableTypeEnum.Boolean,
                    Remarks = "Indicates whether iterators will only scanned once or not."
                },
                new VariableUsageDescriptor(){
                    VariableName = MeasurementRoutineManager.VAR_CONTROL_LE_CROY,
                    VariableType = VariableUsageDescriptor.VariableTypeEnum.Boolean,
                    Remarks = "Indicates whether LeCroy will be controlled"
                },
                new VariableUsageDescriptor(){
                    VariableName = MeasurementRoutineManager.VAR_STOP_AFTER_SCAN,
                    VariableType = VariableUsageDescriptor.VariableTypeEnum.Boolean,
                    Remarks = "Indicates whether the output will be stopped after finishing one scan or not."
                },
                new VariableUsageDescriptor(){
                    VariableName = MeasurementRoutineManager.VAR_SHUFFLE_ITERATIONS,
                    VariableType = VariableUsageDescriptor.VariableTypeEnum.Boolean,
                    Remarks = "Indicates whether iterations are shuffled or not."
                }

            };
        }

        /// <summary>
        /// Executed when the save command is triggered
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        private void Save(object parameter)
        {
            Result = SetScriptResult.SAVE;
            CloseWindow(parameter);
        }

        private bool CanSaveScripts(object parameter)
        {
            return !IsReadOnly && !this.requiresCodeCheck;
        }

        private bool CanShowSampleScript(object parameter)
        {
            return !IsReadOnly;
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

        private void ShowSampleScript(object parameter)
        {
            MeasurementRoutineSampleScriptController sampleScriptController = new MeasurementRoutineSampleScriptController();
            Window w = WindowsHelper.CreateWindowToHostViewModel(sampleScriptController, true);
            w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            w.Title = "Sample Measurement Routine Scripts";
            w.ShowDialog();

            if (sampleScriptController.Result == MeasurementRoutineSampleScriptController.SampleScriptResult.COPY)
            {
                this.InitializationScript = sampleScriptController.SampleInitializationScript;
                this.Script = sampleScriptController.SampleRepetitiveScript;
            }

        }

        //Ebaa 29.05.2018
        private void ShowExecutionSteps(object parameter)
        {
            String stepsInfo = "The execution steps of a measurement routine:\n\n" +
                "1- Execute the repetitive python script and decide which model to be loaded next.\n" +
                "2- Increase current model iterators.\n" +
                "3- Load the next model.\n" +
                "4- Save the next model info to the database.\n" +
                "5- Execute the loaded next model.";
            MessageBox.Show(stepsInfo, "Execution Steps Description", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void ShowExternalEditorForInitializationScript(object parameter)
        {
            CustomMessageBoxController message = new CustomMessageBoxController(this);
            message.Message = "When you click on OK, the default program associated with the \".py\" extension will be used to " +
                "apply changes to the initialization script of the measurement routine. When you are done applying changes, simply save them " + 
                "and close the program. If no default program is specified for the \".py\" extension, a dialog will appear that will allow you " +
                "to choose a program yourself.";
            Window w = WindowsHelper.CreateWindowToHostViewModel(message, true);
            w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            w.Title = "Launching External Editor for Python Script";
            w.ShowDialog();

            if (message.DontShowAgain)
            {
                Console.Write("DontShowAgain clicked");
            }
        }

        private void CheckCode(object parameter)
        {
            String result = "";
            if (Script == null || Script.Trim().Length == 0)
            {
                result = "You must specify a python Script!";
            }
            else
            {
                string errorMessage;
                string scriptToAnalyze = "";

                if (!String.IsNullOrEmpty(InitializationScript))
                    scriptToAnalyze = InitializationScript + "\n";

                scriptToAnalyze += Script;

                if (!MeasurementRoutineManager.ValidatePythonScript(scriptToAnalyze, out errorMessage, false))
                {
                    result = errorMessage;
                }
            }

            if (result.Length > 0)
            {
                MessageBox.Show(result, "Invalid Python Script", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("The script successfully passed validation!", "Script OK", MessageBoxButton.OK, MessageBoxImage.Information);
                this.requiresCodeCheck = false;
            }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public string this[string columnName]
        {
            get
            {
                string result = "";

                switch (columnName)
                {
                    case "Script":

                        break;

                    default:
                        break;
                }

                return result;
            }
        }


        private string OpenPythonScriptInExternalEditor(string script)
        {
            if (Model.Properties.Settings.Default.ShowEditPythonExternallyHint)
            {
                CustomMessageBoxController message = new CustomMessageBoxController(this);
                message.Message = "When you click on OK, the default editor associated with the \".py\" extension will be used to " +
                    "apply changes to the script. When you are done, simply save the open file " +
                    "and close the editor.\n\n If no default program is specified for the \".py\" extension, a dialog will appear that will allow you " +
                    "to choose a program yourself.";
                Window w = WindowsHelper.CreateWindowToHostViewModel(message, true, true);
                w.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                w.Title = "Launching External Editor for Python Script";

                // only consider the value of the "don't show again" check box if the user explicitly clicked on OK (not X).
                if (w.ShowDialog().GetValueOrDefault(false))
                {
                    if (message.DontShowAgain)
                    {
                        Model.Properties.Settings.Default.ShowEditPythonExternallyHint = false;
                        Model.Properties.Settings.Default.Save();
                    }
                }
            }

            string filePath = FileHelper.CreateTemporaryFile(script, ".py");
            ProcessStartInfo processStart = new ProcessStartInfo(filePath);
            processStart.UseShellExecute = true;

            // determine whether the .py extension is already associated with an application or not
            if (processStart.Verbs.Where(verb => verb.ToLower() == "open").Count() == 0)
            {
                processStart.Verb = "openas";
            }
            else
            {
                processStart.Verb = "open";
            }

            Process myProcess = Process.Start(processStart);
            myProcess.WaitForExit();

            if (File.Exists(filePath))
            {
                string result = File.ReadAllText(filePath);
                File.Delete(filePath);

                return result;
            }

            return null;
        }

    }
}
