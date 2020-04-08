namespace PythonUtils.ScriptAnalysis
{
    public class BasicScriptLocation:IScriptLocation
    {
        private readonly string LOCATION;

        public BasicScriptLocation(string location)
        {
            this.LOCATION = location;
        }
        #region IScriptLocation Members

        public string GetLocationAsString()
        {
            return LOCATION;
        }

        #endregion
    }
}
