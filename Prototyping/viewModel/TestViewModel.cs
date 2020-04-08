using Communication.Commands;
using LiveCharts;

using LiveCharts.Configurations;
using LiveCharts.Defaults;
using LiveCharts.Events;
using System;

namespace Prototyping.viewModel
{
    //type
    public delegate void UserTriggeredChangeRangeHandler(object sender, RangeChangedEventArgs e);
    

    class TestViewModel : BasicViewModel
    {

        private bool _ischecked;

        public bool Ischecked
        {
            get { return _ischecked; }
            set { _ischecked = value; }
        }

        public event UserTriggeredChangeRangeHandler RangeChanged;

        public ChartValues<DateTimePoint> OutputArray = new ChartValues<DateTimePoint>();
       
        private ChartValues<DateTimePoint> manipulatedArray;

        public ChartValues<DateTimePoint> ManipulatedArray
        {
            get { return manipulatedArray; }
            set { manipulatedArray = value;
            RaisePropertyChanged("ManipulatedArray");
            }
        }
        
        // int milliSeconds = 1000;
      //  MyArray mr = new MyArray();
        const long STEP = 2000000;//100 nanosecond
        const double STEP_SIZE = 0.02;// in milliseconds
        const int NUM_OF_SAMPLES = 100;
        

        //public ChartValues<DateTimePoint> temp2 = new ChartValues<DateTimePoint>();
        //private ChartValues<DateTimePoint> temp3 = new ChartValues<DateTimePoint>();

        //public ChartValues<DateTimePoint> Temp3
        //{
        //    get { return temp3; }
        //    set
        //    {
        //        temp3 = value;
        //        RaisePropertyChanged("Temp3");
        //    }
        //}

        private string someEvent;

        public string SomeEvent
        {
            get { return someEvent; }
            set
            {
                someEvent = value;
                RaisePropertyChanged("SomeEvent");
            }
        }


        //  public ChartValues<ObservablePoint> temp = new ChartValues<ObservablePoint>();

        private RelayCommand rangeChangedCommand;

        public RelayCommand RangeChangedCommand
        {
            get
            {
                return rangeChangedCommand;
            }
            set
            {
                rangeChangedCommand = value;
            }
        }
        public TestViewModel()
        {
            var dayConfig = Mappers.Xy<DateTimePoint>()
            .X(dayModel => (double)((dayModel.DateTime.Ticks) * 0.00000001))
            .Y(dayModel => dayModel.Value);
            LiveCharts.Charting.For<DateTimePoint>(dayConfig);

           LoadArray();
            rangeChangedCommand = new RelayCommand(AxisChanged);
        }

        protected void OnRangeChanged(RangeChangedEventArgs args)
        {
            if (RangeChanged != null)
                RangeChanged(this, args);
        }
         
        private void AxisChanged(object parameter)
        {
            RangeChangedEventArgs eventargs = (RangeChangedEventArgs)parameter;
            double max = ((LiveCharts.Wpf.Axis)eventargs.Axis).ActualMaxValue;
            double min = ((LiveCharts.Wpf.Axis)eventargs.Axis).ActualMinValue;
            double range = eventargs.Range;
            min = Math.Max(0, min);
            int arrayMin = (int)Math.Round(min / STEP_SIZE);
            int arrayMax = (int)Math.Round(max / STEP_SIZE);
           // arrayMax = Math.Min(arrayMax, OutputArray.Count);
             arrayMax = Math.Min(arrayMax, OutputArray.Count);
            ArrayManipulation(arrayMin, arrayMax);
            double d = (arrayMax - arrayMin) / NUM_OF_SAMPLES;//  remove
            int t = (int)Math.Ceiling(d);//remove
            if (t == 0) t = 1;//  remove
            SomeEvent = String.Format("Arraymin={0}, Arraymax={1} , maxAxis={2} , minAxis={3}, Range ={4} SamplingRate={5}", arrayMin, arrayMax, max, min, range,t);
           // if (Ischecked) SomeEvent = String.Format("test={0}",t);

            OnRangeChanged(eventargs);
        }
        //private void ArrayManipulation(object parameter)
        //{
        //    RangeChangedEventArgs eventargs = (RangeChangedEventArgs)parameter;

