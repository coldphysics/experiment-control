using System.Diagnostics;
using System.Windows;
using Controller;
using Controller.MainWindow;
using Controller.Settings;
using MainProject.Builders;
using Model.Properties;
using Model.Settings;


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

        public MainWindow()
        {
            InitializeComponent();

            if (Debugger.IsAttached)
                Settings.Default.Reset();

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

                    MasterBuilder builder = new MasterBuilder();
                    builder.Build();
                    MainWindowController controller = builder.GetMainController();
                    View.Main.MainWindow mainWindow = new View.Main.MainWindow(controller);
                    mainWindow.Show();

                    InitializeComponent();

                    this.Close();
                }
            }
            else
            {
                MasterBuilder builder = new MasterBuilder();
                builder.Build();
                MainWindowController controller = builder.GetMainController();
                View.Main.MainWindow mainWindow = new View.Main.MainWindow(controller);
                mainWindow.Show();

                InitializeComponent();

                this.Close();
            }
        }
    }
}
