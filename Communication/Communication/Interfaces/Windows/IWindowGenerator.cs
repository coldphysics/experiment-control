using Communication.Interfaces.Controller;
using System.Collections.Generic;
using System.Windows;

namespace Communication.Interfaces.Windows
{
    public interface IWindowGenerator
    {
        void AddWindow(string newWindow);
        Dictionary<string, Window> Generate(Dictionary<string, IWindowController> windows);
    }
}
