using System;
using System.Collections.Generic;
using Communication.Interfaces.Generator;
using Model.Settings;

namespace Buffer.OutputProcessors.Quantization
{
    /// <summary>
    /// Holds the quantized analog output
    /// </summary>
    /// <seealso cref="Communication.Interfaces.Generator.ICardOutput" />
    public class QuantizedAnalogCardOutput : ICardOutput
    {
        private List<List<uint>> output;

        /// <summary>
        /// Gets or sets the output.
        /// </summary>
        /// <value>
        /// The output.
        /// </value>
        public List<List<uint>> Output
        {
            get { return output; }
            set { output = value; }
        }

        public double TotalDurationMillis
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuantizedAnalogCardOutput"/> class.
        /// </summary>
        public QuantizedAnalogCardOutput()
        {
            output = new List<List<uint>>();
        }


        /// <summary>
        /// Replicates the output.
        /// </summary>
        /// <param name="timesToReplicate">The times to replicate.</param>
        public void ReplicateOutput(int timesToReplicate)
        {
            List<List<uint>> result = new List<List<uint>>();
            List<uint> currentReplicatedChannel;

            foreach (List<uint> channelOutput in output)
            {
                currentReplicatedChannel = new List<uint>();
                result.Add(currentReplicatedChannel);

                for (int count = 1; count <= timesToReplicate; count++)
                {
                    currentReplicatedChannel.AddRange(channelOutput);
                }
            }

            TotalDurationMillis *= timesToReplicate;
            output = result;
        }

        public ICardOutput DeepClone()
        {
            throw new NotImplementedException();
        }

    }
}
