namespace NTDLS.Persistence
{
    /// <summary>
    /// Encryption provider used to encrypt and decrypt the persistent data.
    /// </summary>
    public interface IEncryptionProvider
    {
        /// <summary>
        /// Encrypts the serialized data.
        /// </summary>
        byte[] Encrypt(byte[] plainTextBytes);
        /// Decrypts the serialized data.
        byte[] Decrypt(byte[] encryptedTextBytes);
    }
}
