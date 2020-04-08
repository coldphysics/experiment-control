namespace PythonUtils.ScriptAnalysis
{
    /// <summary>
    /// The basic location information for a python script
    /// </summary>
    public interface IScriptLocation
    {
        /// <summary>
        /// Gets the location as string.
        /// </summary>
        /// <returns>A string representation of the location of the script.</returns>
        string GetLocationAsString();
    }
}
