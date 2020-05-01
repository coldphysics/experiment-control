using System;
using System.Windows;
using System.Windows.Controls;
using Controller.Variables;

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
            this.DataContextChanged += VariableIteratorView_DataContextChanged;
        }

        void VariableIteratorView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //System.Console.Write("New Data Context!");
            if (this.DataContext != null)
            {
                (DataContext as VariableController).LoseFocus += VariableIteratorView_LoseFocus;
            }
        }

        void VariableIteratorView_LoseFocus(object sender, EventArgs e)
        {
            //RECO figure out what is this for
            try
            {
                //System.Console.Write("This.Unfocus!!!\n");
                VariableStartValue.Focus();
                VariableStopValue.Focus();
                this.Focus();
            }
            catch
            { }
        }

        private void UserControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListBoxItem item = Utilities.FindParentByType<ListBoxItem>(this);
            item.IsSelected = true;
        }
    }
}
