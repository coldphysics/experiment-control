using Communication.Interfaces.Generator;
using Communication.Interfaces.Model;
using Generator.Generator;
using Generator.Generator.Card;
using Generator.Generator.Channel;
using Generator.Generator.Sequence;
using Generator.Generator.SequenceGroup;
using Generator.Generator.Step;
using Model.Data.Cards;
using Model.Data.Channels;
using Model.Data.SequenceGroups;
using Model.Data.Sequences;
using Model.Data.Steps;
using System;

namespace Generator.Cookbook
{
    /// <summary>
    /// The factory (cook-book) for  sequence group generators.
    /// </summary>
    /// <seealso cref="Communication.Interfaces.Generator.IWrapSequenceGroupGenerator" />
    public class SequenceGroupGeneratorRecipe : IWrapSequenceGroupGenerator
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceGroupGeneratorRecipe"/> class.
        /// </summary>
        public SequenceGroupGeneratorRecipe()
        {

        }


        #region IWrapSequenceGroupGenerator Members

        /// <summary>
        /// Cooks the sequence group output generator by cooking its digital and analog card output generators, and adds the resulting generator to the parent <see cref=" IDataGenerator"/>.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="dataGenerator">The data generator.</param>
        /// <exception cref="System.Exception">This card does not belong to this CookSequence Group:  + card.Name</exception>
        public void CookSequenceGroup(ISequenceGroup model, IDataGenerator dataGenerator)
        {
            SequenceGroupModel sequenceGroupModel = (SequenceGroupModel)model;
            DataOutputGenerator dataOutputGenerator = (DataOutputGenerator)dataGenerator;
            SequenceGroupOutputGenerator sequenceGroupOutputGenerator = new SequenceGroupOutputGenerator(sequenceGroupModel, dataOutputGenerator);

            foreach (CardBasicModel card in sequenceGroupModel.Cards)
            {
                //if (!_cards.Contains(card.Name))
                //    throw new Exception("This card does not belong to this CookSequence Group: " + card.Name);

                if (card.Type == CardBasicModel.CardType.Analog)
                    CookAnalogCard(card, sequenceGroupOutputGenerator);
                if (card.Type == CardBasicModel.CardType.Digital)
                    CookDigitalCard(card, sequenceGroupOutputGenerator);
            }

            dataOutputGenerator.SequenceGroup = sequenceGroupOutputGenerator;
        }

        #endregion

        /// <summary>
        /// Cooks an analog card output generator by cooking its <see cref=" SequenceOutputGenerator"/>'s.
        /// Adds the resulting generator to the parent <see cref=" SequenceGroupOutputGenerator"/>.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="sequenceGroupGenerator">The sequence group output generator.</param>
        private void CookAnalogCard(CardBasicModel model, SequenceGroupOutputGenerator sequenceGroupGenerator)
        {
            AnalogCardOutputGenerator cardOutputGenerator = new AnalogCardOutputGenerator(model, sequenceGroupGenerator);

            foreach (SequenceModel sequence in model.Sequences)
            {
                CookSequence(sequence, cardOutputGenerator);
            }

            sequenceGroupGenerator.Windows.Add(cardOutputGenerator);
        }

        /// <summary>
        /// Cooks a digital card output generator by cooking its <see cref=" SequenceOutputGenerator"/>'s.
        /// Adds the resulting generator to the parent <see cref=" SequenceGroupOutputGenerator"/>.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="sequenceGroupGenerator">The sequence group generator.</param>
        private void CookDigitalCard(CardBasicModel model, SequenceGroupOutputGenerator sequenceGroupGenerator)
        {
            var cardOutputGenerator = new DigitalCardOutputGenerator(model, sequenceGroupGenerator);

            foreach (SequenceModel sequence in model.Sequences)
            {
                CookSequence(sequence, cardOutputGenerator);
            }

            sequenceGroupGenerator.Windows.Add(cardOutputGenerator);
        }

        /// <summary>
        /// Cooks a sequence output generator which is related to an analog card by cooking its channel output generators and the step output generator for each file. Adds the resulting output generator to the parent <see cref=" AnalogCardOutputGenerator"/>.
        /// </summary>
        /// <param name="sequence">The sequence.</param>
        /// <param name="cardGenerator">The analog card output generator.</param>
        private void CookSequence(SequenceModel sequence, AnalogCardOutputGenerator cardGenerator)
        {
            var sequenceGenerator = new SequenceOutputGenerator(sequence, cardGenerator);
            cardGenerator.Tabs.Add(sequenceGenerator);

            foreach (ChannelModel channel in sequence.Channels)//cook all sequences
            {
                var channelController = new ChannelOutputGenerator(channel, sequenceGenerator);
                sequenceGenerator.Channels.Add(channelController);

                foreach (StepBasicModel step in channel.Steps)//cook all steps of a sequence
                {
                    object stepController = null;

                    if (step is StepFileModel)
                    {
                        stepController = new AnalogStepFileOutputGenerator((StepFileModel)step, channelController);
                    }
                    else if(step is StepRampModel)
                    {
                        stepController = new AnalogStepRampOutputGenerator((StepRampModel)step, channelController);
                    }
                    else if (step is StepPythonModel)
                    {
                        stepController = new AnalogStepPythonOutputGenerator((StepPythonModel)step, sequence.Card()._parent._parent, channelController);
                    }
                    else
                    {
                        throw new Exception("Step type " + step.GetType().ToString() + " is not recognized!");
                    }

                    channelController.Steps.Add(stepController);
                }
            }
        }

        /// <summary>
        /// Cooks a sequence output generator which is related to a digital card by cooking its channel output generators and the step output generator for each file. Adds the resulting output generator to the parent <see cref=" AnalogCardOutputGenerator"/>.
        /// </summary>
        /// <param name="sequence">The sequence.</param>
        /// <param name="cardGenerator">The analog card output generator.</param>
        private void CookSequence(SequenceModel sequence, DigitalCardOutputGenerator cardGenerator)
        {
            var sequenceGenerator = new SequenceOutputGenerator(sequence, cardGenerator);
            cardGenerator.Tabs.Add(sequenceGenerator);

            foreach (ChannelModel channel in sequence.Channels)
            {
                var channelController = new ChannelOutputGenerator(channel, sequenceGenerator);
                sequenceGenerator.Channels.Add(channelController);

                foreach (StepBasicModel step in channel.Steps)
                {
                    if (step.GetType() == typeof(StepFileModel))//RECO use "is" keyword
                    {
                        var stepController = new DigitalStepFileOutputGenerator((StepFileModel)step, channelController);
                        channelController.Steps.Add(stepController);
                    }
                    else
                    {
                        var stepController = new DigitalStepRampOutputGenerator((StepRampModel)step, channelController);
                        channelController.Steps.Add(stepController);
                    }
                }
            }
        }

    }
}