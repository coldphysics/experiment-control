using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CustomElements.CheckableTreeView;
using Prototyping.Controller;

namespace Prototyping.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //CheckableTreeView uc = new CheckableTreeView();
            //uc.DataContext = new ItemHierarchyBuilder().GenerateHierarchy();
            //Window w = new Window();
            //w.Content = uc;
            //w.ShowDialog();

            Window w = WindowsHelper.CreateWindowHostingUserControl(new ItemHierarchyBuilder().GenerateHierarchy(), true);
            w.ShowDialog();
            InitializeComponent();
           
        }
    }
}
