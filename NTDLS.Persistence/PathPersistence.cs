using System.Text;
using System.Text.Json;

namespace NTDLS.Persistence
{
    /// <summary>
    /// Reads/writes to the given path.
    /// </summary>
    public class PathPersistence
    {

        /// <summary>
        /// Deletes a persistent file from the disk
        /// </summary>
        public static void DeleteFromDisk(string directoryPath, string applicationName, Type objectType)
        {
            var dataPath = Path.Combine(directoryPath, applicationName);
            string typeName = objectType.IsGenericType ? objectType.GetGenericArguments()[0].Name : objectType.Name;
            string dataFilePath = Path.Combine(dataPath, $"{typeName}.json");
            File.Delete(dataFilePath);
        }

        /// <summary>
        /// Json serializes an object and saves it to the disk.
        /// </summary>
        public static void SaveToDiskProtected<T>(string directoryPath, string applicationName, T obj, JsonSerializerOptions? settings, ICompressionProvider? compressionProvider = null)
        {
            var dataPath = Path.Combine(directoryPath, applicationName);

            settings ??= new JsonSerializerOptions { WriteIndented = false };

            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            var type = typeof(T);
            string typeName = type.IsGenericType ? type.GetGenericArguments()[0].Name : type.Name;
            var jsonBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj, settings)) ?? [];

            if (compressionProvider != null)
            {
                jsonBytes = compressionProvider.Compress(jsonBytes);
            }

            string dataFilePath = Path.Combine(dataPath, $"{typeName}.bin");

            File.WriteAllBytes(dataFilePath, SecretProtector.Protect(jsonBytes) ?? []);
        }

        /// <summary>
        /// Json serializes an object and saves it to the disk.
        /// </summary>
        public static void SaveToDisk<T>(string directoryPath, string applicationName, T obj, JsonSerializerOptions? settings, IEncryptionProvider? encryptionProvider = null, ICompressionProvider? compressionProvider = null)
        {
            var dataPath = Path.Combine(directoryPath, applicationName);

            settings ??= new JsonSerializerOptions { WriteIndented = true };

            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            var type = typeof(T);
            string typeName = type.IsGenericType ? type.GetGenericArguments()[0].Name : type.Name;

            var jsonBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj, settings));
            string dataFilePath = Path.Combine(dataPath, $"{typeName}.json");

            if (compressionProvider != null)
            {
                jsonBytes = compressionProvider.Compress(jsonBytes);
            }

            if (encryptionProvider != null)
            {
                jsonBytes = encryptionProvider.Encrypt(jsonBytes);
            }

            File.WriteAllBytes(dataFilePath, jsonBytes);
        }

        /// <summary>
        /// Loads the object from disk, deserializes it ans returns the object.
        /// </summary>
        public static T? LoadFromDiskProtected<T>(string directoryPath, string applicationName, ICompressionProvider? compressionProvider = null)
        {
            var dataPath = Path.Combine(directoryPath, applicationName);

            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            var type = typeof(T);
            string typeName = type.IsGenericType ? type.GetGenericArguments()[0].Name : type.Name;

            string dataFilePath = Path.Combine(dataPath, $"{typeName}.json");
            if (File.Exists(dataFilePath))
            {
                var bytes = File.ReadAllBytes(dataFilePath);
                bytes = SecretProtector.Unprotect(bytes, out _);
                if (compressionProvider != null)
                {
                    bytes = compressionProvider.Decompress(bytes);
                }
                var obj = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(bytes ?? []));
                return obj;
            }
            return default;
        }

        /// <summary>
        /// Loads the object from disk, deserializes it ans returns the object.
        /// </summary>
        public static T LoadFromDiskProtected<T>(string directoryPath, string applicationName, T defaultResult, ICompressionProvider? compressionProvider = null)
        {
            var result = LoadFromDiskProtected<T>(directoryPath, applicationName, compressionProvider);
            if (result == null)
            {
                return defaultResult;
            }
            return result;
        }

        /// <summary>
        /// Loads the object from disk, deserializes it ans returns the object.
        /// </summary>
        public static T? LoadFromDisk<T>(string directoryPath, string applicationName, IEncryptionProvider? encryptionProvider = null, ICompressionProvider? compressionProvider = null)
        {
            var dataPath = Path.Combine(directoryPath, applicationName);

            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            var type = typeof(T);
            string typeName = type.IsGenericType ? type.GetGenericArguments()[0].Name : type.Name;

            string dataFilePath = Path.Combine(dataPath, $"{typeName}.json");
            if (File.Exists(dataFilePath))
            {
                var jsonBytes = File.ReadAllBytes(dataFilePath);

                if (encryptionProvider != null)
                {
                    jsonBytes = encryptionProvider.Decrypt(jsonBytes);
                }
                if (compressionProvider != null)
                {
                    jsonBytes = compressionProvider.Decompress(jsonBytes);
                }

                var obj = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(jsonBytes));
                return obj;
            }
            return default;
        }

        /// <summary>
        /// Loads the object from disk, deserializes it ans returns the object.
        /// </summary>
        public static T LoadFromDisk<T>(string directoryPath, string applicationName, T defaultResult, IEncryptionProvider? encryptionProvider = null, ICompressionProvider? compressionProvider = null)
        {
            var result = LoadFromDisk<T>(directoryPath, applicationName, encryptionProvider, compressionProvider);
            if (result == null)
            {
                return defaultResult;
            }
            return result;
        }
    }
}
