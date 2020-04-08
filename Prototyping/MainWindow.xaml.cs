using Prototyping.viewModel;
using System.Windows;


namespace Prototyping
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public SeriesCollection SeriesCollection { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
            //SeriesCollection = new SeriesCollection
            //    {
            //        new LineSeries
            //        {
            //            Values = new ChartValues<double> { 3, 5, 7, 4 }
            //        },
            //        new ColumnSeries                
            //        {
            //            Values = new ChartValues<decimal> { 5, 6, 2, 7 }
            //        }
            //    };

            //DataContext = this;
        }


    }
}
