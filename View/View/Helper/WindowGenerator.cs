using System;
using System.Collections.Generic;
using System.Windows;
using Communication.Interfaces.Controller;
using Communication.Interfaces.Windows;
using Controller.Data.Windows;
using Controller.Helper;

namespace View.Helper
{
    public class WindowGenerator : IWindowGenerator
    {
        public enum WindowType
        {
            Analog,
            Digital
        };

        private readonly List<string> _windowsToCreate;
        private Window errorsWindow = null;
        private Window variablesWindow = null;

        public WindowGenerator(List<string> windowsToCreate)
        {
            _windowsToCreate = windowsToCreate;
        }

        public void AddWindow(string newWindow)
        {
            _windowsToCreate.Add(newWindow);
        }

        public Dictionary<string, Window> Generate(Dictionary<string, IWindowController> windows)
        {
            var output = new Dictionary<string, Window>();

            foreach (var window in windows)
            {

                if (window.Key.Equals("Errors"))
                {
                    if (errorsWindow == null)
                        errorsWindow = new Errors.ErrorWindow();

                    output.Add(window.Key, errorsWindow);
                }
                else if (window.Value.GetType() == typeof(Controller.Variables.VariablesController))
                {
                    if (variablesWindow == null)
                    {
                        variablesWindow = WindowsHelper.CreateWindowToHostViewModel(window.Value, true, false, true, true);
                        variablesWindow.Title = "Variables";
                        variablesWindow.ShowInTaskbar = false;
                    }

                    output.Add(window.Key, variablesWindow);
                }
                else
                {
                    if (!_windowsToCreate.Contains(window.Key))
                        throw new Exception("No window defined for controller: " + window.Key);

                    var realController = (WindowBasicController)window.Value;
                    Window currentWindow = WindowsHelper.CreateWindowToHostViewModel(realController, true, false, true, true);
                    currentWindow.ShowInTaskbar = false;
                    currentWindow.Title = realController.Name;
                    output.Add(window.Key, currentWindow);
                }
            }

            return output;
        }
    }
}