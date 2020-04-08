using PythonUtils.ScriptAnalysis;
using PythonUtils.ScriptExecution;

namespace Buffer.OutputProcessors.CalibrationUnit
{
    /// <summary>
    /// Analyzes a python script used for calibrating a channel.
    /// </summary>
    /// <seealso cref="Generator.Python.ErrorAnalysis.AbstractScriptAnalyzer" />
    public class CalibrationScriptAnalyzer:AbstractScriptAnalyzerForHighPerformanceExecuter
    {
        /// <summary>
        /// The dummy value used as input for the calibration script.
        /// </summary>
        private const double DUMMY_INPUT_VALUE = 5.0;
        /// <summary>
        /// Initializes a new instance of the <see cref="CalibrationScriptAnalyzer"/> class.
        /// </summary>
        /// <param name="outputVariableName">Name of the output variable.</param>
        /// <param name="inputVariableNames">The input variable names.</param>
        public CalibrationScriptAnalyzer(string outputVariableName, string[] inputVariableNames)
            : base(outputVariableName, inputVariableNames)
        { }

        /// <summary>
        /// Executes the script using the dummy value
        /// </summary>
        /// <returns>
        /// The value of the output variable after executing the script
        /// </returns>
        protected override double Execute()
        {
            return (executer as HighPerformancePythonScriptExecutor).Execute(DUMMY_INPUT_VALUE);
        }

        /// <summary>
        /// Gets the script error message.
        /// </summary>
        /// <returns>
        /// The script error message.
        /// </returns>
        protected override string GetScriptErrorMessage()
        {
            return "Error while evaluating the Python calibration script";
        }
    }
}
