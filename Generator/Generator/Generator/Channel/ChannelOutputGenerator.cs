using AbstractController.Data.Channels;
using Communication.Interfaces.Generator;
using Generator.Generator.Concatenator;
using Generator.Generator.Sequence;
using Model.Data.Channels;

namespace Generator.Generator.Channel
{
    /// <summary>
    /// Represents a generator that is capable of generating the output of a single channel. 
    /// Notice that the output length might not conform with the length of the output of the other channels for the same sequence.
    /// </summary>
    /// <seealso cref="AbstractController.Data.Channels.AbstractChannelController" />
    /// <seealso cref="Communication.Interfaces.Generator.IValueGenerator" />
    public class ChannelOutputGenerator : AbstractChannelController, IValueGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelOutputGenerator"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="parent">The parent.</param>
        public ChannelOutputGenerator(ChannelModel model, SequenceOutputGenerator parent)
            : base(model, parent)
        {
            
        }

        #region IValueGenerator Members

        /// <summary>
        /// Sets the current channel index as the active channel index in the concatenator, 
        /// and then generates the output of all steps of this channel.
        /// </summary>
        /// <param name="concatenator">The concatenator.</param>
        public void Generate(IConcatenator concatenator)
        {
            OutputConcatenator realConcatenator = (OutputConcatenator)concatenator;
            //Set the index of the current channel as the active channel index
            realConcatenator.ActualChannelNumber = (uint)Index();

            //Generate the output of all the steps.
            foreach (IValueGenerator step in Steps)
            {
                step.Generate(concatenator);
            }
        }

        #endregion
    }
}