using Communication.Interfaces.Model;
using Model.V1.Data;
using System;
using System.Runtime.Serialization;


namespace Model.V1.Root
{
    //RECO encapsulate fields into properties
    //RECO use Composite Design Pattern instead of the explicit has-a relationship
    //RECO could be useless;integrate with DataModel
    //CONSEQ take care of serializable hierarchy

    /// <summary>
    /// General container for the entire data model, also contains the global counter
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class RootModel : IModel
    {
        /// <summary>
        /// Represents the child model of the root model in the hierarchy
        /// </summary>
        [DataMember] public readonly DataModel Data;//RECO read-only variable is prefered to be capitalized
        /// <summary>
        /// Theoretical deadline of the beginning of the sequence cycle (estimated by the previous cycle)
        /// Is maintained and used by the <see cref="Buffer.Basic.OutputHandler"/>
        /// </summary>
        public DateTime EstimatedStartTime;
        /// <summary>
        /// Specifies whether the iteration mode is enabled
        /// Is maintained and used by the <see cref="Buffer.Basic.OutputHandler"/>
        /// </summary>
        public bool IsItererating;
        /// <summary>
        /// Unique key triggered by a variable (iterator, static, dynamic) change. It 
        /// is maintained and used by the <see cref="Buffer.Basic.OutputHandler"/>
        /// </summary>
        public int GlobalCounter;
        /// <summary>
        /// Contains the names and values of all variables used to be written to the database and to a text file.
        /// Is maintained and used by the <see cref="Buffer.Basic.OutputHandler"/>
        /// </summary>
        public System.Collections.Generic.List<string> VariablesList;

        /// <summary>
        /// Constructs a new RootModel.
        /// Creates an empty instance of <see cref="DataModel"/> as a child
        /// </summary>
        /// <param name="experiment">The type of the experiment</param>
        public RootModel(Global.ExperimentTypes experiment)
        {
            Global.Experiment = experiment;
            Data = new DataModel(this);
        }

        /// <summary>
        /// Verifies the validity of the user-input model (before applying it).
        /// Delegates the process to its child <see cref="DataModel"/>
        /// </summary>
        /// <returns><c>true</c> if the model is valid, <c>false</c> otherwise</returns>
        public bool Verify()
        {
            return Data.Verify();
        }
    }
}