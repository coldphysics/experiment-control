using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Controller.Helper;
using Controller.MainWindow;
using Controller.Settings;
using MainProject.Builders;
using Model.Settings;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace MainProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Necessary when changing the settings models.
        /// </summary>
        private const bool CLEAR_PROFILES_WHEN_ERROR = false;
        /// <summary>
        /// Sound frequency of beeps emitted when making the user aware of errors.
        /// </summary>
        private static readonly int BEEP_FREQUENCY = 2000;
        /// <summary>
        /// Number of beeps emitted when making the user aware of errors.
        /// </summary>
        private static readonly int BEEPS = 3;

        private Window _mainWindow;

        public MainWindow()
        {
            ProfilesManager profilesManager = null;

            try
            {
                profilesManager = ProfilesManager.GetInstance();
            }
            catch
            {
                if (CLEAR_PROFILES_WHEN_ERROR)
                {
                    profilesManager = ProfilesManager.RestoreOriginalProfiles();
                }
                else
                {
                    MessageBox.Show("An error occurred while reading the stored profiles, if the error persists, please delete the profile storage folder and run the program again, the folder can be found in:\n" + ProfilesManager.SaveFolder, "Error While Parsing Settings Profiles", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }

            }

            if (profilesManager.ActiveProfile == null)
            {
                MessageBox.Show("This is the first time you run this application! Please choose an active profile and configure it.",
                    "Configuration Needed", MessageBoxButton.OK, MessageBoxImage.Information);
                Controller.Settings.ControllersBuilder pbuilder = new Controller.Settings.ControllersBuilder();
                pbuilder.Build();
                ProfileManagerController pcontroller = pbuilder.GetResult();

                Window managerWindow = WindowsHelper.CreateWindowToHostViewModel(pcontroller, false);
                managerWindow.MinHeight = 360;
                managerWindow.MinWidth = 780;
                managerWindow.Height = 625;
                managerWindow.Width = managerWindow.MinWidth;
                managerWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                managerWindow.Title = "Profiles Management";
                   
                managerWindow.ShowDialog();

                if (pcontroller.Result == ProfileManagerController.ProfileManagerResult.CANCEL_OR_CLOSE)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    //save the settings
                    ProfilesManager.GetInstance().GetValuesFromSnapshot(pcontroller.SettingsSnapshot);
                    ProfilesManager.GetInstance().SaveAllProfiles();

                    StartApplication();
                }
            }
            else
            {
                StartApplication();
            }
        }

        private void StartApplication()
        {
            MasterBuilder builder = new MasterBuilder();
            builder.Build();
            MainWindowController controller = builder.GetMainController();
            controller.OnCreatingWindow();

            MainWindowController
                .WindowsList
                .Where(w=>w.Name == "Errors")
                .First()
                .window
                .Icon = new BitmapImage(new Uri("pack://application:,,,/View;component/Resources/errorIcon.png", UriKind.Absolute));

            _mainWindow = WindowsHelper.CreateWindowToHostViewModel(controller, true, false, true, false);

            if (controller.Icon != null )
            {
                _mainWindow.Icon = new BitmapImage(new Uri("pack://application:,,," + controller.Icon, UriKind.Absolute));
            }

            controller.IndicateErrorOnTaskbarEvent += (sender, e) => BlinkErrorAsync();
            controller.StopIndicatingErrorOnTaskbarEvent += (sender, e) => StopBlinkingAsync();

            _mainWindow.Width = 850;
            _mainWindow.Height = 750;
            _mainWindow.Title = "Experiment Control";
            _mainWindow.Closing += (sender, args) => controller.ShutdownApplication(args);
            _mainWindow.Show();

            InitializeComponent();

            this.Close();
        }

        public void BlinkErrorAsync()
        {
            Console.Write("Blinking Error!\n");

            _mainWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                var taskInfo = new System.Windows.Shell.TaskbarItemInfo
                {
                    ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error,
                    ProgressValue = 100
                };
                _mainWindow.TaskbarItemInfo = taskInfo;
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
                _mainWindow.TaskbarItemInfo = taskInfo;
            }));
        }
    }
}
