using Model.Settings.Settings;
using System.Collections.Generic;

namespace Model.Settings.Builders
{
    /// <summary>
    /// Builds an instance of <see cref="BooleanSetting"/>
    /// </summary>
    /// <seealso cref="Prototyping.Model.Builders.SettingWithChildrenBuilder{System.Boolean}" />
    class BooleanSettingBuilder:SettingWithChildrenBuilder<bool>
    {
        /// <summary>
        /// Gets the result of the building process.
        /// </summary>
        /// <returns>The built instance</returns>
        public BooleanSetting GetResult()
        {
            Dictionary<bool, ICollection<BasicSetting>> relatedSettings = GetRelatedSettingsDictionary();
            BooleanSetting result = new BooleanSetting(name, defaultValue, relatedSettings);

            return result;
        }
    }
}
