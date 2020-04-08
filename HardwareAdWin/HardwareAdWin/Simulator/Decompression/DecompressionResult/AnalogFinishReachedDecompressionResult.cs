namespace HardwareAdWin.Simulator.Decompression.DecompressionResult
{
    /// <summary>
    /// The special end-of-sequence value, with the last analog numeric value of the corresponding channel (double).
    /// </summary>
    /// <seealso cref="AbstractFinishReachedDecompressionResult" />
    class AnalogFinishReachedDecompressionResult : AbstractFinishReachedDecompressionResult
    {
        /// <summary>
        /// The last output numeric value.
        /// </summary>
        private double lastOutputValue;

        /// <summary>
        /// Gets the last output numeric value.
        /// </summary>
        /// <value>
        /// The last output numeric value.
        /// </value>
        public double LastOutputValue
        {
            get { return lastOutputValue; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalogFinishReachedDecompressionResult"/> class.
        /// </summary>
        /// <param name="lastResult">The last analog numeric result.</param>
        public AnalogFinishReachedDecompressionResult(AnalogNumericDecompressionResult lastResult)
            : base(lastResult)
        {
        }

        /// <summary>
        /// Parses a variable of type <see cref=" AnalogNumericDecompressionResult" />, and sets it as the previous numeric value.
        /// </summary>
        /// <param name="lastResult">The variable to parse.</param>
        /// <remarks>
        /// This method is only intended to be called by the constructor of <see cref=" AbstractFinishReachedDecompressionResult" />
        /// </remarks>
        protected override void ParseLastResult(AbstractNumericDecompressionResult lastResult)
        {
            lastOutputValue = (lastResult as AnalogNumericDecompressionResult).VALUE;
        }
    }
}
