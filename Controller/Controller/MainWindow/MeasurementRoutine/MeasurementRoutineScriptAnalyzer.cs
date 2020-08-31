using System;
using System.Collections.Generic;
using Model.MeasurementRoutine.GlobalVariables;
using PythonUtils.ScriptAnalysis;
using PythonUtils.ScriptExecution;

namespace Controller.MainWindow.MeasurementRoutine
{
    public class MeasurementRoutineScriptAnalyzer:AbstractScriptAnalyzer
    {
        public const string DUMMY_VAL_PRIMARY_MODEL = "test";
        public static readonly string[] DUMMY_VAL_SECONDARY_MODELS = new string[]{ "test1", "test2" };
        public const int DUMMY_VAL_CURRENT_MODE = 0;
        public const int DUMMY_VAL_PREVIOUS_MODE = 0;
        public const bool DUMMY_VAL_START_ROUTINE = false;
        public const int DUMMY_VAL_GLOBAL_COUNTER = 1000;
        public const int DUMMY_VAL_NUMBER_OF_ITERATIONS = 5;
        public const int DUMMY_VAL_COMPLETED_SCANS = 1;
        public const int DUMMY_VAL_START_COUNTER_OF_SCANS = 13;
        public const bool DUMMY_VAL_SCAN_ONLY_ONCE = false;
        public const bool DUMMY_VAL_CONTROL_LE_CROY = false;
        public const bool DUMMY_VAL_STOP_AFTER_SCAN = false;
        public const bool DUMMY_VAL_SHUFFLE_ITERATIONS = false;
        public const int DUMMY_VAL_NEXT_ITERATION = 2;
        public readonly List<double> DUMMY_VAL_ROUTINE_ARRAY = new List<double>(new double[]{4});

        protected readonly string[] EXTERNALLY_DEFINED_VARIABLES;


        public MeasurementRoutineScriptAnalyzer(string[] externallyDefinedVariables)
        {
            this.EXTERNALLY_DEFINED_VARIABLES = externallyDefinedVariables;
        }

        private void CreateExecuter(string script)
        {
            NormalPythonExecutorBuilder builder = new NormalPythonExecutorBuilder();
            builder.SetScript(script);

            builder.SetVariableValue(MeasurementRoutineManager.VAR_COMPLETED_SCANS, DUMMY_VAL_COMPLETED_SCANS);
            builder.SetVariableValue(MeasurementRoutineManager.VAR_CONTROL_LE_CROY, DUMMY_VAL_CONTROL_LE_CROY);
            builder.SetVariableValue(MeasurementRoutineManager.VAR_CURRENT_MODE, DUMMY_VAL_CURRENT_MODE);
            builder.SetVariableValue(MeasurementRoutineManager.VAR_GLOBAL_COUNTER, DUMMY_VAL_GLOBAL_COUNTER);
            builder.SetVariableValue(MeasurementRoutineManager.VAR_LAST_ITERATION, DUMMY_VAL_NEXT_ITERATION);
            builder.SetVariableValue(MeasurementRoutineManager.VAR_NUMBER_OF_ITERATIONS, DUMMY_VAL_NUMBER_OF_ITERATIONS);
            builder.SetVariableValue(MeasurementRoutineManager.VAR_PREVIOUS_MODE, DUMMY_VAL_PREVIOUS_MODE);
            builder.SetVariableValue(MeasurementRoutineManager.VAR_PRIMARY_MODEL, DUMMY_VAL_PRIMARY_MODEL);
            builder.SetVariableValue(MeasurementRoutineManager.VAR_SCAN_ONLY_ONCE, DUMMY_VAL_SCAN_ONLY_ONCE);
            builder.SetVariableValue(MeasurementRoutineManager.VAR_SECONDARY_MODELS, DUMMY_VAL_SECONDARY_MODELS);
            builder.SetVariableValue(MeasurementRoutineManager.VAR_SHUFFLE_ITERATIONS, DUMMY_VAL_SHUFFLE_ITERATIONS);
            builder.SetVariableValue(MeasurementRoutineManager.VAR_START_COUNTER_OF_SCANS, DUMMY_VAL_START_COUNTER_OF_SCANS);
            builder.SetVariableValue(MeasurementRoutineManager.VAR_START_ROUTINE, DUMMY_VAL_START_ROUTINE);
            builder.SetVariableValue(MeasurementRoutineManager.VAR_STOP_AFTER_SCAN, DUMMY_VAL_STOP_AFTER_SCAN);
            builder.SetVariableValue(GlobalVariableNames.ROUTINE_ARRAY, DUMMY_VAL_ROUTINE_ARRAY);
            executer = builder.Build();
        }

        protected void InitializeExecuter(string script)
        {
            if (executer == null)
            {
                CreateExecuter(script);
                analysis = new Analysis();
            }
            else
            {
                ((NormalPythonExecutor)executer).Script = script;
            }
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
                errorMessage = "You are trying to access a non-existent member.";
            else if (e is FormatException)
                errorMessage = "The Value of the special python variable should be numeric.";
            else
                errorMessage = e.Message;

            if (location != null)
                errorMessage = AttachErrorLocationMessage(location, errorMessage);

            return errorMessage;
        }



        public bool ValidatePythonScript(IScriptLocation scriptLocation, string script, out string errorMessage)
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
                InitializeExecuter(script);

                Execute();
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

        protected void Execute()
        {
            ((NormalPythonExecutor)executer).Execute();
        }

        protected override string GetScriptErrorMessage()
        {
            return "An error occurred in the python script of the measurement routine.";
        }
    }
}
