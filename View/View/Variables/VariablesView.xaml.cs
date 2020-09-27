using Controller.Variables;
using CustomElements.SizeSavedWindow;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Communication;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace View.Variables
{
    public class TaskListDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate
            SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            if (element != null && item != null && item is VariableController)
            {
                VariableController taskitem = item as VariableController;

                //System.Console.Write("Iswhat: {0}\n", taskitem.isGroupHeader);
                if (taskitem.IsGroupHeader == true)
                    return
                        element.FindResource("VariableStaticGroupHeaderTemplate") as DataTemplate;
                else
                    return
                        element.FindResource("VariableStaticTemplate") as DataTemplate;
            }

            return null;
        }
    }

    /// <summary>
    /// Interaction logic for VariablesView.xaml
    /// </summary>
    public partial class VariablesView : Window
    {
        #region Disable Minimize Button
        private const int GWL_STYLE = -16;

        private const int WS_MINIMIZE = -131073;

        public VariablesView()
        {
            InitializeComponent();
            this.SourceInitialized += (s, e) =>
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                var value = GetWindowLong(hwnd, GWL_STYLE);
                SetWindowLong(hwnd, GWL_STYLE, (int)(value & WS_MINIMIZE));

            };
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        #endregion

        VariablesController _controller;
        public VariablesView(Controller.Variables.VariablesController controller)
            :this()
        {
            this.DataContext = controller;
            this._controller = controller;
            controller.LoseFocus += loseFocus;
            InitializeComponent();
            SizeSavedWindowManager.AddToSizeSavedWindows(this);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void loseFocus(object sender, EventArgs e)
        {
            foreach (VariableController staticView in VariablesIteratorsControl.Items)
            {
                //System.Console.Write("Item!\n");
                staticView.DoLoseFocus();
            }
            //System.Console.Write("LoseFocus!\n");
            //Keyboard.ClearFocus();
        }

        #region GetDataFromListBox(ListBox,Point)
        private static object GetDataFromGrid(Grid source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    if (element.GetType() == typeof(View.Variables.VariableTypes.VariableDynamicView))
                    {
                        var localData = (View.Variables.VariableTypes.VariableDynamicView)element;
                        data = localData.DataContext;
                    }
                    if (element.GetType() == typeof(View.Variables.VariableTypes.VariableIteratorView))
                    {
                        var localData = (View.Variables.VariableTypes.VariableIteratorView)element;
                        data = localData.DataContext;
                    }

                    if (element.GetType() == typeof(View.Variables.VariableTypes.VariableStaticView))
                    {
                        var localData = (View.Variables.VariableTypes.VariableStaticView)element;
                        data = localData.DataContext;
                    }

                    if (data == DependencyProperty.UnsetValue)
                    {
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    }

                    if (element == source)
                    {
                        return null;
                    }
                }

                if (data != DependencyProperty.UnsetValue)
                {
                    return data;
                }
            }

            return null;
        }
        private static object GetDataFromListBox(ListBox source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    data = source.ItemContainerGenerator.ItemFromContainer(element);

                    if (data == DependencyProperty.UnsetValue)
                    {
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    }

                    if (element == source)
                    {
                        return null;
                    }
                }

                if (data != DependencyProperty.UnsetValue)
                {
                    return data;
                }
            }

            return null;
        }
        private static object GetRegionFromGrid(Grid source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;
                while (data == DependencyProperty.UnsetValue)
                {
                    if (element.GetType() == typeof(ListBox))
                    {
                        if (((ListBox)element).Name.Equals("VariablesStaticsControl"))
                        {
                            data = VariableType.VariableTypeStatic;
                        }
                        if (((ListBox)element).Name.Equals("VariablesIteratorsControl"))
                        {
                            data = VariableType.VariableTypeIterator;
                        }
                        if (((ListBox)element).Name.Equals("VariablesDynamicsControl"))
                        {
                            data = VariableType.VariableTypeDynamic;
                        }
                        //VariablesStaticsControl
                        //VariablesIteratorsControl
                        //VariablesDynamicsControl
                    }

                    if (data == DependencyProperty.UnsetValue)
                    {
                        element = VisualTreeHelper.GetParent(element) as UIElement;
                    }
                    //System.Console.Write("EL: {0}\n", element.GetType());

                    if (element == source)
                    {
                        return null;
                    }
                }

                if (data != DependencyProperty.UnsetValue)
                {
                    return data;
                }
            }

            return null;
        }
        #endregion

        private static bool MoveVariableWithArrowsIfNecessary(VariableController controller, System.Windows.Input.KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Up)
                {
                    controller.moveUp(null);
                    e.Handled = true;
                    return true;
                }
                else if (e.Key == Key.Down)
                {
                    controller.moveDown(null);
                    e.Handled = true;
                    return true;
                }
            }

            return false;
        }

        private void VariablesDynamicsControl_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (VariablesDynamicsControl.SelectedIndex >= 0)
            {
                VariableController selectedController = (VariableController)VariablesDynamicsControl.SelectedItem;
                MoveVariableWithArrowsIfNecessary(selectedController, e);
            }
        }

        private void VariablesIteratorsControl_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (VariablesIteratorsControl.SelectedIndex >= 0)
            {
                VariableController selectedController = (VariableController)VariablesIteratorsControl.SelectedItem;
                MoveVariableWithArrowsIfNecessary(selectedController, e);
            }
        }

    }
}
