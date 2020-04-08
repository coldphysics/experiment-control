using PythonUtils.ScriptAnalysis;
using PythonUtils.ScriptExecution;

namespace Generator.Generator.Step
{


    /// <summary>
    /// Analyzes a Python script used in a step.
    /// </summary>
    /// <seealso cref="PythonUtils.ScriptAnalysis.AbstractScriptAnalyzerForHighPerformanceExecuter" />
    public class PythonStepScriptAnalyzer : AbstractScriptAnalyzerForHighPerformanceExecuter
    {
        /// <summary>
        /// The dummy duration used to validate the script
        /// </summary>
        private const double DUMMY_DURATION_MILLIS = 1000;
        /// <summary>
        /// The dummy absolute time used to validate the script
        /// </summary>
        private const double DUMMY_ABSOLUTE_TIME_MILLIS = 2000;
        /// <summary>
        /// The dummy time value used to validate the script
        /// </summary>
        private const double DUMMY_TIME_MILLIS = 500;

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonStepScriptAnalyzer"/> class.
        /// </summary>
        /// <param name="outputVariableName">Name of the output variable.</param>
        /// <param name="inputVariableNames">The input variable names.</param>
        public PythonStepScriptAnalyzer(string outputVariableName, string[] inputVariableNames)
            : base(outputVariableName, inputVariableNames)
        { }

        /// <summary>
        /// Executes the script
        /// </summary>
        /// <returns>
        /// The value of the output variable after executing the script
        /// </returns>
        protected override double Execute()
        {
            return ((HighPerformancePythonScriptExecutor) executer).Execute(DUMMY_TIME_MILLIS, DUMMY_DURATION_MILLIS, DUMMY_ABSOLUTE_TIME_MILLIS);
        }

        /// <summary>
        /// Gets the script error message.
        /// </summary>
        /// <returns>
        /// The script error message.
        /// </returns>
        protected override string GetScriptErrorMessage()
        {
            return "Error while evaluating the python step";
        }
    }
}
