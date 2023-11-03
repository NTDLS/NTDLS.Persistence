namespace NTDLS.Persistence
{
    /// <summary>
    /// Reads/writes to the directory that serves as a common repository for application-specific data for the current roaming user.
    /// </summary>
    public class ApplicationData
    {
        /// <summary>
        /// Json serializes an object and saves it to the disk.
        /// </summary>
        public static void SaveToDisk<T>(string applicationName, T obj)
            => PathPersistence.SaveToDisk<T>(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), applicationName, obj);

        /// <summary>
        /// Loads the object from disk, deserializes it ans returns the object.
        /// </summary>
        public static T? LoadFromDisk<T>(string applicationName)
            => PathPersistence.LoadFromDisk<T>(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), applicationName);

        /// <summary>
        /// Loads the object from disk, deserializes it ans returns the object.
        /// </summary>
        public static T LoadFromDisk<T>(string applicationName, T defaultResult)
            => PathPersistence.LoadFromDisk(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), applicationName, defaultResult);
    }
}
