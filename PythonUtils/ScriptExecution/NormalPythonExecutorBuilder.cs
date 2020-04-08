namespace PythonUtils.ScriptExecution
{
    public class NormalPythonExecutorBuilder:AbstractPythonScriptExecutorBuilder
    {

        public override AbstractPythonExecutor Build()
        {
            return new NormalPythonExecutor(pythonScript, Engine, ScopeManager);
        }
    }
}
