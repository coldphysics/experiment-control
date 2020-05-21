using Communication.Interfaces.Generator;
using Model.Data.Cards;
using Model.Settings;
using System;
using System.Linq;

namespace Generator.Generator.Concatenator
{
    /// <summary>
    /// The output of an analog card for a single sequence.
    /// </summary>
    public class AnalogCardOutput : INonQuantizedCard
    {
        /// <summary>
        /// The current offset index reached during the building of the output for each of the channels.
        /// </summary>
        private readonly uint[] _index;
        /// <summary>
        /// The output array of this card. The array is two-dimensional in which the first dimension indicates the channel index, and the second indicates the time-step index. 
        /// The value stored in the i,j element in this array represents the output value of the i-th channel for the j-th time-step.
        /// </summary>
        private double[,] _output;
        /// <summary>
        /// The model that describes the current analog card.
        /// </summary>
        private CardBasicModel _model;
        /// <summary>
        /// The total duration of the output of this card measured by the number of time-steps (does not consider replication).
        /// </summary>
        private uint _iDuration;

        /// <summary>
        /// Gets the output array of this card. The array is two-dimensional in which the first dimension indicates the channel index, and the second indicates the time-step index. 
        /// The value stored in the i,j element in this array represents the output value of the i-th channel for the j-th time-step.
        /// </summary>
        /// <value>
        /// The output array of this card.
        /// </value>
        public double[,] Output { get { return _output; } }
        /// <summary>
        /// Gets the sample rate.
        /// </summary>
        /// <value>
        /// The sample rate.
        /// </value>
        public double SampleRate { get; private set; }

        public double TotalDurationMillis
        {
            get
            {
                int numberOfSamples = Output.GetLength(1);
                return numberOfSamples * TimeSettingsInfo.GetInstance().SmallestTimeStep;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalogCardOutput"/> class.
        /// </summary>
        /// <param name="numberOfTimeSteps">The number of time-steps.</param>
        /// <param name="numberOfChannels">The number of channels.</param>
        /// <param name="sampleRate">The sample rate.</param>
        /// <param name="model">The model that describes this card.</param>
        public AnalogCardOutput(uint numberOfTimeSteps, uint numberOfChannels, double sampleRate, CardBasicModel model)
        {
            _model = model;
            SampleRate = sampleRate;
            _output = new double[numberOfChannels, numberOfTimeSteps];
            _index = new uint[numberOfChannels];
        }

        /// <summary>
        /// Adds the specified time-steps to the specified channel.
        /// </summary>
        /// <param name="timesteps">The time-steps to add.</param>
        /// <param name="channelNumber">The channel number.</param>
        public void Add(double[] timesteps, uint channelNumber)
        {
            uint offset = _index[channelNumber];

            for (uint iTimeStep = 0; iTimeStep < timesteps.Length; iTimeStep++)
            {
                _output[channelNumber, offset + iTimeStep] = timesteps[iTimeStep];
            }

            _index[channelNumber] += (uint)timesteps.Length;
        }

        /// <summary>
        /// Fills the possible missing values from some channels of the current sequence so that all channels would have the same duration.
        /// </summary>
        /// <param name="totalDurationOfSequence">The total duration of the sequence.</param>
        /// <param name="sequenceIndex">Index of the sequence. (0-based)</param>
        /// <remarks>
        /// The missing values (time-steps) are filled with the value of the last provided time-step of the channel where the missing values are.
        /// And if the current sequence is the first and the current channel doesn't already have any steps, then 0.0 is used.
        /// </remarks>
        public void FillSequence(uint totalDurationOfSequence, int sequenceIndex)
        {
            _iDuration += totalDurationOfSequence;//The new duration of the sequence

            //Looking for missing time-steps (and filling them) through all channels of this card.
            for (uint iChannel = 0; iChannel < _index.Length; iChannel++)
            {
                //the currently reached index for this channel
                uint indexTimeStep = _index[iChannel];

                //this channels has a "complete" set of time-steps, no action is required
                if (indexTimeStep == _iDuration)
                    continue;

                // Will store the value to be copied in place of the missing time-steps.
                // We assume the value 0.0 to consider all cases in which no previous step exists
                double lastValueOfChannel = 0.0;

                /*Determining the value to be copied in place of the missing time-steps*/
                /////////////////////////////////////////////////////////////////////////

                //special case: last known step in the current sequence has a zero duration!
                // if the model indicates that there are steps in the current channel for the current sequence and
                // the duration of the last provided step is zero (which means that its value is not present in the 
                // output array), then copy its value field.
                if (_model.Sequences[sequenceIndex].Channels[(int)iChannel].Steps.Count > 0
                    && _model.Sequences[sequenceIndex].Channels[(int)iChannel].Steps.Last().Duration.Value == 0)
                {

                    lastValueOfChannel =
                        _model.Sequences[sequenceIndex].Channels[(int)iChannel].Steps.Last().Value.Value;
                }
                //do we have any non-zero-duration steps provided for the current channel?
                else if (indexTimeStep > 0)
                {
                    //then the value to copy is the last provided value
                    lastValueOfChannel = _output[iChannel, indexTimeStep - 1];
                }

                /* Filling the missing values*/
                ///////////////////////////////

                //if the value to copy is zero, then no copying is needed because the output array already holds 0's in place of "missing" values. 
                if (Math.Abs(lastValueOfChannel) < 0.001)
                {
                    //advance the currently reached index for this channel forward
                    //RECO put this outside the if statement as it is needed by the if and the else
                    _index[iChannel] = _iDuration;
                    continue;
                }

                //Filling-in the missing values in the current channel
                for (uint iTimeStep = indexTimeStep; iTimeStep < _iDuration; iTimeStep++)
                {
                    _output[iChannel, iTimeStep] = lastValueOfChannel;
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
            double[,] result = new double[_output.GetLength(0), _output.GetLength(1) * timesToReplicate];

            for (int channel = 0; channel < result.GetLength(0); channel++)
            {
                for (int step = 0; step < result.GetLength(1); step++)
                {
                    result[channel, step] = _output[channel, step % _output.GetLength(1)];
                }
            }

            _output = result;
        }

        public double[] GetChannelOutput(int channelIndex)
        {
            double[] result = new double[Output.GetLength(1)];

            for (int i = 0; i < result.Length; i++)
                result[i] = Output[channelIndex, i];

            return result;
        }


        public ICardOutput DeepClone()
        {
            AnalogCardOutput result = new AnalogCardOutput((uint)Output.GetLength(1), (uint)Output.GetLength(0), SampleRate, _model);
            result._iDuration = _iDuration;
            for (int i = 0; i < Output.GetLength(0); i++)
                for (int j = 0; j < Output.GetLength(1); j++)
                    result.Output[i, j] = Output[i, j];

            return result;
        }
    }
}