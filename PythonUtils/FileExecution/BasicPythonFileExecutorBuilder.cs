namespace PythonUtils.FileExecution
{
    public class BasicPythonFileExecutorBuilder : AbstractPythonExecutorBuilder
    {
        public override AbstractPythonExecutor Build()
        {
            return new BasicPythonFileExecutor(Engine, ScopeManager);
        }
    }
}
