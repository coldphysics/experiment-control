using Model.Settings.Settings;
using System.Collections.Generic;

namespace Model.Settings.Builders
{
    /// <summary>
    /// Builds a <see cref="StringMultiOptionSetting"/> instance
    /// </summary>
    /// <seealso cref="Prototyping.Model.Builders.SettingWithChildrenBuilder{System.String}" />
    class StringMultiOptionSettingBuilder:SettingWithChildrenBuilder<string>
    {
        /// <summary>
        /// The options allowed for the setting
        /// </summary>
        private string[] options;

        /// <summary>
        /// Sets the options allowed for the setting.
        /// </summary>
        /// <param name="options">The options.</param>
        public void SetOptions(string[] options)
        {
            this.options = options;
        }


        /// <summary>
        /// Gets the result of the building process.
        /// </summary>
        /// <returns>The built setting instance.</returns>
        public StringMultiOptionSetting GetResult()
        {
            Dictionary<string, ICollection<BasicSetting>> relatedSettings = GetRelatedSettingsDictionary();
            StringMultiOptionSetting result = new StringMultiOptionSetting(name, defaultValue, options, relatedSettings);

            return result;
        }
    }
}
