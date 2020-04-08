using System;
using System.Collections.Generic;

namespace Model.Settings.Settings
{
    /// <summary>
    /// A boolean setting
    /// </summary>
    /// <seealso cref="Model.Settings.Settings.SettingWithDynamicChildren{System.Boolean}" />
    [Serializable]
    public class BooleanSetting : SettingWithDynamicChildren<bool>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanSetting"/> class.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="relatedSettingsDictionary">The related settings dictionary.</param>
        public BooleanSetting(string name, bool defaultValue, Dictionary<bool, ICollection<BasicSetting>> relatedSettingsDictionary)
            : base(name, defaultValue, relatedSettingsDictionary)
        {
            Value = defaultValue;//Necessary to activate the overridden Value property if needed!
        }
    }
}
