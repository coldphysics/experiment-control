using System;
using System.Collections.Generic;
using System.Windows;
using Communication.Interfaces.Controller;
using Communication.Interfaces.Windows;
using Controller.Data.Windows;
using Model.Data.Cards;
using View.Data.Windows;

namespace View.Helper
{
    public class WindowGenerator : IWindowGenerator
    {
        #region WindowType enum

        public enum WindowType
        {
            Analog,
            Digital
        };

        #endregion

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
                //System.Console.Write("{0}\n", window.Key);

                if (window.Key.Equals("Errors"))
                {
                    if (errorsWindow == null)
                        errorsWindow = new Errors.ErrorWindow();
                    output.Add(window.Key, errorsWindow);
                }
                else if (window.Value.GetType() == typeof(Controller.Variables.VariablesController))
                {
                    if(variablesWindow == null)
                        variablesWindow = new View.Variables.VariablesView((Controller.Variables.VariablesController)window.Value);
                    output.Add(window.Key, variablesWindow);
                }
                else
                {
                    if (!_windowsToCreate.Contains(window.Key))
                        throw new Exception("No window defined for controller: " + window.Key);
                    var realController = (WindowBasicController)window.Value;
                    switch (realController.CardType())
                    {
                        case CardBasicModel.CardType.Analog:
                            output.Add(window.Key, new WindowAnalogView(realController));
                            break;
                        case CardBasicModel.CardType.Digital:
                            output.Add(window.Key, new WindowDigitalView(realController));
                            break;
                    }
                }
            }
            return output;
        }
    }
}