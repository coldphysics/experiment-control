using Microsoft.Scripting.Hosting;
using Model.Data;
using System;
using System.Text;

namespace PythonUtils.ScriptExecution
{
    /// <summary>
    /// Executes a python script that has a single output variable and up to 3 input variables
    /// </summary>
    /// <remarks>Use the builder <see cref=" HighPerformancePythonScriptExecutorBuilder"/> to get an instance of this class. Do not instantiate directly via the <c>new</c> keyword.</remarks>
    public class HighPerformancePythonScriptExecutor:AbstractPythonScriptExecutor
    {
        /// <summary>
        /// The name of the return variable
        /// </summary>
        private readonly string OUTPUT_VARIABLE_NAME;

        /// <summary>
        /// The names of input variables
        /// </summary>
        /// <remarks>The list of variables will be provided in this order to the Execute function.</remarks>
        private readonly string[] INPUT_VARIABLE_NAMES;
        /// <summary>
        /// The import statements that will be automatically added to the script.
        /// </summary>
        private string[] importStatements = { "from math import *" };
        /// <summary>
        /// The Python class name that will wrap the script
        /// </summary>
        private const string CLASS_NAME = "WrapperClass";
        /// <summary>
        /// The Python function name that will wrap the script
        /// </summary>
        private const string FUNCTION_NAME = "func";
        /// <summary>
        /// One level of indentation
        /// </summary>
        private const string INDENTION = "   ";

        /// <summary>
        /// The object of the python class that wraps the script
        /// </summary>
        private dynamic pythonObject;



        public override string Script
        {
            set
            {
                if (string.IsNullOrEmpty(this.script) || pythonObject == null || !this.script.Equals(value))//empty or changed
                {
                    this.script = value;
                    engine.Execute(PrepareScript(), Scope);
                    pythonObject = Scope.WrapperClass();
                }
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="HighPerformancePythonScriptExecutor"/> class.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <param name="outputVariableName">Name of the output variable.</param>
        /// <param name="inputVariableNames">The input variable names.</param>
        /// <param name="engine">The engine.</param>
        /// <param name="scope">The scope.</param>
        internal HighPerformancePythonScriptExecutor(string script, string outputVariableName, string[] inputVariableNames,
            ScriptEngine engine, PythonScopeManager scopeManager)
            :base(script, engine, scopeManager)
        {
            if (!String.IsNullOrEmpty(script))
            {
                this.INPUT_VARIABLE_NAMES = inputVariableNames;
                this.OUTPUT_VARIABLE_NAME = outputVariableName;
                Script = script;
            }
        }


        private string CreateFunctionSignature(string functionName, string[] parameters)
        {
            StringBuilder functionParametersBuilder = new StringBuilder();
            functionParametersBuilder.Append("self");

            if (parameters != null && parameters.Length > 0)
            {
                functionParametersBuilder.Append(", ");

                for (int i = 0; i < parameters.Length; i++)
                {
                    functionParametersBuilder.Append(parameters[i] + " = 0");

                    if (i < parameters.Length - 1)
                        functionParametersBuilder.Append(", ");
                }
            }

            string functionSignature = string.Format("def {0}({1}):", functionName, functionParametersBuilder.ToString());

            return functionSignature;
        }
        /// <summary>
        /// Prepares the script by wrapping it within a function and a class
        /// </summary>
        /// <returns></returns>
        private string PrepareScript()
        {
            StringBuilder builder = new StringBuilder();

            //Adding imports
            foreach (string import in importStatements)
            {
                builder.AppendLine(import);
            }

            //Adding class definition
            builder.AppendFormat("class {0}:\n", CLASS_NAME);

            //Adding function definition
            string functionSignature = CreateFunctionSignature(FUNCTION_NAME, INPUT_VARIABLE_NAMES);

            builder.AppendFormat("{0}{1}\n", INDENTION, functionSignature);

            script = script.Replace("\r", "");
            //Adding script
            string[] lines = script.Split('\n');

            foreach (string line in lines)
            {
                builder.AppendFormat("{0}{0}{1}\n", INDENTION, line);
            }

            //Adding return
            builder.AppendFormat("{0}{0}return {1}", INDENTION, OUTPUT_VARIABLE_NAME);

            return builder.ToString();
        }

        /// <summary>
        /// (Re-)adds the variables defined in the variables model to the scope of the script.
        /// </summary>
        /// <param name="dataModel"></param>
        public void ResetVariables(DataModel dataModel)
        {
            //scopeManager.ClearScope();
            scopeManager.AddGlobalVariables();
            scopeManager.AddSequenceEnabledStateVariables(dataModel);
            scopeManager.AddStaticVariablesToScope(dataModel.variablesModel);
            scopeManager.AddIteratorVariablesToScope(dataModel.variablesModel);
            scopeManager.AddDynamicVariablesToScope(dataModel.variablesModel);
        }


        /// <summary>
        /// Executes the script with 0 inputs
        /// </summary>
        /// <returns>The value of output variable</returns>
        public double Execute()
        {
            object result = null;

            if (engine != null)
            {
                result = pythonObject.func();
            }

            return ConvertToDouble(result);
        }

        /// <summary>
        /// Executes the script with 1 input
        /// </summary>
        /// <typeparam name="I1">The type of the input.</typeparam>
        /// <param name="input1">The input.</param>
        /// <returns>The value of output variable</returns>
        public double Execute<I1>(I1 input1)
        {
            object result = null;

            if (engine != null)
            {
                result = pythonObject.func(input1);
            }

            return ConvertToDouble(result);
        }

        /// <summary>
        /// Executes the script with 2 inputs
        /// </summary>
        /// <typeparam name="I1">The type of the first input.</typeparam>
        /// <typeparam name="I2">The type of the second input.</typeparam>
        /// <param name="input1">The 1st input.</param>
        /// <param name="input2">The 2nd input.</param>
        /// <returns>The value of output variable</returns>
        public double Execute<I1, I2>(I1 input1, I2 input2)
        {
            object result = null;

            if (engine != null)
            {
                result = pythonObject.func(input1, input2);
            }

            return ConvertToDouble(result);
        }

        /// <summary>
        /// Executes the script with 3 inputs
        /// </summary>
        /// <typeparam name="I1">The type of the first input.</typeparam>
        /// <typeparam name="I2">The type of the second input.</typeparam>
        /// <typeparam name="I3">The type of the third input.</typeparam>
        /// <param name="input1">The 1st input.</param>
        /// <param name="input2">The 2nd input.</param>
        /// <param name="input3">The 3rd input.</param>
        /// <returns>The value of output variable</returns>
        public double Execute<I1, I2, I3>(I1 input1, I2 input2, I3 input3)
        {
            object result = null;

            if (engine != null)
            {
                result = pythonObject.func(input1, input2, input3);
            }

            return ConvertToDouble(result);
        }




    }
}
