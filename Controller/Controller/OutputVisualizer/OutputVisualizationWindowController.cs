using AbstractController.Data.Card;
using AbstractController.Data.Channels;
using AbstractController.Data.Sequence;
using Buffer.Basic;
using Buffer.OutputProcessors;
using Communication.Commands;
using Communication.Interfaces.Generator;
using Controller.Control.StepBatchAddition;
using Controller.Data.Channels;
using Controller.Data.Tabs;
using Controller.Data.Windows;
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
using System.Windows.Media;
using System.Windows.Threading;
using static Controller.OutputVisualizer.OutputVisualizerController;

namespace Controller.OutputVisualizer
{
    /// <summary>
    /// the view model for the <see cref="OutputVisualizerWindow "/> . It controls the interactions between the OutputVisualizer user controls.
    /// </summary>
    /// <seealso cref="Controller.BaseController" />
    public class OutputVisualizationWindowController : BaseController
    {

        // ******************** Variables/Objects ******************** 
        #region Attributes
        private RootController rootController;
        #endregion

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
        private RelayCommand userControlCollectionCommand;

        public RelayCommand UserControlCollectionCommand
        {
            get { return userControlCollectionCommand; }
            private set { userControlCollectionCommand = value; }
        }

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
        public OutputVisualizationWindowController(RootController root, CTVViewModel treeViewController)
        {

            this.rootController = root;
            this.VisualizationTreeViewController = treeViewController;
            OutputVisualizerCollectionUC = new ObservableCollection<OutputVisualizerController>();
            AllControllers = new ObservableCollection<OutputVisualizerController>();
            //Add intialize commands method in case we have multiple commands
            UserControlCollectionCommand = new RelayCommand(RefreshControllers);
            BuildControllers();
        }


        //******************** Methods ********************       
        public void HandleNewGeneratedOutputEvent()
        {
            if (AutomaticRefresh)
            {
                RefreshControllers(null);
            }
        }

        private void BuildSections()
        {
            const int COLOR_SEED = 1200;
            const double OPACITY = 0.4;
            ObservableCollection<AbstractSequenceController> sequeces =
                rootController.DataController.SequenceGroup.Windows.First().Tabs;
            // providing the same seed everytime, ensures that colors are generated in the same order always (not totally random :))
            Random random = new Random(COLOR_SEED);
            Color color;
            // Clear existing sections
            foreach (OutputVisualizerController ovc in OutputVisualizerCollectionUC)
            {
                ovc.SectionCollection = new SectionsCollection();
                ovc.VisualElments = new VisualElementsCollection();
            }

            foreach (AbstractSequenceController sequence in sequeces)
            {
                color = Color.FromArgb((byte)random.Next(0, 256), (byte)random.Next(0, 256), (byte)random.Next(0, 256), (byte)random.Next(0, 256));

                string indexOfsequenceStr = sequence.Index().ToString();
                string nameOfSequenceStr = ((TabController)sequence).Name;
                double startTime = sequence.ActualStartTime();
                double duration = sequence.LongestDurationAllSequences();

                foreach (OutputVisualizerController ovc in AllControllers)
                {
                    VisualElement nameOfSequence = new VisualElement();

                    //the position of the sequence name
                    nameOfSequence.X = startTime + (duration / 2);
                    nameOfSequence.Y = Double.NaN;

                    TextBlock text = new TextBlock();
                    text.Text = nameOfSequenceStr + " " + indexOfsequenceStr;
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
                }
            }
        }

        private void AddDataToVisibleChannels()
        {
            ProcessorListManager plm = ProcessorListManager.GetInstance();

            if (plm.saver != null)
            {
                // Get the output that was saved before the quantization and compression steps.
                IModelOutput output = plm.saver.GetVisualizerOutput();
                ICollection<CTVItemViewModel> checkedChannels = this.visualizationTreeViewController.GetCheckedLeaves();
                ChannelBasicController channelController;

                foreach (var channel in checkedChannels)
                {
                    channelController = (ChannelBasicController)(channel as CheckableTVItemController).Item;
                    ObservableCollection<AbstractSequenceController> seq = channelController.Parent.Parent.Tabs;
                    string cardName = channelController.Parent.Parent.Model.Name;

                    if (output.Output[cardName] is INonQuantizedCardOutput)
                    {
                        string controllerName = cardName + "-" + channelController.ToString();
                        double[] tempList = ((INonQuantizedCardOutput)output.Output[cardName]).GetChannelOutput(channelController.Index());
                        // select the corresponding controller
                        OutputVisualizerController ovc = AllControllers
                            .Where((controller) => controller.NameOfCardAndChannel.Equals(controllerName))
                            .First();
                        ovc.SetData(tempList);
                        OutputVisualizerCollectionUC.Add(ovc);
                    }
                    else
                    {
                        throw new Exception("Card output must be Non-Quantized");
                    }
                }
            }

        }

        private void RefreshControllers(object param)
        {
            OutputVisualizerCollectionUC.Clear();
            BuildSections();
            AddDataToVisibleChannels();
        }

        private void BuildControllers()
        {
            foreach (AbstractCardController card in rootController.DataController.SequenceGroup.Windows)
            {
                foreach (AbstractChannelController channel in card.Tabs.First().Channels)
                {
                    OutputVisualizerController ovc = new OutputVisualizerController();
                    ovc.SectionCollection = new SectionsCollection();
                    //to display the name of each sequence in the outputVisualizer control.
                    ovc.VisualElments = new VisualElementsCollection();
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
        void controller_AlignTriggered(object sender, AlignTriggeredArgs args)
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

