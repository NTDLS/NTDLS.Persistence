using System.IO.Compression;

namespace NTDLS.Persistence
{
    /// <summary>
    /// Default compression provider that uses the Brotli compression algorithm.
    /// </summary>
    public class BrotliCompressionProvider
        : ICompressionProvider
    {
        /// <summary>
        /// Compression level to use when compressing the payload.
        /// </summary>
        public CompressionLevel CompressionLevel { get; private set; }

        /// <summary>
        /// Creates a new instance of the Brotli compression provider with the default compression level.
        /// </summary>
        public BrotliCompressionProvider()
        {
            CompressionLevel = CompressionLevel.Optimal;
        }

        /// <summary>
        /// creates a new instance of the Brotli compression provider with the specified compression level.
        /// </summary>
        public BrotliCompressionProvider(CompressionLevel compressionLevel)
        {
            CompressionLevel = compressionLevel;
        }

        /// <summary>
        /// Compresses the payload using Brotli.
        /// </summary>
        public byte[] Compress(byte[]? payload)
        {
            if (payload == null)
                return [];

            using var msi = new MemoryStream(payload);
            using var mso = new MemoryStream();
            using (var brotli = new BrotliStream(mso, CompressionLevel))
            {
                msi.CopyTo(brotli);
            }
            return mso.ToArray();
        }

        /// <summary>
        /// Decompresses the payload using Brotli.
        /// </summary>
        public byte[] Decompress(byte[]? compressedPayload)
        {
            if (compressedPayload == null)
                return [];

            using var msi = new MemoryStream(compressedPayload);
            using var mso = new MemoryStream();
            using (var brotli = new BrotliStream(msi, CompressionMode.Decompress))
            {
                brotli.CopyTo(mso);
            }
            return mso.ToArray();
        }
    }
}
