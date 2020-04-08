using Model.V2.Data.SequenceGroups;
using Model.V2.Root;
using Model.V2.Variables;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Model.V2.Data
{
    /// <summary>
    /// Represents a container for all <see cref=" SequenceGroupModel"/>'s (i.e. experiments) 
    /// as well as all <c>Variables</c>. It also specifies the type of the Hardware.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class DataModel
    {
        //RECO hard-coded types. Should be avoided by the usage of polymorphism

        /// <summary>
        /// Lists all possible hardware types
        /// </summary>
        public enum TypeOfHardwareEnum
        {
            NationalInstruments,
            AdWin
        };

        /// <summary>
        /// A reference to the parent <see cref=" RootModel"/>
        /// </summary>
        private readonly RootModel _parent;

        /// <summary>
        /// A dictionary of (Name, <see cref=" SequenceGroupModel"/>) tuples
        /// </summary>
        [DataMember]
        public readonly Dictionary<string, SequenceGroupModel> Groups =
            new Dictionary<string, SequenceGroupModel>();

        /// <summary>
        /// Has the constant value "Data"
        /// </summary>
        public readonly string Name = "Data";//RECO useless, remove

        /// <summary>
        /// The type of hardware 
        /// </summary>
        [DataMember]
        public TypeOfHardwareEnum TypeOfHardware;


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
            bool flag = true;
            foreach (var group in Groups)
            {
                if (!group.Value.Verify())
                    flag = false;
            }
            return flag;
        }
    }
}