using System;
using System.Collections.Generic;

using Communication.Commands;
using HardwareAdWin.Simulator;

namespace Controller.AdWin.Simulator
{
    /// <summary>
    /// The ViewModel for the main window of the AdWin simulator.
    /// </summary>
    /// <seealso cref="AdWinSimulator.ViewModel.ViewModelBase" />
    public class SimulatorController : ViewModelBase
    {
        #region Fields
        /// <summary>
        /// An instance of type <see cref=" ChannelOutputViewModel"/> that is associated to channel 1
        /// </summary>
        private ChannelOutputController channel1;
        /// <summary>
        /// An instance of type <see cref=" ChannelOutputViewModel"/> that is associated to channel 2
        /// </summary>
        private ChannelOutputController channel2;
        /// <summary>
        /// A reference to the AdWin simulator that provides the signal data. (Model in MVVM)
        /// </summary>
        private DummyAdWinHW adwin;
        /// <summary>
        /// The time associated to the last shown signal value, which is the same for channel 1 and channel 2. Could be <c>null</c>.
        /// </summary>
        private DateTime? lastDateTime = null;
        /// <summary>
        /// The command that switches showing the output signal on or off.
        /// </summary>
        private RelayCommand startStopCommand;
        /// <summary>
        /// Information about all possible AdWin channels to pick from.
        /// </summary>
        private ChannelInfo[] allChannelInfos;
        /// <summary>
        /// The current AdWin channel selected to be shown on channel output 1.
        /// </summary>
        private ChannelInfo channel1Selection;
        /// <summary>
        /// The current AdWin channel selected to be shown on channel output 2.
        /// </summary>
        private ChannelInfo channel2Selection;
        /// <summary>
        /// Indicates whether the window is currently showing the output signal or not.
        /// </summary>
        private bool isShowingChannelSignal;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether this instance is currently showing channel signal.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is showing channel signal; otherwise, <c>false</c>.
        /// </value>
        public bool IsShowingChannelSignal//ATTACHED - Affects the text shown on the start/stop button
        {
            get { return isShowingChannelSignal; }
            set 
            {
                isShowingChannelSignal = value;
                OnPropertyChanged("IsShowingChannelSignal");
            }
        }


        /// <summary>
        /// Gets or sets the current AdWin channel selected to be shown on channel output 1.
        /// </summary>
        /// <value>
        /// The channel output 1 selection.
        /// </value>
        public ChannelInfo Channel1Selection//ATTACHED
        {
            set
            {
                channel1Selection = value;
                AssignChannelSelection(1, channel1Selection);
            }

            get { return channel1Selection; }
        }

        /// <summary>
        /// Gets or sets the current AdWin channel selected to be shown on channel output 2.
        /// </summary>
        /// <value>
        /// The channel output 2 selection.
        /// </value>
        public ChannelInfo Channel2Selection//ATTACHED
        {
            set
            {
                channel2Selection = value;
                AssignChannelSelection(2, channel2Selection);
            }

            get { return channel2Selection; }
        }


        /// <summary>
        /// Gets information about all possible AdWin channels to pick from.
        /// </summary>
        /// <value>
        /// All channel infos.
        /// </value>
        public ChannelInfo[] AllChannelInfos//ATTACHED
        {
            get { return allChannelInfos; }
        }

        /// <summary>
        /// Gets the command that switches showing the output signal on or off.
        /// </summary>
        /// <value>
        /// The start stop command.
        /// </value>
        public RelayCommand StartStopCommand//ATTACHED
        {
            get
            {
                if (startStopCommand == null)
                    startStopCommand = new RelayCommand(param => AttachDetach());

                return startStopCommand;
            }
        }

        /// <summary>
        /// Gets the ViewModel instance for channel output 1.
        /// </summary>
        /// <value>
        /// The channel output 1 ViewModel instance.
        /// </value>
        public ChannelOutputController Channel1//ATTACHED
        {
            get
            {
                return channel1;
            }
        }

