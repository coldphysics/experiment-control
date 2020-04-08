using System.Collections.Concurrent;

namespace HardwareAdWin.Simulator.Dequeue
{
    /// <summary>
    /// A simple digital dequeue that is a basic encapsulation of the operations corresponding FIFO.
    /// </summary>
    /// <seealso cref="AbstractDequeue" />
    class DigitalDequeue:AbstractDequeue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalDequeue"/> class.
        /// </summary>
        /// <param name="fifo">The wrapped FIFO.</param>
        public DigitalDequeue(ConcurrentQueue<int> fifo)
            : base(fifo)
        { }

        /// <summary>
        /// Tries to dequeue an element from the FIFO.
        /// </summary>
        /// <param name="dequeResult">The dequeued value.</param>
        /// <returns>
        ///   <c>true</c> if it was possible to dequeue a value, <c>false</c> otherwise.
        /// </returns>
        public override bool Dequeue(out uint dequeResult)
        {
            int intResult = 0;
            bool isSuccess = fifo.TryDequeue(out intResult);
            dequeResult = (uint)intResult;

            return isSuccess;
        }

        /// <summary>
        /// Tries to peek at the head of the wrapped FIFO.
        /// </summary>
        /// <param name="peekResult">The result of peeking.</param>
        /// <returns>
        ///   <c>true</c> if it was possible to peek at the head of the FIFO, <c>false</c> otherwise.
        /// </returns>
        public override bool Peek(out uint peekResult)
        {
            int intResult = 0;
            bool isSuccess = fifo.TryPeek(out intResult);
            peekResult = (uint)intResult;

            return isSuccess;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <remarks>It does nothing!</remarks>
        public override void Init()
        {
            //No state -> nothing to initialize!
        }
    }
}
