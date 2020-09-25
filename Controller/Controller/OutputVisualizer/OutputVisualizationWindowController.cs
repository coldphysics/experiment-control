using AbstractController.Data.Card;
using AbstractController.Data.Channels;
using AbstractController.Data.Sequence;
using Buffer.OutputProcessors;
using Communication.Commands;
using Communication.Interfaces.Generator;
using Controller.Common;
using Controller.Control.StepBatchAddition;
using Controller.Data.Channels;
using Controller.Data.Tabs;
using Controller.Helper;
using Controller.MainWindow;
using Controller.OutputVisualizer.Export;
using Controller.Root;
using CustomElements.CheckableTreeView;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using static Controller.OutputVisualizer.OutputVisualizerController;

namespace Controller.OutputVisualizer
{
    /// <summary>
    /// the view model for the <see cref="OutputVisualizerWindow "/> . It controls the interactions between the OutputVisualizer user controls.
    /// </summary>
    /// <seealso cref="Controller.BaseController" />
    public class OutputVisualizationWindowController : ChildController
    {
        /// <summary>
        /// The last output known to this controller via (1) opening the window (2) explicitly clicking refresh
        /// (3) receiveing an automatic refresh (if applicable).
        /// Needed so that just selecting or unselecting displayed channels does not do an implicit refresh.
        /// </summary>
        public IModelOutput LastKnownOutput { private set; get; }

        // ******************** Properties ******************** 
        #region Properties

        /// <summary>
        ///Gets or sets the visualization TreeView controller
        /// </summary>
        private CTVViewModel visualizationTreeViewController;
        public CTVViewModel VisualizationTreeViewController
        {
            get { return visualizationTreeViewController; }
            set
            {
                visualizationTreeViewController = value;
                OnPropertyChanged("visualizationTreeViewController");
            }
        }

        /// <summary>
        /// Gets or sets the output visualizer collection which contains a collection of <see cref=" OutputVisualizerController" /> 
        /// that correspond to the channels that are selected in the treeView by the user.
        /// </summary>
        private ObservableCollection<OutputVisualizerController> outputVisualizerCollectionUC;

        public ObservableCollection<OutputVisualizerController> OutputVisualizerCollectionUC
        {
            get { return outputVisualizerCollectionUC; }
            set
            {
                outputVisualizerCollectionUC = value;
                OnPropertyChanged("OutputVisualizerCollectionUC");
            }
        }

        /// <summary>
        /// Gets or sets the output visualizer collection, which contains a collection of all <see cref=" OutputVisualizerController" /> 
        /// </summary>
        public ObservableCollection<OutputVisualizerController> AllControllers
        {
            get;
            set;
        }
        /// <summary>
        /// The command  triggered when the Refresh button is clicked to update the latest known output model and visualize it.
        /// </summary>
        public RelayCommand UserControlCollectionCommand { get; private set; }


        public RelayCommand ExportSelectedChannelsCommand { private set; get; }

        /// <summary>
        /// Indicates whether automatic refreshes of the last known output model are enabled or not.
        /// Automatic refreshes take place when a new output generation operation is done.
        /// </summary>
        public bool AutomaticRefresh
        {
            set;
            get;
        }

        #endregion

        //************** Constructor*************

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputVisualizationWindowController"/> class.
        /// </summary>
        /// <param name="mainWindowController">The parent controller (gives access to the buffer)</param>
        /// <param name="treeViewController">The visualization tree view controller</param>
        public OutputVisualizationWindowController(MainWindowController mainWindowController)
            : base(mainWindowController)
        {
            OutputVisualizerCollectionUC = new ObservableCollection<OutputVisualizerController>();
            AllControllers = new ObservableCollection<OutputVisualizerController>();
            UserControlCollectionCommand = new RelayCommand(RefreshControllers);
            ExportSelectedChannelsCommand = new RelayCommand(ExportSelectedChannels);
            // We create the sub-controllers only once so we retain their views when the underlying data changes
            BuildControllers(mainWindowController.GetRootController());
            VisualizationTreeViewController.CheckStateChanged += VisualizationTreeViewController_CheckStateChanged;
        }

        //******************** Methods ********************      

        public RootController GetRootController()
        {
            return ((MainWindowController)parent).GetRootController();
        }

        /// <summary>
        /// Invoked whenever a new output is generated
        /// </summary>
        public void HandleNewGeneratedOutputEvent()
        { 
            if (AutomaticRefresh)
            {
                RefreshControllers(null);
            }
        }

