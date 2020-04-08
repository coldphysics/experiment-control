using System;
using System.Runtime.Serialization;

namespace Model.V2.Data.SequenceGroups
{
    //RECO make singleton
    /// <summary>
    /// Holds time-related settings (time and frequency units, granularity of time steps, values)
    /// </summary>
    [Serializable]
    [DataContract]
    public class Settings
    {
        #region SampleRate enum

        /// <summary>
        /// Lists the possible sample rates
        /// </summary>
        public enum SampleRate
        {
            Hz = 0,
            kHz,
            MHz,
            GHz
        }

        #endregion

        #region TimeBase enum

        /// <summary>
        /// Lists the possible time bases
        /// </summary>
        public enum TimeBase
        {
            s = 0,
            ms,
            us,
            ns
        }

        #endregion

        // RECO give it more time indicative name since  there is another multiplicator related to the output value of each channel
        /// <summary>
        /// Represents the input time multiplicator, currently assigned to '1' in  the MainWindow.cs  
        /// </summary>
        private readonly double _inputMultiplicator = 1;
        /// <summary>
        /// Specifies the timebase unit of the experiment
        /// </summary>
        [DataMember] private readonly TimeBase _inputTimebase;
        /// <summary>
        /// Specifies the sample rate unit of the experiment
        /// </summary>
        [DataMember] private readonly SampleRate _sampleRate;
        /// <summary>
        /// Specifies the multiplicator of the frequency (e.g. a value 50 with SampleRate value as KHz results in 50 KHz)
        /// </summary>
        [DataMember] private readonly double _sampleRateMultiplicator = 1;
        /// <summary>
        /// The multiplication of SampleRate (as a unit) and the sampleRateMultiplicator (as a value), 
        /// e.g. KHz as unit with 50 as a value of multiplicator results in 50000 Hz as sampleRateValue
        /// </summary>
        [DataMember] private readonly double _sampleRateValue = 1;


        //CHANGED from readonly
        /// <summary>
        /// The multiplication of TimeBase (as a unit) and the inputMultiplicator (as a value),
        /// e.g. ms as unit with 1 as a value of multiplicator result in 1000  as inputTimebaseValue (used later in the denominator for calculating the duration of the whole sequencegroup).
        /// This value represents 1/time_measured_in_seconds
        /// </summary>
        [DataMember] public double _inputTimebaseValue = 1;

        //CHANGED from readonly
        /// <summary>
        /// Represents the smallest indivisible time step for each minor cycle,
        /// e.g. if we have our <c>_inputTimeBase</c> as ms and our <c>_inputMultiplicator</c> as 1 (i.e. <c>_inputTimebaseValue == 1000</c>) and the <c>_sampleRate</c> as KHz and the <c>_sampleRateMultiplicator</c> as 50 (i.e. <c>_sampleRateValue = 50000 Hz</c>)
        /// , then <c>_smallestTimestep = 1000 / 50000 = 0.02 ms</c> which represents the number of times (1 ms) can fit within a single Hz.
        /// </summary>
        [DataMember] public double _smallestTimestep;


        /// <summary>
        /// Gets the smallest indivisible time step for each minor cycle,
        /// e.g. if we have our <c>_inputTimeBase</c> as ms and our <c>_inputMultiplicator</c> as 1 (i.e. <c>_inputTimebaseValue == 1000</c>) and the <c>_sampleRate</c> as KHz and the <c>_sampleRateMultiplicator</c> as 50 (i.e. <c>_sampleRateValue = 50000 Hz</c>)
        /// , then <c>_smallestTimestep = 1000 / 50000 = 0.02 ms</c> which represents the number of times (1 ms) can fit within a single Hz.
        /// </summary>
        public double SmallestTimeStep { get { return _smallestTimestep; } }

        /// <summary>
        /// Gets the multiplication of SampleRate (as a unit) and the sampleRateMultiplicator (as a value), 
        /// e.g. KHz as unit with 50 as a value of multiplicator results in 50000 Hz as sampleRateValue
        /// </summary>
        public double SampleRateValue { get { return _sampleRateValue; } }

