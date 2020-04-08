using System;

namespace Model.Settings.Settings
{
    /// <summary>
    /// A generic setting whose value could be of any type. Used as a parent class only.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Model.Settings.Settings.BasicSetting" />
    [Serializable]
    public abstract class Setting<T>:BasicSetting
    {

        /// <summary>
        /// The default value of the value stored by this object.
        /// </summary>
        private T defaultValue;

        /// <summary>
        /// The value of the current setting
        /// </summary>
        private T value;

        /// <summary>
        /// Gets the name of the setting.
        /// </summary>
        /// <value>
        /// The name of the setting.
        /// </value>
        public string Name
        {
            get { return NAME; }
        }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public T DefaultValue
        {
            get { return defaultValue; }
            set { defaultValue = value; }
        }

        /// <summary>
        /// Gets or sets the value of the setting.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public virtual T Value
        {
            set
            {

                this.value = value;
            }
            get
            {
                return this.value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Setting{T}"/> class.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value of the setting.</param>
        public Setting(string name, T defaultValue)
            :base(name)
        {
            this.defaultValue = defaultValue;
            value = defaultValue;
        }


        /// <summary>
        /// Restores the default value for the setting.
        /// </summary>
        public override void RestoreDefaults()
        {
            Value = DefaultValue;
        }


        /// <summary>
        /// Determines whether the value is different from another setting.
        /// </summary>
        /// <param name="other">The other setting.</param>
        /// <returns>
        ///   <c>true</c> when the value is different between the two settings.
        /// </returns>
        public override bool HasValueChanged(BasicSetting other)
        {
            Setting<T> otherT = (Setting<T>)other;

            return !Value.Equals(otherT.Value);
        }

 
    }
}
