using Communication.Interfaces.Generator;
using Communication.Interfaces.Hardware;

namespace Buffer.HardwareManager
{
    //RECO remove the ability to communicate with multiple hardware systems at the same time.

    /// <summary>
    /// Provides the functionality to communicate with the hardware system(s); it prepares and sends the output data to the hardware system(s).
    /// This class is able to deal with any hardware that implements the <see cref=" IHardwareGroup"/> interface.
    /// </summary>
    /// <seealso cref="IHardwareManager" />
    public class HardwareManager : IHardwareManager
    {
        /// <summary>
        /// A dictionary of all <see cref=" IHardwareGroup"/>s this manager is responsible of.
        /// Usually a single item only.
        /// </summary>
        private readonly IHardwareGroup _hardwareGroup;


        /// <summary>
        /// Initializes a new instance of the <see cref="HardwareManager"/> class.
        /// </summary>
        /// <param name="hardwareGroups">The hardware groups this instance will manage. (usually only one hardware group is passed)</param>
        public HardwareManager(IHardwareGroup hardwareGroup)
        {
            _hardwareGroup = hardwareGroup;

        }

        /// <summary>
        /// Determines whether writing the output data to the hardware system has finished.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if writing has finished, <c>false</c> otherwise.
        /// </returns>
        public bool HasFinished()
        {
            return _hardwareGroup.HasFinished();
        }

        /// <summary>
        /// Converts the output to a format understood by the hardware system(s)(if needed), and triggers sending the converted data to the HW system(s).
        /// </summary>
        /// <param name="gate">Used to signal the event that sending data to the hardware system(s) has finished. (does not work with the AdWin hardware)</param>
        /// <param name="outputData">The unconverted output data for all managed <see cref=" IHardwareGroup"/>'s. Each output is mapped to its group name.</param>
        /// <param name="cycleDuration">Duration of the cycle (used for debugging purposes).</param>
        /// <exception cref=" System.Exception">When one output is associated with a non-existing <see cref=" IHardwareGroup"/>.</exception>
        public void Initialize(IModelOutput outputData, double cycleDuration)
        {
            _hardwareGroup.Initialize(outputData);

        }

        /// <summary>
        /// Orders the hardware system(s) to start putting the (already-transferred) data on its (their) output(s).
        /// </summary>
        public void Start()
        {
            _hardwareGroup.Start();

        }



    }
}
