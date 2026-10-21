using System.Text;
using System.Text.Json;

namespace NTDLS.Persistence
{
    /// <summary>
    /// Reads/writes to the given path.
    /// </summary>
    public class PathPersistence
    {
        private const string PlainExtension = "json";
        private const string ProtectedExtension = "bin";

        private static string GetDataFilePath(string dataPath, Type type, string extension, bool createDirectory)
        {
            if (createDirectory && !Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            string typeName = type.IsGenericType ? type.GetGenericArguments()[0].Name : type.Name;
            return Path.Combine(dataPath, $"{typeName}.{extension}");
        }

        /// <summary>
        /// Deletes a persistent file from the disk
        /// </summary>
        public static void DeleteFromDisk(string directoryPath, Type objectType)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            File.Delete(GetDataFilePath(directoryPath, objectType, PlainExtension, false));
            File.Delete(GetDataFilePath(directoryPath, objectType, ProtectedExtension, false));
        }

        /// <summary>
        /// Json serializes an object and saves it to the disk.
        /// </summary>
        public static void SaveToDisk<T>(string directoryPath, T obj, PersistenceOptions? options = null)
        {
            var json = options?.Json ?? new JsonSerializerOptions { WriteIndented = options?.Protect != true };
            var dataFilePath = GetDataFilePath(directoryPath, typeof(T), options?.Protect == true ? ProtectedExtension : PlainExtension, true);

            var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj, json));

            if (options?.Compression != null)
            {
                bytes = options.Compression.Compress(bytes);
            }

            if (options?.Encryption != null)
            {
                bytes = options.Encryption.Encrypt(bytes);
            }

            if (options?.Protect == true)
            {
                bytes = SecretProtector.Protect(bytes) ?? [];
            }

            File.WriteAllBytes(dataFilePath, bytes);
        }

        /// <summary>
        /// Loads the object from disk, deserializes it and returns the object.
        /// </summary>
        public static T? LoadFromDisk<T>(string directoryPath, PersistenceOptions? options = null)
        {
            var dataFilePath = GetDataFilePath(directoryPath, typeof(T), options?.Protect == true ? ProtectedExtension : PlainExtension, false);

            if (!File.Exists(dataFilePath))
            {
                return default;
            }

            var bytes = File.ReadAllBytes(dataFilePath);

            if (options?.Protect == true)
            {
                bytes = SecretProtector.Unprotect(bytes, out _) ?? [];
            }

            if (options?.Encryption != null)
            {
                bytes = options.Encryption.Decrypt(bytes);
            }

            if (options?.Compression != null)
            {
                bytes = options.Compression.Decompress(bytes);
            }

            return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(bytes), options?.Json);
        }

        /// <summary>
        /// Loads the object from disk, deserializes it and returns the object, or the default result if there is no saved object.
        /// </summary>
        public static T LoadFromDisk<T>(string directoryPath, T defaultResult, PersistenceOptions? options = null)
            => LoadFromDisk<T>(directoryPath, options) ?? defaultResult;
    }
}
