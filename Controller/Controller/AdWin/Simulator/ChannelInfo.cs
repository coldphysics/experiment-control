using System;

namespace Controller.AdWin.Simulator
{
    /// <summary>
    /// All information necessary to identify a channel
    /// </summary>
    public class ChannelInfo
    {
        /// <summary>
        /// Gets or sets the name of the card this channel belongs to.
        /// </summary>
        /// <value>
        /// The name of the card.
        /// </value>
        public string CardName { set; get; }

        /// <summary>
        /// Gets or sets the index of the channel within the card.
        /// </summary>
        /// <value>
        /// The index of the channel.
        /// </value>
        public int ChannelIndex { set; get; }

        /// <summary>
        /// Gets or sets the channel total order among all channels of all cards.
        /// </summary>
        /// <value>
        /// The channel total order.
        /// </value>
        public int ChannelTotalOrder { set; get; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            string result = String.Format("{0}-{1:00}", CardName, ChannelIndex + 1);

            return result;
        }
    }
}
