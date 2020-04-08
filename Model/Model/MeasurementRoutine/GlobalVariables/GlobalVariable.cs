namespace Model.MeasurementRoutine.GlobalVariables
{
    /// <summary>
    /// A global variable intended to be read by python scripts throughout the program
    /// </summary>
    /// <typeparam name="T">The data-type of this global variable</typeparam>
    public class GlobalVariable<T> : AbstractGlobalVariable where T : new()
    {
        /// <summary>
        /// The variable value
        /// </summary>
        private T variableValue;


        /// <summary>
        /// Gets or sets the variable value.
        /// </summary>
        /// <value>
        /// The variable value.
        /// </value>
        public T VariableValue
        {
            get { return variableValue; }
            set { variableValue = value; }
        }

        /// <summary>
        /// Resets the value of the global variable.
        /// </summary>
        public override void Reset()
        {
            VariableValue = new T();
        }

        /// <summary>
        /// Gets the value of this global variable.
        /// </summary>
        /// <returns>
        /// The value of the global variable
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override object GetValue()
        {
            return VariableValue;
        }
    }
}
