using System;

namespace Model.Settings.Settings
{
    /// <summary>
    /// A string setting
    /// </summary>
    /// <seealso cref="Model.Settings.Settings.Setting{System.String}" />
    [Serializable]
    public class StringSetting : Setting<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringSetting"/> class.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value of the setting.</param>
        public StringSetting(string name, string defaultValue)
            : base(name, defaultValue)
        {
            CanValueBeEmpty = true;
        }

        /// <summary>
        /// Indicates whether it is allowed for the value of this setting to be empty. This is used for validation.
        /// </summary>
        private bool canValueBeEmpty;

        /// <summary>
        /// Gets or sets a value indicating whether this instance can value be empty.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance can value be empty; otherwise, <c>false</c>.
        /// </value>
        public bool CanValueBeEmpty
        {
            get { return canValueBeEmpty; }
            set { canValueBeEmpty = value; }
        }
    }
}
