using Controller.Variables;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using View.Helper;

namespace View.Variables.VariableTypes
{
    /// <summary>
    /// Interaction logic for VariableDynamicView.xaml
    /// </summary>
    public partial class VariableDynamicView : UserControl
    {
        public VariableDynamicView()
        {
            InitializeComponent();
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = Utilities.FindParentByType<ListBoxItem>(this);
            item.IsSelected = true;
        }
    }
}
