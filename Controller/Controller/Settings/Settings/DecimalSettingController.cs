using Model.Settings.Settings;

namespace Controller.Settings.Settings
{
    /// <summary>
    /// The controller for a <see cref=" DecimalSetting"/>
    /// </summary>
    /// <seealso cref="Controller.Settings.Settings.SettingController" />
    public class DecimalSettingController:SettingController
    {
                /// <summary>
        /// Gets the setting.
        /// </summary>
        /// <value>
        /// The setting.
        /// </value>
        private DecimalSetting Setting
        {
            get
            {
                return (DecimalSetting)setting;
            }
        }

        /// <summary>
        /// Gets or sets the value of the setting.
        /// </summary>
        /// <value>
        /// The value of the setting.
        /// </value>
        public decimal Value
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
        public decimal MinimumValue
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
        public decimal MaximumValue
        {
            get
            {
                return Setting.MAXIMUM_VALUE;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecimalSettingController"/> class.
        /// </summary>
        /// <param name="setting">The setting.</param>
        public DecimalSettingController(DecimalSetting setting)
            :base(setting)
        { }
    }
}
