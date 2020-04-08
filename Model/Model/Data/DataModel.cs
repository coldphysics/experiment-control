using Model.Data.SequenceGroups;
using Model.Root;
using Model.Variables;
using System;
using System.Runtime.Serialization;

namespace Model.Data
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
        /// A dictionary of (Name, <see cref=" SequenceGroupModel"/>) tuples
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