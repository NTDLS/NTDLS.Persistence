using System.Text.Json;

namespace NTDLS.Persistence
{
    /// <summary>
    /// Reads/writes to the directory that serves as a common repository for application-specific data that is used by the current, non-roaming user.
    /// </summary>
    public class LocalUserApplicationData
    {
        /// <summary>
        /// Returns the path to the directory that serves as a common repository for application-specific data that is used by the current, non-roaming user.
        /// </summary>
        /// <param name="applicationName"></param>
        /// <returns></returns>
        public static string Path(string? applicationName = null)
            => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), applicationName ?? string.Empty).Trim();

        /// <summary>
        /// Json serializes an object and saves it to the disk.
        /// </summary>
        public static void SaveToDisk<T>(string applicationName, T obj, IEncryptionProvider? encryptionProvider = null, ICompressionProvider? compressionProvider = null)
            => PathPersistence.SaveToDisk<T>(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), applicationName, obj, null, encryptionProvider, compressionProvider);

        /// <summary>
        /// Json serializes an object and saves it to the disk.
        /// </summary>
        public static void SaveToDisk<T>(string applicationName, T obj, JsonSerializerOptions settings, IEncryptionProvider? encryptionProvider = null, ICompressionProvider? compressionProvider = null)
            => PathPersistence.SaveToDisk<T>(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), applicationName, obj, settings, encryptionProvider, compressionProvider);

        /// <summary>
        /// Loads the object from disk, deserializes it ans returns the object.
        /// </summary>
        public static T? LoadFromDisk<T>(string applicationName, IEncryptionProvider? encryptionProvider = null, ICompressionProvider? compressionProvider = null)
            => PathPersistence.LoadFromDisk<T>(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), applicationName, encryptionProvider, compressionProvider);

        /// <summary>
        /// Loads the object from disk, deserializes it ans returns the object.
        /// </summary>
        public static T LoadFromDisk<T>(string applicationName, T defaultResult, IEncryptionProvider? encryptionProvider = null, ICompressionProvider? compressionProvider = null)
            => PathPersistence.LoadFromDisk(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), applicationName, defaultResult, encryptionProvider, compressionProvider);

        /// <summary>
        /// Deletes a persistent file from the disk
        /// </summary>
        public static void DeleteFromDisk(string applicationName, Type objectType)
            => PathPersistence.DeleteFromDisk(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), applicationName, objectType);

        /// <summary>
        /// Json serializes an object and saves it to the disk.
        /// </summary>
        public static void SaveToDiskProtected<T>(string applicationName, T obj, ICompressionProvider? compressionProvider = null)
            => PathPersistence.SaveToDiskProtected<T>(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), applicationName, obj, null, compressionProvider);

        /// <summary>
        /// Json serializes an object and saves it to the disk.
        /// </summary>
        public static void SaveToDiskProtected<T>(string applicationName, T obj, JsonSerializerOptions settings, ICompressionProvider? compressionProvider = null)
            => PathPersistence.SaveToDiskProtected<T>(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), applicationName, obj, settings, compressionProvider);

        /// <summary>
        /// Loads the object from disk, deserializes it ans returns the object.
        /// </summary>
        public static T? LoadFromDiskProtected<T>(string applicationName, ICompressionProvider? compressionProvider = null)
            => PathPersistence.LoadFromDiskProtected<T>(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), applicationName, compressionProvider);

        /// <summary>
        /// Loads the object from disk, deserializes it ans returns the object.
        /// </summary>
        public static T LoadFromDiskProtected<T>(string applicationName, T defaultResult, ICompressionProvider? compressionProvider = null)
            => PathPersistence.LoadFromDiskProtected(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), applicationName, defaultResult, compressionProvider);
    }
}
