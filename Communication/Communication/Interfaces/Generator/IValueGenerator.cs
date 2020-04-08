namespace Communication.Interfaces.Generator
{
    /// <summary>
    /// An interface of a type that is able to generate raw output and add it to a <see cref=" IConcatenator"/>
    /// </summary>
    public interface IValueGenerator
    {
        /// <summary>
        /// Generates the raw output and add it to the specified concatenator.
        /// </summary>
        /// <param name="concatenator">The concatenator.</param>
        void Generate(IConcatenator concatenator);
    }
}