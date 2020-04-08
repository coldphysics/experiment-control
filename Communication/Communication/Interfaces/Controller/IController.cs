namespace Communication.Interfaces.Controller
{
    /// <summary>
    /// Interface for controller classes (ViewModels) that can issue a copy to buffer command
    /// </summary>
    public interface IController
    {
        /// <summary>
        /// Requests the buffer to accept the current model which will be read to generate the output of future cycles.
        /// </summary>
        void CopyToBuffer();
    }
}
