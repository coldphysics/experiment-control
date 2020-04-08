namespace HardwareAdWin.Simulator.Decompression.DecompressionResult
{
    /// <summary>
    ///  The special end-of-sequence value, with the last digital numeric value of the corresponding channels (an array of doubles).
    /// </summary>
    /// <seealso cref="AbstractFinishReachedDecompressionResult" />
    class DigitalFinishReachedDecompressionResult:AbstractFinishReachedDecompressionResult
    {
        /// <summary>
        /// The last output numeric values
        /// </summary>
        private double[] lastOutputValues;

        /// <summary>
        /// Gets the last output numeric values.
        /// </summary>
        /// <value>
        /// The last output numeric values.
        /// </value>
        public double[] LastOutputValues
        {
            get { return lastOutputValues; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalFinishReachedDecompressionResult"/> class.
        /// </summary>
        /// <param name="lastResult">The last numeric result.</param>
        public DigitalFinishReachedDecompressionResult(AbstractNumericDecompressionResult lastResult)
            :base(lastResult)
        {
        }

        /// <summary>
        /// Parses a variable of type <see cref=" DigitalFinishReachedDecompressionResult" />, and sets it as the previous numeric value.
        /// </summary>
        /// <param name="lastResult">The variable to parse.</param>
        /// <remarks>
        /// This method is only intended to be called by the constructor of <see cref=" AbstractFinishReachedDecompressionResult" />
        /// </remarks>
        protected override void ParseLastResult(AbstractNumericDecompressionResult lastResult)
        {
            lastOutputValues = (lastResult as DigitalNumericDecompressionResult).VALUES;
        }
    }
}
