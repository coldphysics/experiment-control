using Communication.Interfaces.Model;

namespace Communication.Interfaces.Generator
{
    /// <summary>
    /// The interface for any factory (cook-book) for the <c>SequenceGroupGenerator</c>.
    /// </summary>
    public interface IWrapSequenceGroupGenerator
    {
        /// <summary>
        /// Cooks the sequence group generator.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="dataGenerator">The data generator.</param>
        void CookSequenceGroup(ISequenceGroup model, IDataGenerator dataGenerator);
    }
}