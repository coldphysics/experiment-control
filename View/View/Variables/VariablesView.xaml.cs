using Controller.Variables;
using CustomElements.SizeSavedWindow;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Communication;
using System.Windows.Interop;
using System.Runtime.InteropServices;
namespace View.Variables
{
    /// <summary>
    /// Interaction logic for VariablesView.xaml
    /// </summary>
    public partial class VariablesView : Window
    {
        #region Disable Minimize Button
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

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
        #endregion

        VariablesController _controller;
        public VariablesView(Controller.Variables.VariablesController controller)
            :this()
        {
            this.DataContext = controller;
            this._controller = controller;
            controller.LoseFocus += loseFocus;
            InitializeComponent();
            SizeSavedWindow.addToSizeSavedWindows(this);
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

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        #region GetDataFromListBox(ListBox,Point)
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

        //private bool dragging = false;
        //Controller.Variables.VariableController draggedItem;

        /*private void VariablesControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBox parent = (ListBox)sender;
            object data = GetDataFromListBox(parent, e.GetPosition(parent));
            if (data != null && !((Controller.Variables.VariableController)data).VariableLocked)
            {
                //DragDrop.DoDragDrop(parent, data, DragDropEffects.Move);
                dragging = true;
                draggedItem = (Controller.Variables.VariableController)data;
                //System.Console.Write("Drag: {0}\n", draggedItem.VariableName);
            }
            base.OnPreviewMouseLeftButtonDown(e);
        }*/

        /*private void VariablesControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            dragging = false;
            this.Cursor = Cursors.Arrow;
            base.OnPreviewMouseLeftButtonUp(e);
        }*/

        /*private void VariablesControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (dragging && e.LeftButton == MouseButtonState.Released)
            {
                dragging = false;
                this.Cursor = Cursors.Arrow;
            }
            if (dragging && e.LeftButton == MouseButtonState.Pressed)
            {
                this.Cursor = Cursors.Hand;
                Grid parent = (Grid)sender;
                object data = GetDataFromGrid(parent, e.GetPosition(parent));
                if (data != null && !((Controller.Variables.VariableController)data).VariableLocked)
                {
                    Controller.Variables.VariableController droppedItem = (Controller.Variables.VariableController)data;
                    //System.Console.Write("Drag: {0}\n", droppedItem.VariableName);
                    if (draggedItem != droppedItem)
                    {
                        _controller.DoDragDrop(draggedItem, droppedItem);
                    }
                }
                else
                {
                    data = GetRegionFromGrid(parent, e.GetPosition(parent));
                    if (data != null)
                    {

                        if (data.GetType().Equals(typeof(CustomElements.VariableType)))
                        {
                            CustomElements.VariableType newType = (CustomElements.VariableType)data;
                            //System.Console.Write("Drag 0\n");
                            if (newType == CustomElements.VariableType.VariableTypeIterator && _controller.IsIteratorsLocked())
                            { }
                            else
                            {
                                _controller.DoDragDropRegion(draggedItem, newType);
                            }
                        }
                    }
                }
            }
            base.OnPreviewMouseMove(e);
        }*/
    }




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
}
