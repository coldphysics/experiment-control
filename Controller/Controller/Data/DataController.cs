using AbstractController.Data;
using Controller.Root;
using Model.Data;

namespace Controller.Data
{
    public class DataController : AbstractDataController
    {
        private RootController _parent;

        // ******************** constructor ********************
        public DataController(DataModel model, RootController parent) : base(model, parent)
        {
            _parent = parent;
        }

        public RootController _rootController
        {
            get { return _parent; }
        }

        public void CopyToBuffer()
        {
            ((RootController) Parent).CopyToBuffer();
        }
    }
}