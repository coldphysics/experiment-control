

namespace Controller.OutputVisualizer.Export.Abstract
{
    /// <summary>
    /// Represents a single data point (sample) in the output.
    /// </summary>
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
