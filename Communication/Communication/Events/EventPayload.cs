using System;

namespace Communication.Events
{
    public class EventPayload : EventArgs
    {
        #region ActionType enum

        public enum ActionType
        {
            TabAdd,
            TabRemove,
            TabRename,
            TabMove,
            TabLeft,
            TabRight,
            UpdateTime,
        }

        #endregion

        #region DestinationType enum

        public enum DestinationType
        {
            Group,
            Window,
            Tab,
            Channel,
            Step
            
        }

        #endregion

        public readonly ActionType Action;
        public readonly DestinationType Destination;

        public object Cargo;

        public EventPayload(DestinationType destination, ActionType action)
        {
            Destination = destination;
            Action = action;
        }
    }
}