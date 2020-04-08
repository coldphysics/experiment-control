using AbstractController.Data.Card;
using AbstractController.Data.SequenceGroup;
using Communication.Interfaces.Generator;
using Generator.Generator.Concatenator;
using Model.Data.Cards;

namespace Generator.Generator.Card
{
    //RECO unify with the digital card logic, they are exact copies

    /// <summary>
    /// Generates the output of an analog card and stores the results in a <see cref=" IConcatenator"/>
    /// </summary>
    /// <seealso cref="AbstractController.Data.Card.AbstractCardController" />
    /// <seealso cref="Communication.Interfaces.Generator.IValueGenerator" />
    public class AnalogCardOutputGenerator : AbstractCardController, IValueGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalogCardOutputGenerator"/> class.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="parent"></param>
        public AnalogCardOutputGenerator(CardBasicModel model, AbstractSequenceGroupController parent)
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
            var realConcatenator = (OutputConcatenator) concatenator;
            realConcatenator.ActualCardName = Model.Name;
            foreach (IValueGenerator sequence in Tabs)
            {
                sequence.Generate(concatenator);
            }
        }

        #endregion
    }
}