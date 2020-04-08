using Microsoft.Scripting.Hosting;
using System;

namespace PythonUtils
{
    public abstract class AbstractPythonExecutor
    {
        /// <summary>
        /// The scope manager
        /// </summary>
        protected PythonScopeManager scopeManager;

        /// <summary>
        /// The python engine
        /// </summary>
        public ScriptEngine engine;

        /// <summary>
        /// The scope of the script
        /// </summary>
        protected dynamic Scope
        {
            get
            {
                return scopeManager.Scope;
            }
        }

        protected AbstractPythonExecutor(ScriptEngine engine, PythonScopeManager scopeManager)
        {
            this.engine = engine;
            this.scopeManager = scopeManager;
        }

        /// <summary>
        /// Sets the value of a specific variable
        /// </summary>
        /// <param name="variableName">The name of the variable</param>
        /// <param name="variableValue">The new value of the variable</param>
        public void SetVariableValue(string variableName, object variableValue)
        {
            scopeManager.SetPyhtonVariableValue(variableName, variableValue);
        }

        /// <summary>
        /// Converts a numeric value to double
        /// </summary>
        /// <param name="value"></param>
        /// <returns>The double value</returns>
        protected double ConvertToDouble(object value)
        {
            return Double.Parse(value.ToString());
        }

        public string ExplainException(Exception e)
        {
            ExceptionOperations eo = engine.GetService<ExceptionOperations>();
            return eo.FormatException(e);
        }
    }
}
