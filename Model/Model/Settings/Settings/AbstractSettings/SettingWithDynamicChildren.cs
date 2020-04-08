using System;
using System.Collections.Generic;

namespace Model.Settings.Settings
{
    [Serializable]
    /// <summary>
    /// A base class for setting classes that have child settings
    /// </summary>
    /// <remarks>For each value of this setting, there can be a different collection of child settings.</remarks>
    /// <typeparam name="T">The type of the value of this setting</typeparam>
    /// <seealso cref="Model.Settings.Settings.Setting{T}" />
    /// <seealso cref="Model.Settings.Settings.ISettingWithChildren" />
    /// 
    public abstract class SettingWithDynamicChildren<T> : Setting<T>, ISettingWithChildren
    {
        /// <summary>
        /// The collection of related child settings
        /// </summary>
        private ICollection<BasicSetting> relatedSettings;
        /// <summary>
        /// A dictionary that maps a value of the current setting with a collection of child settings.
        /// </summary>
        private Dictionary<T, ICollection<BasicSetting>> relatedSettingsDictionary;

        /// <summary>
        /// Gets the related settings.
        /// </summary>
        /// <value>
        /// The related settings.
        /// </value>
        public ICollection<BasicSetting> RelatedOptions
        {
            get { return relatedSettings; }
        }

        /// <summary>
        /// Gets or sets the value of the setting.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <remarks>When the value changes the <see cref="relatedSettingsDictionary"/> is checked and the set of current child settings is changed accordingly.</remarks>
        public override T Value
        {
            set
            {
                if (relatedSettingsDictionary != null)
                {
                    ICollection<BasicSetting> newRelatedSettings = null;

                    if(relatedSettingsDictionary.ContainsKey(value))
                        newRelatedSettings  = relatedSettingsDictionary[value];

                    this.relatedSettings = newRelatedSettings;
                }

                base.Value = value;
            }

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingWithDynamicChildren{T}"/> class.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="relatedSettingsDictionary">The related settings dictionary.</param>
        public SettingWithDynamicChildren(string name, T defaultValue, Dictionary<T, ICollection<BasicSetting>> relatedSettingsDictionary)
            : base(name, defaultValue)
        {
            this.relatedSettingsDictionary = relatedSettingsDictionary;
        }

        /// <summary>
        /// Finds a child setting based on its name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns></returns>
        public BasicSetting GetChildOptionByName(string settingName)
        {
            return SettingsCollectionSearcher.FindSettingByName(settingName, RelatedOptions);
        }

        /// <summary>
        /// Replaces the setting with the given name with a new setting
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>
        ///   <c>true</c> if the option was found and replaced.
        /// </returns>
        public bool ReplaceChildSettingByName(string settingName, BasicSetting newValue)
        {
            return SettingsCollectionSearcher.ReplaceSettingByName(settingName, newValue, RelatedOptions);
        }

        /// <summary>
        /// Traverses the children.
        /// </summary>
        /// <param name="result">The cumulative result.</param>
        public void TraverseChildren(ICollection<BasicSetting> result)
        {
            SettingsCollectionSearcher.TraversAllSettings(result, RelatedOptions);
        }

        /// <summary>
        /// Restores the default value for the setting and all of its children.
        /// </summary>
        public override void RestoreDefaults()
        {
            base.RestoreDefaults();

            if (relatedSettingsDictionary != null)
            {
                foreach (T key in relatedSettingsDictionary.Keys)
                {
                    foreach (BasicSetting setting in relatedSettingsDictionary[key])
                        setting.RestoreDefaults();
                }
            }
        }

    }
}
