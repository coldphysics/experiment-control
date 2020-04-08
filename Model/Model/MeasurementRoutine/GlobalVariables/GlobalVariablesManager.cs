using System.Collections.Generic;
using System.Linq;

namespace Model.MeasurementRoutine.GlobalVariables
{
    /// <summary>
    /// Manages global variables that are intended to be set by the measurement routine script
    /// and read by the other python scripts of the application.
    /// </summary>
    /// <remarks>Implements the Singleton design pattern.</remarks>
    public class GlobalVariablesManager
    {
        /// <summary>
        /// The single instance
        /// </summary>
        private static GlobalVariablesManager instance = null;
        /// <summary>
        /// A collection holding all the global variables
        /// </summary>
        private ICollection<AbstractGlobalVariable> globalVariablesPrototypes;

        /// <summary>
        /// Prevents a default instance of the <see cref="GlobalVariablesManager"/> class from being created.
        /// </summary>
        private GlobalVariablesManager()
        { }

        /// <summary>
        /// Gets the single instance of this class (lazy loading)
        /// </summary>
        /// <returns>The single instance of this class</returns>
        public static GlobalVariablesManager GetInstance()
        {
            if (instance == null)
            {
                instance = new GlobalVariablesManager();
                instance.globalVariablesPrototypes = GlobalVariablesFactory.GetInstance().BuildVariables();
            }

            return instance;
        }

        /// <summary>
        /// Resets the values of all global variables.
        /// </summary>
        public void ResetGlobalVariables()
        {
            foreach (AbstractGlobalVariable variable in globalVariablesPrototypes)
            {
                variable.Reset();
            }
        }

        /// <summary>
        /// Gets the variable with the specified name
        /// </summary>
        /// <typeparam name="T">The data-type of the variable to get.</typeparam>
        /// <param name="variableName">Name of the variable.</param>
        /// <returns>
        /// The global variable with the specified name, 
        /// or <c>null</c> if the name does not belong to a known variable.
        /// </returns>
        public GlobalVariable<T> GetVariableByName<T>(string variableName)where T:new()
        {
            return (GlobalVariable<T>)this.globalVariablesPrototypes.Where(variable =>
            {
                return (variable.VariableName.Equals(variableName));
            }).FirstOrDefault();
        }

        /// <summary>
        /// Sets the value of the global variable specified by its name.
        /// </summary>
        /// <typeparam name="T">The data-type of the variable.</typeparam>
        /// <param name="variableName">Th name of the variable.</param>
        /// <param name="value">The new value of the global variable.</param>
        public void SetVariableValueByName<T>(string variableName, T value) where T : new()
        {
            GlobalVariable<T> result = (GlobalVariable<T>)this.globalVariablesPrototypes.Where(variable =>
            {
                return (variable.VariableName.Equals(variableName));
            }).FirstOrDefault();

            if (result != null)
                result.VariableValue = value;
        }

        /// <summary>
        /// Gets all global variables.
        /// </summary>
        /// <returns>A collection containing all global variables.</returns>
        public ICollection<AbstractGlobalVariable> GetAllGlobalVariables()
        {
            return globalVariablesPrototypes;
        }
    }
}
