using Model.Settings;
using Model.Settings.Settings;

namespace Controller.Settings.Settings
{
    /// <summary>
    /// A controller for the <see cref="SampleRateSetting"/>
    /// </summary>
    /// <seealso cref="Prototyping.Controller.Settings.IntegerSettingController" />
    public class SampleRateSettingController : IntegerSettingController
    {
        /// <summary>
        /// The hardware type setting controller
        /// </summary>
        private StringMultiOptionSettingController hwTypeSettingController;

        /// <summary>
        /// Gets the setting.
        /// </summary>
        /// <value>
        /// The setting.
        /// </value>
        private SampleRateSetting Setting
        {
            get
            {
                return (SampleRateSetting)setting;
            }
        }

        /// <summary>
        /// Gets or sets the unit of sample rate.
        /// </summary>
        /// <value>
        /// The unit of sample rate.
        /// </value>
        public SampleRateUnit UnitOfSampleRate
        {
            get
            {
                return Setting.UnitOfSampleRate;
            }

            set
            {
                Setting.UnitOfSampleRate = value;
                OnPropertyChanged("UnitOfSampleRate");
                OnPropertyChanged("Unit");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleRateSettingController"/> class.
        /// </summary>
        /// <param name="setting">The model setting.</param>
        /// <param name="master">The controller for the hardware type setting.</param>
        public SampleRateSettingController(SampleRateSetting setting, StringMultiOptionSettingController master)
            : base(setting)
        {
            hwTypeSettingController = master;
            hwTypeSettingController.PropertyChanged += hwTypeSettingController_PropertyChanged;
        }

        /// <summary>
        /// Handles the event when the value of the hardware type is changed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void hwTypeSettingController_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Value")
            {
                if (hwTypeSettingController.Value == HW_TYPES.AdWin_Simulator.ToString())
                    UnitOfSampleRate = SampleRateUnit.Hz;
                else
                    UnitOfSampleRate = SampleRateUnit.kHz;
            }
        }
    }
}
