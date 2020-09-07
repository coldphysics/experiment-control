using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Controller.OutputVisualizer.Export.Abstract
{
    public class DataPoint
    {
        public string CardName { set; get; }

        public int ChannelIndex { set; get; }

        public string SequenceName { set; get; }

        public int SequenceIndex { set; get; }

        public double OutputValue { set; get; }

        public decimal TimeMillis { set; get; }

        
    }
}
