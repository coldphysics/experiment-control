using System;
using System.Collections.Concurrent;

namespace HardwareAdWin.Simulator.Dequeue
{
    /// <summary>
    /// Describes the current half (which 2 bytes) of an integer <see cref=" Int32"/>variable (4 bytes) is being processed.
    /// </summary>
    enum IntegerHalf
    {
        /// <summary>
        /// The lower 2 bytes of an integer
        /// </summary>
        LOWER,
        /// <summary>
        /// The higher 2 bytes of an integer
        /// </summary>
        HIGHER
    }

    /// <summary>
    /// An analog dequeue that takes its compressed content into consideration, namely, it delivers the correct 2-byte integer when asked to dequeue or peek.
    /// </summary>
    /// <seealso cref="AbstractDequeue" />
    class AnalogDequeue:AbstractDequeue
    {
        /// <summary>
        /// A mask for the lower 16 bits of a 4-byte integer (the least significant 16 bits).
        /// </summary>
        private const uint LOWER_16_BITS = 0X0000FFFF;
        /// <summary>
        /// A mask for the higher 16 bits of a 4-byte integer (the most significant 16 bits).
        /// </summary>
        private const uint HIGHER_16_BITS = 0XFFFF0000;
        /// <summary>
        /// The integer half that corresponds to the last returned value with a dequeue operation.
        /// </summary>
        private IntegerHalf lastOutputHalf = IntegerHalf.LOWER;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalogDequeue"/> class.
        /// </summary>
        /// <param name="fifo">The wrapped FIFO.</param>
        public AnalogDequeue(ConcurrentQueue<int> fifo)
            : base(fifo)
        { }

        /// <summary>
        /// Tries to dequeue an element from the FIFO.
        /// </summary>
        /// <param name="dequeResult">The dequeued value.</param>
        /// <returns>
        ///   <c>true</c> if it was possible to dequeue a value, <c>false</c> otherwise.
        /// </returns>
        /// <remarks>
        /// It does not necessarily dequeues a new value from the FIFO, the next value could be the second (low-order) 2-bytes of a previously dequeued 4-byte integer.
        /// </remarks>
        public override bool Dequeue(out uint dequeResult)
        {
            bool result = true;
           
            if (lastOutputHalf == IntegerHalf.LOWER)
            {
                int valInt;
                result = fifo.TryDequeue(out valInt);
                lastDequeuedValue = (uint)valInt;
                dequeResult = (lastDequeuedValue & HIGHER_16_BITS) >> 16;
                lastOutputHalf = IntegerHalf.HIGHER;
            }
            else
            {
                dequeResult = lastDequeuedValue & LOWER_16_BITS;
                lastOutputHalf = IntegerHalf.LOWER;
            }

            return result;
        }

        /// <summary>
        /// Tries to peek at the head of the wrapped FIFO.
        /// </summary>
        /// <param name="peekResult">The result of peeking.</param>
        /// <returns>
        ///   <c>true</c> if it was possible to peek at the head of the FIFO, <c>false</c> otherwise.
        /// </returns>
        ///<remarks>
        /// It does not necessarily read a value from the head of the FIFO, the value could be the second (low-order) 2-bytes of a previously dequeued 4-byte integer.
        /// </remarks>
        public override bool Peek(out uint peekResult)
        {
            bool result = true;

            if (lastOutputHalf == IntegerHalf.LOWER)
            {
                int valInt;
                uint peekedValue;
                result = fifo.TryPeek(out valInt);
                peekedValue = (uint)valInt;
                peekResult = (peekedValue & HIGHER_16_BITS) >> 16;
            }
            else
            {
                peekResult = lastDequeuedValue & LOWER_16_BITS;
            }

            return result;
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <remarks>
        /// Guarantees that the next execution of Peek or Dequeue would run against the FIFO, 
        /// rather than a previously read integer.
        /// </remarks>
        public override void Init()
        {
            lastOutputHalf = IntegerHalf.LOWER;
        }
    }
}
