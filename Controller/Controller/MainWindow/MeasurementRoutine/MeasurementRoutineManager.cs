using System;
using System.Collections.Generic;
using Model.MeasurementRoutine;
using Model.MeasurementRoutine.GlobalVariables;
using PythonUtils.ScriptAnalysis;
using PythonUtils.ScriptExecution;

namespace Controller.MainWindow.MeasurementRoutine
{
    [Serializable]
    public class MeasurementRoutineException : Exception
    {
        public MeasurementRoutineException() { }
        public MeasurementRoutineException(string message) : base(message) { }
        public MeasurementRoutineException(string message, Exception inner) : base(message, inner) { }
        protected MeasurementRoutineException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    public class MeasurementRoutineManager
    {
        public const string VAR_PRIMARY_MODEL = "primaryModel";
        public const string VAR_SECONDARY_MODELS = "secondaryModels";
        public const string VAR_CURRENT_MODE = "currentMode";
        public const string VAR_PREVIOUS_MODE = "previousMode";
        public const string VAR_START_ROUTINE = "startRoutine";
        public const string VAR_GLOBAL_COUNTER = "globalCounter";
        public const string VAR_NUMBER_OF_ITERATIONS = "numberOfIterations";
        public const string VAR_COMPLETED_SCANS = "completedScans";
        public const string VAR_START_COUNTER_OF_SCANS = "startCounterOfScans";
        public const string VAR_SCAN_ONLY_ONCE = "scanOnlyOnce";
        public const string VAR_CONTROL_LE_CROY = "controlLeCroy";
        public const string VAR_STOP_AFTER_SCAN = "stopAfterScan";
        public const string VAR_SHUFFLE_ITERATIONS = "shuffleIterations";

        //The name of next iteration should be last iteration, this follows from the execution order of the measurement routine.
        public const string VAR_NEXT_ITERATION = "lastIteration";
        //public const string VAR_NEXT_ITERATION = "nextIteration";

        private static MeasurementRoutineScriptAnalyzer analyzer;
        
        private NormalPythonExecutor executer;
    
        private bool isFirstCycle = true;
        private int lastMode = 0;
        private int currentMode = 0;

        public static MeasurementRoutineScriptAnalyzer Analyzer
        {
            get
            {
                if (analyzer == null)
                    analyzer = new MeasurementRoutineScriptAnalyzer(GetSpecialVariableNames());

                return analyzer;
            }
        }


        public static bool ValidatePythonScript(string script, out string errorMessage, bool includeChannelInfoInErrorMsg)
        {
            BasicScriptLocation location = null;

            if (includeChannelInfoInErrorMsg)
                location = new BasicScriptLocation("Measurement Routine");

            return Analyzer.ValidatePythonScript(location, script, out errorMessage);
        }

        private static string[] GetSpecialVariableNames()
        {
            string[] result = 
            {
                VAR_COMPLETED_SCANS,
                VAR_CONTROL_LE_CROY,
                VAR_CURRENT_MODE,
                VAR_GLOBAL_COUNTER,
                VAR_NEXT_ITERATION,
                VAR_NUMBER_OF_ITERATIONS,
                VAR_PREVIOUS_MODE,
                VAR_PRIMARY_MODEL,
                VAR_SCAN_ONLY_ONCE,
                VAR_SECONDARY_MODELS,
                VAR_SHUFFLE_ITERATIONS,
                VAR_START_COUNTER_OF_SCANS,
                VAR_START_ROUTINE,
                VAR_STOP_AFTER_SCAN,
                GlobalVariableNames.ROUTINE_ARRAY
                              
            };

            return result;
        }

        private void ReadGlobalVariables()
        {
            List<double> globalArray = executer.GetListVariableValue(GlobalVariableNames.ROUTINE_ARRAY);
            GlobalVariablesManager.GetInstance().SetVariableValueByName(GlobalVariableNames.ROUTINE_ARRAY, globalArray);
        }

        public void RunInitializationScript(MeasurementRoutineModel model)
        {
            GlobalVariablesManager.GetInstance().ResetGlobalVariables();
            string script;

            if (!String.IsNullOrEmpty(model.RoutineInitializationScript))
                script = model.RoutineInitializationScript;
            else
                script = "pass";

            NormalPythonExecutorBuilder builder = new NormalPythonExecutorBuilder();
            builder.SetScript(script);
            builder.SetVariableValue(GlobalVariableNames.ROUTINE_ARRAY, new List<double>());
            this.executer = (NormalPythonExecutor)builder.Build();

            try
            {
                executer.Execute();
                ReadGlobalVariables();
            }
            catch (Exception e)
            {
                BasicScriptLocation location = new BasicScriptLocation("Measurement Routine");
                string message = Analyzer.GenerateErrorMessage(location, e);
                throw new MeasurementRoutineException(message, e);
            }


        }

        private void PrepareExecuter(MeasurementRoutineModel model, MainWindowController controller)
        {
            executer.Script = model.RoutineControlScript;
            executer.SetVariableValue(VAR_CURRENT_MODE, currentMode);
            executer.SetVariableValue(VAR_COMPLETED_SCANS, controller.CompletedScans);
            executer.SetVariableValue(VAR_CONTROL_LE_CROY, controller.ControlLecroy);
            executer.SetVariableValue(VAR_GLOBAL_COUNTER, controller.GlobalCounter);
            executer.SetVariableValue(VAR_NEXT_ITERATION, controller.IterationOfScan);
            executer.SetVariableValue(VAR_NUMBER_OF_ITERATIONS, controller.NumberOfIterations);
            executer.SetVariableValue(VAR_PREVIOUS_MODE, lastMode);
            executer.SetVariableValue(VAR_PRIMARY_MODEL, model.PrimaryModel.FilePath);
            executer.SetVariableValue(VAR_SCAN_ONLY_ONCE, controller.IsOnceChecked);

            List<string> secondaryModelsFilePaths = new List<string>();
            foreach (var item in model.SecondaryModels)
            {
                secondaryModelsFilePaths.Add(item.FilePath);
            }

            executer.SetVariableValue(VAR_SECONDARY_MODELS, secondaryModelsFilePaths.ToArray());
            executer.SetVariableValue(VAR_SHUFFLE_ITERATIONS, controller.ShuffleIterations);

           // executer.SetVariableValue(VAR_START_COUNTER_OF_SCANS, controller.StartCounterOfScans);
            executer.SetVariableValue(VAR_START_COUNTER_OF_SCANS, controller.StartCounterOfScansOfCurrentModel);

            executer.SetVariableValue(VAR_START_ROUTINE, isFirstCycle);
            executer.SetVariableValue(VAR_STOP_AFTER_SCAN, controller.StopAfterScan);
        }

        public bool RequiresInitialization()
        {
            return executer == null;
        }

        /// <summary>
        /// Gets the index of the next model to run.
        /// </summary>
        /// <param name="model">The meaurement routine model that is running.</param>
        /// <param name="controller">The controller.</param>
        /// <returns></returns>
        /// <exception cref="Controller.MainWindow.MeasurementRoutine.MeasurementRoutineException"></exception>
        public int GetNextModelIndex(MeasurementRoutineModel model, MainWindowController controller)
        {
            if (RequiresInitialization())
                RunInitializationScript(model);

            PrepareExecuter(model, controller);
            isFirstCycle = false;

            try
            {
                lastMode = currentMode;
                executer.Execute();
                currentMode = executer.GetIntegerVariableValue(VAR_CURRENT_MODE);
                ReadGlobalVariables();

                return currentMode;
            }
            catch (Exception e)
            {
                BasicScriptLocation location = new BasicScriptLocation("Measurement Routine");
                string message = Analyzer.GenerateErrorMessage(location, e);
                throw new MeasurementRoutineException(message, e);
            }


        }


        /// <summary>
        /// Resets all fields related to the measurement routine model.
        /// </summary>
        public void Reset()
        {
            isFirstCycle = true;
            currentMode = 0;
            lastMode = 0;
            executer = null;
        }
    }
}
