using Communication.Interfaces.Model;

namespace Communication.Interfaces.Generator
{
    /// <summary>
    /// An interface for a type that capable of cooking an <see cref=" IDataGenerator"/> for a specific <see cref=" IModel"/>.
    /// </summary>
    public interface IGeneratorRecipe
    {
        /// <summary>
        /// Cooks an <see cref=" IDataGenerator"/> based on a specific <see cref=" IModel"/>.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>The cooked data output generator.</returns>
        IDataGenerator Cook(IModel model);
    }
}