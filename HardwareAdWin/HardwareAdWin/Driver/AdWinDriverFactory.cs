using Model.Settings;

namespace HardwareAdWin.Driver
{
    //ADDED Ghareeb 04.10.2016

    /// <summary>
    /// Retrieves a suitable instance of the type <see cref=" IAdWinDriver"/>.
    /// </summary>
    /// <remarks>This class serves both as a Factory design pattern and as Singleton design pattern for the AdWin drivers</remarks>
    class AdWinDriverFactory
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="AdWinDriverFactory"/> class from being created.
        /// </summary>
        private AdWinDriverFactory()
        { }

        /// <summary>
        /// An instance of a driver of a real AdWin hardware.
        /// </summary>
        private static AdWinFunctions realAdWin;

        /// <summary>
        /// An instance of a driver of a dummy AdWin hardware.
        /// </summary>
        private static DummyAdWinDriver dummyAdWin;


        /// <summary>
        /// Gets an appropriate instance of the AdWin hardware driver.
        /// </summary>
        /// <returns>
        /// Depending on whether the hw type is AdWin_Simulator or not, returns either an instance of either <see cref=" DummyAdWinDriver"/> or <see cref=" AdWinFunctions"/>.
        /// </returns>
        /// <remarks>Consecutive calls to this method return a reference to the same instance (Singleton Design Pattern)</remarks>
        internal static IAdWinDriver GetAdWinDriver()
        {
            if (Global.GetHardwareType() == HW_TYPES.AdWin_Simulator)
            {
                if (dummyAdWin == null)
                    dummyAdWin = new DummyAdWinDriver();

                return dummyAdWin;
            }
            else
            {
                if (realAdWin == null)
                    realAdWin = new AdWinFunctions();

                return realAdWin;
            }
            
        }
    }
}
