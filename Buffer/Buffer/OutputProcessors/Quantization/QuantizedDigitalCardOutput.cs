using System;
using System.Collections.Generic;
using Communication.Interfaces.Generator;

namespace Buffer.OutputProcessors.Quantization
{
    /// <summary>
    /// Holds the quantized digital output for one card.
    /// </summary>
    /// <seealso cref="Communication.Interfaces.Generator.ICardOutput" />
    public class QuantizedDigitalCardOutput : ICardOutput
    {
        private List<uint> output;

        /// <summary>
        /// Gets or sets the output.
        /// </summary>
        /// <value>
        /// The output.
        /// </value>
        public List<uint> Output
        {
            get { return output; }
            set { output = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuantizedDigitalCardOutput"/> class.
        /// </summary>
        public QuantizedDigitalCardOutput()
        {
        }

        #region ICardOutput Members

        /// <summary>
        /// Replicates the output.
        /// </summary>
        /// <param name="timesToReplicate">The times to replicate.</param>
        public void ReplicateOutput(int timesToReplicate)
        {
            List<uint> result = new List<uint>();

            for (int count = 1; count <= timesToReplicate; count++)
            {
                result.AddRange(output);
            }

            output = result;
        }

        public ICardOutput DeepClone()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
