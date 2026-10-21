using System.Security.Cryptography;
using System.Text;

namespace NTDLS.Persistence
{
    /// <summary>
    /// Protects the secrets that are written to the configuration files (certificate passwords and the password hashes
    ///     used to log in), so that they are not readable in the clear when a file is copied, backed up or shared.
    ///
    /// Input and output are raw bytes. A protected value is the ASCII marker "enc:dpapi:" (Windows: DPAPI, machine scope,
    ///     so that it works the same whichever account the service or a debugger runs as, but only on that machine) or
    ///     "enc:aes:" (elsewhere: AES-256-GCM with a random key kept in a key file next to the configuration, readable
    ///     only by its owner) followed directly by the binary payload. For AES the payload is nonce, tag, then cipher text.
    ///
    /// A value that is not protected (a file written by an older version, or a value pasted in by hand) is read as is and is
    ///     protected the next time the file is saved.
    ///
    /// This keeps the secrets out of the files. It does not defend against someone who can run code as an
    ///     administrator (or as the service account) on the same machine.
    /// </summary>
    internal static class SecretProtector
    {
        private static ReadOnlySpan<byte> DpapiPrefix => "enc:dpapi:"u8;
        private static ReadOnlySpan<byte> AesPrefix => "enc:aes:"u8;
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const int KeySize = 32;

        private static readonly byte[] _entropy = Encoding.UTF8.GetBytes("NTDLS.Persistence.v1");
        private static readonly Lock _keyLock = new();
        private static byte[]? _key;

        public static bool IsProtected(ReadOnlySpan<byte> value)
            => value.StartsWith(DpapiPrefix) || value.StartsWith(AesPrefix);

        /// <summary>
        /// Returns the protected form of a secret. Empty values and values that are already protected are returned unchanged.
        /// </summary>
        public static byte[]? Protect(byte[]? secret)
        {
            if (secret == null || secret.Length == 0 || IsProtected(secret))
            {
                return secret;
            }

            if (OperatingSystem.IsWindows())
            {
                var encrypted = ProtectedData.Protect(secret, _entropy, DataProtectionScope.LocalMachine);

                var output = new byte[DpapiPrefix.Length + encrypted.Length];
                DpapiPrefix.CopyTo(output);
                encrypted.CopyTo(output.AsSpan(DpapiPrefix.Length));
                return output;
            }

            var result = new byte[AesPrefix.Length + NonceSize + TagSize + secret.Length];
            var span = result.AsSpan();

            AesPrefix.CopyTo(span);
            var nonce = span.Slice(AesPrefix.Length, NonceSize);
            var tag = span.Slice(AesPrefix.Length + NonceSize, TagSize);
            var cipher = span[(AesPrefix.Length + NonceSize + TagSize)..];

            RandomNumberGenerator.Fill(nonce);
            using (var aes = new AesGcm(GetKey(), TagSize))
            {
                aes.Encrypt(nonce, secret, cipher, tag, _entropy);
            }

            return result;
        }

        /// <summary>
        /// Returns the secret from its stored form. A value that is not protected is returned as it is, with
        ///     <paramref name="wasPlainText"/> set so that the caller can save it again in protected form.
        ///     A value that cannot be decrypted (for example a file copied from another machine) throws, so that
        ///     a lost password hash can never become an empty one that matches.
        /// </summary>
        public static byte[]? Unprotect(byte[]? stored, out bool wasPlainText)
        {
            wasPlainText = false;

            if (stored == null || stored.Length == 0)
            {
                return stored;
            }

            if (!IsProtected(stored))
            {
                wasPlainText = true;
                return stored;
            }

            if (stored.AsSpan().StartsWith(DpapiPrefix))
            {
                if (!OperatingSystem.IsWindows())
                {
                    throw new PlatformNotSupportedException("The value was protected with Windows DPAPI.");
                }

                return ProtectedData.Unprotect(stored[DpapiPrefix.Length..], _entropy, DataProtectionScope.LocalMachine);
            }

            var data = stored.AsSpan(AesPrefix.Length);
            if (data.Length < NonceSize + TagSize)
            {
                throw new InvalidDataException("The protected value is truncated.");
            }

            var nonce = data[..NonceSize];
            var tag = data.Slice(NonceSize, TagSize);
            var cipher = data[(NonceSize + TagSize)..];
            var plain = new byte[cipher.Length];
            using (var aes = new AesGcm(GetKey(), TagSize))
            {
                aes.Decrypt(nonce, cipher, tag, plain, _entropy);
            }
            return plain;
        }

        /// <summary>
        /// The key for platforms without DPAPI. Created on first use, in a file that only its owner can read.
        /// </summary>
        private static byte[] GetKey()
        {
            lock (_keyLock)
            {
                if (_key != null)
                {
                    return _key;
                }

                // The key is per-user (e.g. ~/.local/share) because the machine-wide folder (e.g. /usr/share) is
                //  generally not writable by ordinary users. A key already created in the machine-wide folder is still honored.
                var legacyPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "NTDLS.Persistence", "key.bin");
                var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NTDLS.Persistence");
                var path = File.Exists(legacyPath) ? legacyPath : Path.Combine(folder, "key.bin");

                if (File.Exists(path))
                {
                    var existing = File.ReadAllBytes(path);
                    if (existing.Length != KeySize)
                    {
                        throw new InvalidDataException($"The key file '{path}' is not valid.");
                    }
                    return _key = existing;
                }

                Directory.CreateDirectory(folder);

                var key = RandomNumberGenerator.GetBytes(KeySize);
                var options = new FileStreamOptions { Mode = FileMode.CreateNew, Access = FileAccess.Write };
                if (!OperatingSystem.IsWindows())
                {
                    options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
                }

                try
                {
                    using var stream = new FileStream(path, options);
                    stream.Write(key);
                }
                catch (IOException) when (File.Exists(path))
                {
                    // Another process created the key first; use its key rather than ours.
                    var existing = File.ReadAllBytes(path);
                    if (existing.Length != KeySize)
                    {
                        throw new InvalidDataException($"The key file '{path}' is not valid.");
                    }
                    return _key = existing;
                }

                return _key = key;
            }
        }
    }
}
