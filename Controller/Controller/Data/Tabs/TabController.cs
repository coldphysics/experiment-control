using System;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using AbstractController.Data.Sequence;
using Communication.Commands;
using Communication.Events;
using Controller.Data.WindowGroups;
using Controller.Data.Windows;
using Model.Data.SequenceGroups;
using Model.Data.Sequences;
using Model.Settings;
using PythonUtils;

namespace Controller.Data.Tabs
{
    public class IsEnabledToBrushConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value)
            {
                {
                    return new SolidColorBrush(Colors.LightGreen);
                }
            }
            return new SolidColorBrush(Colors.LightPink);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
    public class TabController : AbstractSequenceController, INotifyPropertyChanged
    {
        // ******************** properties ********************
        public Root.RootController _rootController
        {
            get { return _parent._rootController; }
        }

        public string Name
        {
            get { return Model.Name; }
            set
            {
                if (value != Model.Name)
                {
                    var eventPayload = new EventPayload(EventPayload.DestinationType.Tab,
                                                        EventPayload.ActionType.TabRename);
                    var indexAndName = new Tuple<int, string>(Index(), value);
                    eventPayload.Cargo = indexAndName;
                    ((WindowGroupController)Group()).OnGroupUpdate(this, eventPayload);
                }
            }
        }

        public string Duration
        {
            get { return " - " + Convert.ToString(LongestDurationAllSequences()) + TimeSettingsInfo.GetInstance().TimeUnit; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this sequence is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this sequence is enabled; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>Changing the value triggers many events to reflect the changes on other tabs/windows</remarks>
        public bool IsEnabled
        {
            get
            {
                return Model.IsEnabled;
            }

            set
            {
                foreach (WindowBasicController window in _rootController.DataController.SequenceGroup.Windows)
                {
                    ((TabController)window.Tabs[Model.Index()]).ChangeIsEnabledState(value);
                }

                var payload = new EventPayload(EventPayload.DestinationType.Group, EventPayload.ActionType.UpdateTime);
                ((WindowGroupController)Group()).OnGroupUpdate(this, payload);
                CopyToBuffer();
            }
        }

        /// <summary>
        /// Gets the index of the current sequence
        /// </summary>
        /// <value>
        /// The 0-based index.
        /// </value>
        public string Index
        {
            get { return String.Format("({0})", Index()); }
        }


        // ******************** events ********************
        public ICommand TabRemove { get; protected set; }

        public ICommand TabAdd { get; protected set; }

        public ICommand TabLeft { get; protected set; }

        public ICommand TabRight { get; protected set; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public event EventHandler TabUpdate;


        // ******************** variables ********************
        public WindowBasicController _parent;
        //private SequenceModel _model;

        // ******************** constructor ********************
        public TabController(SequenceModel model, WindowBasicController parent)
            : base(model, parent)
        {
            _parent = parent;
            //_model = model;
            TabAdd = new RelayCommand(Add);
            TabRemove = new RelayCommand(Remove);

            TabLeft = new RelayCommand(TabMoveLeft);
            TabRight = new RelayCommand(TabMoveRight);

            parent.WindowUpdate += OnTabUpdate;
        }


        public void TabMoveLeft(object parameter)
        {
            //System.Console.WriteLine("OnTabUpdate");
            //System.Console.WriteLine("cnt: " + Parent.Tabs.Count + " ind: " + Index());
            if (Index() == 0 || Parent.Tabs.Count <= 1)
            {
                //System.Console.WriteLine("Do NOT move");
                return;
            }
            //MessageBoxResult res = MessageBox.Show("Do you really want to move this sequence? The output should be deactivated during this process! If so, press ok.","Move Sequence?", MessageBoxButton.OKCancel);
            System.Console.WriteLine("MoveL");

            SequenceGroupModel group = _rootController.returnModel.Data.group;

            for (int j = 0; j < group.Cards.Count; j++)
            {
                Model.Data.Cards.CardBasicModel card = group.Cards[j];

                Model.Data.Sequences.SequenceModel sequence = card.Sequences[Index()];
                card.Sequences[Index()] = card.Sequences[Index() - 1];
                card.Sequences[Index() - 1] = sequence;
            }

            this._rootController.Variables.DoRefreshWindows();
            //MessageBox.Show("You should now restart the windows (go to the debug section of the main window!). Otherwise the graphical output will not be correct.","Please refresh all windows!", MessageBoxButton.OK);
        }

        public void TabMoveRight(object parameter)
        {

            //System.Console.WriteLine("OnTabUpdate");
            //System.Console.WriteLine("cnt: " + Parent.Tabs.Count + " ind: " + Index());
            if (Index() == Parent.Tabs.Count - 1 || Parent.Tabs.Count <= 1)
            {
                //System.Console.WriteLine("Do NOT move");
                return;
            }
            //MessageBoxResult res = MessageBox.Show("Do you really want to move this sequence? The output should be deactivated during this process! If so, press ok.","Move Sequence?", MessageBoxButton.OKCancel);
            System.Console.WriteLine("MoveR");
            SequenceGroupModel group = _rootController.returnModel.Data.group;
            for (int j = 0; j < group.Cards.Count; j++)
            {
                Model.Data.Cards.CardBasicModel card = group.Cards[j];

                Model.Data.Sequences.SequenceModel sequence = card.Sequences[Index()];
                card.Sequences[Index()] = card.Sequences[Index() + 1];
                card.Sequences[Index() + 1] = sequence;
            }

            this._rootController.Variables.DoRefreshWindows();
            //MessageBox.Show("You should now restart the windows (go to the debug section of the main window!). Otherwise the graphical output will not be correct.","Please refresh all windows!", MessageBoxButton.OK);
        }

        //public string StartTimeOf
        //{
        //    get { return " - " + Convert.ToString(Parent.GetStartTime(this) + Model.SequenceGroupSettings().Unit); }
        //}



        public void OnTabUpdate(object sender, EventArgs e)
        {
            var payload = (EventPayload)e;

            if (payload.Destination == EventPayload.DestinationType.Channel)
            {
                //ChannelUpdate(sender, payload);
                return;
            }

            EventHandler handler = TabUpdate;
            if (handler != null)
                handler(sender, e);

            if (payload.Action == EventPayload.ActionType.UpdateTime)
            {
                if (null != PropertyChanged)
                    PropertyChanged(this, new PropertyChangedEventArgs("Duration"));
            }

            if (payload.Action == EventPayload.ActionType.TabRemove)//If some other tab is removed, then the index should be updated
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Index"));
                }
            }

        }


        /*private void ChannelUpdate(object sender, EventPayload payload)
        {
            return;
        }*/

        internal void SetName(string name)
        {
            Model.Name = name;
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs("Name"));
        }

        /*private void Move(object calling)
        {
            var eventPayload = new EventPayload(EventPayload.DestinationType.Tab, EventPayload.ActionType.TabMove);
            eventPayload.Cargo = Index();
            ((WindowGroupController)Group()).OnGroupUpdate(this, eventPayload);
        }*/

        private void Remove(object calling)
        {
            if (!CheckUsageOfTabInScripts())
            {
                var eventPayload = new EventPayload(EventPayload.DestinationType.Tab, EventPayload.ActionType.TabRemove);
                eventPayload.Cargo = Index();
                ((WindowGroupController)Group()).OnGroupUpdate(this, eventPayload);
                //CHANGE 22.06.2017 Added by Ghareeb: removing a tab can change a lot!
                CopyToBuffer();
            }
        }

        /// <summary>
        /// Checks the usages of a tab in all Python scripts.
        /// </summary>
        /// <returns><c>true</c> if the tab's state is used as a Python variable.</returns>
        private bool CheckUsageOfTabInScripts()
        {
            string pythonVariableName = "seq_" + Index();
            VariableUsageChecker checker = new VariableUsageChecker(_parent._rootController.returnModel);
            StringBuilder usages = new StringBuilder("");
            string currentUsage = "";

            foreach (VariableUsage usage in checker.GetUsagesOfVariable(pythonVariableName))
            {
                switch (usage.UsageType)
                {
                    case VariableUsageType.CalibrationScript:
                        currentUsage = String.Format("Calibration Script at ({0})", usage.ScriptLocation.GetLocationAsString());
                        break;
                    case VariableUsageType.Duration:
                        currentUsage = String.Format("Step Duration at ({0})", usage.ScriptLocation.GetLocationAsString());
                        break;
                    case VariableUsageType.Value:
                        currentUsage = String.Format("Step Value at ({0})", usage.ScriptLocation.GetLocationAsString());
                        break;
                    case VariableUsageType.DynamicVariable:
                        currentUsage = String.Format("Dynamic Variable at ({0})", usage.ScriptLocation.GetLocationAsString());
                        break;
                    case VariableUsageType.PythonStep:
                        currentUsage = String.Format("Python Step at ({0})", usage.ScriptLocation.GetLocationAsString());
                        break;
                }

                usages.Append("\n" + currentUsage);

            }

            if (usages.ToString().Length != 0)
            {
                MessageBox.Show(
                    string.Format("The sequence cannot be removed as its state is being used as a Python variable ({0}) in the following locations:\n{1}", pythonVariableName, usages),
                    "Sequence Cannot Be Removed", MessageBoxButton.OK, MessageBoxImage.Error
                    );

                return true;
            }

            return false;
        }

        private void Add(object calling)
        {
            var eventPayload = new EventPayload(EventPayload.DestinationType.Tab, EventPayload.ActionType.TabAdd);
            ((WindowGroupController)Group()).OnGroupUpdate(this, eventPayload);
        }

        public void CopyToBuffer()
        {
            //System.Console.Write("Tab\n");
            ((WindowBasicController)Parent).CopyToBuffer();
        }

        /// <summary>
        /// Changes the enabled state of the current sequence
        /// </summary>
        /// <param name="newState">if set to <c>true</c> [new state].</param>
        private void ChangeIsEnabledState(bool newState)
        {
            Model.IsEnabled = newState;

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs("IsEnabled"));
            }
        }

    }
}