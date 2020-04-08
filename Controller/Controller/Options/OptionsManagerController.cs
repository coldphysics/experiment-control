using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Communication.Commands;
using Model.Options;

namespace Controller.Options
{
    public class OptionsManagerController : BaseController, IDataErrorInfo
    {
        public enum OptionsManagerResult
        {
            CANCEL_OR_CLOSE,
            SAVE_NO_RESTART,
            SAVE_RESTART
        }


        public OptionsManagerResult Result
        {
            set;
            get;
        }

        public OptionsManager OptionsCopy
        {
            get
            {
                return optionsManager;
            }
        }



        /// <summary>
        /// The profile manager is verifying settings of the active profile.
        /// </summary>
        private bool isVerifyingSettings;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is verifying the settings.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is verifying settings; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This property is used to show the "please wait" dialog while verifying the settings.
        /// </remarks>
        public bool IsVerifyingSettings
        {
            set
            {
                isVerifyingSettings = value;
                OnPropertyChanged("IsVerifyingSettings");
            }
            get
            {
                return isVerifyingSettings;
            }
        }

        private OptionsManager originalOptionsManager;

        /// <summary>
        /// The profile manager
        /// </summary>
        private OptionsManager optionsManager;
        /// <summary>
        /// The collection of <see cref="ProfileController"/>'s
        /// </summary>
        private ICollection<OptionsGroupController> optionGroups;


        /// <summary>
        /// The command to be executed to close the ProfilesManager
        /// </summary>
        private RelayCommand closeCommand;
        /// <summary>
        /// The restore defaults command
        /// </summary>
        private RelayCommand restoreDefaultsCommand;
        /// <summary>
        /// The save command
        /// </summary>
        private RelayCommand saveCommand;


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
                if (saveCommand == null)
                    saveCommand = new RelayCommand(param => Save(param), param => IsValid);
                return saveCommand;
            }

        }

        /// <summary>
        /// Gets the restore defaults command.
        /// </summary>
        /// <value>
        /// The restore defaults command.
        /// </value>
        public RelayCommand RestoreDefaultsCommand
        {
            get
            {
                if (restoreDefaultsCommand == null)
                    restoreDefaultsCommand = new RelayCommand(RestoreDefaultValuesForOptions);

                return restoreDefaultsCommand;
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
                if (closeCommand == null)
                    closeCommand = new RelayCommand(Close);

                return closeCommand;
            }

        }


        /// <summary>
        /// Gets or sets the collection of profile controllers
        /// </summary>
        /// <value>
        /// The profile controllers.
        /// </value>
        public ICollection<OptionsGroupController> OptionGroups
        {
            get
            {
                return this.optionGroups;
            }
            set
            {
                this.optionGroups = value;
            }
        }

        /// <summary>
        /// Gets the model of the profiles
        /// </summary>
        /// <value>
        /// The profiles model.
        /// </value>
        public ICollection<OptionsGroup> OptionGroupsModel
        {
            get
            {
                return optionsManager.OptionGroups;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileManagerController"/> class.
        /// </summary>
        /// <param name="manager">The profile manager.</param>
        public OptionsManagerController(OptionsManager manager)
        {
            OptionGroups = new ObservableCollection<OptionsGroupController>();
            this.originalOptionsManager = manager;
            this.optionsManager = this.originalOptionsManager.DeepCopyOptionsManager();
            this.Result = OptionsManagerResult.CANCEL_OR_CLOSE;
        }

        /// <summary>
        /// Closes the profiles manager window.
        /// </summary>
        /// <param name="parameter">The user control that represents the profiles manager.</param>
        private void Close(object parameter)
        {
            UserControl uc = (UserControl)parameter;
            Window w = Window.GetWindow(uc);
            w.Close();

        }

        /// <summary>
        /// Saves the changes made in the profiles manager window
        /// </summary>
        /// <param name="parameter">Not used.</param>
        private void Save(object parameter)
        {

            BackgroundWorker worker = new BackgroundWorker();

            //this is where the long running process should go
            worker.DoWork += (o, ea) =>
            {
                string validationResult = DeepValidate();
                ea.Result = validationResult;
            };

            worker.RunWorkerCompleted += (o, ea) =>
            {
                IsVerifyingSettings = false;
                string validationResult = (string)ea.Result;

                if (validationResult != "")
                {
                    MessageBox.Show(validationResult, "Cannot Save the Options", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    if (originalOptionsManager.IsRestartRequired(optionsManager.OptionGroups))
                    {
                        MessageBoxResult mbResult = MessageBox.Show("The changes you have made require the program to restart. " +
                        "Do you still want to apply them? If you click No, the changes will be discarded!", "Restart Required", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);

                        switch (mbResult)
                        {
                            case MessageBoxResult.Yes:
                                Result = OptionsManagerResult.SAVE_RESTART;
                                Close(parameter);
                                break;
                            case MessageBoxResult.No:
                                Result = OptionsManagerResult.CANCEL_OR_CLOSE;
                                Close(parameter);
                                break;
                            case MessageBoxResult.Cancel:
                                //Do nothing, stay on the same window.
                                break;
                        }

                    }
                    else
                    {
                        Result = OptionsManagerResult.SAVE_NO_RESTART;
                        Close(parameter);
                    }

                }

            };
            //set the IsBusy before you start the thread
            IsVerifyingSettings = true;
            worker.RunWorkerAsync();

        }

        /// <summary>
        /// Restores the default values for all options.
        /// </summary>
        /// <param name="param">Not used</param>
        private void RestoreDefaultValuesForOptions(object param)
        {
            string message = "Are you sure you want to restore the default values for all options?";
            MessageBoxResult result = MessageBox.Show(message, "Restore to Defaults Verification", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                RestoreDefaults();
        }

        /// <summary>
        /// Returns true if there exists an active profile and it has all settings without validation errors.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get
            {
                foreach (OptionsGroupController group in OptionGroups)
                    if (!group.IsValid())
                        return false;
                return true;
            }
        }

        private void RestoreDefaults()
        {
            foreach (OptionsGroupController group in optionGroups)
            {
                group.RestoreDefaultValues();
            }
        }

        /// <summary>
        /// Validates settings after clicking on the save button. The invalidities that are caught here are the ones not being checked by the IDataErrorInfo interface 
        /// </summary>
        /// <returns></returns>
        private string DeepValidate()
        {
            return "";
        }

        #region IDataErrorInfo Members

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
            get
            {
                string result = "";

                return result;
            }
        }

        #endregion
    }
}