        /// <summary>
        /// Invoked whenever the associated window opens
        /// </summary>
        public void HandleWindowOpeningEvent()
        {
            // refresh the checkable tree view (in case number of cards or names of channels change)
            VisualizationTreeViewController = ModelBasedCTVBuilder.BuildCheckableTree(GetRootController());
            RefreshControllers(null);
        }

        /// <summary>
        /// Invoked when the check-state of any element of the channels tree view is changed.
        /// Used to either display or hide the corresponding channel output visualizers.
        /// </summary>
        /// <param name="sender">The tree element whose state is changed</param>
        /// <param name="e">Not used</param>
        private void VisualizationTreeViewController_CheckStateChanged(object sender, EventArgs e)
        {
            CTVItemViewModel checkedItem = (CTVItemViewModel)sender;
            // Check if this is a leaf node, i.e., a channel
            if (checkedItem.Children == null || checkedItem.Children.Count == 0)
            {
                if (checkedItem.IsChecked.Value)
                {
                    BuildSectionsForChannel(checkedItem);
                    AddDataToChannel(checkedItem);
                }
                else
                {
                    RemoveSectionsOfChannel(checkedItem);
                    RemoveDataFromChannel(checkedItem);
                }

            }
        }

        /// <summary>
        /// Builds the sections, i.e., the sequence colors and labels, for a given channel.
        /// </summary>
        /// <param name="channel">The checked channel to be displayed</param>
        private void BuildSectionsForChannel(CTVItemViewModel channel)
        {
            const int COLOR_SEED = 1200;
            const double OPACITY = 0.4;
            ObservableCollection<AbstractSequenceController> sequeces =
                GetRootController().DataController.SequenceGroup.Windows.First().Tabs;
            // providing the same seed everytime, ensures that colors are generated in the same order always (not totally random :))
            Random random = new Random(COLOR_SEED);
            OutputVisualizerController ovc = GetControllerOfChannel(channel);
            ClearSectionsOfChannel(ovc);
            int seqIndex = 0;

            foreach (AbstractSequenceController sequence in sequeces)
            {
                Color color = Color.FromArgb((byte)random.Next(0, 256), (byte)random.Next(0, 256), (byte)random.Next(0, 256), (byte)random.Next(0, 256));
                string name = $"{((TabController) sequence).Name} ({sequence.Index().ToString()})";
                double startTime = sequence.ActualStartTime();
                double duration = sequence.LongestDurationAllSequences() * GetRootController().TimesToReplicateOutput;

                VisualElement nameOfSequence = new VisualElement();

                //the position of the sequence name
                nameOfSequence.X = startTime + (duration / 2);
                nameOfSequence.Y = double.NaN;

                TextBlock text = new TextBlock();
                text.Text = name;
                nameOfSequence.UIElement = text;
                nameOfSequence.HorizontalAlignment = HorizontalAlignment.Center;
                nameOfSequence.VerticalAlignment = VerticalAlignment.Bottom;

                AxisSection section = new AxisSection();
                section.Value = startTime;
                section.SectionWidth = duration;
                section.Fill = new SolidColorBrush
                {
                    Color = color,
                    Opacity = OPACITY
                };

                ovc.SectionCollection.Add(section);
                ovc.VisualElments.Add(nameOfSequence);
                ++seqIndex;
            }

        }

        private void RemoveSectionsOfChannel(CTVItemViewModel channel)
        {
            ClearSectionsOfChannel(GetControllerOfChannel(channel));
        }

        private void BuildSectionsForVisibleChannels()
        {
            ICollection<CTVItemViewModel> checkedChannels = this.visualizationTreeViewController.GetCheckedLeaves();

            foreach (var channel in checkedChannels)
            {
                BuildSectionsForChannel(channel);
            }
        }

        private void ClearSectionsOfChannel(OutputVisualizerController ovc)
        {
            if (ovc.SectionCollection != null && ovc.VisualElments != null)
            {
                foreach (AxisSection section in ovc.SectionCollection)
                {
                    BindingOperations.ClearAllBindings(section);
                }

                foreach (VisualElement ve in ovc.VisualElments)
                {
                    BindingOperations.ClearAllBindings(ve);
                }

                ovc.SectionCollection = new SectionsCollection();
                ovc.VisualElments = new VisualElementsCollection();
            }

        }

