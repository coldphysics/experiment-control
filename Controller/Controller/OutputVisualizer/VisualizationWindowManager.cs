using Buffer.Basic;
using Controller.Common;
using Controller.MainWindow;
using CustomElements.CheckableTreeView;
using System;
using System.Windows;

namespace Controller.OutputVisualizer
{
    /// <summary>
    /// This class is a singleton, it's used to open the output visualizer window once and manage it. 
    /// The output visualizer can be opened from the main window (view => output-Visualizer)
    /// or from the card window.
    /// </summary>
    public class VisualizationWindowManager
    {
        private static VisualizationWindowManager _singleton;
        private Window _visualizationWindow;
        private bool isVisualizationWindowOpen = false;

        public OutputVisualizationWindowController OutputVisualizationController { private set; get; }


        private VisualizationWindowManager(MainWindowController mainWindowController)
        {
            CTVViewModel treeView = ModelBasedCTVBuilder.BuildCheckableTree(mainWindowController.GetRootController());
            OutputVisualizationController = new OutputVisualizationWindowController(treeView, mainWindowController);
        }

        /// <summary>
        /// Initializes this instance. Must be invoked once and only once, before ever calling GetInstance
        /// </summary>
        /// <param name="mainWindowController">The controller of the main window (parent controller)</param>
        public static void Initialize(MainWindowController mainWindowController)
        {
            if (_singleton == null)
            {
                _singleton = new VisualizationWindowManager(mainWindowController);
            }
            else
            {
                throw new Exception("Trying to initialize singleton more than once!");
            }
        }

        /// <summary>
        /// Gets the single instance of this class. The Initialize method must be invoked once before the first call to this method
        /// </summary>
        /// <returns>an instance of the <see cref="VisualizationWindowManager"/> class</returns>
        public static VisualizationWindowManager GetInstance()
        {
            if (_singleton == null)
                throw new Exception("Trying to get instance without initialization");

            return _singleton;
        }

        /// <summary>
        /// Attaches the event that a new output is generate with the event handler at the <seealso cref="OutputVisualizationWindowController"/>
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="args">Indicates whether the generation process was successful</param>
        public void HandleNewGeneratedOutputEvent(object sender, FinishedModelGenerationEventArgs args)
        {
            if (args.IsSuccessful)
            {
                _visualizationWindow?.Dispatcher.Invoke(() =>
                    OutputVisualizationController.HandleNewGeneratedOutputEvent());
            }
        }

        /// <summary>
        /// Opens the visualization window. Initializes the window if it is the first time to open it.
        /// </summary>
        public void OpenWindow()
        {
            if (_visualizationWindow == null || !isVisualizationWindowOpen)
            {
                _visualizationWindow =
                    WindowsHelper.CreateCustomWindowToHostViewModel(OutputVisualizationController, false);
                isVisualizationWindowOpen = true;
                _visualizationWindow.Closed += new EventHandler((sender, args) => isVisualizationWindowOpen = false);

                _visualizationWindow.MinHeight = 360;
                _visualizationWindow.MinWidth = 500;
                _visualizationWindow.Height = 400;
                _visualizationWindow.Width = _visualizationWindow.MinWidth;
                _visualizationWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                _visualizationWindow.Title = "Output Visualizer";
            }

            OutputVisualizationController.HandleWindowOpeningEvent();
            _visualizationWindow.Show();
            _visualizationWindow.Focus();
        }


    }
}