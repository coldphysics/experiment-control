using System.ComponentModel;

namespace Controller
{
    /// <summary>
    /// The parent class for all controllers
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public abstract class BaseController:INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets a value indicating whether the <see cref=" PropertyChanged"/> event is attached.
        /// </summary>
        /// <value>
        /// <c>true</c> if the event is attached; otherwise, <c>false</c>.
        /// </value>
        public bool IsEventAttached
        {
            get
            {
                return PropertyChanged != null;
            }
        }

        /// <summary>
        /// Called when a property value is changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
