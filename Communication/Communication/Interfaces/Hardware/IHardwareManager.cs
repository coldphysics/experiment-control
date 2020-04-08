using Communication.Interfaces.Generator;

namespace Communication.Interfaces.Hardware
{
    /// <summary>
    /// Provides the functionality to communicate with the hardware system; it prepares and sends the output data to the hardware system.
    /// </summary>
    public interface IHardwareManager
    {
        /// <summary>
        /// Triggers sending the output to the system.
        /// </summary>
        /// <param name="outputData">The output data.</param>
        /// <param name="cycleDuration">Duration of the cycle (used for debugging purposes).</param>
        void Initialize(IModelOutput outputData, double cycleDuration);

        /// <summary>
        /// Orders the hardware to start putting the (already-transferred) data on its output.
        /// </summary>
        void Start();

        /// <summary>
        /// Asks the hardware system whether it has finished processing all data.
        /// </summary>
        /// <returns><c>true</c> if processing has finished, <c>false</c> otherwise.</returns>
        bool HasFinished();
    }
}
