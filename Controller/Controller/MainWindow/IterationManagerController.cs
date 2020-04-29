using System.Windows;
using System.Windows.Input;

namespace Controller.MainWindow
{
    public class IterationManagerController : ChildController
    {
        private MainWindowController Parent
        {
            get
            {
                return (MainWindowController)parent;
            }
        }

        public ICommand ScanOnlyOnceCommand { get { return Parent.OnlyOnceCommand; } }

        private bool _isScanOnlyOnceEnabled = true;

        public bool IsScanOnlyOnceEnabled
        {
            get { return _isScanOnlyOnceEnabled; }
            set
            {
                _isScanOnlyOnceEnabled = value;
                OnPropertyChanged("IsScanOnlyOnceEnabled");
            }
        }
        public bool StopAfterScan
        {
            set
            {
                Parent.StopAfterScan = value;
            }
            get
            {
                return Parent.StopAfterScan;
            }
        }

        private bool _isStopAfterScanEnabled = true;
        public bool IsStopAfterScanEnabled
        {
            get
            { return _isStopAfterScanEnabled; }
            set
            {
                _isStopAfterScanEnabled = value;
                OnPropertyChanged("IsStopAfterScanEnabled");
            }

        }
        public bool ShuffleIterations
        {
            set
            {
                Parent.ShuffleIterations = value;
            }

            get
            {
                return Parent.ShuffleIterations;
            }
        }

        private bool _isShuffleIterationsEnabled = true;

        public bool IsShuffleIterationsEnabled
        {
            get { return _isShuffleIterationsEnabled; }
            set
            {
                _isShuffleIterationsEnabled = value;
                OnPropertyChanged("IsShuffleIterationsEnabled");
            }
        }
        public bool AlwaysIncrease
        {
            set
            {
                Parent.AlwaysIncrease = value;
            }

            get
            {
                return Parent.AlwaysIncrease;
            }
        }
        private bool _isAlwaysIncreaseEnabled = true;

        public bool IsAlwaysIncreaseEnabled
        {
            get { return _isAlwaysIncreaseEnabled; }
            set
            {
                _isAlwaysIncreaseEnabled = value;
                OnPropertyChanged("IsAlwaysIncreaseEnabled");
            }
        }
        public bool Pause
        {
            set
            {
                Parent.Pause = value;
            }

            get
            {
                return Parent.Pause;
            }
        }

        private bool _isPauseEnabled;

        public bool IsPauseEnabled
        {
            get { return _isPauseEnabled; }
            set
            {
                _isPauseEnabled = value;
                OnPropertyChanged("IsPauseEnabled");
            }
        }
        public bool IsOnceChecked
        {

            get
            {
                return Parent.IsOnceChecked;
            }

        }


        //Ebaa 11.06
        public int StartCounterOfScansOfCurrentModel
        {
            get
            {
                return Parent.StartCounterOfScansOfCurrentModel;
            }
        }

        public int LastStartCounterOfScans
        {
            get { return Parent.LastStartCounterOfScans; }
        }

        public int NumberOfIterations
        {
            get { return Parent.NumberOfIterations; }
        }

        public int IterationOfScan
        {
            get { return Parent.IterationOfScan; }
        }

        //public int IterationOfScan
        //{
        //    get { return Parent.MeasurementRoutineController.CurrentRoutineModel.RoutineModel.Counters.IterationOfScan; }
        //}

        public int CompletedScans
        {
            get { return Parent.CompletedScans; }
        }

        private string _nameOfTheCurrentStartGCOfScans = "Current Start GC of Scans:";
        public string NameOfTheCurrentStartGCOfScans
        {
            get { return _nameOfTheCurrentStartGCOfScans; }
            set
            {
                _nameOfTheCurrentStartGCOfScans = value;
                OnPropertyChanged("NameOfTheCurrentStartGCOfScans");
            }
    }

        private Visibility _isPreviousStartGCOfScansVisible=Visibility.Visible;

        public Visibility IsPreviousStartGCOfScansVisible
        {
            get { return _isPreviousStartGCOfScansVisible; }
            set { _isPreviousStartGCOfScansVisible = value;
            OnPropertyChanged("IsPreviousStartGCOfScansVisible");
            }
        }
        public IterationManagerController(MainWindowController parent)
            : base(parent)
        {
        }

    }
}
