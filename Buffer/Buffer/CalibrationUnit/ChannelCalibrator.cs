using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Generator;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Model.Variables;

namespace Buffer.CalibrationUnit
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
        private PythonExecuter executer;



        /// <summary>
        /// The scope of python variables.
        /// </summary>
        private PythonScopeManager scope;

        /// <summary>
        /// Gets the scope manager.
        /// </summary>
        /// <value>
        /// The scope manager.
        /// </value>
        public PythonScopeManager ScopeManager
        {
            get { return scope; }
        }

        /// <summary>
        /// Gets the executer.
        /// </summary>
        /// <value>
        /// The executer.
        /// </value>
        public PythonExecuter Executer
        {
            get { return executer; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelCalibrator"/> class.
        /// </summary>
        /// <param name="pythonScript">The python script.</param>
        /// <param name="variablesModel">The variables model.</param>
        public ChannelCalibrator(string pythonScript, VariablesModel variablesModel)
        {
            ScriptEngine engine = Python.CreateEngine();
            ScriptScope pScope = engine.CreateScope();
            scope = new PythonScopeManager(pScope);
            this.executer = new PythonExecuter(pythonScript, engine, pScope);
            ReadVariables(variablesModel);
        }



        /// <summary>
        /// Reads the variables defined by the user and adds them to the Python scope.
        /// </summary>
        /// <param name="variablesModel">The variables model.</param>
        private void ReadVariables(VariablesModel variablesModel)
        {
            scope.ClearScope();
            scope.AddStaticVariablesToScope(variablesModel);
            scope.AddIteratorVariablesToScope(variablesModel);
            scope.AddDynamicVariablesToScope(variablesModel);
        }

        /// <summary>
        /// Calibrates the value.
        /// </summary>
        /// <param name="uncalibratedValue">The uncalibrated value.</param>
        /// <returns>The calibrated value. </returns>
        /// <remarks>The python variables defined within the script will remain in the scope of this channel until the end of the sequence. This means that the values assigned to these variables at time-step i will be available at time steps </remarks>
        public double CalibrateValue(double uncalibratedValue)
        {
            double result = uncalibratedValue;

            if (executer.PythonScript != null && executer.PythonScript.Length > 0)
            {
                scope.SetPyhtonVariableValue(PYTHON_VARIABLE_NAME_FOR_UNCALIBRATED_OUPTUT, uncalibratedValue);
                executer.Execute();
                result = scope.GetPythonVariableValueAsDouble(PYTHON_VARIABLE_NAME_FOR_CALIBRATED_OUPTUT);
            }

            return result;
        }

    }
}
