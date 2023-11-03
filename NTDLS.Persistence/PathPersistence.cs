using Newtonsoft.Json;

namespace NTDLS.Persistence
{
    /// <summary>
    /// Reads/writes to the given path.
    /// </summary>
    public class PathPersistence
    {
        /// <summary>
        /// Json serializes an object and saves it to the disk.
        /// </summary>
        public static void SaveToDisk<T>(string directoryPath, string applicationName, T obj)
        {
            var dataPath = Path.Combine(directoryPath, applicationName);

            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

            var type = typeof(T);
            string typeName = type.IsGenericType ? type.GetGenericArguments()[0].Name : type.Name;

            var jsonText = JsonConvert.SerializeObject(obj, Formatting.Indented);
            string dataFilePath = Path.Combine(dataPath, $"{typeName}.json");
            File.WriteAllText(dataFilePath, jsonText);
        }

        /// <summary>
        /// Loads the object from disk, deserializes it ans returns the object.
        /// </summary>
        public static T? LoadFromDisk<T>(string directoryPath, string applicationName)
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
                var jsonText = File.ReadAllText(dataFilePath);
                var obj = JsonConvert.DeserializeObject<T>(jsonText);
                return obj;
            }
            return default;
        }

        /// <summary>
        /// Loads the object from disk, deserializes it ans returns the object.
        /// </summary>
        public static T LoadFromDisk<T>(string directoryPath, string applicationName, T defaultResult)
        {
            var result = LoadFromDisk<T>(directoryPath, applicationName);
            if (result == null)
            {
                return defaultResult;
            }
            return result;
        }
    }
}
