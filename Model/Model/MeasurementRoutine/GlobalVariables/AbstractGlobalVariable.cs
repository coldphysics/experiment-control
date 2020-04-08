namespace Model.MeasurementRoutine.GlobalVariables
{
    /// <summary>
    /// An abstract representation of a global variable
    /// </summary>
    public abstract class AbstractGlobalVariable
    {
        /// <summary>
        /// The variable name
        /// </summary>
        private string variableName;

        /// <summary>
        /// Gets or sets the name of the variable.
        /// </summary>
        /// <value>
        /// The name of the variable.
        /// </value>
        public string VariableName
        {
            get { return variableName; }
            set { variableName = value; }
        }

        /// <summary>
        /// Resets the value of the global variable.
        /// </summary>
        public abstract void Reset();

        /// <summary>
        /// Gets the value of this global variable.
        /// </summary>
        /// <returns>The value of the global variable</returns>
        public abstract object GetValue();
    }
}
