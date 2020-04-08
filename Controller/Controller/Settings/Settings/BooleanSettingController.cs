using Model.Settings.Settings;

namespace Controller.Settings.Settings
{
    /// <summary>
    /// A controller for a <see cref="BooleanSetting"/>
    /// </summary>
    /// <seealso cref="Prototyping.Controller.Settings.SettingWithChildrenController{System.Boolean}" />
    public class BooleanSettingController : SettingWithChildrenController<bool>
    {
        /// <summary>
        /// Gets the setting.
        /// </summary>
        /// <value>
        /// The setting.
        /// </value>
        private BooleanSetting Setting
        {
            get
            {
                return (BooleanSetting)setting;
            }
        }

        /// <summary>
        /// Gets or sets a value of the setting
        /// </summary>
        /// <value>
        ///   The value of the setting
        /// </value>
        public bool Value
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
        /// Initializes a new instance of the <see cref="BooleanSettingController"/> class.
        /// </summary>
        /// <param name="setting">The setting.</param>
        public BooleanSettingController(BooleanSetting setting)
            : base(setting)
        { }
    }
}
