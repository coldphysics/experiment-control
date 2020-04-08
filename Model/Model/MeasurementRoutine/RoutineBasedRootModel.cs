using Model.Root;
using System.Runtime.Serialization;

namespace Model.MeasurementRoutine
{
    [DataContract]
    public class RoutineBasedRootModel
    {
        [DataMember]
        public string FilePath { set; get; }

        [DataMember]
        public int TimesToReplicate { set; get; }

        [IgnoreDataMember]
        public RootModel ActualModel { set; get; }

        [IgnoreDataMember]
        public ModelSpecificCounters Counters { set; get; }


        public RoutineBasedRootModel()
        {
            Counters = new ModelSpecificCounters();
        }

        /// <summary>
        /// Resets all iterators and model-specific counters of the model.
        /// </summary>
        public void Reset()
        {
            ResetIterators();
            ResetModelSpecificCounters();
        }

        /// <summary>
        /// Resets the model iterators.
        /// </summary>
        private void ResetIterators()
        {
            this.ActualModel.Data.variablesModel.ResetIterators();
        }

        /// <summary>
        /// Resets the model specific counters.
        /// </summary>
        private void ResetModelSpecificCounters()
        {
            this.Counters.Reset();
        }
    }
}