        /// <summary>
        /// Gets the ViewModel instance for channel output 2.
        /// </summary>
        /// <value>
        /// The channel output 2 ViewModel instance.
        /// </value>
        public ChannelOutputController Channel2
        {
            get
            {
                return channel2;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        /// <param name="adwin">A reference to the AdWin simulator. (The Model of MVVM)</param>
        public SimulatorController(DummyAdWinHW adwin)
        {
            this.adwin = adwin;
            this.adwin.NotificationBatchSize = ChannelOutputController.DRAWING_PERIOD_MILLIS / adwin.CONSUMPTION_PERIOD;
            channel1 = new ChannelOutputController();
            channel2 = new ChannelOutputController();
            InitializeChannelInfos();
            IsShowingChannelSignal = false;
        }
        #endregion

        /// <summary>
        /// Handles the ChannelOutputAvailable event of the adwin control.
        /// </summary>
        /// <param name="source">The source of the event. (<c>null</c>)</param>
        /// <param name="args">The <see cref="ChannelsOutputAvailableEventArgs"/> instance containing the event data.</param>
        /// <remarks>This method handles </remarks>
        public void adwin_ChannelOutputAvailable(object source, ChannelsOutputAvailableEventArgs args)
        {
            DateTime now = DateTime.Now;
            double[][] allChannels = args.CurrentOutput;
            List<ChannelSnapshot> channel1Snapshots = new List<ChannelSnapshot>();
            List<ChannelSnapshot> channel2Snapshots = new List<ChannelSnapshot>();
            DateTime currentDateTime = DateTime.Now;

            foreach (double[] snapshot in allChannels)//"snapshot" has one value per channel
            {
                if (lastDateTime != null)
                    currentDateTime = lastDateTime.Value + TimeSpan.FromMilliseconds(adwin.CONSUMPTION_PERIOD);

                lastDateTime = currentDateTime;

                channel1Snapshots.Add(new ChannelSnapshot
                {
                    Value = snapshot[Channel1Selection.ChannelTotalOrder],
                    DateTime = currentDateTime
                });

                channel2Snapshots.Add(new ChannelSnapshot
                {
                    Value = snapshot[Channel2Selection.ChannelTotalOrder],
                    DateTime = currentDateTime
                });
            }

            Channel1.ShowNewValues(channel1Snapshots);
            Channel2.ShowNewValues(channel2Snapshots);
        }

        /// <summary>
        /// Attaches or detaches the <see cref=" adwin_ChannelOutputAvailable"/> method to/from the <see cref=" adwin_ChannelOutputAvailable"/> event.
        /// </summary>
        private void AttachDetach()
        {
            if (IsShowingChannelSignal)
            {
                adwin.ChannelOutputAvailable -= adwin_ChannelOutputAvailable;
                lastDateTime = null;
            }
            else
            {
                adwin.ChannelOutputAvailable += adwin_ChannelOutputAvailable;
            }

            IsShowingChannelSignal = !IsShowingChannelSignal;
        }

        private void AssignChannelSelection(int channelNumber, ChannelInfo selection)
        {
            if (channelNumber == 1)
                channel1.ChannelName = selection.ToString();
            else
                channel2.ChannelName = selection.ToString();
        }

        /// <summary>
        /// Generates all AdWin channel infos based on the number of analog and digital cards, and the number of channels hosted by each of them, 
        /// and initializes the selected AdWin channel to be the first channel.
        /// </summary>
        private void InitializeChannelInfos()
        {
            int NUMBER_OF_ANALOG_CARDS = Global.GetNumAnalogCards();
            int NUMBER_OF_DIGITAL_CARDS = Global.GetNumDigitalCards();
            int NUMBER_OF_ANALOG_CHANNELS_PER_CARD = Global.GetNumAnalogChannelsPerCard();
            int NUMBER_OF_DIGITAL_CHANNELS_PER_CARD = Global.GetNumDigitalChannelsPerCard();
            int NUMBER_OF_ANALOG_CHANNELS = NUMBER_OF_ANALOG_CARDS * NUMBER_OF_ANALOG_CHANNELS_PER_CARD;
            int NUMBER_OF_DIGITAL_CHANNELS = NUMBER_OF_DIGITAL_CARDS * NUMBER_OF_DIGITAL_CHANNELS_PER_CARD;

            allChannelInfos = new ChannelInfo[NUMBER_OF_ANALOG_CHANNELS + NUMBER_OF_DIGITAL_CHANNELS];

            int index = 0;

            for (int i = 0; i < NUMBER_OF_ANALOG_CARDS; i++)
            {
                for (int j = 0; j < NUMBER_OF_ANALOG_CHANNELS_PER_CARD; j++)
                {
                    allChannelInfos[index] = new ChannelInfo { CardName = String.Format("A{0}", i + 1), ChannelIndex = j, ChannelTotalOrder = index };
                    ++index;
                }
            }

            for (int i = 0; i < NUMBER_OF_DIGITAL_CARDS; i++)
            {
                for (int j = 0; j < NUMBER_OF_DIGITAL_CHANNELS_PER_CARD; j++)
                {
                    allChannelInfos[index] = new ChannelInfo { CardName = String.Format("D{0}", i + 1), ChannelIndex = j, ChannelTotalOrder = index };
                    ++index;
                }
            }

            //Initial Selection
            Channel1Selection = allChannelInfos[0];
            Channel2Selection = allChannelInfos[0];
        }


    }
}
