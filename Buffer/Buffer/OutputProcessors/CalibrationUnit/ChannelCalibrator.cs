using Model.Data;
using PythonUtils.ScriptExecution;

namespace Buffer.OutputProcessors.CalibrationUnit
{
    /// <summary>
    /// Manages the calibration process of a single channel.
    /// </summary>
    public class ChannelCalibrator
    {
        /// <summary>
        /// The special variable name that indicates the input to the calibration script (the uncalibrated value).
        /// </summary>
        public const string PYTHON_VARIABLE_NAME_FOR_UNCALIBRATED_OUPTUT = "uncal";
        /// <summary>
        /// The special variable name that indicates the output of the calibration script (the calibrated value).
        /// </summary>
        public const string PYTHON_VARIABLE_NAME_FOR_CALIBRATED_OUPTUT = "cal";

        /// <summary>
        /// The python executer.
        /// </summary>
        private HighPerformancePythonScriptExecutor executer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelCalibrator"/> class.
        /// </summary>
        /// <param name="pythonScript">The python script.</param>
        /// <param name="variablesModel">The variables model.</param>
        public ChannelCalibrator(string pythonScript, DataModel dataModel)
        {
            HighPerformancePythonScriptExecutorBuilder builder = new HighPerformancePythonScriptExecutorBuilder();
            builder.SetScript(pythonScript);
            builder.SetModelVariableValues(dataModel, true);
            builder.AddGlobalVariablesToScope();
            builder.SetInputVariableNames(new string[] { PYTHON_VARIABLE_NAME_FOR_UNCALIBRATED_OUPTUT });
            builder.SetOutputVariableName(PYTHON_VARIABLE_NAME_FOR_CALIBRATED_OUPTUT);

            this.executer = (HighPerformancePythonScriptExecutor) builder.Build();
        }


        /// <summary>
        /// Calibrates the value.
        /// </summary>
        /// <param name="uncalibratedValue">The uncalibrated value.</param>
        /// <returns>The calibrated value. </returns>
        public double CalibrateValue(double uncalibratedValue)
        {
            double result = uncalibratedValue;

            if (executer.Script != null && executer.Script.Length > 0)
            {
                result = executer.Execute(uncalibratedValue);
            }

            return result;
        }

    }
}
