using Controller.Data.Steps;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace View.Data.Steps
{
    /// <summary>
    /// Interaction logic for StepBasicView.xaml
    /// </summary>
    public partial class StepBasicView : UserControl
    {
        public StepBasicView()
        {
            InitializeComponent();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var realSender = (TextBox)sender;
            var dataContext = (StepBasicController)realSender.DataContext;
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Left)
                {
                    dataContext.DoMoveLeft(dataContext);
                    e.Handled = true;
                }
                else if (e.Key == Key.Right)
                {
                    dataContext.DoMoveRight(dataContext);
                    e.Handled = true;
                }
            }
        }
    }
}
