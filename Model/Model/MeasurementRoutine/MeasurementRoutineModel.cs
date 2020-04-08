using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Model.MeasurementRoutine
{
    [DataContract]
    public class MeasurementRoutineModel
    {
        [DataMember]
        public RoutineBasedRootModel PrimaryModel { set; get; }

        [DataMember]
        public ObservableCollection<RoutineBasedRootModel> SecondaryModels { set; get; }

        [DataMember]
        public string RoutineInitializationScript { set; get; }

        [DataMember]
        // The script contains the algorithm that controls the execution order and frequency of measurement routine models. 
        public string RoutineControlScript { set; get; }

        public MeasurementRoutineModel()
        {
            PrimaryModel = new RoutineBasedRootModel() { TimesToReplicate = 1 };
            SecondaryModels = new ObservableCollection<RoutineBasedRootModel>();
        }

    }
}
