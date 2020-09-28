using Communication.Commands;
using Controller.Helper;
using Controller.OutputVisualizer.Export;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Events;
using LiveCharts.Wpf;
using Model.Options;
using Model.Settings;
using Model.Settings.Settings;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Controller.OutputVisualizer
{

    /// <summary>
    ///  The view model for the <see cref="OutputVisualizer"/>.
    /// </summary>
    /// <seealso cref="Controller.BaseController" />
    public class OutputVisualizerController : BaseController
    {
        /// <summary>
        /// The arguments for the event raised when the Align button is pressed
        /// </summary>
        public class AlignTriggeredArgs : EventArgs
        {
            public double MinValue { set; get; }
            public double MaxValue { set; get; }
        }
        /// <summary>
        /// Event handler for the [UserTriggeredAlignHandler] event 
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="AlignTriggeredArgs"/> instance containing the event data.</param>
        public delegate void UserTriggeredAlignHandler(object sender, AlignTriggeredArgs args);


        // ******************** Variables/Objects ********************   
        #region attributes        
        /// <summary>
        /// Occurs when [align triggered].
        /// </summary>
        public event UserTriggeredAlignHandler alignTriggered;

        /// <summary>
        ///  The array that holds the generated output converted to the DateTimePoint type.
        /// </summary>
        private ChartValues<DateTimePoint> outputArray = new ChartValues<DateTimePoint>();

        /// <summary>
        ///  The smallest time step in miliisecond (it's calculated from the sample rate that the user puts in the profile settings).
        /// </summary>
        private double stepSizeMillis;

        /// <summary>
        /// Holds the smallest time step but in 100 nanoseconds (The mapper converts this value to millisecond to be displayed).
        /// </summary>
        private long stepSize100Nanos;

        /// <summary>
        /// The number of samples shown at once (at most) in the chart
        /// </summary>
        private readonly int NUMBER_OF_SAMPLES;

        #endregion


        // ******************** Properties ******************** 
        #region Properties

        public string CardName { set; get; }

        public string ChannelName { set; get; }

        public int ChannelIndex { set; get; }

        private SectionsCollection sectionCollection;

        /// <summary>
        /// Gets or sets the collectionSection, which is used to display the names and the colors of the sequences.
        /// </summary>
        public SectionsCollection SectionCollection
        {
            get { return sectionCollection; }
            set
            {
                sectionCollection = value;
                OnPropertyChanged("SectionCollection");
            }
        }

        private Func<double, string> labelFormatter;

        /// <summary>
        /// Gets or sets the label formatter to restrict the precision of the y axis values to 3 digits after the point.
        /// </summary>
        public Func<double, string> LabelFormatter
        {
            get { return labelFormatter; }
            set
            {
                labelFormatter = value;
                OnPropertyChanged("LabelFormatter");
            }
        }

        private VisualElementsCollection visualElments;

        /// <summary>
        /// Gets ot sets the visual elements. Used to display the names of the sequences since the label attribute of the Sections does not work in this library. 
        /// </summary>
        public VisualElementsCollection VisualElments
        {
            get { return visualElments; }
            set
            {
                visualElments = value;
                OnPropertyChanged("VisualElments");
            }
        }

        private string nameOfCardAndChannel;

        /// <summary>
        /// The name of card and channel that this controller is associated with (separated with a '-')
        /// </summary>
        public string NameOfCardAndChannel
        {
            get { return nameOfCardAndChannel; }
            private set
            {
                nameOfCardAndChannel = value;
                OnPropertyChanged("NameOfCardAndChannel");
            }
        }

        private bool buttonVisibility;

        /// <summary>
        /// Controls whether the align button is enabled or not. Used to make the align button enabled only after the first zoom or drag in the controller
        /// (to avoid the exception that happens if the Min- and MaxValues are not set).
        /// </summary>
        public bool ButtonVisibility
        {
            get { return buttonVisibility; }
            set
            {
                buttonVisibility = value;
                OnPropertyChanged("ButtonVisibility");
            }
        }

        private double _minValue;

        /// <summary>
        /// Sets or gets the minimum visible value of the x axis (time axis) in milliseconds.
        /// </summary>
        public double MinValue
        {
            get { return _minValue; }
            set
            {
                ButtonVisibility = true;
                _minValue = value;
                OnPropertyChanged("MinValue");
            }
        }

        private double _maxValue;

        /// <summary>
        /// Sets or gets the maximum visible value of the x axis (time axis) in milliseconds
        /// </summary>
        public double MaxValue
        {
            get { return _maxValue; }
            set
            {
                ButtonVisibility = true;
                _maxValue = value;
                OnPropertyChanged("MaxValue");
            }
        }

        private ChartValues<DateTimePoint> manipulatedArray;

        /// <summary>
        /// Gets the manipulated array. The output array that is displayed in the output
        /// It contains the values after the sampling and extraction from the original output according to the 
        /// boundaries of the x-axis .
        /// </summary>
        public ChartValues<DateTimePoint> ManipulatedArray
        {
            get { return manipulatedArray; }
            private set
            {
                manipulatedArray = value;
                OnPropertyChanged("ManipulatedArray");
            }
        }

        private RelayCommand alignTriggeredCommand;

        /// <summary>
        /// The command triggered when the Align button is clicked.
        /// </summary>
        public RelayCommand AlignTriggeredCommand
        {
            get { return alignTriggeredCommand; }
            set
            {
                alignTriggeredCommand = value;
                OnPropertyChanged("AlignTriggeredCommand");
            }
        }

        /// <summary>
        /// The command triggered when the ax-xis range changes because of the zooming or dragging actions performed by the user.
        /// </summary>
        public RelayCommand RangeChangedCommand { get; set; }


        public RelayCommand ExportChannelCommand { set; get; }

        #endregion

        #region Constructor
        //************** Constructor*************

        public OutputVisualizerController(string cardName, string channelName, int channelIndex)
        {
            CardName = cardName;
            ChannelName = channelName;
            ChannelIndex = channelIndex;
            NameOfCardAndChannel = cardName + "-" + channelName;

            NUMBER_OF_SAMPLES = OptionsManager.GetInstance().GetOptionValueByName<int>(OptionNames.VISUALIZED_SAMPLES);
            // Maps the 100 nanosecond values that are passed as datetime type (number of ticks) to millisecond, and display the y values without any change (in volt)
            var toMillisMapper = Mappers.Xy<DateTimePoint>()
            .X(dayModel => (double)((dayModel.DateTime.Ticks) * 0.00000001))
            .Y(dayModel => dayModel.Value);

            // This method saves the mapper at the application level, 
            // every time LiveCharts detects the DateTimePoint type in a Chart Values instance, it will use the toMillisMapper.
            Charting.For<DateTimePoint>(toMillisMapper);

            VisualElments = new VisualElementsCollection();
            SectionCollection = new SectionsCollection();
            CalculateStepSize();
            MaxValue = double.NaN;
            MinValue = double.NaN;
            LabelFormatter = x => string.Format("{0:0.000}", x);
            ButtonVisibility = false;
            RangeChangedCommand = new RelayCommand(OnAxisChanged);
            //align
            AlignTriggeredCommand = new RelayCommand(AlignUserControls);
            ExportChannelCommand = new RelayCommand(ShowExportWindow);
        }
        #endregion



        //******************** Methods ********************        
        #region Methods

        /// <summary>
        /// Calculates the size of the step in 100nanos unit (DateTime Ticks) based on the step size in millis retrieved from
        /// <seealso cref="TimeSettingsInfo"/>.
        /// </summary>
        private void CalculateStepSize()
        {
            stepSizeMillis = TimeSettingsInfo.GetInstance().SmallestTimeStep;
            stepSize100Nanos = (long)(stepSizeMillis * 100000000);//the smallest time step in 100 nanosecond
        }

        /// <summary>
        /// Triggers the alignment event of other controllers based on this controller
        /// </summary>
        /// <param name="args"></param>
        protected void OnAlignTriggered(AlignTriggeredArgs args)
        {
            alignTriggered?.Invoke(this, args);
        }


        /// <summary>
        /// Determines the indices of the min and max x-axis values within the output array, and triggers resampling 
        /// </summary>
        public void ChangeAxis()
        {
            double min = Math.Max(0, MinValue);
            int arrayMin = (int)Math.Round(min / stepSizeMillis);
            int arrayMax = (int)Math.Round(MaxValue / stepSizeMillis);
            arrayMax = Math.Min(arrayMax, outputArray.Count);
            SampleOutput(arrayMin, arrayMax);
        }

        /// <summary>
        /// Calls the ChangeAxis method when the axis changes 
        /// </summary>
        /// <param name="parameter">not used</param>
        public void OnAxisChanged(object parameter)
        {
            ChangeAxis();
        }

        /// <summary>
        /// Triggers an event to align the other controllers based on the min and max values of the x-axis of this controller
        /// </summary>
        /// <param name="parameter">not used</param>
        private void AlignUserControls(object parameter)
        {
            AlignTriggeredArgs args = new AlignTriggeredArgs();
            args.MinValue = MinValue;
            args.MaxValue = MaxValue;
            OnAlignTriggered(args);
        }


        /// <summary>
        /// This method converts the values of the output array into DateTimePoint type to be converted later to milliseconds
        /// </summary>
        /// <param name="output">The output.</param>
        public void SetData(double[] output)
        {
            int sizeOfArray = output.Length;
            outputArray.Clear();

            // populate the outputArray
            for (int i = 0; i < sizeOfArray; i++)
            {
                DateTime dt = new DateTime(i * stepSize100Nanos);
                DateTimePoint dtp = new DateTimePoint(dt, output[i]);
                outputArray.Add(dtp);
            }

            // if the min and max values are not set before, make them correspond to the whole array
            if (Double.IsNaN(MinValue) || Double.IsNaN(MaxValue))
            {
                _minValue = 0;
                _maxValue = sizeOfArray * stepSizeMillis;
                ChangeAxis();
            }
            else
            {
                // try to use the existing Min, Max before sampling
                ChangeAxis();
            }
        }

        /// <summary>
        /// Samples the currently displayed array in such a way that at most NUMBER_OF_SAMPLES values are displayed
        /// picked with equal intervals.
        /// </summary>
        /// <param name="min">The minimum shown time value index.</param>
        /// <param name="max">The maximum shown time value index.</param>
        public void SampleOutput(int min, int max)
        {
            ChartValues<DateTimePoint> tempArray = new ChartValues<DateTimePoint>();
            double temp = (max - min) / NUMBER_OF_SAMPLES;
            int samplingStep = (int)Math.Ceiling(temp);

            if (samplingStep == 0)
            {
                samplingStep = 1;
            }

            int k = min;

            for (int j = 0; j < NUMBER_OF_SAMPLES && k < max; j++)
            {
                tempArray.Add(outputArray[k]);
                k += samplingStep;
            }

            ManipulatedArray = tempArray;
        }

        private void ShowExportWindow(object parameter)
        {
            Dictionary<string, List<int>> channels = new Dictionary<string, List<int>>
            {
                { CardName, new List<int>() { ChannelIndex } }
            };

            ExportWindowController controller = new ExportWindowController(VisualizationWindowManager.GetInstance().OutputVisualizationController, channels );
            Window window = WindowsHelper.CreateWindowToHostViewModel(controller, true, true);
            window.Title = "Output Exporter";
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.ShowDialog();
        }
        #endregion

    }
}



