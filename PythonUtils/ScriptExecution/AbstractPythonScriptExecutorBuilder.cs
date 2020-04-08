namespace PythonUtils.ScriptExecution
{
    public abstract class AbstractPythonScriptExecutorBuilder:AbstractPythonExecutorBuilder
    {
        /// <summary>
        /// The python script
        /// </summary>
        protected string pythonScript;


        /// <summary>
        /// Sets the script.
        /// </summary>
        /// <param name="pythonScript">The python script.</param>
        public void SetScript(string pythonScript)
        {
            this.pythonScript = pythonScript;
        }

    }
}
