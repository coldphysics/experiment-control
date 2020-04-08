using System;
using System.Collections.Generic;

namespace Model.Settings.Settings
{
    /// <summary>
    /// A setting that allows a predetermined set of string options for its value
    /// </summary>
    /// <seealso cref="Model.Settings.Settings.MultiOptionSetting{System.String}" />
    [Serializable]
    public class StringMultiOptionSetting:MultiOptionSetting<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringMultiOptionSetting"/> class.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="options">The options for the value.</param>
        /// <param name="relatedSettingsDictionary">The dictionary for the child settings.</param>
        public StringMultiOptionSetting(string name, string defaultValue, string[] options, Dictionary<string, ICollection<BasicSetting>> relatedSettingsDictionary)
            : base(name, defaultValue, options, relatedSettingsDictionary)
        {
 
        }
    }
}
