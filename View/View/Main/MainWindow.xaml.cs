using System;
using System.ComponentModel;
using System.Windows;
using Buffer.Basic;
using Controller.MainWindow;
using Controller.OutputVisualizer;
using CustomElements.SizeSavedWindow;
using Errors;



namespace View.Main
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowController _controller;
        public MainWindow(MainWindowController controller)
        {
            VisualizationWindowManager.Initialize(controller);
            controller.OnCreatingWindow();
            _controller = controller;
            DataContext = _controller;
            InitializeComponent();

            SizeSavedWindow.addToSizeSavedWindows(this);
            ErrorWindow.MainWindow = this;

        }


        private void ShutdownApplication(object sender, CancelEventArgs e)
        {
            //Evtl hardware ausschalten? also aDwinSystem1.Processes[1].Stop(); ??
            //Buffer.HardwareManager.HardwareManager

            if (_controller.OutputHandler.OutputLoopState != OutputHandler.OutputLoopStates.Sleeping)
            {
                MessageBoxResult result =
                    MessageBox.Show(
                        "The hardware Output is still in progress. Do you really want to Close this application?",
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



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            // show info when in debugging mode
            if (Global.GetHardwareType() == Model.Settings.HW_TYPES.AdWin_Simulator || Global.GetHardwareType() == Model.Settings.HW_TYPES.NO_OUTPUT || !Global.CanAccessDatabase())
            {
                String output = "According to the selected profile:";
                if (Global.GetHardwareType() == Model.Settings.HW_TYPES.AdWin_Simulator || Global.GetHardwareType() == Model.Settings.HW_TYPES.NO_OUTPUT)
                    output += "\n * No hardware output will take place!";
                if (!Global.CanAccessDatabase())
                    output += "\n * No data will be written into the database!";
                MessageBox.Show(output, "Debug Mode", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

    }
}
