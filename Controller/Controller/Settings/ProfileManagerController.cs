using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Communication.Commands;
using Controller.Settings.Settings;
using Microsoft.Win32;
using Model.Settings;

namespace Controller.Settings
{
    /// <summary>
    /// The controller for the Profile Manager window
    /// </summary>
    /// <seealso cref="Prototyping.Controller.BaseController" />
    public class ProfileManagerController : BaseController, IDataErrorInfo
    {
        public enum ProfileManagerResult
        {
            CANCEL_OR_CLOSE,
            SAVE_NO_RESTART,
            SAVE_RESTART
        }


        public ProfileManagerResult Result
        {
            set;
            get;
        }

        public ProfilesManagerSnapshot SettingsSnapshot
        {
            get
            {
                return profilesManager;
            }
        }

        private static bool _isSaveButtonEnabled = true;

        public  bool IsSaveButtonEnabled
        {
            get { return _isSaveButtonEnabled; }
            set
            {
                _isSaveButtonEnabled = value;
            OnPropertyChanged("IsSaveButtonEnabled");
            }
        }
        /// <summary>
        /// The base name for new profiles
        /// </summary>
        private const string NEW_PROFILE_NAME_BASE = "New_Profile_";

        /// <summary>
        /// The template of the name of new profiles e.g., New_Profile_1
        /// </summary>
        private const string NEW_PROFILE_NAME_TEMPLATE = NEW_PROFILE_NAME_BASE + "(\\d*)";


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

        private ProfilesManager originalProfilesManager;

        /// <summary>
        /// The profile manager
        /// </summary>
        private ProfilesManagerSnapshot profilesManager;
        /// <summary>
        /// The collection of <see cref="ProfileController"/>'s
        /// </summary>
        private ICollection<ProfileController> profiles;

        /// <summary>
        /// The command to be executed to remove a profile
        /// </summary>
        private RelayCommand removeProfileCommand;
        /// <summary>
        /// The command to be executed to create a new profile
        /// </summary>
        private RelayCommand createNewProfileCommand;
        /// <summary>
        /// The command to be executed to change the active profile
        /// </summary>
        private RelayCommand setActiveProfileCommand;
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
        /// The export profile command
        /// </summary>
        private RelayCommand exportProfileCommand;
        /// <summary>
        /// The import profile command
        /// </summary>
        private RelayCommand importProfileCommand;



        /// <summary>
        /// Gets or sets the import profile command.
        /// </summary>
        /// <value>
        /// The import profile command.
        /// </value>
        public RelayCommand ImportProfileCommand
        {
            get 
            {
                if (importProfileCommand == null)
                    importProfileCommand = new RelayCommand(param => ImportProfile());

                return importProfileCommand; 
            }
            
        }

        /// <summary>
        /// Gets or sets the export profile command.
        /// </summary>
        /// <value>
        /// The export profile command.
        /// </value>
        public RelayCommand ExportProfileCommand
        {
            get 
            {
                if (exportProfileCommand == null)
                    exportProfileCommand = new RelayCommand(param => ExportProfile((ProfileController)param));
                return exportProfileCommand; 
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
                if (saveCommand == null)
                    saveCommand = new RelayCommand(param =>Save(param), param=>IsValid);
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
                    restoreDefaultsCommand = new RelayCommand(RestoreDefaultValuesForProfile);

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
                if(closeCommand == null)
                    closeCommand = new RelayCommand(Close);

                return closeCommand; 
            }

        }

        /// <summary>
        /// Gets the command to be executed to change the active profile
        /// </summary>
        /// <value>
        /// The command to be executed to change the active profile
        /// </value>
        public RelayCommand SetActiveProfileCommand
        {
            get 
            {
                if (setActiveProfileCommand == null)
                    setActiveProfileCommand = new RelayCommand(SetActiveProfile);
                return setActiveProfileCommand; 
            }

        }

        /// <summary>
        /// Gets the command to be executed to create a new profile
        /// </summary>
        /// <value>
        /// The command to be executed to create a new profile
        /// </value>
        public RelayCommand CreateNewProfileCommand
        {
            get 
            {
                if (createNewProfileCommand == null)
                    createNewProfileCommand = new RelayCommand(param => CreateNewProfile());
                return createNewProfileCommand; 
            }
        }

        /// <summary>
        /// Gets the command to be executed to remove a profile
        /// </summary>
        /// <value>
        /// The command to be executed to remove a profile
        /// </value>
        public RelayCommand RemoveProfileCommand
        {
            get 
            {
                if (removeProfileCommand == null)
                {
                    removeProfileCommand = new RelayCommand(param => RemoveSelectedProfile((ProfileController)param));
                }

                return removeProfileCommand; 
            }

        }

