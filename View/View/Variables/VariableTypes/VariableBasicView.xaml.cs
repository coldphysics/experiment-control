using Controller.Variables;
using CustomElements.SubmitTextBox;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace View.Variables.VariableTypes
{
    /// <summary>
    /// Interaction logic for VariableBasicView.xaml
    /// </summary>
    public partial class VariableBasicView : UserControl
    {
        public VariableBasicView()
        {
            InitializeComponent();
        }

        private void VariableName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var realSender = (SubmitTextBox)sender;
            var dataContext = (VariableController)realSender.DataContext;

            if (e.Key == Key.Up)
            {
                dataContext.moveUp(dataContext);
                e.Handled = true;
            } else if (e.Key == Key.Down)
            {
                dataContext.moveDown(dataContext);
                e.Handled = true;
            }
        }
    }
}
