using AbstractController.Data.Sequence;
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
        ///Gets or sets the output visualizer collection uc which contains a collection of <see cref=" OutputVisualizerController" /> 
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
        /// The command  triggered when the Refresh button is clicked to display the <see cref=" OutputVisualizerController"/> that are checked in the tree view.
        /// </summary>
        /// <value> The command triggered when the Refresh button is clicked </value>
        private RelayCommand userControlCollectionCommand;

        public RelayCommand UserControlCollectionCommand
        {
            get { return userControlCollectionCommand; }
            private set { userControlCollectionCommand = value; }
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
            //Add intialize commands method in case we have multiple commands
            UserControlCollectionCommand = new RelayCommand(buildUserControls);
        }


        //******************** Methods ********************        
        /// <summary>
        /// Builds the user controls when the "Refresh" button is clicked
        /// </summary>
        /// <param name="parameter">not used here.</param>
        /// <exception cref="System.Exception">Card must be Non-Quantized</exception>
        private void buildUserControls(object parameter)
        {
           
            foreach (var controller in outputVisualizerCollectionUC)
            {
                controller.alignTriggered -= controller_AlignTriggered;
            }
            //clear the collection of the "OutputVisualizer" controls at the beginning
            outputVisualizerCollectionUC.Clear();

           
            ICollection<CTVItemViewModel> checkedChannels = this.visualizationTreeViewController.GetCheckedLeaves();
            ChannelBasicController channelController = null;
            ProcessorListManager plm = ProcessorListManager.GetInstance();

            //Get the output that was saved before the quantization and compression steps.
            IModelOutput output = plm.saver.GetVisualizerOutput();

            //get a collection of all sequences in the model.
            ObservableCollection<AbstractSequenceController> seq1 = ((ChannelBasicController)((checkedChannels.ElementAt(0) as CheckableTVItemController).Item)).Parent.Parent.Tabs;

            //Generate random colors for all sequences
            Random random = new Random();
            List<Color> colorArray = new List<Color>();
            foreach (var sequence in seq1)
            {
                Color color = Color.FromArgb((byte)random.Next(0, 256), (byte)random.Next(0, 256), (byte)random.Next(0, 256), (byte)random.Next(0, 256));
                colorArray.Add(color);

            }


            foreach (var channel in checkedChannels)
            {
                channelController = (ChannelBasicController)(channel as CheckableTVItemController).Item;
                string cardName = ((WindowBasicController)channelController.Parent.Parent).Name;
                ObservableCollection<AbstractSequenceController> seq = channelController.Parent.Parent.Tabs;

              
                if (output.Output[cardName] is INonQuantizedCard)
                {
                    double[] tempList = ((INonQuantizedCard)output.Output[cardName]).GetChannelOutput(channelController.Index());

                    OutputVisualizerController ovc = new OutputVisualizerController(tempList);

                    //to display the color of each sequence in the outputVisualizer control.
                    ovc.SectionCollection = new SectionsCollection();

                    //to display the name of each sequence in the outputVisualizer control.
                    ovc.VisualElments = new VisualElementsCollection();

                    ovc.NameOfCardAndChannel = cardName + "-" + channelController.ToString();
                 
                    
                    int colorNumber = 0;
                    foreach (var sequence in seq)
                    {
                      
                        string indexOfsequence = sequence.Index().ToString();
                        double startTime = sequence.ActualStartTime();
                        // double duration = sequence.ActualDuration();
                        //double duration = sequence.ActualDuration();
                       double duration = sequence.LongestDurationAllSequences();
                        AxisSection section = new AxisSection();
                        VisualElement nameOfSequence = new VisualElement();

                        //the position of the sequence name
                        nameOfSequence.X = startTime + (duration / 2);
                        nameOfSequence.Y = Double.NaN;

                        TextBlock text = new TextBlock();
                        text.Text = ((TabController)sequence).Name + " " + indexOfsequence;
                        nameOfSequence.UIElement = text;
                        nameOfSequence.HorizontalAlignment = HorizontalAlignment.Center;
                        nameOfSequence.VerticalAlignment = VerticalAlignment.Bottom;


                        section.Value = startTime;
                        section.SectionWidth = duration;


                        Color color = colorArray.ElementAt(colorNumber);
                        section.Fill = new SolidColorBrush
                        {
                            Color = color,
                            Opacity = .4
                        };

                        ovc.SectionCollection.Add(section);
                        ovc.VisualElments.Add(nameOfSequence);
                        colorNumber++;

                    }

                    OutputVisualizerCollectionUC.Add(ovc);
                }
                else
                {
                    throw new Exception("Card must be Non-Quantized");

                }

            }

            foreach (var controller in outputVisualizerCollectionUC)
            {
                controller.alignTriggered += controller_AlignTriggered;
                
            }
        }



        /// <summary>
        /// Handles the AlignTriggered event of the controller control.Align all <see cref="OutputVisualizer "/>  controls to the values of the sender control.(align the x axis)
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="LiveCharts.Events.RangeChangedEventArgs"/> instance containing the event data.</param>
        void controller_AlignTriggered(object sender, LiveCharts.Events.RangeChangedEventArgs args)
        {
            foreach (var controller in outputVisualizerCollectionUC)
            {
                controller.MinValue = ((LiveCharts.Wpf.Axis)args.Axis).MinValue;
                controller.MaxValue = ((LiveCharts.Wpf.Axis)args.Axis).MaxValue;
                controller.ChangeAxis(args);   
            }
        }
       
    }
}