        /// <summary>
        /// Associates a channels visualization controller with the data it needs to visualize
        /// </summary>
        /// <param name="channel">The checked channel to be displayed</param>
        private void AddDataToChannel(CTVItemViewModel channel)
        {
            ChannelBasicController channelController = (ChannelBasicController)((CheckableTVItemController) channel).Item;
            string cardName = channelController.Parent.Parent.Model.Name;
            OutputVisualizerController ovc = GetControllerOfChannel(channel);

            if (LastKnownOutput.Output[cardName] is INonQuantizedCardOutput)
            {
                double[] tempList = ((INonQuantizedCardOutput)LastKnownOutput.Output[cardName]).GetChannelOutput(channelController.Index());
                // select the corresponding controller
                ovc.SetData(tempList);
                OutputVisualizerCollectionUC.Add(ovc);
            }
            else
            {
                throw new Exception("Card output must be Non-Quantized");
            }
        }

        private void RemoveDataFromChannel(CTVItemViewModel channel)
        {
            OutputVisualizerController ovc = GetControllerOfChannel(channel);
            ovc.SetData(new double[0]);
            OutputVisualizerCollectionUC.Remove(ovc);
        }

        private void AddDataToVisibleChannels()
        {
            if (LastKnownOutput != null)
            {
                ICollection<CTVItemViewModel> checkedChannels = this.visualizationTreeViewController.GetCheckedLeaves();

                foreach (var channel in checkedChannels)
                {
                    AddDataToChannel(channel);
                }
            }

        }

        /// <summary>
        /// Given a checked tree view element, returns the corresponding channel visalization controller.
        /// </summary>
        /// <param name="channel">a checked tree view element</param>
        /// <returns>the corresponding channel visualization controller</returns>
        private OutputVisualizerController GetControllerOfChannel(CTVItemViewModel channel)
        {
            ChannelBasicController channelController = (ChannelBasicController)(channel as CheckableTVItemController).Item;
            string cardName = channelController.Parent.Parent.Model.Name;
            // select the corresponding controller
            return AllControllers
                .First(controller => controller.CardName == cardName && controller.ChannelIndex == channelController.Index());
        }

        /// <summary>
        /// Occurs when the last known output needs to be updated
        /// </summary>
        /// <param name="param">Not used</param>
        private void RefreshControllers(object param)
        {
            // retrieve the latst output
            ProcessorListManager plm = ProcessorListManager.GetInstance();

            if (plm.saver != null)
            {
                // Get the output that was saved before the quantization and compression steps.
                this.LastKnownOutput = plm.saver.GetVisualizerOutput();

                OutputVisualizerCollectionUC.Clear();
                BuildSectionsForVisibleChannels();
                AddDataToVisibleChannels();
            }
        }


        private void BuildControllers(RootController rootController)
        {
            VisualizationTreeViewController = ModelBasedCTVBuilder.BuildCheckableTree(GetRootController());

            foreach (AbstractCardController card in rootController.DataController.SequenceGroup.Windows)
            {
                foreach (AbstractChannelController channel in card.Tabs.First().Channels)
                {
                    OutputVisualizerController ovc = new OutputVisualizerController(card.Model.Name, channel.ToString(), channel.Index());
                    ovc.alignTriggered += controller_AlignTriggered;
                    AllControllers.Add(ovc);
                }
            }
        }


        /// <summary>
        /// Handles the AlignTriggered event of the controller control. Aligns all <see cref="OutputVisualizer "/> controls to the values of the sender control.(align the x axis)
        /// </summary>
        /// <param name="sender">The source of the event (not used).</param>
        /// <param name="args">The <see cref="AlignTriggeredArgs"/> instance containing the event data.</param>
        private void controller_AlignTriggered(object sender, AlignTriggeredArgs args)
        {
            foreach (var controller in outputVisualizerCollectionUC)
            {
                controller.MinValue = args.MinValue;
                controller.MaxValue = args.MaxValue;
                controller.ChangeAxis();
            }
        }

        /// <summary>
        /// Shows the output export window initialized with the set of channels selected in this window
        /// </summary>
        /// <param name="parameter">not used</param>
        private void ExportSelectedChannels(object parameter)
        {
            ExportWindowController controller = new ExportWindowController(this, visualizationTreeViewController);
            Window window = WindowsHelper.CreateWindowToHostViewModel(controller, true, true);
            window.Title = "Output Exporter";
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.ShowDialog();
        }

    }
}

