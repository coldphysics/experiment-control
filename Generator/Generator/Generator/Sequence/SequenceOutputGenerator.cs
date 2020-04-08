using AbstractController.Data.Card;
using AbstractController.Data.Sequence;
using Communication.Interfaces.Generator;
using Generator.Generator.Concatenator;
using Model.Data.Sequences;
using Model.Settings;
using System;

namespace Generator.Generator.Sequence
{
    /// <summary>
    /// Represents a generator that is capable of generating the output of a single sequence in a specific card.
    /// </summary>
    /// <seealso cref="AbstractController.Data.Sequence.AbstractSequenceController" />
    /// <seealso cref="Communication.Interfaces.Generator.IValueGenerator" />
    public class SequenceOutputGenerator : AbstractSequenceController, IValueGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceOutputGenerator"/> class.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="parent"></param>
        public SequenceOutputGenerator(SequenceModel model, AbstractCardController parent)
            : base(model, parent)
        {
        }

        #region IValueGenerator Members

        /// <summary>
        /// Generates the output for all the channels and then fills the possible "missing" values from some of these channels so that the output of all channels has the same length.
        /// </summary>
        /// <param name="concatenator">The concatenator.</param>
        public void Generate(IConcatenator concatenator)
        {
            if (Model.IsEnabled)
            {
                //generate the output for all channels
                foreach (IValueGenerator channel in Channels)
                {
                    channel.Generate(concatenator);
                }

                /*fill the missing output*/
                //get the maximum length of the output of this sequence measured in time steps
                uint totalSamplesofSequence = (uint)Math.Round(LongestDurationAllSequences() / TimeSettingsInfo.GetInstance().SmallestTimeStep);
                OutputConcatenator realConcatenator = (OutputConcatenator)concatenator;
                //fill the missing time-steps
                realConcatenator.FillSequence(totalSamplesofSequence, Index());
            }
        }

        #endregion
    }
}