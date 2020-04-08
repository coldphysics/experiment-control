namespace HardwareAdWin.Simulator.Decompression.DecompressionResult
{
    /// <summary>
    /// A numeric decompression result for a digital FIFO, which means that it value is an array of doubles (one double per digital channel).
    /// </summary>
    /// <seealso cref="AbstractNumericDecompressionResult" />
    class DigitalNumericDecompressionResult:AbstractNumericDecompressionResult
    {
        /// <summary>
        /// The decompressed numeric array of values
        /// </summary>
        public readonly double[] VALUES;

        /// <summary>
        /// The number of channels hosted by a 
        /// </summary>
        private readonly int CHANNELS_PER_DIGITAL_CARD;
        /// <summary>
        /// The voltage level of a logical true.
        /// </summary>
        /// 
        private const double DIGITAL_HIGH_VOLTAGE = DecompressionUnit.DIGITAL_HIGH_VOLTAGE;
        /// <summary>
        /// The voltage level of a logical false.
        /// </summary>
        private const double DIGITAL_LOW_VOLTAGE = DecompressionUnit.DIGITAL_LOW_VOLTAGE;

        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalNumericDecompressionResult"/> class.
        /// </summary>
        /// <param name="digitalValue">The decompressed value encoded as a single 32-bit integer (each bit represents a channel)</param>
        /// <param name="initialRepetitions">The initial number of repetitions.</param>
        public DigitalNumericDecompressionResult(uint digitalValue, uint initialRepetitions)
            : base(initialRepetitions)
        {
            this.CHANNELS_PER_DIGITAL_CARD = Global.GetNumDigitalChannelsPerCard();
            this.VALUES = new double[CHANNELS_PER_DIGITAL_CARD];

            //decode 32-bits into 32 doubles
            for (int i = 0; i < CHANNELS_PER_DIGITAL_CARD; i++)
            {
                VALUES[i] = ((digitalValue & 1) == 1) ? DIGITAL_HIGH_VOLTAGE : DIGITAL_LOW_VOLTAGE;
                digitalValue >>= 1;
            }
        }
    }
}
