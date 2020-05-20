using System.Collections.Generic;
using System.Windows;
using Communication.Interfaces.Windows;

namespace Controller.Control
{
    public class WindowList : IWindowList
    {
        private readonly Dictionary<string, Window> _existingWindows = new Dictionary<string, Window>();

        public WindowList(Dictionary<string, Window> newWindowList)
        {
            _existingWindows = newWindowList;
        }

        #region IWindowList Members

        public Dictionary<string, Window> Windows()
        {
            return _existingWindows;
        }

        public void AddWindow(string name, Window window)
        {
            _existingWindows.Add(name, window);
        }

        public void CloseAll()
        {
            foreach (var window in _existingWindows)
            {
                window.Value.Close();
            }
        }

        public void CloseByName(string name)
        {
            if (_existingWindows.ContainsKey(name))
            {
                _existingWindows[name].Close();
            }
        }

        public void ShowAll()
        {
            CloseAll();

            foreach (var window in _existingWindows)
            {
                window.Value.Show();
            }
        }

        public void ShowByName(string name)
        {
            if (_existingWindows.ContainsKey(name))
            {
                CloseByName(name);
                _existingWindows[name].Show();
            }
        }

        #endregion
    }
}