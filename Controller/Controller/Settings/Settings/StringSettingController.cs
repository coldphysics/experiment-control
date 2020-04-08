using Model.Settings.Settings;

namespace Controller.Settings.Settings
{
    /// <summary>
    /// The controller for a <see cref="StringSetting"/>
    /// </summary>
    /// <seealso cref="Prototyping.Controller.Settings.SettingController" />
    public class StringSettingController : SettingController
    {
        /// <summary>
        /// Gets the setting.
        /// </summary>
        /// <value>
        /// The setting.
        /// </value>
        private StringSetting Setting
        {
            get
            {
                return (StringSetting)setting;
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value
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
        /// Initializes a new instance of the <see cref="StringSettingController"/> class.
        /// </summary>
        /// <param name="setting">The setting.</param>
        public StringSettingController(StringSetting setting)
            : base(setting)
        {
        }

        

    }
}
