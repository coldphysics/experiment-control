using AbstractController.Data.Card;
using AbstractController.Data.Channels;
using AbstractController.Data.Sequence;
using Buffer.OutputProcessors;
using Communication.Commands;
using Communication.Interfaces.Generator;
using Controller.Control.StepBatchAddition;
using Controller.Data.Channels;
using Controller.Data.Tabs;
using Controller.MainWindow;
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
        private IModelOutput lastKnownOutput;

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
        /// Gets or sets the output visualizer collection uc which contains a collection of <see cref=" OutputVisualizerController" /> 
        /// according to the channels that are selected in the treeView by the user 
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
        /// The command  triggered when the Refresh button is clicked to display the <see cref=" OutputVisualizerController"/> that are checked in the tree view.
        /// </summary>
        /// <value> The command triggered when the Refresh button is clicked </value>
        public RelayCommand UserControlCollectionCommand { get; private set; }

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
        /// <param name="root">the root</param>
        /// <param name="treeViewController"> the visualization tree view controller</param>
        public OutputVisualizationWindowController(CTVViewModel treeViewController, MainWindowController mainWindowController)
            : base(mainWindowController)
        {
            this.VisualizationTreeViewController = treeViewController;
            OutputVisualizerCollectionUC = new ObservableCollection<OutputVisualizerController>();
            AllControllers = new ObservableCollection<OutputVisualizerController>();
            //Add intialize commands method in case we have multiple commands
            UserControlCollectionCommand = new RelayCommand(RefreshControllers);
            BuildControllers(mainWindowController.GetRootController());
            VisualizationTreeViewController.CheckStateChanged += VisualizationTreeViewController_CheckStateChanged;
        }

        //******************** Methods ********************      

        private RootController GetRootController()
        {
            return ((MainWindowController)parent).GetRootController();
        }

        public void HandleNewGeneratedOutputEvent()
        {
            if (AutomaticRefresh)
            {
                RefreshControllers(null);
            }
        }

        public void HandleWindowOpeningEvent()
        {
            RefreshControllers(null);
        }

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
                string name = string.Format("{0} {1}", ((TabController)sequence).Name, sequence.Index().ToString());
                double startTime = sequence.ActualStartTime();
                double duration = sequence.LongestDurationAllSequences();

                VisualElement nameOfSequence = new VisualElement();

                //the position of the sequence name
                nameOfSequence.X = startTime + (duration / 2);
                nameOfSequence.Y = Double.NaN;

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

        private void AddDataToChannel(CTVItemViewModel channel)
        {
            ChannelBasicController channelController = (ChannelBasicController)(channel as CheckableTVItemController).Item;
            string cardName = channelController.Parent.Parent.Model.Name;
            OutputVisualizerController ovc = GetControllerOfChannel(channel);

            if (lastKnownOutput.Output[cardName] is INonQuantizedCardOutput)
            {
                double[] tempList = ((INonQuantizedCardOutput)lastKnownOutput.Output[cardName]).GetChannelOutput(channelController.Index());
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
            if (lastKnownOutput != null)
            {
                ICollection<CTVItemViewModel> checkedChannels = this.visualizationTreeViewController.GetCheckedLeaves();

                foreach (var channel in checkedChannels)
                {
                    AddDataToChannel(channel);
                }
            }

        }

        private OutputVisualizerController GetControllerOfChannel(CTVItemViewModel channel)
        {
            ChannelBasicController channelController = (ChannelBasicController)(channel as CheckableTVItemController).Item;
            string cardName = channelController.Parent.Parent.Model.Name;
            string controllerName = cardName + "-" + channelController.ToString();
            // select the corresponding controller
            return AllControllers
                .Where((controller) => controller.NameOfCardAndChannel.Equals(controllerName))
                .First();
        }

        private void RefreshControllers(object param)
        {
            ProcessorListManager plm = ProcessorListManager.GetInstance();

            if (plm.saver != null)
            {
                // Get the output that was saved before the quantization and compression steps.
                this.lastKnownOutput = plm.saver.GetVisualizerOutput();

                OutputVisualizerCollectionUC.Clear();
                BuildSectionsForVisibleChannels();
                AddDataToVisibleChannels();
            }
        }


        private void BuildControllers(RootController rootController)
        {
            foreach (AbstractCardController card in rootController.DataController.SequenceGroup.Windows)
            {
                foreach (AbstractChannelController channel in card.Tabs.First().Channels)
                {
                    OutputVisualizerController ovc = new OutputVisualizerController();
                    //to display the name of each sequence in the outputVisualizer control.
                    ovc.NameOfCardAndChannel = card.Model.Name + "-" + channel.ToString();
                    ovc.alignTriggered += controller_AlignTriggered;
                    AllControllers.Add(ovc);
                }
            }
        }


        /// <summary>
        /// Handles the AlignTriggered event of the controller control.Align all <see cref="OutputVisualizer "/>  controls to the values of the sender control.(align the x axis)
        /// </summary>
        /// <param name="sender">The source of the event.</param>
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

    }
}

