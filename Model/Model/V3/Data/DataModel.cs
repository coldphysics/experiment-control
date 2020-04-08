using Model.V3.Data.SequenceGroups;
using Model.V3.Root;
using Model.V3.Variables;
using System;
using System.Runtime.Serialization;

namespace Model.V3.Data
{
    /// <summary>
    /// Represents a container for all <see cref=" SequenceGroupModel"/>'s (i.e. experiments) 
    /// as well as all <c>Variables</c>. It also specifies the type of the Hardware.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class DataModel
    {
        /// <summary>
        /// A reference to the parent <see cref=" RootModel"/>
        /// </summary>
        private readonly RootModel _parent;

        /// <summary>
        /// The sequence group
        /// </summary>
        [DataMember]
        public SequenceGroupModel group = null;


        /// <summary>
        /// A reference to <see cref=" VariablesModel"/> which manages the variables associated with the experiment
        /// </summary>
        [DataMember]
        public VariablesModel variablesModel;

        /// <summary>
        /// Constructs a new DataModel.
        /// Doesn't create any <see cref=" SequenceGroupModel"/> instances
        /// </summary>
        /// <param name="parent">The parent</param>
        public DataModel(RootModel parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// Delegates the validation to children
        /// </summary>
        /// <returns><c>true</c> if the <see cref=" DataModel"/> is valid, <c>false</c> otherwise</returns>
        public bool Verify()
        {
            return group.Verify();
        }
    }
}