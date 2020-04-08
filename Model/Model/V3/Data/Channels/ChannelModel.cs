using Model.V3.Data.Sequences;
using System;
using System.Runtime.Serialization;

namespace Model.V3.Data.Channels
{
    /// <summary>
    /// A derived class of <see cref=" ChannelBasicModel"/> contains a simple implementation.
    /// </summary>
    [Serializable]
    [DataContract]
    public class ChannelModel : ChannelBasicModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelModel"/> class.
        /// </summary>
        /// <param name="parent">The sequence this channel belongs to</param>
        public ChannelModel(SequenceModel parent) : base(parent)
        {
        }
    }
}