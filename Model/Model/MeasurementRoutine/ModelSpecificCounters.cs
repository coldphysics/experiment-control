namespace Model.MeasurementRoutine
{
    /// <summary>
    /// Counters that are specific for the model
    /// </summary>
    public class ModelSpecificCounters
    {
        public ModelSpecificCounters()
        {
            IterationOfScan = 1;
            GCIsSet = false;
        }

        public int IterationOfScan { set; get; }

        public int CompletedScans { set; get; }

        /// <summary>
        /// The value of the global counter when this model is executed for the first time
        /// </summary>
        public int StartCounterOfScansOfCurrentModel { set; get; }

        /// <summary>
        /// indicates whether the <see cref="StartCounterOfScansOfCurrentModel"/> is set or not.
        /// it is used to prevent setting the <see cref=" StartCounterOfScansOfCurrentModel"/> multiple times.
        /// Details: in a routine, each model should have its own counters object, so the StartCounterOfScans should not be set 
        /// every time LoadModel method is invoked. Only the first time (or the first time we start iterating)
        /// </summary>
        public bool GCIsSet { set; get; }


        /// <summary>
        /// Resets model-specific counters.
        /// </summary>
        public void Reset()
        {
            GCIsSet = false;
            IterationOfScan = 1;
            CompletedScans = 0;
        }
    }
}
