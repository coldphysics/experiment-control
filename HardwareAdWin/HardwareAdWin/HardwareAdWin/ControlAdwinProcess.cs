using System;
using System.IO;
using Errors.Error;
using HardwareAdWin.Driver;
using Model.Settings;
using Model.Settings.Settings;

namespace HardwareAdWin.HardwareAdWin //more generic name
{
    //CHANGED Ghareeb 04.10.2016 use of IAdWinDriver
    /// <summary>
    /// Provides static methods to start and stop the AdWinSystem.
    /// </summary>
    public class ControlAdwinProcess
    {
        /// <summary>
        /// A reference to the AdWin system's driver.
        /// </summary>
        private static IAdWinDriver adwin;

        /// <summary>
        /// Initializes the <see cref="ControlAdwinProcess"/> class.
        /// </summary>
        static ControlAdwinProcess()
        {
            adwin = AdWinDriverFactory.GetAdWinDriver();
        }

        /// <summary>
        /// Initializes the AdWin system and starts it.
        /// </summary>
        /// <remarks>
        /// This method boots the adwin system and then loads the process to it and starts it.
        /// </remarks>
        public static void StartAdwin()
        {
            HW_TYPES hwType = Global.GetHardwareType();
            string process = "";

            if (hwType != HW_TYPES.AdWin_Simulator)
            {
                Profile profile = ProfilesManager.GetInstance().ActiveProfile;
                process = ((FileSetting)profile.GetSettingByName(SettingNames.ADBASIC_CODE_PATH)).Value;

                //if the process file doesn't exist show an error
                if (!File.Exists(process))
                {
                    ErrorCollector errorCollector = ErrorCollector.Instance;
                    errorCollector.AddError("AdWin executable file could not be found!", ErrorWindow.MainHardware, true, ErrorTypes.Other);

                    return;
                }

                try
                {
                    //boot the system
                    if (hwType == HW_TYPES.AdWin_T11)
                        adwin.BootAdWinT11();
                    else if (hwType == HW_TYPES.AdWin_T12)
                        adwin.BootAdWinT12();
                }
                catch(Exception e)
                {
                    ErrorCollector errorCollector = ErrorCollector.Instance;
                    errorCollector.AddError("AdWin Error: " + e.Message, ErrorWindow.MainHardware, true, ErrorTypes.Other);
                    return;
                }
            }

            adwin.SetDeviceNumber(1);

            //load the process
            adwin.LoadProcess(1, process);
            //run the process
            adwin.StartProcess(1);
        }

        /// <summary>
        /// Stops the process.
        /// </summary>
        public static void StopAdwin()
        {
            adwin.StopProcess(1);
        }
    }
}
