using System;
using Microsoft.Scripting.Hosting;

namespace PythonUtils.ScriptExecution
{
    public class AbstractPythonScriptExecutor : AbstractPythonExecutor
    {        
        /// <summary>
        /// The python script to execute.
        /// </summary>
        protected string script;

        /// <summary>
        /// The python script to execute.
        /// </summary>
        public virtual string Script
        {
            set
            {
                this.script = value;
            }

            get
            {
                return this.script;
            }
        }


        public AbstractPythonScriptExecutor(string script, ScriptEngine engine, PythonScopeManager scopeManager) : base(engine, scopeManager) 
        {
            if (!String.IsNullOrEmpty(script))
            {
                this.script = script;   
            }
        }

        /// <summary>
        /// Indicates whether the specified variableName is used within the script.
        /// </summary>
        /// <param name="variableName">The program-variable name</param>
        /// <returns><c>true</c> if the script uses the specified variable; otherwise, <c>false</c></returns>
        public bool IsVariableUsedInScript(string variableName)
        {
            return PythonScriptVariablesAnalyzer.IsVariableUsedInScript(variableName, Script);
        }
    }
}
