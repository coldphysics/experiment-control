namespace PythonUtils.ScriptAnalysis
{
    /// <summary>
    /// The location information for a Python script used as a dynamic variable.
    /// </summary>
    /// <seealso cref="PythonUtils.ScriptAnalysis.IScriptLocation" />
    public class VariableBasedScriptLocation:IScriptLocation
    {
        /// <summary>
        /// Gets or sets the name of the variable.
        /// </summary>
        /// <value>
        /// The name of the variable.
        /// </value>
        public string VariableName
        {
            set;
            get;
        }
        #region BasicScriptLocation Members

        /// <summary>
        /// Gets the location as string.
        /// </summary>
        /// <returns>
        /// A string representation of the location of the script.
        /// </returns>
        public string GetLocationAsString()
        {
            return string.Format("Variable Name: {0}", VariableName);
        }

        #endregion
    }
}
