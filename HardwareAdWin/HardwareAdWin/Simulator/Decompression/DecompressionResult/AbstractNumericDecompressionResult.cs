using System;

namespace HardwareAdWin.Simulator.Decompression.DecompressionResult
{
    /// <summary>
    /// A numeric decompression result, which holds a value and its repetition.
    /// </summary>
    /// <seealso cref="IDecompressionResult" />
    abstract class AbstractNumericDecompressionResult:IDecompressionResult
    {
        /// <summary>
        /// The number of repetitions for the decompressed value.
        /// </summary>
        protected uint repetitions;

        /// <summary>
        /// Gets the repetitions.
        /// </summary>
        /// <value>
        /// The number of repetitions for the decompressed value.
        /// </value>
        public uint Repetitions
        {
            get { return repetitions; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractNumericDecompressionResult"/> class.
        /// </summary>
        /// <param name="initialRepetitions">The initial number of repetitions.</param>
        public AbstractNumericDecompressionResult(uint initialRepetitions)
        {
            repetitions = initialRepetitions;
        }

        /// <summary>
        /// Uses the value.
        /// </summary>
        /// <exception cref="System.Exception">Thrown when trying to use a decompression result more than its repetitions!</exception>
        /// <remarks>
        /// It decreases the value of <see cref=" Repetitions"/> by 1.
        /// </remarks>
        public void UseValue()
        {
            if (repetitions > 0)
                --repetitions;
            else
                throw new Exception("AdWinEmulator: trying to use a decompression Result more than its repetitions!");
        }
    }
}
