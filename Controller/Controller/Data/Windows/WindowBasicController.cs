using System;
using AbstractController.Data.Card;
using Communication.Events;
using Communication.Interfaces.Controller;
using Controller.Data.Cookbook;
using Controller.Data.Tabs;
using Controller.Data.WindowGroups;
using Model.Data.Cards;
using Model.Data.Sequences;
using System.Collections.ObjectModel;
using Controller.Control;
using System.ComponentModel;
using Controller.MainWindow;
using Communication.Commands;
using System.Windows.Input;
using System.Windows;
using Controller.OutputVisualizer;
using Buffer.Basic;

namespace Controller.Data.Windows
{
    public abstract class WindowBasicController : AbstractCardController, IWindowController, INotifyPropertyChanged
    {
        // ******************** events ********************
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler WindowUpdate;

        // ******************** properties ********************

        public ObservableCollection<ShowableWindow> WindowsList
        {
            get
            {
                //System.Console.Write("AAAaAA\n");
                return MainWindowController.WindowsList;
            }
        }

        public Root.RootController _rootController
        {
            get { return _parent._rootController; }
        }

        public string Name
        {
            get { return Model.Name; }
        }

        // ******************** variables ********************
        private readonly WindowGroupControllerRecipe _tabGenerator = new WindowGroupControllerRecipe();
        //look
        public static Window visualizationWindow;
        //look
        private static bool isVisualizationWindowOpen = false;

        private WindowGroupController _parent;

        //******************** Commands ********************
        //look 
        /// <summary>
        /// Gets or sets the open visualizer window command
        /// </summary>
        /// <value>
        /// The open visualizer window command.
        /// </value>
        public ICommand OpenVisualizeWindowCommand { get; private set; }

        // ******************** constructor ********************
        protected WindowBasicController(CardBasicModel model, WindowGroupController parent)
            : base(model, parent)
        {
            _parent = parent;
            parent.GroupUpdate += OnWindowUpdate;
            MainWindowController.WindowsListChanged += MainWindowController_DoWindowsListChanged;
            //look
            //Ebaa 
            OpenVisualizeWindowCommand = new RelayCommand(OpenVisualizeWindow);

        }

        void MainWindowController_DoWindowsListChanged(object sender, EventArgs e)
        {
            //System.Console.Write("AA2\n");
            if (null != this.PropertyChanged)
            {
                //System.Console.Write("BB2\n");
                PropertyChanged(this, new PropertyChangedEventArgs("WindowsList"));
            }
        }

        public void OnWindowUpdate(object sender, EventArgs e)
        {
            //System.Console.Write("WindowBasicController: OnWindowUpdate!\n");
            var payload = (EventPayload)e;
            if (payload.Destination == EventPayload.DestinationType.Tab)
            {
                UpdateTab(sender, payload);
                return;
            }
            EventHandler handler = WindowUpdate;
            if (handler != null)
                handler(sender, e);

        }

        private void UpdateTab(object sender, EventPayload payload)
        {
            switch (payload.Action)
            {
                case EventPayload.ActionType.TabAdd:
                    TabCreate();
                    break;
                case EventPayload.ActionType.TabRemove:
                    TabRemove((int)payload.Cargo);
                    break;
                case EventPayload.ActionType.TabRename:
                    TabRename((Tuple<int, string>)payload.Cargo);
                    break;
            }
        }

        private void TabRename(Tuple<int, string> indexAndName)
        {
            int index = indexAndName.Item1;
            string name = indexAndName.Item2;
            ((TabController)Tabs[index]).SetName(name);
        }

        private void TabRemove(int index)
        {
            if (Tabs.Count <= 1)
                return;
            Model.Sequences.RemoveAt(index);
            Tabs.RemoveAt(index);
            //If some tab is removed, then the index of the remaining tabs should be updated
            foreach (TabController tab in Tabs)
            {
                tab.OnTabUpdate(this, new EventPayload(EventPayload.DestinationType.Tab, EventPayload.ActionType.TabRemove));
            }

        }


        private void OpenVisualizeWindow(object parameter)
        {
            if (!DoubleBuffer.ModelIsWrong)
            {
                VisualizationWindowManager vwm = VisualizationWindowManager.GetInstance();
                vwm.OpenWindow();
            }
            else
            {
                MessageBoxResult errorBox = new MessageBoxResult();
                String error = "Due to errors in the current model, the ouput visualizer can not show the output";
                errorBox = MessageBox.Show(error);
            }

        }


        private void TabCreate()
        {
            SequenceModel newSequenceModel = Model.SequenceAdd();
            TabController newSequenceController = _tabGenerator.CookTab(newSequenceModel, this);
        }

        public void CopyToBuffer()
        {
            ((WindowGroupController)Parent).CopyToBuffer();
        }

        public override string ToString()
        {
            return Model.Name;
        }

    }
}