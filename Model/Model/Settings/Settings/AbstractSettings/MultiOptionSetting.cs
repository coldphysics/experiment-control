using System;
using System.Collections.Generic;

namespace Model.Settings.Settings
{
    /// <summary>
    /// A base class for settings that have a predetermined set of values (other than the boolean settings)
    /// </summary>
    /// <typeparam name="T">The type of values stored by this setting</typeparam>
    /// <seealso cref="Model.Settings.Settings.SettingWithDynamicChildren{T}" />
    [Serializable]
    public abstract class MultiOptionSetting<T> : SettingWithDynamicChildren<T> 
    {
        /// <summary>
        /// The predetermined possible options for the value of the setting.
        /// </summary>
        private T[] options;

        /// <summary>
        /// Gets the predetermined possible options for the value of the setting.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public T[] Options
        {
            get { return options; }
        }

        /// <summary>
        /// Gets or sets the value of the setting.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <remarks>
        /// It ensures that the new value is one of the allowed options.
        /// When the value changes the <see cref="relatedSettingsDictionary" /> is checked and the set of current child settings is changed accordingly.
        /// </remarks>
        public override T Value
        {
            set
            {
                foreach(T option in options)
                    if (value.Equals(option))
                    {
                        base.Value = value;
                        break;
                    }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiOptionSetting{T}"/> class.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="options">The options for the value.</param>
        /// <param name="relatedSettingsDictionary">The related settings dictionary.</param>
        public MultiOptionSetting(string name, T defaultValue, T[] options, Dictionary<T, ICollection<BasicSetting>> relatedSettingsDictionary)
            : base(name, defaultValue, relatedSettingsDictionary)
        {
            this.options = options;
            Value = defaultValue;//Necessary to activate the overridden Value property if needed!
        }

    }
}
