using Model.Data;

namespace PythonUtils.ScriptExecution
{
    /// <summary>
    /// Builds an instance of <see cref=" HighPerformancePythonScriptExecutor"/>
    /// </summary>
    public class HighPerformancePythonScriptExecutorBuilder:AbstractPythonScriptExecutorBuilder
    {
        /// <summary>
        /// The names of the input variable 
        /// </summary>
        private string[] inputVariableNames;
        /// <summary>
        /// The output variable name
        /// </summary>
        private string outputVariableName;


        /// <summary>
        /// Sets the name of the output variable.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        public void SetOutputVariableName(string variableName)
        {
            this.outputVariableName = variableName;
        }

        /// <summary>
        /// Sets the input variable names.
        /// </summary>
        /// <param name="variableNames">The variable names.</param>
        public void SetInputVariableNames(params string[] variableNames)
        {
            this.inputVariableNames = variableNames;
        }



        /// <summary>
        /// Adds the values of all model-based variables (static, dynamic, iterators) to the scope of the script.
        /// </summary>
        /// <param name="dataModel">The model variables.</param>
        public void SetModelVariableValues(DataModel dataModel, bool addDynamicVariables)
        {
            ScopeManager.AddSequenceEnabledStateVariables(dataModel);
            ScopeManager.AddStaticVariablesToScope(dataModel.variablesModel);
            ScopeManager.AddIteratorVariablesToScope(dataModel.variablesModel);

            if(addDynamicVariables)
                ScopeManager.AddDynamicVariablesToScope(dataModel.variablesModel);
        }

        /// <summary>
        /// Adds the values of global variables to the scope.
        /// </summary>
        public void AddGlobalVariablesToScope()
        {
            ScopeManager.AddGlobalVariables();
        }
        /// <summary>
        /// Builds the resulting instance.
        /// </summary>
        /// <returns>The resulting instance.</returns>
        public override AbstractPythonExecutor Build()
        {
            HighPerformancePythonScriptExecutor result = new HighPerformancePythonScriptExecutor(pythonScript, outputVariableName, inputVariableNames, Engine, ScopeManager);
            return result;
        }
    }
}
