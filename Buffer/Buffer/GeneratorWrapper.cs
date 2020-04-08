using Generator.Generator.Step;
using Model.Data;

namespace Buffer
{
    /// <summary>
    /// Provides access to static methods from the <see cref=" Generator"/> project.
    /// </summary>
    /// <remarks>
    /// This Facade class is intended to be used by members of external projects 
    /// in order to give access to the "child" project (Generator) instead of directly 
    /// referencing the Generator project!
    /// </remarks>
    public class GeneratorWrapper
    {
        public static bool ValidateScriptOfPythonStep(string cardName, int channelIndex, int stepIndex, string script, DataModel dataModel, out string errorMessage, bool includeChannelInfoInErrorMsg)
        {
            return AnalogStepPythonOutputGenerator.ValidatePythonScript(cardName, channelIndex, stepIndex, script, dataModel, out errorMessage, includeChannelInfoInErrorMsg);
        }
    }
}
