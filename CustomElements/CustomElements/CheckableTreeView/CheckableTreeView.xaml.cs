using System.Windows.Controls;

namespace CustomElements.CheckableTreeView
{
    public partial class CheckableTreeView : UserControl
    {
        public CheckableTreeView()
        {
            InitializeComponent();

            //CTVItemViewModel root = this.tree.Items[0] as CTVItemViewModel;

            //base.CommandBindings.Add(
            //    new CommandBinding(
            //        ApplicationCommands.Undo,
            //        (sender, e) => // Execute
            //        {                        
            //            e.Handled = true;
            //            root.IsChecked = false;
            //            this.tree.Focus();
            //        },
            //        (sender, e) => // CanExecute
            //        {
            //            e.Handled = true;
            //            e.CanExecute = (root.IsChecked != false);
            //        }));

            //this.tree.Focus();
        }
    }
}