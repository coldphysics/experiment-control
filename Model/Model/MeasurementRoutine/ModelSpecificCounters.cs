namespace Model.MeasurementRoutine
{
    /// <summary>
    /// Counters that are specific for the model
    /// </summary>

    public class ModelSpecificCounters
    {

        public int IterationOfScan = 1;

        public int CompletedScans;



        /// <summary>
        /// The value of the global counter when this model is executed for the first time
        /// </summary>
        public int StartCounterOfScansOfCurrentModel;



        /// <summary>
        /// indicates whether the <see cref="StartCounterOfScansOfCurrentModel"/> is set or not.
        /// it is used to prevent setting the <see cref=" StartCounterOfScansOfCurrentModel"/> multiple times.
        /// </summary>
        public bool GCIsSet = false;

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
