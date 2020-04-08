using System.Collections.Concurrent;

namespace HardwareAdWin.Simulator.Dequeue
{
    /// <summary>
    /// A wrapper around <see cref=" ConcurrentQueue{T}"/> that provides the ability change how dequeuing and peeking works.
    /// </summary>
    abstract class AbstractDequeue
    {
        /// <summary>
        /// The wrapped FIFO.
        /// </summary>
        protected ConcurrentQueue<int> fifo;

        /// <summary>
        /// Contains the value of the last dequeued element.
        /// </summary>
        protected uint lastDequeuedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractDequeue"/> class.
        /// </summary>
        /// <param name="fifo">The wrapped FIFO.</param>
        public AbstractDequeue(ConcurrentQueue<int> fifo)
        {
            this.fifo = fifo;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public abstract void Init();

        /// <summary>
        /// Tries to dequeue an element from the FIFO.
        /// </summary>
        /// <param name="dequeResult">The dequeued value.</param>
        /// <returns><c>true</c> if it was possible to dequeue a value, <c>false</c> otherwise.</returns>
        public abstract bool Dequeue(out uint dequeResult);

        /// <summary>
        /// Tries to peek at the head of the wrapped FIFO.
        /// </summary>
        /// <param name="peekResult">The result of peeking.</param>
        /// <returns><c>true</c> if it was possible to peek at the head of the FIFO, <c>false</c> otherwise.</returns>
        public abstract bool Peek(out uint peekResult);

        /// <summary>
        /// Determines whether the wrapped FIFO is empty.
        /// </summary>
        /// <returns><c>true</c> if the encapsulated FIFO is empty, <c>false</c> otherwise.</returns>
        public bool IsEmpty()
        {
            return fifo.IsEmpty;
        }
    }
}
