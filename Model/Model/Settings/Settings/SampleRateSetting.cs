using System;

namespace Model.Settings.Settings
{
    /// <summary>
    /// A setting that allows specifying the sample rate of the experiment
    /// </summary>
    /// <seealso cref="Model.Settings.Settings.IntegerSetting" />
    /// <remarks>
    /// This setting is tightly-coupled to another setting that dictates the unit of the sample rate. 
    /// The idea here is to force the unit to be Hz for the simulator hardware and to be kHz otherwise.
    /// </remarks>
    [Serializable]
    public class SampleRateSetting:IntegerSetting
    {
        /// <summary>
        /// The setting that specifies the type of the hardware
        /// </summary>
        private StringMultiOptionSetting hardwareTypeSetting;
        /// <summary>
        /// The unit of sample rate
        /// </summary>
        private SampleRateUnit unitOfSampleRate;

        /// <summary>
        /// Gets or sets the unit of sample rate.
        /// </summary>
        /// <value>
        /// The unit of sample rate.
        /// </value>
        public SampleRateUnit UnitOfSampleRate
        {
            get { return unitOfSampleRate; }
            set 
            {
                unitOfSampleRate = value;
                Unit = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the hardware type setting.
        /// </summary>
        /// <value>
        /// The hardware type setting.
        /// </value>
        public StringMultiOptionSetting HardwareTypeSetting
        {
            get { return hardwareTypeSetting; }
            set { hardwareTypeSetting = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SampleRateSetting"/> class.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="minimumValue">The minimum value.</param>
        /// <param name="maximumValue">The maximum value.</param>
        /// <param name="unit">The unit.</param>
        /// <param name="masterSetting">The setting that specifies the type of the hardware.</param>
        public SampleRateSetting(string name, int defaultValue, int minimumValue, int maximumValue, SampleRateUnit unit, StringMultiOptionSetting masterSetting)
            :base(name, defaultValue, minimumValue, maximumValue, unit.ToString())
        {
            hardwareTypeSetting = masterSetting;
            this.UnitOfSampleRate = unit;
        }
    }
}