        /// <summary>
        /// Gets a string representation of the current inputTimeBase (e.g. "us").
        /// </summary>
        public string Unit { get { return _inputTimebase.ToString(); } }

        //ADDED Ghareeb 18.11.2016 to allow converting the settings class to future versions
        #region Added for forward-compatibility
        /// <summary>
        /// Gets the input timebase value1.
        /// </summary>
        /// <value>
        /// The input timebase value1.
        /// </value>
        public double InputTimebaseValue1
        {
            get { return _inputTimebaseValue; }
        }
        /// <summary>
        /// Gets the smallest timestep.
        /// </summary>
        /// <value>
        /// The smallest timestep.
        /// </value>
        public double SmallestTimestep
        {
            get { return _smallestTimestep; }
        }
        /// <summary>
        /// Gets the sample rate enum.
        /// </summary>
        /// <value>
        /// The sample rate enum.
        /// </value>
        public SampleRate SampleRateEnum
        {
            get { return _sampleRate; }
        }

        /// <summary>
        /// Gets the input multiplicator.
        /// </summary>
        /// <value>
        /// The input multiplicator.
        /// </value>
        public double InputMultiplicator
        {
            get { return _inputMultiplicator; }
        }

        /// <summary>
        /// Gets the input timebase value.
        /// </summary>
        /// <value>
        /// The input timebase value.
        /// </value>
        public double InputTimebaseValue
        {
            get { return _inputTimebaseValue; }
        }

        /// <summary>
        /// Gets the sample rate multiplicator.
        /// </summary>
        /// <value>
        /// The sample rate multiplicator.
        /// </value>
        public double SampleRateMultiplicator
        {
            get { return _sampleRateMultiplicator; }
        }


        /// <summary>
        /// Gets the input timebase.
        /// </summary>
        /// <value>
        /// The input timebase.
        /// </value>
        public TimeBase InputTimebase
        {
            get { return _inputTimebase; }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// It calculates the values of sampleRateValue and inputTimebaseValue based on the corresponding units and the multiplicators.
        /// It calculates the smallest time step as explained above.
        /// </summary>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="sampleRateMultiplicator">The sample rate multiplicator.</param>
        /// <param name="inputTimebase">The input timebase.</param>
        /// <param name="inputMultiplicator">The input multiplicator.</param>
        public Settings(SampleRate sampleRate, double sampleRateMultiplicator, TimeBase inputTimebase,
                        double inputMultiplicator)
        {
            _sampleRate = sampleRate;
            _inputTimebase = inputTimebase;
            _sampleRateMultiplicator = sampleRateMultiplicator;
            _inputMultiplicator = inputMultiplicator;

            
            switch (_sampleRate)
            {
                case SampleRate.Hz:
                    _sampleRateValue = _sampleRateMultiplicator;
                    break;
                case SampleRate.kHz:
                    _sampleRateValue = _sampleRateMultiplicator * 1e3;
                    break;
                case SampleRate.MHz:
                    _sampleRateValue = _sampleRateMultiplicator * 1e6;
                    break;
                case SampleRate.GHz:
                    _sampleRateValue = _sampleRateMultiplicator * 1e9;
                    break;
            }

            System.Console.Write("sampleRate: {0}\n", _sampleRateValue);

            switch (_inputTimebase)
            {
                case TimeBase.s:
                    _inputTimebaseValue = _inputMultiplicator;
                    break;
                case TimeBase.ms:
                    _inputTimebaseValue = _inputMultiplicator * 1e3;
                    break;
                case TimeBase.us:
                    _inputTimebaseValue = _inputMultiplicator * 1e6;
                    break;
                case TimeBase.ns:
                    _inputTimebaseValue = _inputMultiplicator * 1e9;
                    break;
            }

            _smallestTimestep = _inputTimebaseValue / _sampleRateValue;
            System.Console.Write("SmallestTimeStep: {0}\n", _smallestTimestep);
        }


    }
}