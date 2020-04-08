namespace Communication.Interfaces.Generator
{
    /// <summary>
    /// The interface of any DataGenerator which is responsible of generating the final output of a model.
    /// </summary>
    public interface IDataGenerator
    {
        /// <summary>
        /// Generates the final output.
        /// </summary>
        /// <returns>
        /// A Dictionary of key-value pairs.
        /// The keys are the <c>SequenceGroup</c>'s names, i.e. the experiment's names.
        /// The values are themselves dictionaries that map each card's name (as a <c>string</c>) to an object of a card type (from either the namespace <see cref=" Generator.AdWin.Concatenator" />, or the namespace <see cref=" Generator.NationalInstruments.Concatenator" />) which represents the final raw output.
        /// </returns>
        IModelOutput Generate();
    }
}
