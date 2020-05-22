using Communication.Commands;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Events;
using LiveCharts.Wpf;
using Model.Options;
using Model.Settings;
using Model.Settings.Settings;
using System;

namespace Controller.OutputVisualizer
{

    /// <summary>
    ///  The view model for the <see cref=" OutputVisualizer"/>.
    /// </summary>
    /// <seealso cref="Controller.BaseController" />
    public class OutputVisualizerController : BaseController
    {
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
        ///  the array that holds the generated output converted to the DateTimePoint type.
        /// </summary>
        public ChartValues<DateTimePoint> outputArray = new ChartValues<DateTimePoint>();
        /// <summary>
        ///  the smallest time step in miliisecond (it's calculated from the sample rate that the user puts in the profile settings).
        /// </summary>
        private double stepSizeMillis;
        /// <summary>
        /// holds the smallest time step but in 100 nanoseconds (The mapper converts this value to millisecond to be displayed).
        /// </summary>
        private long stepSize100Nanos;

        /// <summary>
        /// The number of samples shown at once in the chart
        /// </summary>
        private readonly int NUMBER_OF_SAMPLES;

        #endregion


        // ******************** Properties ******************** 
        #region Properties

        /// <summary>
        /// Gets or sets the collectionSection which is used to display the names and the colors of the sequences.
        /// </summary>
        private SectionsCollection sectionCollection;
        public SectionsCollection SectionCollection
        {
            get { return sectionCollection; }
            set
            {
                sectionCollection = value;
                OnPropertyChanged("SectionCollection");
            }
        }

        /// <summary>
        /// Gets or sets the label formatter to restrict the precision of the y axis values to 3 digits after the point.
        /// </summary>
        Func<double, string> labelFormatter;

        public Func<double, string> LabelFormatter
        {
            get { return labelFormatter; }
            set
            {
                labelFormatter = value;
                OnPropertyChanged("LabelFormatter");
            }
        }


        /// <summary>
        /// Gets ot sets the visual elements. Used to display the names of the sequences since the label attribute of the Sections does not work in this library. 
        /// </summary>
        private VisualElementsCollection visualElments;

        public VisualElementsCollection VisualElments
        {
            get { return visualElments; }
            set
            {
                visualElments = value;
                OnPropertyChanged("VisualElments");
            }
        }


        /// <summary>
        /// The name of card and channel that were entered by the user.
        /// </summary>
        private string nameOfCardAndChannel;

        public string NameOfCardAndChannel
        {
            get { return nameOfCardAndChannel; }
            set
            {
                nameOfCardAndChannel = value;
                OnPropertyChanged("NameOfCardAndChannel");
            }
        }


        /// <summary>
        /// The button visibility.Used to make the align button visible after the first zoom or drag in the controller
        /// (to avoid the exception that happens if there is no parameters sent).
        /// </summary>
        private bool buttonVisibility;

        public bool ButtonVisibility
        {
            get { return buttonVisibility; }
            set
            {
                buttonVisibility = value;
                OnPropertyChanged("ButtonVisibility");
            }
        }

        /// <summary>
        /// The minimum value of the x axis that is used by the <see cref=" OutputVisualizationWindowController"/> when align button is clicked
        /// to adjust all other controllers to this value.
        /// </summary>
        private double _minValue;

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

        /// <summary>
        /// The maximum value of the x axis that is used by the <see cref=" OutputVisualizationWindowController"/> when align button is clicked
        /// to adjust all other controllers to this value.
        /// </summary>
        private double _maxValue;

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



        /// <summary>
        /// The manipulated array. The output array that is displayed in the output
        /// It contains the values after the sampling and extracting the values from the original output according to the 
        /// boundaries of the axis .
        /// </summary>
        private ChartValues<DateTimePoint> manipulatedArray;

        public ChartValues<DateTimePoint> ManipulatedArray
        {
            get { return manipulatedArray; }
            set
            {
                manipulatedArray = value;
                OnPropertyChanged("ManipulatedArray");
            }
        }

