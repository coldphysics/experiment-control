namespace Communication.Interfaces.Generator
{
    /// <summary>
    /// Represents the generated output of a card
    /// </summary>
    public interface ICardOutput
    {
        /// <summary>
        /// Returns the total duration of the output of this card measured in milliseconds
        /// </summary>
        double TotalDurationMillis
        {
            get;
        }
        /// <summary>
        /// Replicates the output.
        /// </summary>
        /// <param name="timesToReplicate">The times to replicate.</param>
        void ReplicateOutput(int timesToReplicate);

        ICardOutput DeepClone();
    }
}
