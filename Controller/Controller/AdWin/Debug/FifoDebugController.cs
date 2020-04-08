using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using Communication.Interfaces.Controller;
using Communication.Interfaces.Hardware;


namespace Controller.AdWin.Debug
{

    public class FifoDebugController : INotifyPropertyChanged, IWindowController
    {
        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public ObservableCollection<DebugItemController> Bars
        {
            get { return _Bars; }
            set 
            {
                _Bars = value;
                if (null != this.PropertyChanged)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Bars"));
                    //System.Console.Write("update2!\n");
                }
            }
        }
        private ObservableCollection<DebugItemController> _Bars = new ObservableCollection<DebugItemController>();


        private int MAX_BARS; // analog: 8 buffers, digital: 1 buffer

        public FifoDebugController(List<IAdWinDebug> hardwareInterface)
        {
            MAX_BARS = Global.GetNumOfBuffers(); // creates Bar controllers equals to the number of FIFOs we have
            _hardwareInterface = hardwareInterface;
            for (int i = 0; i < MAX_BARS; i++)
            {
                Bars.Add(new DebugItemController() { ChannelName = i.ToString()});
            }
            //System.Console.Write("starting Thread!\n");
            Thread thread = new Thread(updateFifoBars);
            thread.Name = "Update Fifo ProgressBars";
            thread.IsBackground = true;
            thread.Start();
        }

        private List<IAdWinDebug> _hardwareInterface;
    
        private void updateFifoBars()
        {
            while (true)
            {
                for (int i = 0; i < MAX_BARS; i++)
                {
                    Bars[i].TotalFifoSize = _hardwareInterface[i].GetTotalFifo();
                    Bars[i].CurrentFifoLoad = Bars[i].TotalFifoSize-_hardwareInterface[i].GetFifoStatus();
                    Bars[i].CurrentFifoLoadPercent = Bars[i].CurrentFifoLoad*100/Bars[i].TotalFifoSize;
                    //System.Console.Write("{0} ", Bars[i].CurrentFifoLoad);
                }
                //System.Console.Write("\n");
                if (null != this.PropertyChanged)
                {
                    //PropertyChanged(Bars, new PropertyChangedEventArgs("CurrentFifoLoad"));
                    //System.Console.Write("update2!\n");
                }

                object lockObj = new object();
                lock (lockObj)
                {
                    Monitor.Wait(lockObj, 200); // Thread.Sleep(100);
                }
            }
        }
    }
}
