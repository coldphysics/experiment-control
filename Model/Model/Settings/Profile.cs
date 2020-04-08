using Model.Settings.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Model.Settings
{
    /// <summary>
    /// A profile of settings. A profile best suits a specific experiment.
    /// </summary>
    [Serializable]
    public class Profile
    {
        /// <summary>
        /// The collection of settings attached to the profile
        /// </summary>
        private ICollection<BasicSetting> settings;

        /// <summary>
        /// The name of the profile
        /// </summary>
        private string name;

        /// <summary>
        /// Gets or sets the name of the profile.
        /// </summary>
        /// <value>
        /// The name of the profile.
        /// </value>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Gets the settings attached to the profile.
        /// </summary>
        /// <value>
        /// The settings attached to the profile.
        /// </value>
        public ICollection<BasicSetting> Settings
        {
            get 
            { 
                return settings; 
            }

            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Profile"/> class.
        /// </summary>
        public Profile()
            : this(new ObservableCollection<BasicSetting>(), "")
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Profile"/> class.
        /// </summary>
        /// <param name="settings">The settings attached to the profile.</param>
        /// <param name="name">The name of the profile.</param>
        public Profile(ICollection<BasicSetting> settings, string name)
        {
            this.settings = settings;
            this.name = name;
        }

        /// <summary>
        /// Gets a setting by its name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns>The child setting whose name is specified.</returns>
        public BasicSetting GetSettingByName(string settingName)
        {
            return SettingsCollectionSearcher.FindSettingByName(settingName, Settings);
        }

        // <summary>
        // Gets the value of the specified setting.
        // </summary>
        // <typeparam name="T">The type of the value of the setting</typeparam>
        // <param name="settingName">Name of the setting.</param>
        // <returns>The value of the setting</returns>

        public T GetSettingValueByName<T>(string settingName)
        {
            Setting<T> setting = (Setting<T>)GetSettingByName(settingName);
            if (setting != null)
            {
                return setting.Value;
            }

            return default(T);
        }

        /// <summary>
        /// Indicates whether the specified setting exist in the profile
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns<c>true</c> if the specified setting exist, <c>false</c> otherwise.</returns>
        public bool DoesSettingExist(string settingName)
        {
            return GetSettingByName(settingName) != null;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }


        /// <summary>
        /// Determines whether a restart of the program is required due to a change in the settings of this profile.
        /// </summary>
        /// <param name="otherProfile">The other profile.</param>
        /// <returns><c>true</c> if a restart is required.</returns>
        public bool IsRestartRequired(Profile otherProfile)
        {
            ICollection<BasicSetting> mySettings = new List<BasicSetting>();
            SettingsCollectionSearcher.TraversAllSettings(mySettings, settings);//mySettings: flattened, settings: not flattened
            BasicSetting other;

            foreach (BasicSetting setting in mySettings)
            {
                if (setting.RequiresRestart)
                {
                    other = otherProfile.GetSettingByName(setting.NAME);

                    if (other != null)
                    {
                        if (setting.HasValueChanged(other))
                        {
                            return true;
                        }
                    }
                    else//A setting that requires restart has been introduced newly
                    {
                        return true;
                    }
                }
            }


            return false;
        }
    }
}
