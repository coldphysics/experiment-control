namespace Communication.Interfaces.Generator
{

    /// <summary>
    /// Represents the non quantized digital and analog cards 
    /// </summary>
    /// <seealso cref="Communication.Interfaces.Generator.ICardOutput" />
    public interface INonQuantizedCard : ICardOutput
    {
        /// <summary>
        /// Gets the channel output.
        /// </summary>
        /// <param name="channelIndex">Index of the channel.</param>
        double[] GetChannelOutput(int channelIndex);
    }
}
