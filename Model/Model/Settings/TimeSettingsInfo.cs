using Model.Settings.Settings;

namespace Model.Settings
{
    /// <summary>
    /// Provides access to and basic conversion for the settings related to the timing of the experiment (e.g., TimeUnit, SampleRateValue,..)
    /// </summary>
    /// <remarks>The class is a singleton, so use <see cref=" GetInstance"/> method to get an instance of it.</remarks>
    public class TimeSettingsInfo
    {
        /// <summary>
        /// The instance
        /// </summary>
        private static TimeSettingsInfo instance;

        /// <summary>
        /// Gets the single instance associated to this class.
        /// </summary>
        /// <returns>An instance of the class</returns>
        public static TimeSettingsInfo GetInstance()
        {
            if (instance == null)
                instance = new TimeSettingsInfo();

            return instance;
        }

        /// <summary>
        /// The time unit ("ms")
        /// </summary>
        private const string TIME_UNIT = "ms";

        /// <summary>
        /// The number of occurrences of a single time unit (ms) during one second, i.e., 1000
        /// </summary>
        private const decimal INPUT_TIME_BASE = 1000.0M;

        /// <summary>
        /// Gets the value of the sample rate in Hz (decimal)
        /// </summary>
        /// <value>
        /// The sample rate value in Hz.
        /// </value>
        public decimal SampleRateValueDecimal
        {
            get
            {
                ProfilesManager manager = ProfilesManager.GetInstance();
                SampleRateSetting setting = (SampleRateSetting)manager.ActiveProfile.GetSettingByName(SettingNames.SAMPLE_RATE);
                SampleRateUnit unit = setting.UnitOfSampleRate;
                int sampleRateValueOriginal = setting.Value;
                decimal _sampleRateValue = -1.0M;

                switch (unit)
                {
                    case SampleRateUnit.Hz:
                        _sampleRateValue = sampleRateValueOriginal;
                        break;
                    case SampleRateUnit.kHz:
                        _sampleRateValue = sampleRateValueOriginal * 1e3M;
                        break;
                    case SampleRateUnit.MHz:
                        _sampleRateValue = sampleRateValueOriginal * 1e6M;
                        break;
                    case SampleRateUnit.GHz:
                        _sampleRateValue = sampleRateValueOriginal * 1e9M;
                        break;
                }

                return _sampleRateValue;
            }
        }

        /// <summary>
        /// Gets the value of the sample rate in Hz
        /// </summary>
        /// <value>
        /// The sample rate value in Hz.
        /// </value>
        public double SampleRateValue
        {
            get
            {
                return (double)SampleRateValueDecimal;
            }
        }

        /// <summary>
        /// Gets the time unit as a string.
        /// </summary>
        /// <value>
        /// The time unit.
        /// </value>
        public string TimeUnit
        {
            get
            {
                return TIME_UNIT;
            }
        }

        /// <summary>
        /// Gets the input time base as a decimal.
        /// </summary>
        /// <value>
        /// The input time base.
        /// </value>
        public decimal InputTimeBaseDecimal
        {
            get
            {
                return INPUT_TIME_BASE;
            }
        }

        /// <summary>
        /// Gets the input time base as a double.
        /// </summary>
        /// <value>
        /// The input time base.
        /// </value>
        public double InputTimeBase
        {
            get
            {
                return (double)InputTimeBaseDecimal;
            }
        }

        /// <summary>
        /// Gets the smallest time step measured in ms (decimal).
        /// </summary>
        /// <value>
        /// The smallest time step measured in ms.
        /// </value>
        public decimal SmallestTimeStepDecimal
        {
            get
            {
                return InputTimeBaseDecimal / SampleRateValueDecimal;
            }
        }

        /// <summary>
        /// Gets the smallest time step measured in ms.
        /// </summary>
        /// <value>
        /// The smallest time step measured in ms.
        /// </value>
        public double SmallestTimeStep
        {
            get
            {
                return (double)SmallestTimeStepDecimal;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="TimeSettingsInfo"/> class from being created.
        /// </summary>
        private TimeSettingsInfo()
        {

        }


    }
}
