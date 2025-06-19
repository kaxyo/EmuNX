namespace EmuNX.Lib.MetadataParserNX.Parser;

using System.IO;
using LibHac.Common.Keys;

/// <summary>
/// Provides functionality to parse metadata from ROM files (XCI/NSP),
/// this includes TitleID, Name, and Icon, regardless of language.
/// This class is designed for efficient ROM parsing by avoiding redundant
/// operations. For example, decryption keys are loaded only once and then
/// reused to analyze multiple ROMs without the need to reload them again.
/// </summary>
public class RomMetadataParser
{
    private KeySet _keyset = null;

    /// <summary>
    /// Loads the encryption keys from prod.keys file.
    /// These are required to read the metadata.
    /// </summary>
    /// <param name="prodKeysPath">Path to the prod.keys file</param>
    /// <returns>RomMetadataParserError if an error occurs, otherwise null.</returns>
    public RomMetadataParserError? LoadKeys(string prodKeysPath)
    {
        _keyset = null;

        // Check file existence
        if (!File.Exists(prodKeysPath)) return RomMetadataParserError.KeysProdNotFound;

        // Load keys
        _keyset = new KeySet();
        ExternalKeyReader.ReadKeyFile(_keyset, prodKeysPath, null, null, null);

        // Validate keys
        if (_keyset.HeaderKey.IsZeros()) return RomMetadataParserError.KeysProdInvalid;
        
        return null;
    }
}