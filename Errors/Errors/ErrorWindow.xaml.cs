using CustomElements.SizeSavedWindow;
using Errors.Error;
using Errors.Error.ErrorItems;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace Errors
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ErrorWindow : Window
    {
        #region Disable Minimize Button
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MINIMIZE = -131073;
        #endregion

        public ErrorCollector errorClass;

        public static Window MainWindow;

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)  
        {
            e.Cancel = true;
            this.Hide();
        }

        public ErrorWindow()
        {
            InitializeComponent();
            this.SourceInitialized += (s, e) =>
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                var value = GetWindowLong(hwnd, GWL_STYLE);
                SetWindowLong(hwnd, GWL_STYLE, (int)(value & WS_MINIMIZE));

            };
            SizeSavedWindowManager.AddToSizeSavedWindows(this);
            ErrorCollector errorClass = ErrorCollector.Instance;
            errorClass.SetParent(this);
            this.DataContext = errorClass;
        }

        public void blink()
        {
            System.Console.Write("ErrorBlink!\n");
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var taskInfo = new System.Windows.Shell.TaskbarItemInfo();
                taskInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Error;
                taskInfo.ProgressValue = 100;
                MainWindow.TaskbarItemInfo = taskInfo;
                this.Activate();
            }));
            new Thread(delegate()
            {
                Console.Beep(2000, 200);
                Thread.Sleep(200);
                Console.Beep(2000, 200);
                Thread.Sleep(200);
                Console.Beep(2000, 200);
            }).Start();
        }

        public object TaskBarLock = new object();
        public void stopBlink()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var taskInfo = new System.Windows.Shell.TaskbarItemInfo();
                taskInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
                MainWindow.TaskbarItemInfo = taskInfo;
            }));
        }

        private void DeleteThisErrorClick(object sender, RoutedEventArgs e)
        {
            if (sender.GetType() != typeof(Button))
            {
                return;
            }
            Button realSender = (Button) sender;
            ErrorCollector errorClass = ErrorCollector.Instance;

            // FIXME it might happen that get a DisconnectedItem here (see http://go4answers.webhost4life.com/Question/thread-brought-attention-response-442346.aspx)
            if (realSender.DataContext.GetType() != typeof (AbstractErrorItem))
            {
                Console.WriteLine("error: wrong class");
                return;
            }
            if (realSender.DataContext is ErrorHeader)
            {
                errorClass.RemoveErrorsOfWindowEvenStayOnDelete(((ErrorHeader) realSender.DataContext).ErrorCategory);
            }
            else
            {
                errorClass.RemoveSingleError(((ConcreteErrorItem) realSender.DataContext));
            }
        }

        private void ClearCategoryClick(object sender, RoutedEventArgs e)
        {
            Button realSender = (Button)sender;
            ErrorCollector errorClass = ErrorCollector.Instance;
            // FIXME it might happen that get a DisconnectedItem here (see http://go4answers.webhost4life.com/Question/thread-brought-attention-response-442346.aspx)
            if (realSender.DataContext.GetType() != typeof(AbstractErrorItem))
            {
                Console.WriteLine("error: wrong class");
                return;
            }
            errorClass.RemoveErrorsOfWindow(((AbstractErrorItem)realSender.DataContext).ErrorCategory);
        }

        private void OpenCategoryClick(object sender, RoutedEventArgs e)
        {
            Button realSender = (Button)sender;
            ErrorCollector errorClass = ErrorCollector.Instance;
            // FIXME it might happen that get a DisconnectedItem here (see http://go4answers.webhost4life.com/Question/thread-brought-attention-response-442346.aspx)
            if (realSender.DataContext.GetType() != typeof(AbstractErrorItem))
            {
                Console.WriteLine("error: wrong class");
                return;
            }
            if (((AbstractErrorItem)realSender.DataContext).ErrorCategory == Error.ErrorCategory.Pulseblaster)
            {
                errorClass.ShowPulseblaster = true;
                return;
            }
            if (((AbstractErrorItem)realSender.DataContext).ErrorCategory == Error.ErrorCategory.Basic)
            {
                errorClass.ShowBasic = true;
                return;
            }
            if (((AbstractErrorItem)realSender.DataContext).ErrorCategory == Error.ErrorCategory.Variables)
            {
                errorClass.ShowVariables = true;
                return;
            }
            if (((AbstractErrorItem)realSender.DataContext).ErrorCategory == Error.ErrorCategory.MainHardware)
            {
                errorClass.ShowMainHardware = true;
                return;
            }
            if (((AbstractErrorItem)realSender.DataContext).ErrorCategory == Error.ErrorCategory.Python)
            {
                errorClass.ShowPython = true;
                return;
            }
        }

        private void CloseCategoryClick(object sender, RoutedEventArgs e)
        {
            Button realSender = (Button)sender;
            //System.Console.Write("real sender: {0}\n", (((ErrorItem)realSender.DataContext)).errorWindow);
            ErrorCollector errorClass = ErrorCollector.Instance;
            // FIXME it might happen that get a DisconnectedItem here (see http://go4answers.webhost4life.com/Question/thread-brought-attention-response-442346.aspx)
            if (realSender.DataContext.GetType() != typeof(AbstractErrorItem))
            {
                Console.WriteLine("error: wrong class");
                return;
            }
            if (((AbstractErrorItem)realSender.DataContext).ErrorCategory == Error.ErrorCategory.Pulseblaster)
            {
                errorClass.ShowPulseblaster = false;
                return;
            }
            if (((AbstractErrorItem)realSender.DataContext).ErrorCategory == Error.ErrorCategory.Basic)
            {
                errorClass.ShowBasic = false;
                return;
            }
            if (((AbstractErrorItem)realSender.DataContext).ErrorCategory == Error.ErrorCategory.Variables)
            {
                errorClass.ShowVariables = false;
                return;
            }
            if (((AbstractErrorItem)realSender.DataContext).ErrorCategory == Error.ErrorCategory.MainHardware)
            {
                errorClass.ShowMainHardware = false;
                return;
            }
            if (((AbstractErrorItem)realSender.DataContext).ErrorCategory == Error.ErrorCategory.Python)
            {
                errorClass.ShowPython = false;
                return;
            }
        }
    }

    public class PropertyDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate errorItemTemplate { get; set; }
        public DataTemplate errorItemTemplateCrit { get; set; }
        public DataTemplate errorItemHeaderTemplate { get; set; }
        public DataTemplate errorItemHeaderTemplateUp { get; set; }

        public override DataTemplate SelectTemplate(object item,
                   DependencyObject container)
        {
            ErrorCollector errorClass = ErrorCollector.Instance;
            //System.Console.Write("typeof item : {0}\n", container.GetType());
            if (item is ErrorHeader)
            {
                if (((ErrorHeader)item).ErrorCategory == Error.ErrorCategory.Pulseblaster)
                {
                    if (errorClass.ShowPulseblaster)
                    {
                        return errorItemHeaderTemplateUp;
                    }
                    return errorItemHeaderTemplate;
                }
                if (((ErrorHeader)item).ErrorCategory == Error.ErrorCategory.Basic)
                {
                    if (errorClass.ShowBasic)
                    {
                        return errorItemHeaderTemplateUp;
                    }
                    return errorItemHeaderTemplate;
                }
                if (((ErrorHeader)item).ErrorCategory == Error.ErrorCategory.Variables)
                {
                    if (errorClass.ShowVariables)
                    {
                        return errorItemHeaderTemplateUp;
                    }
                    return errorItemHeaderTemplate;
                }
                if (((ErrorHeader)item).ErrorCategory == Error.ErrorCategory.MainHardware)
                {
                    if (errorClass.ShowMainHardware)
                    {
                        return errorItemHeaderTemplateUp;
                    }
                    return errorItemHeaderTemplate;
                }
                if (((ErrorHeader)item).ErrorCategory == Error.ErrorCategory.Python)
                {
                    if (errorClass.ShowPython)
                    {
                        return errorItemHeaderTemplateUp;
                    }
                    return errorItemHeaderTemplate;
                }
            }
            if (((ConcreteErrorItem)item).StayOnDelete)
            {
                return errorItemTemplateCrit;
            }
            else
            {
                return errorItemTemplate;
            }
           
        }
    }
   
}