using System;
using System.Collections.Generic;
using LiveCharts;
using LiveCharts.Configurations;

namespace Controller.AdWin.Simulator
{
    /// <summary>
    /// The ViewModel for a single channel
    /// </summary>
    /// <seealso cref="AdWinSimulator.ViewModel.ViewModelBase" />
    public class ChannelOutputController : ViewModelBase
    {
        #region Constants
        /// <summary>
        /// The timespan of the shown signal.
        /// </summary>
        private const int SHOW_PERIOD_SECONDS = 8;

        /// <summary>
        /// The timespan shown as empty ahead of the signal.
        /// </summary>
        private const int SHOW_AHEAD_SECONDS = 1;

        /// <summary>
        /// The number of output values to show at once (related to the show period and the sampling period)
        /// </summary>
        private const int VALUES_TO_KEEP = 160;//Show_period / Sampling_Rate

        /// <summary>
        /// The timespan of a single step on the x-axis.
        /// </summary>
        private const double TIME_AXIS_STEP_SECONDS = 0.5;

        /// <summary>
        /// The period in which the drawing is refreshed.
        /// </summary>
        public const int DRAWING_PERIOD_MILLIS = 200;
        #endregion

        #region Fields
        /// <summary>
        /// The maximum value of the x-axis.
        /// </summary>
        private double _axisMax;
        /// <summary>
        /// The minimum value of the x-axis.
        /// </summary>
        private double _axisMin;
        /// <summary>
        /// The mapper that knows how to convert a <see cref=" ChannelSnapshot"/> instance into coordinates.
        /// </summary>
        private object mapper;
        /// <summary>
        /// The human-readable name of the channel.
        /// </summary>
        private string channelName;

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the human-readable name of the channel.
        /// </summary>
        /// <value>
        /// The human-readable name of the channel.
        /// </value>
        public string ChannelName//ATTACHED
        {
            get { return channelName; }
            set
            {
                channelName = value;
                OnPropertyChanged("ChannelName");
            }
        }



        /// <summary>
        /// Gets the chart mapper, and initializes it if necessary.
        /// </summary>
        /// <value>
        /// The chart mapper.
        /// </value>
        public Object ChartMapper//ATTACHED
        {
            get
            {
                if (mapper == null)
                {
                    mapper = Mappers.Xy<ChannelSnapshot>()
                        .X(snapshot => snapshot != null ? snapshot.DateTime.Ticks : 0)   //use DateTime.Ticks as X
                        .Y(snapshot => snapshot != null ? snapshot.Value : 0);           //use the value property as Y
                }

                return mapper;
            }
        }

        /// <summary>
        /// Gets or sets the currently-shown signal values.
        /// </summary>
        /// <value>
        /// The currently-shown signal values.
        /// </value>
        public ChartValues<ChannelSnapshot> ChartValues//ATTACHED
        { get; set; }

        /// <summary>
        /// Gets or sets the date time formatter.
        /// </summary>
        /// <value>
        /// The date time formatter.
        /// </value>
        public Func<double, string> DateTimeFormatter//ATTACHED
        { get; set; }

        /// <summary>
        /// Gets or sets the x-axis step.
        /// </summary>
        /// <value>
        /// The x-axis step.
        /// </value>
        public double AxisStep//ATTACHED
        { get; set; }

        /// <summary>
        /// Gets or sets the x-axis maximum value.
        /// </summary>
        /// <value>
        /// The x-axis maximum value.
        /// </value>
        public double AxisMax//ATTACHED
        {
            get { return _axisMax; }
            set
            {
                _axisMax = value;
                OnPropertyChanged("AxisMax");
            }
        }

        /// <summary>
        /// Gets or sets the axis minimum.
        /// </summary>
        /// <value>
        /// The axis minimum.
        /// </value>
        public double AxisMin//ATTACHED
        {
            get { return _axisMin; }
            set
            {
                _axisMin = value;
                OnPropertyChanged("AxisMin");
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelOutputViewModel"/> class.
        /// </summary>
        public ChannelOutputController()
        {
            //the values property will store our values array
            ChartValues = new ChartValues<ChannelSnapshot>();

            //lets set how to display the X Labels
            DateTimeFormatter = value => new DateTime((long)value).ToString("mm:ss");

            AxisStep = TimeSpan.FromSeconds(TIME_AXIS_STEP_SECONDS).Ticks;

            SetAxisLimits(DateTime.Now);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Shows a set of new signal values.
        /// </summary>
        /// <param name="snapshots">The snapshots to show.</param>
        public void ShowNewValues(List<ChannelSnapshot> snapshots)
        {
            ChartValues.AddRange(snapshots);
            
            //lets only use the last VALUES_TO_KEEP values
            if (ChartValues.Count > VALUES_TO_KEEP)
            {
                int toRemove = ChartValues.Count - VALUES_TO_KEEP;

                for (int i = 0; i < toRemove; i++)
                    ChartValues.RemoveAt(0);
            }

            SetAxisLimits(snapshots[snapshots.Count - 1].DateTime);
        }

        /// <summary>
        /// Sets the x-axis upper and lower limits based on the time associated with last signal value to be shown.
        /// </summary>
        /// <param name="value">The time associated with the last signal value to be shown.</param>
        private void SetAxisLimits(DateTime value)
        {
            AxisMax = value.Ticks + TimeSpan.FromSeconds(SHOW_AHEAD_SECONDS).Ticks; // lets force the axis to be 1 second ahead
            AxisMin = value.Ticks - TimeSpan.FromSeconds(SHOW_PERIOD_SECONDS).Ticks; //we only care about the last 8 seconds
        }
        #endregion
    }
}
