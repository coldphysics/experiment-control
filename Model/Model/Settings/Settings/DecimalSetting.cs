using System;

namespace Model.Settings.Settings
{
    /// <summary>
    /// A decimal setting
    /// </summary>
    [Serializable]
    public class DecimalSetting:NumericSetting<decimal>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecimalSetting"/> class.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="minimumValue">The minimum value.</param>
        /// <param name="maximumValue">The maximum value.</param>
        /// <param name="unit">The unit.</param>
        public DecimalSetting(string name, decimal defaultValue, decimal minimumValue, decimal maximumValue, string unit)
            :base(name, defaultValue, minimumValue, maximumValue, unit)
        { }
    }
}
