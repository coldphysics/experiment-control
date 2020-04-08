using Microsoft.Scripting.Hosting;
using System.Collections.Generic;

namespace PythonUtils.ScriptExecution
{
    public class NormalPythonExecutor : AbstractPythonExecutor
    {
        private ScriptSource source;

        private string script;

        public string Script
        {
            set
            {
                if (string.IsNullOrEmpty(this.script) || this.source == null || !this.script.Equals(value))//empty or changed
                {
                    this.script = value;
                    this.script = this.script.Replace("\r", "");
                    //RECO the scope should be reset too!
                    this.source = engine.CreateScriptSourceFromString(this.script, Microsoft.Scripting.SourceCodeKind.AutoDetect);
                }
            }
        }

        public NormalPythonExecutor(string script, ScriptEngine engine, PythonScopeManager scopeManager)
            : base(engine, scopeManager)
        {
            Script = script;//This triggers source creation
        }

        public void Execute()
        {

            if (source != null)
            {
                source.Execute(Scope);
            }

        }

        public int GetIntegerVariableValue(string variableName)
        {
            return scopeManager.GetPythonVariableValueAsInteger(variableName);
        }

        public List<double> GetListVariableValue(string variableName)
        {
            return scopeManager.GetPythonVariableAsCollectionOfDoubles(variableName);
        }
    }
}
