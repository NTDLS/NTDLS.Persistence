namespace NTDLS.Persistence
{
    /// <summary>
    /// Reads/writes to the directory that serves as a common repository for application-specific data that is used by all users.
    /// </summary>
    public class CommonApplicationData
    {
        /// <summary>
        /// Json serializes an object and saves it to the disk.
        /// </summary>
        public static void SaveToDisk<T>(string applicationName, T obj)
            => PathPersistence.SaveToDisk<T>(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), applicationName, obj);

        /// <summary>
        /// Loads the object from disk, deserializes it ans returns the object.
        /// </summary>
        public static T? LoadFromDisk<T>(string applicationName)
            => PathPersistence.LoadFromDisk<T>(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), applicationName);

        /// <summary>
        /// Loads the object from disk, deserializes it ans returns the object.
        /// </summary>
        public static T LoadFromDisk<T>(string applicationName, T defaultResult)
            => PathPersistence.LoadFromDisk(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), applicationName, defaultResult);
    }
}
