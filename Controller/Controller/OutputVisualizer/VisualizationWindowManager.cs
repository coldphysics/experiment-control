using Buffer.Basic;
using Controller.Control.StepBatchAddition;
using Controller.MainWindow;
using Controller.Root;
using CustomElements.CheckableTreeView;
using System;
using System.Windows;

namespace Controller.OutputVisualizer
{

    /// <summary>
    /// This class is a singleton, it's used to open the output visualizer window once 
    /// as the output visualizer can be opened from the main window (view => output-Visualizer)
    /// or from the card window.
    /// </summary>
    public class VisualizationWindowManager
    {
        private static VisualizationWindowManager singleton;
        private Window visualizationWindow;
        private OutputVisualizationWindowController outputVisualizationController;
        private bool isVisualizationWindowOpen = false;


        private VisualizationWindowManager(MainWindowController mainWindowController)
        {
            CTVViewModel treeView = ModelBasedCTVBuilder.BuildCheckableTree(mainWindowController.GetRootController());
            outputVisualizationController = new OutputVisualizationWindowController(treeView, mainWindowController);
        }
        public static void Initialize(MainWindowController mainWindowController)
        {
            if (singleton == null)
            {
                singleton = new VisualizationWindowManager(mainWindowController);
            }
            else
            {
                throw new Exception("Trying to initialize singleton more than once!");
            }
        }
        public static VisualizationWindowManager GetInstance()
        {
            if (singleton == null)
                throw new Exception("Trying to get instance without initialization");

            return singleton;
        }

        public void HandleNewGeneratedOutputEvent(object sender, FinishedModelGenerationEventArgs args)
        {
            if (args.IsSuccessful && visualizationWindow != null)
            {
                visualizationWindow.Dispatcher.Invoke(() => outputVisualizationController.HandleNewGeneratedOutputEvent());
            }
        }

        public void OpenWindow()
        {
            if (visualizationWindow == null || !isVisualizationWindowOpen)
            {
                visualizationWindow = WindowsHelper.CreateCustomWindowToHostViewModel(outputVisualizationController, false);
                isVisualizationWindowOpen = true;
                visualizationWindow.Closed += new EventHandler((sender, args) => isVisualizationWindowOpen = false);

                visualizationWindow.MinHeight = 360;
                visualizationWindow.MinWidth = 500;
                visualizationWindow.Height = 400;
                visualizationWindow.Width = visualizationWindow.MinWidth;
                visualizationWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                visualizationWindow.Title = "Output Visualizer";
                //visulizationWindow.ShowDialog();
            }
            visualizationWindow.Show();
            visualizationWindow.Focus();

        }

    }
}
