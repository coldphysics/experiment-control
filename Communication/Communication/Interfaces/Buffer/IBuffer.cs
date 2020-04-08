using Communication.Interfaces.Model;
using System;

namespace Communication.Interfaces.Buffer
{
    /// <summary>
    /// Represents a buffer between the GUI and the output mechanism.
    /// </summary>
    public interface IBuffer
    {
        /// <summary>
        /// Copies a new and valid model hierarchy to the buffer in order to send it eventually to the hardware system.
        /// </summary>
        /// <param name="copyJob">A new and a valid model hierarchy.</param>
        /// <param name="timesToReplicateOutput">The number of times to replicate output.</param>
        void CopyData(IModel copyJob, int timesToReplicateOutput);

        /// <summary>
        /// Occurs when the current state of the generator changes.
        /// </summary>
        event EventHandler OnGeneratorStateChange;
    }
}