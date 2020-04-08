namespace Communication.Interfaces.Generator
{
    /// <summary>
    /// Represents the generated output of a card
    /// </summary>
    public interface ICardOutput
    {
        /// <summary>
        /// Replicates the output.
        /// </summary>
        /// <param name="timesToReplicate">The times to replicate.</param>
        void ReplicateOutput(int timesToReplicate);

        ICardOutput DeepClone();
    }
}
