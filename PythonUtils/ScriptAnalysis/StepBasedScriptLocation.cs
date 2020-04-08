using System;

namespace PythonUtils.ScriptAnalysis
{
    /// <summary>
    /// The location information for a python script used as a step.
    /// </summary>
    /// <seealso cref="Generator.Python.ErrorAnalysis.BasicScriptLocation" />
    public class StepBasedScriptLocation : ChannelBasedScriptLocation
    {
        /// <summary>
        /// Gets or sets the name of the sequence.
        /// </summary>
        /// <value>
        /// The name of the sequence.
        /// </value>
        public string SequenceName { set; get; }
        /// <summary>
        /// Gets or sets the index of the step.
        /// </summary>
        /// <value>
        /// The index of the step.
        /// </value>
        public int StepIndex { set; get; }

        /// <summary>
        /// Gets the location as string.
        /// </summary>
        /// <returns>
        /// A string representation of the location of the script.
        /// </returns>
        public override string GetLocationAsString()
        {
            return String.Format("Card: {0}, Sequence: {1}, Channel: {2}, Step: {3}", CardName, SequenceName, ChannelIndex, StepIndex);
        }
    }
}