        /// <summary>
        /// Gets or sets the active profile.
        /// </summary>
        /// <value>
        /// The active profile.
        /// </value>
        public ProfileController ActiveProfile
        {
            set
            {
                if (value != null)
                {
                    profilesManager.ActiveProfile = value.Profile;
                    OnPropertyChanged("ActiveProfile");
                }
            }

            get
            {
                //ProfileController active = Profiles.Where(param => param.Profile == profilesManager.ActiveProfile).FirstOrDefault();

                return GetActiveProfileController();
            }
        }


        /// <summary>
        /// Gets the index of the initially selected profile.
        /// </summary>
        /// <value>
        /// The initial selected profile.
        /// </value>
        public int InitialSelectedProfile
        {
            get
            {
                Profile active = profilesManager.ActiveProfile;

                int index = 0;

                foreach (ProfileController controller in Profiles)
                {
                    if (controller.Profile == active)
                        return index;

                    index++;
                }

                return 0;

            }
        }

        /// <summary>
        /// Gets or sets the collection of profile controllers
        /// </summary>
        /// <value>
        /// The profile controllers.
        /// </value>
        public ICollection<ProfileController> Profiles
        {
            get
            {
                return this.profiles;
            }
            set
            {
                this.profiles = value;
            }
        }

        /// <summary>
        /// Gets the model of the profiles
        /// </summary>
        /// <value>
        /// The profiles model.
        /// </value>
        public ICollection<Profile> ProfilesModel
        {
            get
            {
                return profilesManager.Profiles;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileManagerController"/> class.
        /// </summary>
        /// <param name="manager">The profile manager.</param>
        public ProfileManagerController(ProfilesManager manager)
        {
            Profiles = new ObservableCollection<ProfileController>();
            this.originalProfilesManager = manager;
            this.profilesManager = this.originalProfilesManager.CreateSnapshot();
            this.Result = ProfileManagerResult.CANCEL_OR_CLOSE;
        }


        /// <summary>
        /// Removes the selected profile.
        /// </summary>
        /// <param name="toRemove">To remove.</param>
        private void RemoveSelectedProfile(ProfileController toRemove)
        {
            string message = string.Format("Are you sure you want to remove the profile ({0})? This operation cannot be reversed afterwards!", toRemove.Name);
            MessageBoxResult result = MessageBox.Show(message, "Remove Profile Verification", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                PerformProfileRemove(toRemove);
            }
        }

        /// <summary>
        /// Performs the profile remove without waiting a confirmation from the user.
        /// </summary>
        /// <param name="toRemove">The profile to remove.</param>
        private void PerformProfileRemove(ProfileController toRemove)
        {
            Profiles.Remove(toRemove);

            if (profilesManager.ActiveProfile == toRemove.Profile)
            {
                profilesManager.ActiveProfile = null;
                OnPropertyChanged("ActiveProfile");
            }

            profilesManager.Profiles.Remove(toRemove.Profile);

            OnPropertyChanged("Profiles");
        }

        /// <summary>
        /// Shows a dialog to export a profile.
        /// </summary>
        /// <param name="toExport">To export.</param>
        private void ExportProfile(ProfileController toExport)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Profile Files (*.profile)|*.profile";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            saveFileDialog.Title = "Export a Profile";
            saveFileDialog.CheckPathExists = true;
            saveFileDialog.AddExtension = true;


            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                originalProfilesManager.SaveProfileToPath(toExport.Profile, filePath);

                MessageBox.Show("The profile has been exported successfully!", "Successful Export", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Shows a dialog to import a profile.
        /// </summary>
        private void ImportProfile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Profile Files (*.profile)|*.profile";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            openFileDialog.Title = "Import a Profile";
            openFileDialog.CheckPathExists = true;
            openFileDialog.AddExtension = true;

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                Profile importedProfile = originalProfilesManager.LoadProfile(filePath);
                ProfileController toDelete = null;

                foreach (ProfileController controller in profiles)
                {
                    if (importedProfile.Name == controller.Profile.Name)
                    {
                        toDelete = controller;
                        break;
                    }
                }

                if (toDelete != null)
                {
                    MessageBoxResult result = MessageBox.Show("The profile you have selected already exists! Do you want to replace the existing profile with the new one?", "Replacement Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.No)
                        return;

                    PerformProfileRemove(toDelete);
                }

                (profilesManager.Profiles as ObservableCollection<Profile>).Insert(0, importedProfile);
                (Profiles as ObservableCollection<ProfileController>).Insert(0, ControllersBuilder.BuildProfileController(importedProfile));
                //MessageBox.Show("The profile has been imported successfully!", "Successful Operation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Creates a new profile.
        /// </summary>
        /// <remarks>It ensures a new unique name for the profile</remarks>
        private void CreateNewProfile()
        {
            string name = GenerateProfileName();
            Profile generated = profilesManager.CreateNewProfile(name);
            profilesManager.Profiles.Add(generated);
            Profiles.Add(ControllersBuilder.BuildProfileController(generated));

        }

        /// <summary>
        /// Sets the active profile.
        /// </summary>
        /// <param name="param">The parameter.</param>
        private void SetActiveProfile(object param)
        {
            ProfileController profile = (ProfileController)param;
            string message = string.Format("Are you sure you want to set the profile ({0}) as the active profile?", profile.Name);
            MessageBoxResult result = MessageBox.Show(message, "Set Active Profile Verification", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                ActiveProfile = profile;

            OnPropertyChanged("Profiles");//This updates the error view that is activated when no active profile is chosen
        }

        /// <summary>
        /// Generates a unique name of a profile.
        /// </summary>
        /// <returns>A unique name of a profile</returns>
        private string GenerateProfileName()
        {
            List<int> existing = new List<int>();
            Regex matcher = new Regex(NEW_PROFILE_NAME_TEMPLATE, RegexOptions.Compiled);
            Match currentMatch = null;
            int currentNumber;

            foreach (ProfileController p in Profiles)
            {
                currentMatch = matcher.Match(p.Name);

                if (currentMatch.Success)
                {
                    currentNumber = Int32.Parse(currentMatch.Groups[1].Value);
                    existing.Add(currentNumber);
                }
            }

            int myNumber;

            if (existing.Count > 0)
                myNumber = existing.Max() + 1;
            else
                myNumber = 1;

            return String.Format("{0}{1}", NEW_PROFILE_NAME_BASE, myNumber);
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
                    MessageBox.Show(validationResult, "Cannot Save the Settings", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    if (originalProfilesManager.IsRestartReuqired(profilesManager))
                    {
                        MessageBoxResult mbResult = MessageBox.Show("The changes you have made require the program to restart. " +
                        "Do you still want to apply them? If you click No, the changes will be discarded!", "Restart Required", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);

                        switch (mbResult)
                        {
                            case MessageBoxResult.Yes:
                                Result = ProfileManagerResult.SAVE_RESTART;
                                Close(parameter);
                                break;
                            case MessageBoxResult.No:
                                Result = ProfileManagerResult.CANCEL_OR_CLOSE;
                                Close(parameter);
                                break;
                            case MessageBoxResult.Cancel:
                                //Do nothing, stay on the same window.
                                break;
                        }
                        
                    }
                    else
                    {
                        Result = ProfileManagerResult.SAVE_NO_RESTART;
                        Close(parameter);
                    }
                           
                }

            };
            //set the IsBusy before you start the thread
            IsVerifyingSettings = true;
            worker.RunWorkerAsync();
 
        }

        /// <summary>
        /// Searches all profiles controllers for the profile controller that represents the active profile.
        /// </summary>
        /// <returns>The active profile controller</returns>
        private ProfileController GetActiveProfileController()
        {
            Profile active = profilesManager.ActiveProfile;

            foreach (ProfileController controller in Profiles)
            {
                if (controller.Profile == active)
                    return controller;
            }

            return null;
        }

        /// <summary>
        /// Restores the default values for the currently shown profile.
        /// </summary>
        /// <param name="profileParam">The profile.</param>
        private void RestoreDefaultValuesForProfile(object profileParam)
        {
            ProfileController profile = (ProfileController)profileParam;
            string message = string.Format("Are you sure you want to restore the default values for the profile ({0}) ?", profile.Name);
            MessageBoxResult result = MessageBox.Show(message, "Restore to Defaults Verification", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if(result == MessageBoxResult.Yes)       
                profile.RestoreDefaultValues();
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
                if (ActiveProfile == null)
                    return false;
                else
                {
                    return ActiveProfile.IsValid();
                }
            }
        }

        
        /// <summary>
        /// Validates settings after clicking on the save button. The invalidities that are caught here are the ones not being checked by the IDataErrorInfo interface 
        /// </summary>
        /// <returns></returns>
        private string DeepValidate()
        {

            //Checking profile names for empty ones and for duplicates
            List<string> names = new List<string>();
            foreach (ProfileController profile in Profiles)
            {
                if (string.IsNullOrEmpty(profile.Name))
                    return "One or more profiles have empty names. This is not allowed!";

                if (names.Contains(profile.Name))
                    return string.Format("More than one profile has the Name {0}. This is not allowed!", profile.Name);

                names.Add(profile.Name);
            }

            //Checking the database connection
            ICollection<SettingController> allSettings = ActiveProfile.GetAllSettings();

            foreach (SettingController setting in allSettings)
            {
                if (setting is DatabaseConnectionSettingController)
                {
                    DatabaseConnectionSettingController dbSetting = (DatabaseConnectionSettingController)setting;
                    bool isValid;
                    string connectionError = dbSetting.ValidateDatabaseConnection(out isValid);

                    if (!isValid)
                        return connectionError;
                }
            }

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

                switch (columnName)
                {
                    case "Profiles":
                        if (ActiveProfile == null)
                            result = "A profile has to be chosen as active.";
                        break;

                }

                return result;
            }
        }

        #endregion
    }
}
