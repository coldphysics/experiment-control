using AbstractController.Data.SequenceGroup;
using Communication.Interfaces.Generator;
using Generator.Generator.Concatenator;
using Model.Data.Cards;
using Model.Data.SequenceGroups;
using Model.Settings;
using System;

namespace Generator.Generator.SequenceGroup
{
    /// <summary>
    /// Represents a generator that is capable of generating the output of a <c>SequenceGroup</c>.
    /// </summary>
    /// <seealso cref="AbstractController.Data.SequenceGroup.AbstractSequenceGroupController" />
    /// <seealso cref="Communication.Interfaces.Generator.IGenerator" />
    public class SequenceGroupOutputGenerator : AbstractSequenceGroupController, IGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SequenceGroupOutputGenerator"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="parent">The parent.</param>
        public SequenceGroupOutputGenerator(SequenceGroupModel model, DataOutputGenerator parent)
            : base(model, parent)
        {
        }

        #region IGenerator Members


        /// <summary>
        /// Generates the output of the entire sequence group.
        /// </summary>
        /// <returns>
        /// A dictionary that maps each card's name (as a <c>string</c>) to an object of a card type.
        /// </returns>
        public IModelOutput Generate()
        {
            OutputConcatenator concatenator = new OutputConcatenator();
            //how many time steps the whole SequenceGroup has
            uint length = (uint)Math.Round(Duration() / TimeSettingsInfo.GetInstance().SmallestTimeStep);
            uint numberOfChannels;
            string name;
            AnalogCardOutput analogCard;
            DigitalCardOutput digitalCard;

            //creates all analog and digital cards and adds all of them to the concentrator
            foreach (CardModel card in Model.Cards)
            {
                CardBasicModel.CardType type = card.Type;
                numberOfChannels = card.NumberOfChannels;
                name = card.Name;

                if (type == CardBasicModel.CardType.Analog)
                {
                    analogCard = new AnalogCardOutput(length, numberOfChannels, TimeSettingsInfo.GetInstance().SampleRateValue, card);
                    concatenator.AddCard(name, analogCard);
                }
                else if (type == CardBasicModel.CardType.Digital)
                {
                    digitalCard = new DigitalCardOutput(length, numberOfChannels, TimeSettingsInfo.GetInstance().SampleRateValue);
                    concatenator.AddCard(name, digitalCard);
                }
            }

            foreach (IValueGenerator valueGenerator in Windows)
            {
                valueGenerator.Generate(concatenator);
            }

            return concatenator.Output();
        }

        #endregion

    }
}