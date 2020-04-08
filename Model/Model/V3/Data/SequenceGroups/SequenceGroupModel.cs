using Communication.Interfaces.Model;
using Model.V3.Data.Cards;
using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Model.V3.Data.SequenceGroups
{
    /// <summary>
    /// Holds the sampling settings and the list of the cards
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class SequenceGroupModel : ISequenceGroup
    {
        /// <summary>
        /// A reference to its child <see cref="CardBasicModel"/> 
        /// </summary>
        [DataMember] public readonly ObservableCollection<CardBasicModel> Cards;

        /// <summary>
        /// A reference to its parent <see cref="DataModel"/> 
        /// </summary>
        [DataMember] private DataModel _parent;


        /// <summary>
        /// Initializes a new instance of this class. It creates an empty list of <see cref="CardBasicModel"/>'s
        /// </summary>

        /// <param name="parent">The parent.</param>
        /// <param name="name">The name.</param>
        public SequenceGroupModel(DataModel parent)
        {
            _parent = parent;
            Cards = new ObservableCollection<CardBasicModel>();
        }

        /// <summary>
        /// Delegates the validation to children
        /// </summary>
        /// <returns><c>true</c> if the <see cref=" SequenceGroupModel"/> is valid, <c>false</c> otherwise</returns>
        public bool Verify()
        {
            bool flag = true;

            foreach (CardBasicModel card in Cards)
            {
                if (!card.Verify())
                    flag = false;
            }
            return flag;
        }

        
    }
}