        /// <summary>
        /// The command triggered when the Align button is clicked.
        /// </summary>
        private RelayCommand alignTriggeredCommand;

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
        /// The command triggered when the axis range changes because of the zooming or dragging actions performed by the user.
        /// </summary>
        private RelayCommand rangeChangedCommand;

        public RelayCommand RangeChangedCommand
        {
            get
            {
                return rangeChangedCommand;
            }
            set
            {
                rangeChangedCommand = value;
            }
        }


        #endregion



        #region Constructor
        //************** Constructor*************

        public OutputVisualizerController()
        {
            NUMBER_OF_SAMPLES = OptionsManager.GetInstance().GetOptionValueByName<int>(OptionNames.VISUALIZED_SAMPLES);
            // Maps the 100 nanosecond values that are passed as datetime type to millisecond, and display the y values without any change (in volt)
            var toMillisMapper = Mappers.Xy<DateTimePoint>()
            .X(dayModel => (double)((dayModel.DateTime.Ticks) * 0.00000001))
            .Y(dayModel => dayModel.Value);

            // This method saves the mapper at your application level, 
            //every time LiveCharts detects the DateTimePoint type in a Chart Values instance, it will use the toMillisMapper.
            Charting.For<DateTimePoint>(toMillisMapper);
            CalculateStepSize();
            MaxValue = double.NaN;
            MinValue = double.NaN;
            LabelFormatter = x => string.Format("{0:0.000}", x);
            ButtonVisibility = false;
            RangeChangedCommand = new RelayCommand(OnAxisChanged);
            //align
            AlignTriggeredCommand = new RelayCommand(AlignUserControls);
        }
        #endregion



        //******************** Methods ********************        
        #region Methods

        /// <summary>
        /// Gets the size of the step in millisecond.
        /// It's calculated from the sampleRate which is entered by the user in the profile settings.
        /// </summary>
        private void CalculateStepSize()
        {
            ProfilesManager pm = ProfilesManager.GetInstance();
            double stepSize = ((IntegerSetting)pm.ActiveProfile.GetSettingByName(SettingNames.SAMPLE_RATE)).Value;
            //double result;
            switch (((SampleRateSetting)pm.ActiveProfile.GetSettingByName(SettingNames.SAMPLE_RATE)).UnitOfSampleRate)
            {
                case SampleRateUnit.Hz:
                    stepSizeMillis = 1000 / stepSize;
                    break;

                case SampleRateUnit.kHz:
                    stepSizeMillis = 1 / stepSize;
                    break;

                default:
                    stepSizeMillis = stepSize;
                    break;
            }

            stepSize100Nanos = (long)(stepSizeMillis * 100000000);//the smallest time step in 100 nanosecond
        }




        //align
        protected void OnAlignTriggered(AlignTriggeredArgs args)
        {
            alignTriggered?.Invoke(this, args);
        }


        /// <summary>
        /// Extracts values from the output array that correspond to the x axis, and triggers resampling 
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
        /// calls the changeAxis method when the axis changes 
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public void OnAxisChanged(object parameter)
        {
            ChangeAxis();
        }

        //align
        public void AlignUserControls(object parameter)
        {
            AlignTriggeredArgs args = new AlignTriggeredArgs();
            args.MinValue = MinValue;
            args.MaxValue = MaxValue;
            OnAlignTriggered(args);
        }


        /// <summary>
        /// This method converts the values of the output array into DateTimePoint type to be converted later to millisecond
        /// </summary>
        /// <param name="output">The output.</param>
        public void SetData(double[] output)
        {
            int sizeOfArray = output.Length;
            outputArray.Clear();
            for (int i = 0; i < sizeOfArray; i++)
            {
                DateTime dt = new DateTime(i * stepSize100Nanos);
                DateTimePoint dtp = new DateTimePoint(dt, output[i]);
                outputArray.Add(dtp);
            }

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
        /// <param name="min">The minimum shown time value.</param>
        /// <param name="max">The maximum shown time value.</param>
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
        #endregion

    }
}



