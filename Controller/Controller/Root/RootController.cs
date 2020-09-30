using AbstractController;
using Buffer.Basic;
using Communication.Interfaces.Buffer;
using Communication.Interfaces.Controller;
using Model.Root;
using Errors.Error;
using Model.MeasurementRoutine;
using System;

namespace Controller.Root
{
    public class RootController : AbstractRootController, IController
    {
        private readonly IBuffer _buffer;
        public Variables.VariablesController Variables;
        public int TimesToReplicateOutput { set; get; }

        public RootController(RootModel model, IBuffer buffer)
            : base(model)
        {
            _buffer = buffer;
            Model = model;
        }

        private static object _actualLock = null;//TODO? this variable is defined as static to save its value when switching from one model to another in the measurement routine."Check if this is neccessary"
        private static bool _pendingChanges = false;//TODO? this variable is defined as static to save its value when switching from one model to another in the measurement routine."Check if this is neccessary"
        private bool _enableCopyToBuffer = false;

        /// <summary>
        /// Sets the model counters in the outputHandler.
        /// Values of the counters reach the RootController class from the <see cref="RoutineBasedRootModel"/> 
        /// via the <see cref="MeasurementRoutineMangerController"/>
        /// </summary>
        /// <param name="counters">The counters.</param>
        public void SetModelCounters(ModelSpecificCounters counters)
        {
            (_buffer as DoubleBuffer).OutputHandler.ModelCounters = counters;
        }

        /// <summary>
        /// Prevents multiple threads form acessing the buffer.
        /// </summary>
        /// <returns>the lock object</returns>
        ///  TODO? check if locking the buffer is necessary (Check if there are indeed multiple threads accessing the buffer).
        public object BulkUpdateStart()//Not safe if many threads try to access the method!!
        {
            object candidateLock = new object();

            if (_actualLock == null)
            {
                _actualLock = candidateLock;
            }
            DisableCopyToBuffer();

            return candidateLock;
        }

        /// <summary>
        /// Unlock the buffer to allow one of the threads to access it.
        /// </summary>
        /// <param name="updateLock">The lock object.</param>
        public bool BulkUpdateEnd(object updateLock)
        {
            if (updateLock == _actualLock)
            {
                _actualLock = null;
                EnableCopyToBufferAndCopyChanges();
                return true;
            }

            return false;

        }
        /// <summary>
        /// Re-enables the CopyToBuffer function and triggers it once
        /// </summary>
        public void EnableCopyToBufferAndCopyChanges()
        {
            _enableCopyToBuffer = true;

            if (_pendingChanges)
            {
                CopyToBuffer();
            }
            else
            {
                Console.WriteLine("Copying to buffer enabled but no pending changes were found!");
            }
        }
        public void DisableCopyToBuffer()
        {
            _enableCopyToBuffer = false;
        }

        /// <summary>
        /// Requests the buffer to accept the current model which will be read to generate the output of future cycles.
        /// </summary>
        public void CopyToBuffer()
        {
            ErrorCollector errorCollector = ErrorCollector.Instance;
            object token = errorCollector.StartBulkUpdate();

            errorCollector.RemoveErrorsOfWindow(ErrorCategory.MainHardware);

            if (Model.Verify())
            {
                //errorCollector.stopBlink();

                if (_enableCopyToBuffer)
                {
                    _buffer.CopyData(Model, TimesToReplicateOutput);
                    _pendingChanges = false;
                }
                else
                {
                    _pendingChanges = true;
                }
            }

            errorCollector.EndBulkUpdate(token);
            
        }
    }
}