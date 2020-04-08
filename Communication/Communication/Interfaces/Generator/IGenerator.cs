namespace Communication.Interfaces.Generator
{
    /// <summary>
    /// Represents a generator that is capable of generating the output for all cards. It is mainly implemented by <c>SequenceGroupOutputGenerator</c>'s.
    /// </summary>
    public interface IGenerator
    {
        /// <summary>
        /// Generates the output of the entire sequence group.
        /// </summary>
        /// <returns>an object of a card output type.</returns>
        IModelOutput Generate();

    }
}