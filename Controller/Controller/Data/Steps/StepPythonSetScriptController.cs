using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Buffer;
using Model.Data;
using Communication.Commands;

namespace Controller.Data.Steps
{
    /// <summary>
    /// A controller for the window that allows to change the Python script associated to a step.
    /// </summary>
    /// <seealso cref="Controller.BaseController" />
    /// <seealso cref="System.ComponentModel.IDataErrorInfo" />
    public class StepPythonSetScriptController : BaseController, IDataErrorInfo
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
        /// The script text.
        /// </summary>
        private string script;
        /// <summary>
        /// The command that is triggered when the save button is clicked.
        /// </summary>
        private RelayCommand _saveCommand;
        /// <summary>
        /// The command that is triggered when the close button is clicked.
        /// </summary>
        private RelayCommand _closeCommand;
        /// <summary>
        /// The controller for the step
        /// </summary>
        private StepBasicController controller;

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
                    _saveCommand = new RelayCommand(
                        param => this.Save(param)
                        , param => this.IsValid
                        );
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

        /// <summary>
        /// Gets the name of the card.
        /// </summary>
        /// <value>
        /// The name of the card.
        /// </value>
        public string CardName
        {
            get
            {
                return controller._model.Card().Name;
            }
        }

        /// <summary>
        /// Gets the index of the channel.
        /// </summary>
        /// <value>
        /// The index of the channel.
        /// </value>
        public int ChannelIndex
        {
            get
            {
                return controller._model.Channel().Index();
            }

        }

        /// <summary>
        /// Gets the index of the step.
        /// </summary>
        /// <value>
        /// The index of the step.
        /// </value>
        public int StepIndex
        {
            get
            {
                return controller.IndexOfModel();
            }
        }

        /// <summary>
        /// Gets the variables model.
        /// </summary>
        /// <value>
        /// The variables model.
        /// </value>
        public DataModel TheDataModel
        {
            get
            {
                return controller._rootController.returnModel.Data;
            }
        }



        /// <summary>
        /// Initializes a new instance of the <see cref="StepPythonSetScriptController"/> class.
        /// </summary>
        /// <param name="initialScript">The initial script.</param>
        /// <param name="controller">The controller.</param>
        public StepPythonSetScriptController(string initialScript, StepBasicController controller)
        {
            this.script = initialScript;
            this.controller = controller;
            Result = SetScriptResult.CANCEL_OR_CLOSE;
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

        #region IDataErrorInfo Members

        /// <summary>
        /// The validated properties
        /// </summary>
        private static readonly string[] ValidatedProperties = 
        { 
            "Script"
        };

        /// <summary>
        /// Returns true if the step is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this step is valid; otherwise, <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public string this[string columnName]
        {
            get { 
                string result = "";

                switch (columnName)
                {
                    case "Script":
                        if (Script == null || Script.Trim().Length == 0)
                        {
                            result = "You must specify a python Script!";
                        }
                        else
                        {
                            string errorMessage;

                            if (!GeneratorWrapper.ValidateScriptOfPythonStep(CardName, ChannelIndex, StepIndex, Script, TheDataModel, out errorMessage, false))
                            {
                                result = errorMessage;
                            }
                        }
                        break;

                    default:
                        break;
                }

                return result;       
            }
        }

        #endregion
    }
}
