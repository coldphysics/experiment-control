using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Controller.Settings.Settings;
using Controller.Settings.Settings.AbstractSettingControllers;
using Model.Settings;

namespace Controller.Settings
{
    /// <summary>
    /// The controller for a <see cref="Profile"/>
    /// </summary>
    /// <seealso cref="Prototyping.Controller.BaseController" />
    public class ProfileController : BaseController, IDataErrorInfo
    {
        /// <summary>
        /// The profile
        /// </summary>
        private Profile profile;

        /// <summary>
        /// Gets the profile.
        /// </summary>
        /// <value>
        /// The profile.
        /// </value>
        public Profile Profile
        {
            get { return profile; }
        }

        /// <summary>
        /// Gets or sets the collection of associated <see cref="SettingController"/>.
        /// </summary>
        /// <value>
        /// The collection of associated <see cref="SettingController"/>..
        /// </value>
        public ICollection<SettingController> Settings
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get
            {
                return profile.Name;
            }

            set
            {
                profile.Name = value;
                OnPropertyChanged("Name");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileController"/> class.
        /// </summary>
        /// <param name="profile">The profile.</param>
        public ProfileController(Profile profile)
        {
            Settings = new ObservableCollection<SettingController>();
            this.profile = profile;
        }

        /// <summary>
        /// Restores the default values for the settings of the profile.
        /// </summary>
        public void RestoreDefaultValues()
        {
            foreach (SettingController controller in Settings)
                controller.RestoreDefaults();
        }

        /// <summary>
        /// Gets a flat collection of all settings (and nested settings) controllers associated with this profile.
        /// </summary>
        /// <returns></returns>
        public ICollection<SettingController> GetAllSettings()
        {
            ICollection<SettingController> result = new List<SettingController>();

            foreach (SettingController current in Settings)
            {
                result.Add(current);

                if (current is ISettingWithChildrenController)
                {
                    (current as ISettingWithChildrenController).TraverseChildren(result);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if all settings return true;
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid()
        {
            if(this["Name"] != "")
                return false;

            foreach (SettingController setting in Settings)
                if (!setting.IsValid())
                    return false;

            return true;

        }


        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <exception cref="System.NotImplementedException">Always thrown!</exception>
        public string Error
        {
            get { throw new System.NotImplementedException(); }
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>The potential error message.</returns>
        public string this[string columnName]
        {
            get 
            {
                string result = "";

                switch (columnName)
                {
                    case "Name":
                        if (string.IsNullOrEmpty(Name))
                            result = "The Name of the profile cannot be empty";
                        break;
                }

                return result;
            }
        }


    }
}
