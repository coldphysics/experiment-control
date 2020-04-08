using Communication.Interfaces.Buffer;
using Communication.Interfaces.Controller;
using Communication.Interfaces.Model;
using Controller.Data;
using Controller.Data.Cookbook;
using Controller.Root;
using Controller.Variables;
using Model.Root;

namespace Controller.Cookbook
{
    public class ControllerRecipe : IControllerRecipe
    {
        private readonly WindowGroupControllerRecipe _windowGroupControllerWrapper;
        private readonly IBuffer _buffer;

        public ControllerRecipe(WindowGroupControllerRecipe windowGroupControllerWrapper, IBuffer buffer)
        {
            _windowGroupControllerWrapper = windowGroupControllerWrapper;
            _buffer = buffer;
        }

        #region IWrapController Members

        /// <summary>
        /// Cooks the specified model. This method overrides cook method in <see cref=" IControllerRecipe"/> Interface.
        /// </summary>
        /// <param name="model">The model that the controller tree will be based upon</param>
        /// <param name="variablesController">The variables controller.</param>
        /// <returns>
        /// An instance of the RootController
        /// </returns>
        public IController Cook(IModel model, IController variablesController)
        {
            var realModel = (RootModel)model;

            var modelView = new RootController(realModel, _buffer);
            modelView.Variables = (VariablesController)variablesController;
            ((VariablesController)variablesController).SetParentController(modelView);

            var dataController = new DataController(realModel.Data, modelView);
            modelView.DataController = dataController;
            _windowGroupControllerWrapper.CookWindowGroup(realModel.Data.group, dataController);

            //removed to avoid copying to the buffer when this is unnecessary (copying to the buffer when it is not necessary caused outputing the same iterators values multiple times)
            // in addition the cook method should only be responsible for building the controllers tree but not copying to the buffer.
            //((RootController)modelView).EnableCopyToBufferAndCopyChanges();
            return modelView;
        }

        #endregion
    }
}