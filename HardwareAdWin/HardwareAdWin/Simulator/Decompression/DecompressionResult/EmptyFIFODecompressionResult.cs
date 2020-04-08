namespace HardwareAdWin.Simulator.Decompression.DecompressionResult
{
    /// <summary>
    /// Represent the returned value when the FIFO is found to be abnormally empty (it did not end with a end-of-sequence value).
    /// </summary>
    /// <seealso cref="IDecompressionResult" />
    class EmptyFIFODecompressionResult:IDecompressionResult
    {
    }
}
