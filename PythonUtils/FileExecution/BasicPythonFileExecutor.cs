using Microsoft.Scripting.Hosting;

namespace PythonUtils.FileExecution
{
    public class BasicPythonFileExecutor : AbstractPythonExecutor
    {
        public BasicPythonFileExecutor(ScriptEngine engine, PythonScopeManager scopeManager) : base(engine, scopeManager)
        {
        }

        public void Execute(string filePath)
        {
            this.engine.ExecuteFile(filePath, Scope);
        }
    }
}
