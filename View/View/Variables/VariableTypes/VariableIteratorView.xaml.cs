using System;
using System.Windows;
using System.Windows.Controls;
using Controller.Variables;
using View.Helper;

namespace View.Variables.VariableTypes
{
    /// <summary>
    /// Interaction logic for VariableIteratorView.xaml
    /// </summary>
    public partial class VariableIteratorView : UserControl
    {
        public VariableIteratorView()
        {
            InitializeComponent();
        }

        private void UserControl_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListBoxItem item = Utilities.FindParentByType<ListBoxItem>(this);
            item.IsSelected = true;
        }
    }
}
