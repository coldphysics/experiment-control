using System;

namespace Model.Settings.Settings
{
    /// <summary>
    /// A base class for all numeric-based settings
    /// </summary>
    /// <typeparam name="T">A numeric type</typeparam>
    /// <seealso cref="Model.Settings.Settings.Setting{T}" />
    [Serializable]
    public abstract class NumericSetting<T> : Setting<T> where T : IComparable<T>
    {
        /// <summary>
        /// The minimum allowed value
        /// </summary>
        public readonly T MINIMUM_VALUE;
        /// <summary>
        /// The maximum allowed value
        /// </summary>
        public readonly T MAXIMUM_VALUE;
        /// <summary>
        /// The unit of measurement for the setting
        /// </summary>
        private string unit;

        /// <summary>
        /// Gets or sets the unit of measurement.
        /// </summary>
        /// <value>
        /// The unit.
        /// </value>
        public string Unit
        {
            get { return unit; }
            protected set { unit = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericSetting{T}"/> class.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="minimumValue">The minimum value.</param>
        /// <param name="maximumValue">The maximum value.</param>
        /// <param name="unit">The unit.</param>
        public NumericSetting(string name, T defaultValue, T minimumValue, T maximumValue, string unit)
            : base(name, defaultValue)
        {
            this.MINIMUM_VALUE = minimumValue;
            this.MAXIMUM_VALUE = maximumValue;
            this.unit = unit;
        }

        /// <summary>
        /// Gets or sets the value of the setting.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <remarks>It ensures that the new value is in the allowed range. It does not change it otherwise.</remarks>
        public override T Value
        {
            set
            {
                if (value.CompareTo(MINIMUM_VALUE) >= 0 && value.CompareTo(MAXIMUM_VALUE) <= 0)
                    base.Value = value;
            }
        }



    }
}
