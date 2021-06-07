using System.Windows;
using System.Windows.Input;

namespace Controller.MainWindow
{
    public class IterationManagerController : ChildController
    {
        private bool _isScanOnlyOnceEnabled = true;
        private bool _isStopAfterScanEnabled = true;
        private bool _isShuffleIterationsEnabled = true;
        private string _nameOfTheCurrentStartGCOfScans = "Current Start GC of Scans:";
        private Visibility _isPreviousStartGCOfScansVisible = Visibility.Visible;

        public IterationManagerController(MainWindowController parent)
            : base(parent)
        {
        }


        private MainWindowController Parent
        {
            get
            {
                return (MainWindowController)parent;
            }
        }

        public ICommand ScanOnlyOnceCommand { get { return Parent.OnlyOnceCommand; } }

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


        public bool IsShuffleIterationsEnabled
        {
            get { return _isShuffleIterationsEnabled; }
            set
            {
                _isShuffleIterationsEnabled = value;
                OnPropertyChanged("IsShuffleIterationsEnabled");
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

        public int CompletedScans
        {
            get { return Parent.CompletedScans; }
        }


        public string NameOfTheCurrentStartGCOfScans
        {
            get { return _nameOfTheCurrentStartGCOfScans; }
            set
            {
                _nameOfTheCurrentStartGCOfScans = value;
                OnPropertyChanged("NameOfTheCurrentStartGCOfScans");
            }
        }


        public Visibility IsPreviousStartGCOfScansVisible
        {
            get { return _isPreviousStartGCOfScansVisible; }
            set
            {
                _isPreviousStartGCOfScansVisible = value;
                OnPropertyChanged("IsPreviousStartGCOfScansVisible");
            }
        }

    }
}
