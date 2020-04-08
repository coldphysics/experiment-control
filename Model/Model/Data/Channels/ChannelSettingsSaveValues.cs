namespace Model.Data.Channels
{
    /// <summary>
    /// The values that are saved in the default channel settings values file for a specific channel
    /// </summary>
    public class ChannelSettingsSaveValues
    {
        /// <summary>
        /// The name of the channel 
        /// </summary>
        public string Name;
        /// <summary>
        /// The maximum value the channel output should have.
        /// </summary>
        public double Max;
        /// <summary>
        /// The minimum value the channel output should have.
        /// </summary>
        public double Min;
    }
}
