using AbstractController.Data.Card;
using Communication.Interfaces.Generator;
using Generator.Generator.Concatenator;
using Generator.Generator.SequenceGroup;
using Model.Data.Cards;

namespace Generator.Generator.Card
{
    /// <summary>
    /// Generates the output of a digital card and stores the results in a <see cref=" IConcatenator"/>
    /// </summary>
    /// <seealso cref="AbstractController.Data.Card.AbstractCardController" />
    /// <seealso cref="Communication.Interfaces.Generator.IValueGenerator" />
    public class DigitalCardOutputGenerator : AbstractCardController, IValueGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalCardOutputGenerator"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="parent">The parent.</param>
        public DigitalCardOutputGenerator(CardBasicModel model, SequenceGroupOutputGenerator parent)
            : base(model, parent)
        {
        }

        #region IValueGenerator Members

        /// <summary>
        /// Sets the current card of the concatenator to this card, and delegates the task to the child <see cref=" AbstractController.Data.Card.AbstractCardController.Tabs"/>
        /// </summary>
        /// <param name="concatenator">The concatenator.</param>
        public void Generate(IConcatenator concatenator)
        {
            var realConcatenator = (OutputConcatenator)concatenator;
            realConcatenator.ActualCardName = Model.Name;

            foreach (IValueGenerator sequence in Tabs)
            {
                sequence.Generate(concatenator);
            }
        }

        #endregion
    }
}