using System;
using System.Windows;
using System.Windows.Controls;
using Communication.Commands;
using Controller.Data.Channels;
using Controller.Root;
using CustomElements.CheckableTreeView;

namespace Controller.Control.StepBatchAddition
{
    /// <summary>
    /// A controller for the Step Batch-Add window
    /// </summary>
    /// <seealso cref="Controller.BaseController" />
    public class StepBatchAdderController:BaseController
    {
        /// <summary>
        /// The is step type analog
        /// </summary>
        private bool isStepTypeAnalog = true;
        /// <summary>
        /// The root controller
        /// </summary>
        private RootController rootController;
        /// <summary>
        /// The tree view controller
        /// </summary>
        private CTVViewModel treeViewController;
        /// <summary>
        /// The command triggered when an analog card is selected.
        /// </summary>
        private RelayCommand analogSelectedCommand;
        /// <summary>
        /// The command triggered when a digital card is selected.
        /// </summary>
        private RelayCommand digitalSelectedCommand;
        /// <summary>
        /// The command triggered when the close button is clicked.
        /// </summary>
        private RelayCommand closeCommand;
        /// <summary>
        /// The command triggered when the add button is clicked.
        /// </summary>
        private RelayCommand addCommand;
        /// <summary>
        /// The duration of the steps to add.
        /// </summary>
        private decimal duration;
        /// <summary>
        /// The value of the steps to add if they were analog steps
        /// </summary>
        private decimal valueAnalog;
        /// <summary>
        /// The value of the steps to add if they were digital steps (0 -> low, 1 -> high).
        /// </summary>
        private int valueDigitalIndex;

        /// <summary>
        /// Gets or sets the value of the steps to add if they were digital steps (0 -> low, 1 -> high).
        /// </summary>
        /// <value>
        /// The value of the steps to add if they were digital steps (0 -> low, 1 -> high).
        /// </value>
        public int ValueDigitalIndex
        {
            get { return valueDigitalIndex; }
            set 
            { 
                valueDigitalIndex = value;
                OnPropertyChanged("ValueDigitalIndex");
            }
        }


        /// <summary>
        /// Gets or sets the value of the steps to add if they were analog steps
        /// </summary>
        /// <value>
        /// The value of the steps to add if they were analog steps
        /// </value>
        public decimal ValueAnalog
        {
            get { return valueAnalog; }
            set 
            { 
                valueAnalog = value;
                OnPropertyChanged("ValueAnalog");
            }
        }

        /// <summary>
        /// Gets or sets the duration of the steps to add.
        /// </summary>
        /// <value>
        /// The duration of the steps to add.
        /// </value>
        public decimal Duration
        {
            get { return duration; }
            set 
            { 
                duration = value;
                OnPropertyChanged("Duration");
            }
        }


        /// <summary>
        /// Gets the command triggered when the add button is clicked.
        /// </summary>
        /// <value>
        /// The command triggered when the add button is clicked.
        /// </value>
        public RelayCommand AddCommand
        {
            get 
            {
                if (addCommand == null)
                    addCommand = new RelayCommand(AddSteps);

                return addCommand; 
            }

        }

        /// <summary>
        /// Gets the command triggered when the close button is clicked.
        /// </summary>
        /// <value>
        /// The command triggered when the close button is clicked.
        /// </value>
        public RelayCommand CloseCommand
        {
            get 
            {
                if (closeCommand == null)
                    closeCommand = new RelayCommand(Close);
                return closeCommand; 
            }

        }


        /// <summary>
        /// Gets the command triggered when a digital card is selected.
        /// </summary>
        /// <value>
        /// The command triggered when a digital card is selected.
        /// </value>
        public RelayCommand DigitalSelectedCommand
        {
            get 
            {
                if (digitalSelectedCommand == null)
                    digitalSelectedCommand = new RelayCommand(param => ChangeStepType(false));

                return digitalSelectedCommand; 
            }

        }


        /// <summary>
        /// Gets the command triggered when an analog card is selected.
        /// </summary>
        /// <value>
        /// The command triggered when an analog card is selected.
        /// </value>
        public RelayCommand AnalogSelectedCommand
        {
            get 
            {
                if (analogSelectedCommand == null)
                    analogSelectedCommand = new RelayCommand(param => ChangeStepType(true));

                return analogSelectedCommand; 
            }

        }


        /// <summary>
        /// Gets or sets the TreeView controller.
        /// </summary>
        /// <value>
        /// The TreeView controller.
        /// </value>
        public CTVViewModel TreeViewController
        {
            get 
            { 
                return treeViewController; 
            }
            set 
            { 

                treeViewController = value;
                OnPropertyChanged("TreeViewController");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepBatchAdderController"/> class.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="treeViewController">The tree view controller.</param>
        public StepBatchAdderController(RootController root, CTVViewModel treeViewController)
        {
            this.rootController = root;
            this.treeViewController = treeViewController;
        }

        /// <summary>
        /// Changes the type of the step.
        /// </summary>
        /// <param name="isAnalog">if set to <c>true</c> then the step will become analog.</param>
        public void ChangeStepType(bool isAnalog)
        {
            isStepTypeAnalog = isAnalog;
            TreeViewController = ModelBasedCTVBuilder.Build(rootController, !isAnalog);
        }

        /// <summary>
        /// Closes the window
        /// </summary>
        /// <param name="parameter">The user control.</param>
        public void Close(object parameter)
        {
            UserControl uc = (UserControl)parameter;
            Window w = Window.GetWindow(uc);
            w.Close();
        }

        /// <summary>
        /// Adds steps to the specified channels.
        /// </summary>
        /// <param name="parameter">Not used</param>
        public void AddSteps(object parameter)
        {
            double valueToAdd;

            if(isStepTypeAnalog)
                valueToAdd = (double)ValueAnalog;
            else
                valueToAdd = (double)ValueDigitalIndex;

            ChannelBasicController current = null;
            int counter = 0;

            foreach (CTVItemViewModel checkedItem in treeViewController.GetCheckedLeaves())
            {
                current = (ChannelBasicController)(checkedItem as CheckableTVItemController).Item;
                current.InsertConstantStepAtBeginning(valueToAdd, (double)Duration);
                counter++;
            }

            if (counter > 0)//At least one added
            {
                rootController.CopyToBuffer();
                MessageBox.Show(String.Format("{0} steps were added to the beginning of the first sequence.", counter), "Steps Added Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("No steps were added as no channels are selected. Make sure that at least one channel is selected!", "Steps Not Added", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
