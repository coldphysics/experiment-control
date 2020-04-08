using AbstractController.Data;
using Model.Root;

namespace AbstractController
{
    /// <summary>
    /// The controller at the root level of the hierarchy.
    /// Provides view-independent functionalities of the ViewModel of the MVVM design pattern
    /// </summary>
    public class AbstractRootController
    {
        //RECO encapsulate!

        /// <summary>
        /// The child <see cref=" AbstractDataController"/> of the root controller
        /// </summary>
        public AbstractDataController DataController;
        /// <summary>
        /// The <see cref=" RootModel"/> this controller is attached to
        /// </summary>
        protected RootModel Model;

        /// <summary>
        /// Gets a reference to the model this controller is associated with
        /// </summary>
        public RootModel returnModel
        {
            get { return Model; }
        }

        /// <summary>
        /// Initializes a new instance of the this class. Does not instantiate the child <see cref=" AbstractDataController"/>.
        /// </summary>
        /// <param name="model">The <see cref=" RootModel"/> this instance will be associated to.</param>
        public AbstractRootController(RootModel model)
        {            
            Model = model;
        }

        // RECO use an abstract controller class to put a Duration method there

        /// <summary>
        /// Returns the duration of the entire model as the duration of its <see cref=" AbstractDataController"/>
        /// </summary>
        /// <returns>The duration of the <see cref=" AbstractDataController"/></returns>
        public double Duration()
        {
            if (DataController != null)
            {
                return DataController.Duration();
            }

            return 0;
        }
    }
}