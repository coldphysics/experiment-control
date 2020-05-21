using Communication.Interfaces.Generator;
using Model.Settings;

namespace Generator.Generator.Concatenator
{
    /// <summary>
    /// The output of a digital card for a single sequence.
    /// </summary>
    public class DigitalCardOutput : INonQuantizedCard
    {
        /// <summary>
        /// The current offset index reached during the building of the output for each of the channels.
        /// </summary>
        private readonly uint[] _index;
        /// <summary>
        /// Has one element per channel, and for the element i, it stores (output_value * 2^i) , 
        /// where output_value is the output value of the last step of the channel i in the current sequence (either 0 or 1).
        /// It is used by the <see cref=" FillSequence"/> method to fill in the possible missing output information for some channels in the current sequence.
        /// </summary>
        private readonly uint[] _lastValue;
        /// <summary>
        /// The output array of this card. It has one value per time-step, and each value encodes the output of all 32 channels (binary encoding).
        /// </summary>
        private uint[] _output;
        /// <summary>
        /// The total duration of the output of this card measured by the number of time-steps.
        /// </summary>
        private uint _iDuration;
        /// <summary>
        /// Gets the sample rate.
        /// </summary>
        /// <value>
        /// The sample rate.
        /// </value>
        public double SampleRate { get; private set; }
        /// <summary>
        /// Gets the output array of this card. It has one value per time-step, and each value encodes the output of all 32 channels (binary encoding).
        /// </summary>
        /// <value>
        /// The output array of this card.
        /// </value>
        public uint[] Output { get { return _output; } }

        public double TotalDurationMillis
        {
            get
            {
                int numberOfSamples = Output.Length;
                return numberOfSamples * TimeSettingsInfo.GetInstance().SmallestTimeStep;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalCardOutput"/> class.
        /// </summary>
        /// <param name="numberOfTimeSteps">The number of time-steps.</param>
        /// <param name="numberOfChannels">The number of channels.</param>
        /// <param name="sampleRate">The sample rate.</param>
        public DigitalCardOutput(uint numberOfTimeSteps, uint numberOfChannels, double sampleRate)
        {
            SampleRate = sampleRate;
            _index = new uint[numberOfChannels];
            _lastValue = new uint[numberOfChannels];
            _output = new uint[numberOfTimeSteps];
        }


        /// <summary>
        /// Adds the specified time-steps to the specified channel.
        /// </summary>
        /// <param name="timesteps">The time-steps to add. 
        /// They could all have the same value if the ramp step is used, or they could have possibly different values if a step of a file type is used.</param>
        /// <param name="channelNumber">The channel number.</param>
        public void Add(uint[] timesteps, uint channelNumber)
        {
            if (timesteps.Length == 0)//Nothing to add
                return;

            uint offset = _index[channelNumber];//the current position reached for this channel
            uint shiftedBit = 0;//will contain (timestep_value * 2^channel_number)


            for (uint iTimeStep = 0; iTimeStep < timesteps.Length; iTimeStep++)
            {
                shiftedBit = timesteps[iTimeStep] << (int)channelNumber;//useful when the time-step's value is 1 (for channel 3 this yields (1000)binary ), useless when the value is 0
                _output[offset + iTimeStep] |= shiftedBit;
            }

            _lastValue[channelNumber] = shiftedBit;//This overwrites the last stored value for this channel and guarantees that only the value for the last "step" is stored
            _index[channelNumber] += (uint)timesteps.Length;//Updating the last reached index for this channel
        }

        /// <summary>
        /// Fills the possible missing values from some channels of the current sequence so that all channels would have the same duration.
        /// </summary>
        /// <param name="totalDurationOfSequence">The total duration of the sequence.</param>
        /// <remarks>The missing values (time-steps) are filled with the value of the last provided time-step of the channel where the missing values are.</remarks>
        public void FillSequence(uint totalDurationOfSequence)
        {
            _iDuration += totalDurationOfSequence;//The new duration of the sequence

            //Looking for missing time-steps (and filling them) through all channels of this card.
            for (uint iChannel = 0; iChannel < _index.Length; iChannel++)
            {
                uint actualIndex = _index[iChannel];//the reached index for the channel iChannel.

                //this channels has a "complete" set of time-steps, no action is required
                if (actualIndex == _iDuration)
                    continue;

                //the last provided value for this channel
                uint lastValue = _lastValue[iChannel];

                //if the last provided value is 0, then there is no need for adding the missing 0-valued steps, 
                //as the binary encoding already "assumes" 0 output.
                if (lastValue == 0)
                {
                    //advance the currently reached index for this channel forward
                    //RECO put this outside the if statement as it is needed by the if and the else
                    _index[iChannel] = _iDuration;
                    continue;
                }

                //the missing time-steps are 1's so the output should be changed accordingly
                for (uint iTimeStep = actualIndex; iTimeStep < _iDuration; iTimeStep++)
                {
                    _output[iTimeStep] |= lastValue;
                }

                _index[iChannel] = _iDuration;
            }
        }

        /// <summary>
        /// Replicates the output.
        /// </summary>
        /// <param name="timesToReplicate">The times to replicate.</param>
        public void ReplicateOutput(int timesToReplicate)
        {
            uint[] result = new uint[_output.Length * timesToReplicate];

            for (int step = 0; step < result.Length; step++)
            {
                result[step] = _output[step % _output.Length];
            }

            _output = result;
        }

        private double ExtractChannelValue(uint allChannels, int channelIndex)
        {
            const double VALUE_TRUE = 5.0;
            const double VALUE_FALSE = 0.0;

            uint mask = (uint)(1 << channelIndex);
            double result = (allChannels & mask) == mask ? VALUE_TRUE : VALUE_FALSE;

            return result;
        }

        public double[] GetChannelOutput(int channelIndex)
        {
            double[] result = new double[Output.Length];

            for (int i = 0; i < result.Length; i++)
                result[i] = ExtractChannelValue(Output[i], channelIndex);

            return result;
        }

        public ICardOutput DeepClone()
        {
            DigitalCardOutput result = new DigitalCardOutput((uint)Output.Length, (uint)_lastValue.Length, SampleRate);
            result._iDuration = _iDuration;
            Output.CopyTo(result.Output, 0);
            return result;

        }
    }
}