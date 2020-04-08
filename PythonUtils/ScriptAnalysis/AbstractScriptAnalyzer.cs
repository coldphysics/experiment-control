using System;

namespace PythonUtils.ScriptAnalysis
{
    /// <summary>
    /// The result of the analysis
    /// </summary>
    public class Analysis
    {
        /// <summary>
        /// Gets or sets the script.
        /// </summary>
        /// <value>
        /// The script.
        /// </value>
        public string Script { set; get; }
        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="Analysis"/> has found no errors.
        /// </summary>
        /// <value>
        ///   <c>true</c> if no errors were found otherwise, <c>false</c>.
        /// </value>
        public bool Result { set; get; }
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage { set; get; }
    }

    public abstract class AbstractScriptAnalyzer
    {
        /// <summary>
        /// The executer that executes the string
        /// </summary>
        protected AbstractPythonExecutor executer;
        /// <summary>
        /// The result of the last performed analysis (kept for possible re-use which enhances performance)
        /// </summary>
        protected Analysis analysis;


        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractScriptAnalyzer" /> class.
        /// </summary>
        public AbstractScriptAnalyzer()
        {
        }



        public abstract string GenerateErrorMessage(IScriptLocation location, Exception e);

        /// <summary>
        /// Gets the script error message.
        /// </summary>
        /// <returns>The script error message.</returns>
        protected abstract string GetScriptErrorMessage();


        
        /// <summary>
        /// Attaches the error location to the error message.
        /// </summary>
        /// <param name="locationInfo">The location information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The error message including the location info</returns>
        protected string AttachErrorLocationMessage(IScriptLocation locationInfo, string errorMessage)
        {
            return String.Format("{0} at ({1}).\nDetails: {2}", GetScriptErrorMessage(), locationInfo.GetLocationAsString(), errorMessage);
        }

    }
}
