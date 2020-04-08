using Model.Settings.Settings;
using System.Collections.Generic;

namespace Model.Settings
{
    /// <summary>
    /// A helper class that allows searching for a specific setting by its name in a collection of settings. 
    /// It allows searching for settings that are nested in other settings.
    /// </summary>
    public class SettingsCollectionSearcher
    {
        /// <summary>
        /// Finds setting by its name.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settings">The collection of settings to search.</param>
        /// <returns>The setting whose name is specified, or <c>null</c> if it is not found.</returns>
        public static BasicSetting FindSettingByName(string settingName, ICollection<BasicSetting> settings)
        {
            BasicSetting temp;
            if (settings != null)
            {
                foreach (BasicSetting setting in settings)
                {
                    if (setting.NAME == settingName)
                        return setting;

                    if (setting is ISettingWithChildren)
                    {
                        temp = ((ISettingWithChildren)setting).GetChildOptionByName(settingName);

                        if (temp != null)
                            return temp;
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Traverses all settings in a nested settings structure.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="settings">The initial settings collection (non-flat).</param>
        public static void TraversAllSettings(ICollection<BasicSetting> result, ICollection<BasicSetting> settings)
        {
            if (settings != null)
            {
                foreach (BasicSetting setting in settings)
                {
                    result.Add(setting);

                    if (setting is ISettingWithChildren)
                    {
                        ((ISettingWithChildren)setting).TraverseChildren(result);
                    }
                }
            }
        }


        /// <summary>
        /// Replaces the specified setting by another one.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="newValue">The new value.</param>
        /// <param name="settings">The settings collection.</param>
        /// <returns><c>true</c> if the option was found and replaced.</returns>
        public static bool ReplaceSettingByName(string settingName, BasicSetting newValue, ICollection<BasicSetting> settings)
        {
            BasicSetting directChild = null;
            
            if (settings != null)
            {
                bool isDirectChild = false;

                foreach (BasicSetting setting in settings)
                {
                    if (setting.NAME == settingName)
                    {
                        directChild = setting;
                        isDirectChild = true;
                        break;
                    }
                    

                    if (setting is ISettingWithChildren)
                    {
                        if (((ISettingWithChildren)setting).ReplaceChildSettingByName(settingName, newValue))
                        {
                            return true;
                        }
                    }
                }

                if (isDirectChild)
                {
                    settings.Remove(directChild);
                    settings.Add( newValue);

                    return true;
                }
            }

            return false;
        }
    }
}