        //    double max = ((LiveCharts.Wpf.Axis)eventargs.Axis).ActualMaxValue;
        //    double min = ((LiveCharts.Wpf.Axis)eventargs.Axis).ActualMinValue;
        //    double range = eventargs.Range;
        //    min = Math.Max(0, min);
        //    int Arraymin = (int)Math.Round(min / 0.02);
        //    int ArrayMax = (int)Math.Round(max / 0.02);
        //    int Arraymax = Math.Min(ArrayMax, mr.C.Count);
        //    ChartValues<double> tempArray = new ChartValues<double>();

        //    for (int i = Arraymin; i < Arraymax; i++)
        //    {
        //        tempArray.Add(mr.C[i]);
        //    }

        //    temp2.Clear();
        //    int SamplingStep = tempArray.Count / 100;
        //    int k = 0;
        //    int l = Arraymin;

        //    for (int j = 0; j < 100 && k < tempArray.Count; j++)
        //    {
        //        DateTime d = new DateTime(l * step);
        //        DateTimePoint t = new DateTimePoint(d, tempArray[k]);
        //        temp2.Add(t);
        //        k += SamplingStep;
        //        l += SamplingStep;
        //    }
        //    Temp3 = temp2;
        //    SomeEvent = String.Format("Arraymin={0}, Arraymax={1} , maxAxis={2} , minAxis={3}, Range ={4}", Arraymin, Arraymax, max, min, range);





            // ChartValues<ObservablePoint> s=new ChartValues<ObservablePoint>();
            //for (int k =(int) min; k < mr.C.Count -(int) max;k++ )
            //{
            //    temp.Add(mr.C[l]);
            //    l += 10;
            //}


      //  }
        public void LoadArray()
        {

            double[] testArray = new double[100000];
            for (int i = 0; i < 100000; i++)
            {
                if (i < 20000)
                    testArray[i] = i;
                // mr.C.Add(r.NextDouble());
                // mr.C.Add(i);
                else if (i < 40000)
                    testArray[i] = i / 2;
                //  mr.C.Add(i / 2);
                else if (i < 60000)
                    testArray[i] = i * 3;
                //mr.C.Add(i * 3);
                else if (i < 80000)
                    testArray[i] = 8000 - i;
                // mr.C.Add(80000 - i);
                else testArray[i] = i;
                //  mr.C.Add(i);
             
            }
            SetData(testArray);
        }


        //    int k = 0;
        //    for (int j = 0; j < 100; j++)
        //    {


        //        //if (j==0)
        //        // d = new DateTime(1, 1, 1, 0,0,0, 0);
        //        //else if (j>=1  & j<60  )
        //        //{
        //        //    d = new DateTime(1, 1, 1, 0, 0, j, 0);
        //        //}
        //        //else { d = new DateTime(1, 1, 1, 0, j / 60, 0, 0); }
        //        DateTime d = new DateTime(k * step);
        //        DateTimePoint t = new DateTimePoint(d, mr.C[k]);
        //        temp2.Add(t);
        //        k += 1000;
        //    }
        //    Temp3 = temp2;
        //}
        public void SetData(double[] output)
        {
            int sizeOfArray = output.GetLength(0);
           
            
            for(int i=0;i< sizeOfArray;i++)
            {
                DateTime dt = new DateTime(i* STEP);
                DateTimePoint dtp = new DateTimePoint(dt, output[i]);
                OutputArray.Add(dtp);
                

            }
            ArrayManipulation(0, OutputArray.Count);
        }
        public void ArrayManipulation(int min,int max)
            {
               
               
                ChartValues<DateTimePoint> tempArray = new ChartValues<DateTimePoint>();
                double temp = (max - min) / NUM_OF_SAMPLES;
                int samplingRate = (int)Math.Ceiling (temp);
            if(samplingRate==0)
            { samplingRate = 1; }

             int k=min;

             for (int j = 0; j < 100 && k < max ; j++)
                     {
                         tempArray.Add(OutputArray[k]);
                         k += samplingRate;

                     }
                 ManipulatedArray= tempArray;
        //    {
        //        DateTime d = new DateTime(l * step);
        //        DateTimePoint t = new DateTimePoint(d, tempArray[k]);
        //        temp2.Add(t);
        //        k += SamplingStep;
        //        l += SamplingStep;
        //    }
        //    Temp3 = temp2;
               


            }

    }
}
