using System;
using System.Windows;
using Communication.Commands;
using Controller.Data.Channels;
using Controller.Helper;
using Model.Data.Steps;

namespace Controller.Data.Steps
{
    /// <summary>
    /// A controller for the step that determine their output using a Python script.
    /// </summary>
    /// <seealso cref="Controller.Data.Steps.StepBasicController" />
    public class StepPythonController:StepBasicController
    {
        /// <summary>
        /// The set script command
        /// </summary>
        private RelayCommand setScriptCommand;

        /// <summary>
        /// Gets the command that is triggered when the set script button is clicked
        /// </summary>
        /// <value>
        /// The set script command.
        /// </value>
        public RelayCommand SetScriptCommand
        {
            get 
            {
                if (setScriptCommand == null)
                    setScriptCommand = new RelayCommand(OpenSetScript);
                return setScriptCommand; 
            }
            
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public StepPythonModel Model
        {
            get
            {
                return (StepPythonModel)_model;
            }
        }

        /// <summary>
        /// Gets or sets the analog type selected.
        /// </summary>
        /// <value>
        /// The analog type selected.
        /// </value>
        public override Channels.ChannelBasicController.AnalogTypes AnalogTypeSelected
        {
            get
            {
                return
                    (ChannelBasicController.AnalogTypes)
                    Enum.Parse(typeof(ChannelBasicController.AnalogTypes), Model.Store.ToString());
            }
            set
            {
                StepPythonModel.StoreType store;
                if (Enum.TryParse(value.ToString(), out store))
                {
                    Model.Store = store;
                }
                else
                {
                    ((ChannelBasicController)Parent).ChangeStep(this, value.ToString());
                }
                ((ChannelBasicController)Parent).CopyToBuffer();
            }
        }

        /// <summary>
        /// Gets or sets the digital type selected.
        /// </summary>
        /// <value>
        /// The digital type selected.
        /// </value>
        /// <exception cref="System.NotImplementedException">
        /// No Python steps for digital cards!
        /// </exception>
        public override Channels.ChannelBasicController.DigitalTypes DigitalTypeSelected
        {
            get
            {
                throw new NotImplementedException("No Python steps for digital cards!");
            }
            set
            {
                throw new NotImplementedException("No Python steps for digital cards!");
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="StepPythonController"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="parent">The parent.</param>
        public StepPythonController(StepPythonModel model, ChannelBasicController parent)
            : base(parent, model)
        {
            ReattachVariable();
        }

        /// <summary>
        /// Opens the window that allows to change the script.
        /// </summary>
        /// <param name="param">The parameter.</param>
        public void OpenSetScript(object param)
        {
            StepPythonSetScriptController controller = new StepPythonSetScriptController(Model.Script, this);
            Window managerWindow = WindowsHelper.CreateWindowToHostViewModel(controller, false);
            managerWindow.MinHeight = 400;
            managerWindow.MinWidth = 400;
            managerWindow.Height = 450;
            managerWindow.Width = 600;
            managerWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            managerWindow.Title = "Python Script";

            managerWindow.ShowDialog();

            if (controller.Result == StepPythonSetScriptController.SetScriptResult.SAVE)
            {
                Model.Script = controller.Script;
                //Changes need to be reflected on future models
                _rootController.CopyToBuffer();
            }
                    
        }
    }
}
