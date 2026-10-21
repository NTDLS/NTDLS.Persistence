namespace NTDLS.Persistence
{
    /// <summary>
    /// Interface for compression provider used to compress and decompress the persistent data.
    /// </summary>
    public interface ICompressionProvider
    {
        /// <summary>
        /// Compress the frame payload before it is sent.
        /// </summary>
        /// <param name="payload">Contains the raw uncompressed data.</param>
        /// <returns>Return the altered bytes.</returns>
        public byte[] Compress(byte[]? payload);

        /// <summary>
        /// Encrypt the frame payload after it is received.
        /// </summary>
        /// <param name="compressedPayload">Contains the compressed data.</param>
        /// <returns>Return the altered bytes.</returns>
        public byte[] Decompress(byte[]? compressedPayload);
    }
}
