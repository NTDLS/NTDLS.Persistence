namespace NTDLS.Persistence
{
    /// <summary>
    /// Reads/writes to the directory that serves as a common repository for application-specific data that is used by all users.
    /// </summary>
    public class CommonApplicationData
    {
        /// <summary>
        /// Returns the path to the directory that serves as a common repository for application-specific data that is used by all users.
        /// </summary>
        public static string Path(string? applicationName = null)
            => System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), applicationName ?? string.Empty).Trim();

        /// <summary>
        /// Json serializes an object and saves it to the disk.
        /// </summary>
        public static void SaveToDisk<T>(string applicationName, T obj, PersistenceOptions? options = null)
            => PathPersistence.SaveToDisk(Path(applicationName), obj, options);

        /// <summary>
        /// Loads the object from disk, deserializes it and returns the object.
        /// </summary>
        public static T? LoadFromDisk<T>(string applicationName, PersistenceOptions? options = null)
            => PathPersistence.LoadFromDisk<T>(Path(applicationName), options);

        /// <summary>
        /// Loads the object from disk, deserializes it and returns the object, or the default result if there is no saved object.
        /// </summary>
        public static T LoadFromDisk<T>(string applicationName, T defaultResult, PersistenceOptions? options = null)
            => PathPersistence.LoadFromDisk(Path(applicationName), defaultResult, options);

        /// <summary>
        /// Deletes a persistent file from the disk
        /// </summary>
        public static void DeleteFromDisk(string applicationName, Type objectType)
            => PathPersistence.DeleteFromDisk(Path(applicationName), objectType);
    }
}
