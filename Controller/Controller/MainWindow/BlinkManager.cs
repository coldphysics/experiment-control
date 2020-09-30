using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;

namespace Controller.MainWindow
{
    public class BlinkManager
    {
        private static BlinkManager instance;
        private static readonly int BEEP_FREQUENCY = 2000;
        private static readonly int BEEPS = 3;

        private Window mainWindow;

        private BlinkManager(Window window)
        {
            mainWindow = window;
        }

        public static void Initialize(Window mainWindow )
        {
            if (instance != null)
                throw new Exception("Trying to initialize singleton more than once!");

            instance = new BlinkManager(mainWindow);
        }

        public static BlinkManager GetInstance()
        {
            if (instance == null)
                throw new Exception("Trying to retrieve instance before initialization!");

            return instance;
        }


        public void BlinkErrorAsync()
        {
            Console.Write("Blinking Error!\n");

            mainWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                var taskInfo = new System.Windows.Shell.TaskbarItemInfo
                {
                    ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error,
                    ProgressValue = 100
                };
                mainWindow.TaskbarItemInfo = taskInfo;
                MainWindowController.WindowsList.Where(w => w.Name == "Errors").First().window.Activate();
            }));

            Task.Run(() =>
            {
                for (int i = 1; i <= BEEPS; ++i)
                {
                    Console.Beep(BEEP_FREQUENCY, 200);
                    Thread.Sleep(200);
                }
            });
            
        }

        public void StopBlinkingAsync()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var taskInfo = new System.Windows.Shell.TaskbarItemInfo();
                taskInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                mainWindow.TaskbarItemInfo = taskInfo;
            }));
        }

    }
}
