namespace Communication.Interfaces.Model
{
    /// <summary>
    /// Describes a generic type for any model type.
    /// </summary>
    public interface IModel
    {
        /// <summary>
        /// Verifies whether the current model is valid or not. 
        /// </summary>
        /// <returns><c>true</c> if the current model is valid; otherwise, <c>false</c>.</returns>
        bool Verify();
    }
}