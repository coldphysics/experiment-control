namespace HardwareAdWin.Simulator.Decompression.DecompressionResult
{
    /// <summary>
    /// A numeric decompression result for an analog FIFO, which means that it value is double.
    /// </summary>
    /// <seealso cref="AbstractNumericDecompressionResult" />
    class AnalogNumericDecompressionResult:AbstractNumericDecompressionResult
    {
        /// <summary>
        /// The decompressed value
        /// </summary>
        public readonly double VALUE;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalogNumericDecompressionResult"/> class.
        /// </summary>
        /// <param name="value">The decompressed value.</param>
        /// <param name="initialRepetitions">The initial number of repetitions.</param>
        public AnalogNumericDecompressionResult(double value, uint initialRepetitions)
            :base(initialRepetitions)
        {
            VALUE = value;
        }

    }
}
