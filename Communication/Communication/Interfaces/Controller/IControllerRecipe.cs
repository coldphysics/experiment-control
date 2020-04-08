using Communication.Interfaces.Model;

namespace Communication.Interfaces.Controller
{
    /// <summary>
    /// Interface for the factory class that creates an instance of the RootConotroller
    /// </summary>
    public interface IControllerRecipe
    {
        /// <summary>
        /// Cooks the specified model.
        /// </summary>
        /// <param name="model">The model that the controller tree will be based upon</param>
        /// <param name="variablesController">The variables controller.</param>
        /// <returns>An instance of the RootController</returns>
        IController Cook(IModel model, IController variablesController);
    }
}
