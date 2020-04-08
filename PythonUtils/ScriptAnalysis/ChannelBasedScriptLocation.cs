using System;

namespace PythonUtils.ScriptAnalysis
{
    /// <summary>
    /// The location information for a python script at a channel level
    /// </summary>
    /// <seealso cref="PythonUtils.ScriptAnalysis.IScriptLocation" />
    public class ChannelBasedScriptLocation:IScriptLocation
    {
        /// <summary>
        /// Gets or sets the name of the card.
        /// </summary>
        /// <value>
        /// The name of the card.
        /// </value>
        public string CardName { set; get; }
        /// <summary>
        /// Gets or sets the index of the channel.
        /// </summary>
        /// <value>
        /// The index of the channel.
        /// </value>
        public int ChannelIndex { set; get; }

        #region BasicScriptLocation Members

        /// <summary>
        /// Gets the location as string.
        /// </summary>
        /// <returns>
        /// A string representation of the location of the script.
        /// </returns>
        public virtual string GetLocationAsString()
        {
            return String.Format("Card: {0}, Channel: {1}", CardName, ChannelIndex);
        }

        #endregion
    }
}
