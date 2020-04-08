namespace Prototyping.Model
{
    class DataModel
    {

        public System.DateTime DateTime { get; set; }
        public double Value { get; set; }
        public DataModel(System.DateTime t, double v)
        {
            DateTime = t;
            Value = v;
        }

    }
}
