using System;
using AbstractController.Data.SequenceGroup;
using Communication.Events;
using Model.Data.SequenceGroups;

namespace Controller.Data.WindowGroups
{
    public class WindowGroupController : AbstractSequenceGroupController
    {
        // ******************** properties ********************
        public Root.RootController _rootController
        {
            get { return _parent._rootController; }
        }

        // ******************** events ********************
        public event EventHandler GroupUpdate;


        // ******************** variables ********************
        private DataController _parent;



        // ******************** constructor ********************
        public WindowGroupController(SequenceGroupModel model, DataController parent) : base(model, parent)
        {
            _parent = parent;
        }
        

        public void OnGroupUpdate(object sender, EventArgs e)
        {
            //System.Console.Write("GU\n");
            var payload = (EventPayload) e;
            if (payload.Destination == EventPayload.DestinationType.Window)
            {
                WindowUpdate(sender, payload);
                return;
            }

            EventHandler handler = GroupUpdate;
            if (handler != null)
                handler(sender, e);

            //System.Console.Write("GU end\n");
        }

        private void WindowUpdate(object sender, EventPayload payload)
        {
            return;
        }

        public void CopyToBuffer()
        {
            ((DataController) Parent).CopyToBuffer();
        }
    }
}