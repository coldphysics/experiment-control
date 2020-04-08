using Communication.Interfaces.Generator;

namespace Communication.Interfaces.Hardware
{
    /// <summary>
    /// Provides the functionality to communicate with the hardware system.
    /// </summary>
    public interface IHardwareGroup
    {
        /// <summary>
        /// Triggers sending the converted data to the system.
        /// </summary>
        /// <param name="data">The output data.</param>
        void Initialize(IModelOutput data);


        /// <summary>
        /// Orders the hardware system to start putting the data on its outputs.
        /// </summary>
        void Start();

        /// <summary>
        /// Determines whether sending the data to the hardware system has finished.
        /// </summary>
        /// <returns><c>true</c> if sending the data to the hardware system has finished, <c>false</c> otherwise.</returns>
        bool HasFinished();
    }
}