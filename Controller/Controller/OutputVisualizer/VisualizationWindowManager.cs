using Controller.Control.StepBatchAddition;
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


        private VisualizationWindowManager(RootController root)
        {
            CTVViewModel treeView = ModelBasedCTVBuilder.BuildCheckableTree(root);
            outputVisualizationController = new OutputVisualizationWindowController(root, treeView);

        }

        public static VisualizationWindowManager GetInstance(RootController root)
        {
            if (singleton == null)
                singleton = new VisualizationWindowManager(root);

            return singleton;
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
