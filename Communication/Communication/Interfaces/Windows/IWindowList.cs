using System.Collections.Generic;
using System.Windows;

namespace Communication.Interfaces.Windows
{
    public interface IWindowList
    {
        Dictionary<string, Window> Windows();
        void AddWindow(string name, Window window);        
        void CloseAll();
        void ShowAll();
        void CloseByName(string name);
        void ShowByName(string name);
    }
}