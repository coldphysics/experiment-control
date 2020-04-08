using System.ComponentModel;

namespace Controller.AdWin.Debug
{
    public class DebugItemController:INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public int TotalFifoSize
        {
            get { return _totalFifoSize; }
            set
            {
                _totalFifoSize = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("TotalFifoSize"));
                }
            }
        }
        private int _totalFifoSize;


        public int CurrentFifoLoad
        {
            get { return _currentFifoLoad; }
            set 
            {
                _currentFifoLoad = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CurrentFifoLoad"));
                }
            }
        }
        private int _currentFifoLoad;

        public int CurrentFifoLoadPercent
        {
            get { return _currentFifoLoadPercent; }
            set
            {
                _currentFifoLoadPercent = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("CurrentFifoLoadPercent"));
                }
            }
        }
        private int _currentFifoLoadPercent;


        public string ChannelName { get; set; }
    }
}
