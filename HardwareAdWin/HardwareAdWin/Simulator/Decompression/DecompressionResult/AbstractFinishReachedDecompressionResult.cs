namespace HardwareAdWin.Simulator.Decompression.DecompressionResult
{
    //The FinishReacehedDecompressionResult class hierarchy can be minimized by haveing a single class that has a reference of type AbstractNumericDecompressionResult
    /// <summary>
    /// Represents the end-of-sequence special value, and encapsulates the last read numeric value (which is needed for current and future output).
    /// </summary>
    /// <seealso cref="IDecompressionResult" />
    abstract class AbstractFinishReachedDecompressionResult:IDecompressionResult
    {

        /// <summary>
        /// Parses a variable of type <see cref=" AbstractNumericDecompressionResult"/>, and sets it as the previous numeric value.
        /// </summary>
        /// <param name="lastResult">The variable to parse.</param>
        /// <remarks>
        /// This method is only intended to be called by the constructor of <see cref=" AbstractFinishReachedDecompressionResult"/>
        /// </remarks>
        protected abstract void ParseLastResult(AbstractNumericDecompressionResult lastResult);

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractFinishReachedDecompressionResult"/> class.
        /// </summary>
        /// <param name="lastResult">The last numeric result.</param>
        public AbstractFinishReachedDecompressionResult(AbstractNumericDecompressionResult lastResult)
        {
            //Parsing logic is implemented by children of this class.
            ParseLastResult(lastResult);
        }

    }
}
