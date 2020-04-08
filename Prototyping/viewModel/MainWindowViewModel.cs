namespace Prototyping.viewModel
{
    class MainWindowViewModel : BasicViewModel
    {
        public TestViewModel TestVM
        {
            get;
            set;
        }
        private bool checkBox;

        public bool CheckBox
        {
            get { return checkBox; }
            set { checkBox = value;
            TestVM.Ischecked = value;
            }
        }
        public MainWindowViewModel()
        {
            TestVM = new TestViewModel();
            TestVM.RangeChanged += TestVM_RangeChanged;
            //TestVM.RangeChanged -= TestVM_RangeChanged; DO NOT FORGET TO DEREGISTER AT THE END.
        }

        void TestVM_RangeChanged(object sender, LiveCharts.Events.RangeChangedEventArgs e)
        {
            
        }
    }
}
