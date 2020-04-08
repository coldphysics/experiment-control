using Model.Settings.Settings;

namespace Controller.Settings.Settings
{
    /// <summary>
    /// The controller for a <see cref="StringMultiOptionSetting"/>
    /// </summary>
    /// <seealso cref="Prototyping.Controller.Settings.SettingWithChildrenController{System.String}" />
    public class StringMultiOptionSettingController:SettingWithChildrenController<string>
    {
        /// <summary>
        /// Gets the setting.
        /// </summary>
        /// <value>
        /// The setting.
        /// </value>
        private StringMultiOptionSetting Setting
        {
            get
            {
                return (StringMultiOptionSetting)setting;
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
            set
            {
                Setting.Value = value;
                OnPropertyChanged("Value");
                OnPropertyChanged("ChildSettings");
            }
            get
            {
                return Setting.Value;
            }
        }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        public string[] Options
        {
            get
            {
                return Setting.Options;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringMultiOptionSettingController"/> class.
        /// </summary>
        /// <param name="setting">The setting.</param>
        public StringMultiOptionSettingController(StringMultiOptionSetting setting)
            : base(setting)
        { }
    }
}
