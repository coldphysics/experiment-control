using Controller.Data.WindowGroups;
using Model.Data.Cards;

namespace Controller.Data.Windows
{
    public class WindowController : WindowBasicController
    {
        public WindowController(CardBasicModel model, WindowGroupController parent) : base(model, parent)
        {
        }
    }
}