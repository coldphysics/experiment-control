using Communication.Commands;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Events;
using LiveCharts.Wpf;
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


        // ******************** Variables/Objects ********************   
        #region attributes        
        /// <summary>
        /// Event handler for the [UserTriggeredAlignHandler] event 
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="RangeChangedEventArgs"/> instance containing the event data.</param>
        public delegate void UserTriggeredAlignHandler(object sender, RangeChangedEventArgs args);

        /// <summary>
        /// Occurs when [align triggered].
        /// </summary>
        public event UserTriggeredAlignHandler AlignTriggered;



        /// <summary>
        ///  the smallest time step in miliisecond (it's calculated from the sample rate that the user puts in the profile settings.
        /// </summary>
        public static double TIME_STEP_SIZE;



        /// <summary>
        ///  the array that holds the generated output converted to the DateTimePoint type.
        /// </summary>
        public ChartValues<DateTimePoint> OutputArray = new ChartValues<DateTimePoint>();

       /// <summary>
        /// holds the smallest time step but in 100 nanoseconds (The mapper converts this value to millisecond to be displayed).
       /// </summary>

        static long STEP;
       
        
        //TODO     
        //It could be an option specified by the user 
        /// <summary>
        /// The number of samples.
        /// </summary>
        const int NUM_OF_SAMPLES = 1000;

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
        /// The arguments of the x axis that are set when the axisChanged.
        /// Used to align all controllers to this controller when the align button is clicked.
        /// </summary>
        private RangeChangedEventArgs args;

        public RangeChangedEventArgs Args
        {
            get { return args; }
            set { args = value; }
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

        public OutputVisualizerController(double[] tempList)
        {
            // Maps the 100 nanosecond values that are passed as datetime type to millisecond, and display the y values without any change (in volt)
            var dayConfig = Mappers.Xy<DateTimePoint>()
            .X(dayModel => (double)((dayModel.DateTime.Ticks) * 0.00000001))
            .Y(dayModel => dayModel.Value);

           // This method saves the configuration at your application level, 
            //every time LiveCharts detects this type in a Chart Values instance, it will use the dayConfig mapper
            LiveCharts.Charting.For<DateTimePoint>(dayConfig);
            GetStepSize();
            SetData(tempList);
            MaxValue = Double.NaN;
            MinValue = Double.NaN;
            ButtonVisibility = false; 
            RangeChangedCommand = new RelayCommand(AxisChanged);
            //align
            AlignTriggeredCommand = new RelayCommand(AlignUserControls);
            LabelFormatter = x => string.Format("{0:0.000}", x);
       
        }
        #endregion



        //******************** Methods ********************        
        #region Methods

        /// <summary>
        /// Gets the size of the step size in millisecond.
        /// It's calculated from the sampleRate which is entered by the user in the profile settings.
        /// </summary>
        private void GetStepSize() 
        {
            ProfilesManager pm = ProfilesManager.GetInstance();
            double stepSize = ((IntegerSetting)pm.ActiveProfile.GetSettingByName(SettingNames.SAMPLE_RATE)).Value;
            //double result;
            switch (((SampleRateSetting)pm.ActiveProfile.GetSettingByName(SettingNames.SAMPLE_RATE)).UnitOfSampleRate)
            {
                
                case SampleRateUnit.Hz:
                    {
                        TIME_STEP_SIZE = 1000 / stepSize;
                        break;
                    }
                case SampleRateUnit.kHz: { TIME_STEP_SIZE = 1 / stepSize; break; }
               
                default: { TIME_STEP_SIZE = stepSize; break; }
            }
           
            STEP = (long)(TIME_STEP_SIZE * 100000000);//the smallest time step in 100 nanosecond
          


        }


     
     
        //align
        protected void OnAlignTriggered(RangeChangedEventArgs args)
        {
            if (AlignTriggered != null)
                AlignTriggered(this, args);

        }


        /// <summary>
        /// extracts values from the output array that correspond to the x axis  
        /// </summary>
        /// <param name="eventargs">The <see cref="RangeChangedEventArgs"/> instance containing the event data.</param>
        public void ChangeAxis(RangeChangedEventArgs eventargs)
        {
            
            double max = ((LiveCharts.Wpf.Axis)eventargs.Axis).MaxValue;
            double min = ((LiveCharts.Wpf.Axis)eventargs.Axis).MinValue;
            double range = eventargs.Range;
            min = Math.Max(0, min);
            int arrayMin = (int)Math.Round(min / TIME_STEP_SIZE);
            int arrayMax = (int)Math.Round(max / TIME_STEP_SIZE);
            arrayMax = Math.Min(arrayMax, OutputArray.Count);
            ArrayManipulation(arrayMin, arrayMax);
           
       //     SomeEvent = String.Format("Arraymin={0}, Arraymax={1} , maxAxis={2} , minAxis={3}, Range ={4} SamplingRate={5}", arrayMin, arrayMax, max, min, range, t);


        }

        /// <summary>
        /// calls the changeAxis method when the axis changes 
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        public void AxisChanged(object parameter)
        {

            RangeChangedEventArgs eventargs = (RangeChangedEventArgs)parameter;
            Args = eventargs;
            ButtonVisibility = true;
            ChangeAxis(eventargs);
            
        }

        //align
        public void AlignUserControls(object parameter)
        {

            OnAlignTriggered(Args);
        }


        /// <summary>
        /// This method gets the converts the values of the output array into Datetype type to be converted later to millisecond
        /// </summary>
        /// <param name="output">The output.</param>
        public void SetData(double[] output)
        {
            int sizeOfArray = output.Length;


            for (int i = 0; i < sizeOfArray; i++)
            {
                DateTime dt = new DateTime(i * STEP);
                DateTimePoint dtp = new DateTimePoint(dt, output[i]);
                OutputArray.Add(dtp);
            }
            ArrayManipulation(0, OutputArray.Count);
        }

        //TODO (the number of points to display could be an option )
        /// <summary>
        /// Does sampling for the currently displayed array in such a way that 1000 array values are displayed 
        /// </summary>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        public void ArrayManipulation(int min, int max)
        {


            ChartValues<DateTimePoint> tempArray = new ChartValues<DateTimePoint>();
            double temp = (max - min) / NUM_OF_SAMPLES;
            int samplingRate = (int)Math.Ceiling(temp);
            if (samplingRate == 0)
            { samplingRate = 1; }

            int k = min;

            for (int j = 0; j < NUM_OF_SAMPLES && k < max; j++)
            {

                tempArray.Add(OutputArray[k]);
                k += samplingRate;

            }

            ManipulatedArray = tempArray;



        }
        #endregion

    }
}



