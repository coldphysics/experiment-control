using Communication.Interfaces.Generator;
using Generator.Generator.Channel;
using Generator.Generator.Concatenator;
using Generator.Generator.Step.Abstract;
using Model.Data;
using Model.Data.Steps;
using PythonUtils.ScriptAnalysis;
using PythonUtils.ScriptExecution;
using System;

namespace Generator.Generator.Step
{
    /// <summary>
    /// The exception that is thrown when an error is detected in the Python step
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class PythonStepException : Exception
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonStepException"/> class.
        /// </summary>
        public PythonStepException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonStepException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public PythonStepException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonStepException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner exception.</param>
        public PythonStepException(string message, Exception inner) : base(message, inner) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PythonStepException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        protected PythonStepException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }



    /// <summary>
    /// A generator that is capable of generating the output of a python step. 
    /// The script can access all the variables defined by the user and uses special variables to define the inputs and the output.
    /// </summary>
    /// <seealso cref="Generator.Generator.Step.Abstract.BasicStepOutputGenerator" />
    /// <seealso cref="Communication.Interfaces.Generator.IValueGenerator" />
    public class AnalogStepPythonOutputGenerator : BasicStepOutputGenerator, IValueGenerator
    {
        /// <summary>
        /// A reference to the data model.
        /// </summary>
        private DataModel dataModel;
        /// <summary>
        /// The analyzer that is used to detect the potential errors in the script.
        /// </summary>
        private static PythonStepScriptAnalyzer analyzer;
        /// <summary>
        /// The executer that is used to execute the script.
        /// </summary>
        private HighPerformancePythonScriptExecutor executer;

        /// <summary>
        /// The name of the output variable
        /// </summary>
        public const string OUTPUT_PYTHON_VARIABLE_NAME = "out";
        /// <summary>
        /// The name of the variable representing the current time.
        /// </summary>
        public const string TIME_PYTHON_VARIABLE_NAME = "t";
        /// <summary>
        /// The name of the variable representing the duration of the step.
        /// </summary>
        public const string DURATION_PYTHON_VARIABLE_NAME = "T";
        /// <summary>
        /// The name of the variable representing the absolute starting time of the step.
        /// </summary>
        public const string ABSOLUTE_TIME_PYTHON_VARIABLE_NAME = "t0";

        /// <summary>
        /// The model that describes the step
        /// </summary>
        public StepPythonModel Model
        {
            get
            {
                return (StepPythonModel)_model;
            }
        }

        /// <summary>
        /// Gets the analyzer.
        /// </summary>
        /// <value>
        /// The analyzer.
        /// </value>
        /// <remarks>Singleton pattern is used for the analyzer.</remarks>
        public static PythonStepScriptAnalyzer Analyzer
        {
            get
            {
                if (analyzer == null)
                    analyzer = new PythonStepScriptAnalyzer(OUTPUT_PYTHON_VARIABLE_NAME, new string[] { TIME_PYTHON_VARIABLE_NAME, ABSOLUTE_TIME_PYTHON_VARIABLE_NAME, DURATION_PYTHON_VARIABLE_NAME });

                return analyzer;
            }

        }


        /// <summary>
        /// Initializes a new instance of the <see cref="AnalogStepPythonOutputGenerator"/> class.
        /// </summary>
        /// <param name="model">The step model.</param>
        /// <param name="variables">The variables variables.</param>
        /// <param name="parent">The parent <see cref=" ChannelOutputGenerator"/>.</param>
        public AnalogStepPythonOutputGenerator(StepPythonModel model, DataModel dataModel, ChannelOutputGenerator parent)
            : base(model, parent)
        {
            this.dataModel = dataModel;
        }

        /// <summary>
        /// Creates the executer.
        /// </summary>
        /// <param name="dataModel">The variables model.</param>
        /// <param name="script">The script.</param>
        /// <returns>A Python executer for the specified script and variables</returns>
        private static HighPerformancePythonScriptExecutor CreateExecuter(DataModel dataModel, string script)
        {
            HighPerformancePythonScriptExecutorBuilder builder = new HighPerformancePythonScriptExecutorBuilder();
            builder.SetScript(script);
            builder.SetModelVariableValues(dataModel, true);
            builder.AddGlobalVariablesToScope();
            builder.SetInputVariableNames(new string[] { TIME_PYTHON_VARIABLE_NAME, DURATION_PYTHON_VARIABLE_NAME, ABSOLUTE_TIME_PYTHON_VARIABLE_NAME });
            builder.SetOutputVariableName(OUTPUT_PYTHON_VARIABLE_NAME);

            return (HighPerformancePythonScriptExecutor)builder.Build();
        }



        /// <summary>
        /// Checks whether a script is a valid Python script and ensures that the special output python variable is assigned a value.
        /// </summary>
        /// <param name="cardName">Index of the card.</param>
        /// <param name="channelIndex">Index of the channel.</param>
        /// <param name="stepIndex">Index of the step.</param>
        /// <param name="script">The script.</param>
        /// <param name="dataModel">The variables model.</param>
        /// <param name="errorMessage">The error message returned if the script is invalid.</param>
        /// <param name="includeChannelInfoInErrorMsg">if set to <c>true</c> [include channel information in error MSG].</param>
        /// <returns>
        ///   <c>true</c> if the script is valid, <c>false</c> otherwise.
        /// </returns>
        public static bool ValidatePythonScript(string cardName, int channelIndex, int stepIndex, string script, DataModel dataModel, out string errorMessage, bool includeChannelInfoInErrorMsg)
        {
            StepBasedScriptLocation location = null;

            if (includeChannelInfoInErrorMsg)
                location = new StepBasedScriptLocation() { CardName = cardName, ChannelIndex = channelIndex, StepIndex = stepIndex };

            return Analyzer.ValidatePythonScript(location, script, dataModel, out errorMessage);
        }


        #region IValueGenerator Members

        /// <summary>
        /// Generates the raw output and adds it to the specified concatenator.
        /// </summary>
        /// <param name="concatenator">The concatenator.</param>
        /// <exception cref="Generator.Generator.Step.PythonStepException">When the script has an error.</exception>
        public void Generate(IConcatenator concatenator)
        {
            executer = CreateExecuter(dataModel, Model.Script);
            int numberOfSteps = GetNumberOfTimeSteps();
            double[] result = new double[numberOfSteps];
            double t;
            double t0 = Model.StartTime;
            double T = Model.Duration.Value;

            try
            {
                for (int stepIndex = 0; stepIndex < result.Length; stepIndex++)
                {
                    t = (T / numberOfSteps) * stepIndex;//determine the time of the current time-step (in millis)
                    result[stepIndex] = executer.Execute(t, T, t0);//execute the script with the specified input
                }

                ((OutputConcatenator)concatenator).AddSteps(result);
            }
            catch (Exception e)
            {
                StepBasedScriptLocation location = new StepBasedScriptLocation() { CardName = Model.Card().Name, ChannelIndex = Model.Channel().Index(), StepIndex = Model.Index() };
                string message = Analyzer.GenerateErrorMessage(location, e);
                throw new PythonStepException(message, e);
            }
        }

        #endregion

    }
}
