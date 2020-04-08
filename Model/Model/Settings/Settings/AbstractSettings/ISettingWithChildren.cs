using System.Collections.Generic;

namespace Model.Settings.Settings
{
    /// <summary>
    /// Describes a setting that has related child settings attached to it.
    /// </summary>
    public interface ISettingWithChildren
    {
        /// <summary>
        /// Gets the related settings.
        /// </summary>
        /// <value>
        /// The related settings.
        /// </value>
        ICollection<BasicSetting> RelatedOptions
        {
            get;
        }

        /// <summary>
        /// Finds a child setting based on its name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <returns></returns>
        BasicSetting GetChildOptionByName(string settingName);


        /// <summary>
        /// Traverses the children.
        /// </summary>
        /// <param name="settings">The collection of all settings.</param>
        void TraverseChildren(ICollection<BasicSetting> settings);

        /// <summary>
        /// Replaces the setting with the given name with a new setting
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns><c>true</c> if the option was found and replaced.</returns>
        bool ReplaceChildSettingByName(string settingName, BasicSetting newValue);
    }
}
