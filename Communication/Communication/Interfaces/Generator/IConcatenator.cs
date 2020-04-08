namespace Communication.Interfaces.Generator
{
    /// <summary>
    /// A wrapper type for accumulating the output time-steps for all cards
    /// </summary>
    public interface IConcatenator
    {
        /// <summary>
        /// Generate the final output of the concatenator.
        /// </summary>
        /// <returns>
        ///  A dictionary that maps each card's name (as a <c>string</c>) to an object of a card type (from either the namespace <see cref=" Generator.AdWin.Concatenator" />, or the namespace <see cref=" Generator.NationalInstruments.Concatenator" />) which represents the final raw output.
        /// </returns>
        IModelOutput Output();
    }
}