# NTDLS.Persistence

NTDLS.Persistence is a small .NET helper library (targets .NET 8, 9 and 10) for persisting objects to disk as JSON. It writes your objects into the standard Windows application-data folders — per-user local, roaming, or machine-wide common data — so you don't have to manage paths, directories, or serialization yourself. Files are named after the type being saved, live in a folder named after your application, and can optionally be encrypted at rest by supplying your own `IEncryptionProvider`.

📦 Be sure to check out the NuGet package: https://www.nuget.org/packages/NTDLS.Persistence

## When to use this package

Use NTDLS.Persistence when you need simple file-based persistence without rolling your own path handling and JSON code, for example:

- **Saving configuration files** — application settings, user preferences, connection strings, and similar state that should survive restarts.
- **Choosing the right storage location automatically** — pick between per-user non-roaming data (`LocalUserApplicationData`), per-user roaming data (`RoamingUserApplicationData`, synced via roaming profiles), or machine-wide data shared by all users (`CommonApplicationData`) with a single class swap.
- **Encrypting persisted data at rest** — pass any implementation of `IEncryptionProvider` to encrypt the serialized bytes before they touch disk and decrypt them on load.
- **Customizing serialization** — an overload accepts `JsonSerializerOptions` (Newtonsoft.Json) when you need custom converters, naming strategies, or formatting.
- **Keeping things boringly simple** — no database, no ORM, no registry: just one JSON file per type under `%LOCALAPPDATA%`, `%APPDATA%`, or the common program-data directory.

## Usage examples

### Saving and loading a settings object

The most common case: save an object to disk and load it back later. The file is stored as `<TypeName>.json` inside a folder named after your application in the chosen data directory.

```csharp
using NTDLS.Persistence;

// The object to persist — any JSON-serializable type works.
class UIPreferences
{
    public string Username { get; set; } = string.Empty;
    public bool RememberPassword { get; set; }
}

static void Main()
{
    // Create some preferences.
    var preferences = new UIPreferences
    {
        Username = "JPatterson",
        RememberPassword = false
    };

    // Save to the current user's LocalAppData folder:
    //   %LOCALAPPDATA%\MyProgramName\UIPreferences.json
    LocalUserApplicationData.SaveToDisk("MyProgramName", preferences);

    // Load it back later. Returns null if the file does not exist yet.
    UIPreferences? loaded = LocalUserApplicationData.LoadFromDisk<UIPreferences>("MyProgramName");
}
```

### Loading with a default value

If the file may not exist (first run), use the overload that takes a default result instead of dealing with `null`:

```csharp
// Returns the provided default if no saved file exists.
var prefs = LocalUserApplicationData.LoadFromDisk(
    "MyProgramName",
    new UIPreferences()); // defaultResult
```

### Choosing where the data lives

All three facades expose the same API — swap the class to change the storage location:

```csharp
using NTDLS.Persistence;

// Per-user, non-roaming: %LOCALAPPDATA%\MyProgramName\...
LocalUserApplicationData.SaveToDisk("MyProgramName", settings);

// Per-user, roaming: %APPDATA%\MyProgramName\...
RoamingUserApplicationData.SaveToDisk("MyProgramName", settings);

// Machine-wide, all users: C:\ProgramData\MyProgramName\...
CommonApplicationData.SaveToDisk("MyProgramName", settings);
```

### Custom JSON serialization settings

Pass `JsonSerializerOptions` when you need control over how the object is serialized:

```csharp
using Newtonsoft.Json;
using NTDLS.Persistence;

var settings = new JsonSerializerOptions
{
    ContractResolver = new CamelCasePropertyNamesContractResolver(),
    Formatting = Formatting.None // compact output instead of indented
};

LocalUserApplicationData.SaveToDisk("MyProgramName", preferences, settings);
```

### Encrypting persisted data

Implement `IEncryptionProvider` and pass it to any save/load call. The provider receives the UTF-8 JSON bytes:

```csharp
using NTDLS.Persistence;

// A minimal AES-based provider (your key management is up to you).
sealed class MyEncryptionProvider : IEncryptionProvider
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public MyEncryptionProvider(byte[] key, byte[] iv)
    {
        _key = key;
        _iv = iv;
    }

    public byte[] Encrypt(byte[] plainTextBytes)
    {
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        return aes.EncryptCbc(plainTextBytes);
    }

    public byte[] Decrypt(byte[] encryptedTextBytes)
    {
        using var aes = System.Security.Cryptography.Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;
        return aes.DecryptCbc(encryptedTextBytes);
    }
}

// Use it on both save and load — the same provider must be used for both.
var provider = new MyEncryptionProvider(key, iv);
LocalUserApplicationData.SaveToDisk("MyProgramName", preferences, provider);
var loaded = LocalUserApplicationData.LoadFromDisk<UIPreferences>("MyProgramName", provider);
```

### Deleting a saved file

```csharp
// Deletes %LOCALAPPDATA%\MyProgramName\UIPreferences.json
LocalUserApplicationData.DeleteFromDisk("MyProgramName", typeof(UIPreferences));
```

## License

[MIT](https://choosealicense.com/licenses/mit/)
