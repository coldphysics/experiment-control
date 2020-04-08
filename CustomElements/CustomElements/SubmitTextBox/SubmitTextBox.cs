using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CustomElements.SubmitTextBox
{
    public class SubmitTextBox : TextBox
    {
        private const double OrangeThickness = 1.3;
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                var field = e.OriginalSource as TextBox;
                field.ClearValue(TextBox.BorderBrushProperty);
                field.ClearValue(TextBox.BorderThicknessProperty);

                var uie = e.OriginalSource as UIElement;
                e.Handled = true;
                uie.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
            else
            {
                var uie = e.OriginalSource as TextBox;
                uie.BorderBrush = new SolidColorBrush(Colors.Orange);
                uie.BorderThickness = new Thickness(OrangeThickness);
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Delete || e.Key == Key.Back)
            {
                var uie = e.OriginalSource as TextBox;
                uie.BorderBrush = new SolidColorBrush(Colors.Orange);
                uie.BorderThickness = new Thickness(OrangeThickness);   
            }
            base.OnPreviewKeyDown(e);
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            var field = e.OriginalSource as TextBox;
            field.ClearValue(TextBox.BorderBrushProperty);
            field.ClearValue(TextBox.BorderThicknessProperty);
            base.OnLostFocus(e);
            
        }


    }
}