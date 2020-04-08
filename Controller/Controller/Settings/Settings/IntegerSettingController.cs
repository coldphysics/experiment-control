using Model.Settings.Settings;


namespace Controller.Settings.Settings
{
    /// <summary>
    /// The controller for an <see cref="IntegerSetting"/>
    /// </summary>
    /// <seealso cref="Prototyping.Controller.Settings.SettingController" />
    public class IntegerSettingController:SettingController
    {
        /// <summary>
        /// Gets the setting.
        /// </summary>
        /// <value>
        /// The setting.
        /// </value>
        private IntegerSetting Setting
        {
            get
            {
                return (IntegerSetting)setting;
            }
        }

        /// <summary>
        /// Gets or sets the value of the setting.
        /// </summary>
        /// <value>
        /// The value of the setting.
        /// </value>
        public int Value
        {
            get
            {
                return Setting.Value;
            }

            set
            {
                Setting.Value = value;
                OnPropertyChanged("Value");
            }
        }

        /// <summary>
        /// Gets the unit.
        /// </summary>
        /// <value>
        /// The unit.
        /// </value>
        public string Unit
        {
            get
            {
                return Setting.Unit;
            }

        }

        /// <summary>
        /// Gets the minimum value.
        /// </summary>
        /// <value>
        /// The minimum value.
        /// </value>
        public int MinimumValue
        {
            get
            {
                return Setting.MINIMUM_VALUE;
            }
        }

        /// <summary>
        /// Gets the maximum value.
        /// </summary>
        /// <value>
        /// The maximum value.
        /// </value>
        public int MaximumValue
        {
            get
            {
                return Setting.MAXIMUM_VALUE;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerSettingController"/> class.
        /// </summary>
        /// <param name="setting">The setting.</param>
        public IntegerSettingController(IntegerSetting setting)
            :base(setting)
        { }
    }
}
