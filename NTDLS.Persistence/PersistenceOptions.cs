using System.Text.Json;

namespace NTDLS.Persistence
{
    /// <summary>
    /// Optional behavior for saving and loading persistent data. The same options should be used to save and to load.
    /// On save the pipeline is: serialize, compress, encrypt, protect. On load it is reversed.
    /// </summary>
    public class PersistenceOptions
    {
        /// <summary>
        /// Json serializer options. When null, saving uses indented output (compact when <see cref="Protect"/> is set).
        /// </summary>
        public JsonSerializerOptions? Json { get; set; }

        /// <summary>
        /// Optional compression of the serialized bytes.
        /// </summary>
        public ICompressionProvider? Compression { get; set; }

        /// <summary>
        /// Optional caller-supplied encryption of the (possibly compressed) bytes.
        /// </summary>
        public IEncryptionProvider? Encryption { get; set; }

        /// <summary>
        /// When true, the bytes are protected with the operating system (DPAPI on Windows, a local key elsewhere)
        /// and stored in a ".bin" file instead of a ".json" file.
        /// </summary>
        public bool Protect { get; set; }
    }
}
