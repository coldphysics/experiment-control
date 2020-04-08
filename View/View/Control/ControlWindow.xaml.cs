using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Controller.Control;
using Buffer.Basic;
using CustomElements.SizeSavedWindow;
using Errors;
using HardwareAdWin.Debug;

namespace View.Control
{
    /// <summary>
    /// Interaction logic for ControlWindow.xaml
    /// </summary>
    public partial class ControlWindow : Window
    {
        private readonly ControlWindowController _controller;
        public ControlWindow(ControlWindowController controller)
        {
            _controller = controller;
            DataContext = _controller;
            _controller.PropertyChanged += _controller_PropertyChanged;

            InitializeComponent();

            
            //SizeSavedWindow sizeSavedWindow = new SizeSavedWindow();
            SizeSavedWindow.addToSizeSavedWindows(this);
            ErrorWindow.MainWindow = this;

            if (Global.Experiment == Global.ExperimentTypes.Superatom)
            {
                ControlLecroyCheckBox.IsChecked = true;
                ControlLecroyCheckBox.Visibility = Visibility.Visible;
                try
                {
                    Uri uri = new Uri("../../../../View/View/Control/SuperatomsIcon.png", UriKind.Relative);
                    this.Icon = BitmapFrame.Create(uri);
                }
                catch (Exception)
                {}

            }
            else 
            {
                try
                {
                    Uri uri = new Uri("../../../../View/View/Control/cr.png", UriKind.Relative);
                    this.Icon = BitmapFrame.Create(uri);
                }
                catch (Exception)
                {}
            }

        }

        void _controller_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            /*System.Console.Write("PC!\n");
            foreach (ShowableWindow w in Controller.Control.ControlWindowController.WindowsList)
            {
                System.Console.Write("NamePC: {0}\n", w.Name);
            }*/
           // WindowsListBox.
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                WindowsListBox.ItemsSource = Controller.Control.ControlWindowController.WindowsList;
            }));
        }

        private void ShutdownApplication(object sender, CancelEventArgs e)
        {
            //Evtl hardware ausschalten? also aDwinSystem1.Processes[1].Stop(); ??
            //Buffer.HardwareManager.HardwareManager

            if (_controller.OutputHandler.OutputLoopState != OutputHandler.OutputLoopStates.Sleeping)
            {
                MessageBoxResult result =
                    MessageBox.Show(
                        "The hardware output is still in progress. Do you really want to Close this application?",
                        "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
                if (result == MessageBoxResult.Yes)
                {
                    HardwareAdWin.HardwareAdWin.ControlAdwinProcess.StopAdwin();
                    Application.Current.Shutdown();
                }
            }
            Application.Current.Shutdown();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("notepad", "settings.txt");
        }

        private void CycleScriptClick(object sender, RoutedEventArgs e)
        {
            //"C:\Program Files (x86)\Notepad++\notepad++.exe"
            foreach (Setting setting in ((ControlWindowController)DataContext).SettingsList)
            {
                if (setting.Name.Equals("cyclePythonScript"))
                {
                    System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Notepad++\notepad++.exe",setting.Value);
                    break;
                }
            }
        }


        private void LoopScriptClick(object sender, RoutedEventArgs e)
        {
            foreach (Setting setting in ((ControlWindowController)DataContext).SettingsList)
            {
                if (setting.Name.Equals("loopPythonScript"))
                {
                    System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Notepad++\notepad++.exe", setting.Value);
                    break;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            // show info when in debugging mode
            if (Global.UseDebugHardware || !Global.WriteToDatabase)
            {
                String output = "DEBUG MODE";
                if (Global.UseDebugHardware)
                    output += "\n * no hardware output";
                if (!Global.WriteToDatabase)
                    output += "\n * no data will be written into the database";
                MessageBox.Show(output);
            }
        }

    }
}
