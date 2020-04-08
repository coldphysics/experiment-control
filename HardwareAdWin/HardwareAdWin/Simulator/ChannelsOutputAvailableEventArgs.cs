using System;

namespace HardwareAdWin.Simulator
{
    /// <summary>
    /// Describes the signature of any method that is responsible of handling the <c>ChannelsOutputAvailable</c> event.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="args">The <see cref="ChannelsOutputAvailableEventArgs"/> instance containing the event data.</param>
    public delegate void ChannelsOutputAvailableEventHandler(object source, ChannelsOutputAvailableEventArgs args);

    /// <summary>
    /// The event arguments available when a batch of channel outputs is available to be sent to the event listeners.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ChannelsOutputAvailableEventArgs:EventArgs
    {
        /// <summary>
        /// Gets or sets the current output.
        /// </summary>
        /// <value>
        /// The current output.
        /// </value>
        /// <remarks>
        /// For each time step we have a double array that represents the output of all channels after a single iteration of the AdWin system.
        /// </remarks>
        public double[][] CurrentOutput
        {
            set;
            get;
        }
    }
}
