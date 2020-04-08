using System;

namespace Controller.AdWin.Simulator
{
    /// <summary>
    /// Represents a single output value.
    /// </summary>
    public class ChannelSnapshot
    {
        /// <summary>
        /// Gets or sets the date time the value is associated to.
        /// </summary>
        /// <value>
        /// The date time of the value is associated to.
        /// </value>
        public DateTime DateTime { get; set; }

        /// <summary>
        /// Gets or sets the value to output.
        /// </summary>
        /// <value>
        /// The value to output.
        /// </value>
        public double Value { get; set; }
    }
}
