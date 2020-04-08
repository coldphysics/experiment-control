using Model.Data;
using PythonUtils.ScriptExecution;
using System;

namespace PythonUtils.ScriptAnalysis
{


    /// <summary>
    /// Analyzes a python script looking for errors and creating the appropriate error message.
    /// </summary>
    public abstract class AbstractScriptAnalyzerForHighPerformanceExecuter:AbstractScriptAnalyzer
    {
        ///// <summary>
        ///// The dummy value for the routine array
        ///// </summary>
        //private List<double> DUMMY_VALUE_FOR_ROUTINE_ARRAY = new List<double>(
        //    new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }
        //    );
        /// <summary>
        /// The output variable name
        /// </summary>
        private string outputVariableName;
        /// <summary>
        /// The input variable names
        /// </summary>
        private string[] inputVariableNames;

        /// <summary>
        /// Gets the name of the output variable.
        /// </summary>
        /// <value>
        /// The name of the output variable.
        /// </value>
        public string OutputVariableName
        {
            get
            {
                return outputVariableName;
            }
        }

        /// <summary>
        /// Gets the input variable names.
        /// </summary>
        /// <value>
        /// The input variable names.
        /// </value>
        public string[] InputVariableNames
        {
            get
            {
                return inputVariableNames;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractScriptAnalyzerForHighPerformanceExecuter"/> class.
        /// </summary>
        /// <param name="outputVariableName">Name of the output variable.</param>
        /// <param name="inputVariableNames">The input variable names.</param>
        public AbstractScriptAnalyzerForHighPerformanceExecuter(string outputVariableName, string[] inputVariableNames)
        {
            this.inputVariableNames = inputVariableNames;
            this.outputVariableName = outputVariableName;
        }





        /// <summary>
        /// Creates the executer.
        /// </summary>
        /// <param name="dataModel">The data model.</param>
        /// <param name="script">The script.</param>
        private void CreateExecuter(DataModel dataModel, string script)
        {
            HighPerformancePythonScriptExecutorBuilder builder = new HighPerformancePythonScriptExecutorBuilder();
            builder.SetScript(script);
            builder.SetOutputVariableName(OutputVariableName);
            builder.SetInputVariableNames(InputVariableNames);
            builder.SetModelVariableValues(dataModel, true);
            builder.AddGlobalVariablesToScope();

            executer = builder.Build();
            //executer.SetVariableValue(GlobalVariableNames.ROUTINE_ARRAY, DUMMY_VALUE_FOR_ROUTINE_ARRAY);
        }

        /// <summary>
        /// Generates the error message.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <param name="e">The exception.</param>
        /// <returns>The error message</returns>
        public override string GenerateErrorMessage(IScriptLocation location, Exception e)
        {
            string errorMessage = e.Message;

            if (e is MissingMemberException)
                errorMessage = string.Format("The special python variable \"{0}\" must be assigned to in the script!", OutputVariableName);
            else if (e is FormatException)
                errorMessage = string.Format("The Value of the special variable \"{0}\" should be numeric.", OutputVariableName);
            else
                errorMessage = e.Message;

            if (location != null)
                errorMessage = AttachErrorLocationMessage(location, errorMessage);

            return errorMessage;
        }

        /// <summary>
        /// Executes the script
        /// </summary>
        /// <returns>The value of the output variable after executing the script</returns>
        protected abstract double Execute();

        /// <summary>
        /// Validates the python script.
        /// </summary>
        /// <param name="scriptLocation">The script location.</param>
        /// <param name="script">The script.</param>
        /// <param name="dataModel">The data model.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns><c>true</c> only if the script is free of errors.</returns>
        public bool ValidatePythonScript(IScriptLocation scriptLocation, string script, DataModel dataModel, out string errorMessage)
        {
            //Enhances performance by reusing the existing analysis result.
            if (analysis != null && analysis.Script.Equals(script))
            {
                errorMessage = analysis.ErrorMessage;
                return analysis.Result;
            }

            bool resultBool = false;
            errorMessage = "";

            try
            {
                if (executer == null)
                {
                    CreateExecuter(dataModel, script);
                    analysis = new Analysis();
                }
                else
                {
                    ((HighPerformancePythonScriptExecutor) executer).ResetVariables(dataModel);//In case new variables were declared.
                    ((HighPerformancePythonScriptExecutor)executer).Script = script;
                   
                }

                double result = Execute();
                errorMessage = "Script is fine!";

                resultBool = true;
            }
            catch (Exception e)
            {
                errorMessage = GenerateErrorMessage(scriptLocation, e);
                resultBool = false;
            }
            finally
            {
                analysis.ErrorMessage = errorMessage;
                analysis.Script = script;
                analysis.Result = resultBool;
            }

            return resultBool;
        }

    }
}
