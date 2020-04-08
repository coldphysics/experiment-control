using System;

namespace Communication.Events
{
    public class BufferPayload : EventArgs
    {
        #region ActionType enum

        public enum ActionType
        {
            UpdateFailed,
            DataUpdate,
            GeneratingOutput,
            GeneratingFinished,
            OutputFinished,
            Stopped,
            Started,
            CountUpdate,
            IterationStopped
        }

        #endregion

        public readonly ActionType Action;

        public BufferPayload(ActionType action)
        {
            Action = action;
        }
    }
}