using Model.V3.Data.SequenceGroups;
using System;
using System.Runtime.Serialization;

namespace Model.V3.Data.Cards
{
    /// <summary>
    /// A simple implementation to <see cref=" CardBasicModel"/>
    /// </summary>
    [Serializable]
    [DataContract]
    public class CardModel : CardBasicModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CardModel"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="numberOfChannels">The number of channels.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="type">The type.</param>
        /// <param name="sequenceGroup">The sequence group.</param>
        public CardModel(string name, uint numberOfChannels, CardType type, SequenceGroupModel sequenceGroup)
            : base(name, numberOfChannels, type, sequenceGroup)
        {
        }
    }
}