namespace EmuNX.Lib.MetadataParserNX.Parser;

using System.IO;
using System.Linq;
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
    /// The next thing you can execute is <see cref="LoadRootFsFromRom(string)"/>.
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

    /// <summary>
    /// Tries to load the Root FileSystem from any rom file.
    /// The rom can be any XCI or NSP, and the Root FileSystem will be loaded
    /// with the adequate method.
    /// This Root FileSystem contains multiple NCAs like CNMT, CONTROL, etc...
    /// The next thing you can execute is <see cref="..."/>.
    /// </summary>
    /// <param name="romPath">Path to the xci|nsp file</param>
    /// <returns>RomMetadataParserError if an error occurs, otherwise null.</returns>
    public RomMetadataParserError? LoadRootFsFromRom(string romPath)
    {
        var romExtension = romPath.Split(".").Last();

        return romExtension switch
        {
            "xci" => LoadRootFsFromXCI(romPath),
            "nsp" => LoadRootFsFromNSP(romPath),
            _ => RomMetadataParserError.RomUnknownFormat
        };
    }
    
    /// <summary>
    /// Tries to load the Root FileSystem from any XCI file.
    /// This Root FileSystem contains multiple NCAs like CNMT, CONTROL, etc...
    /// The next thing you can execute is <see cref="..."/>.
    /// </summary>
    /// <param name="xciPath">Path to the xci file</param>
    /// <returns>RomMetadataParserError if an error occurs, otherwise null.</returns>
    /// <returns></returns>
    public RomMetadataParserError? LoadRootFsFromXCI(string xciPath)
    {
        return RomMetadataParserError.Unknown;
    }
    
    /// <summary>
    /// Tries to load the Root FileSystem from any NSP file.
    /// This Root FileSystem contains multiple NCAs like CNMT, CONTROL, etc...
    /// The next thing you can execute is <see cref="..."/>.
    /// </summary>
    /// <param name="nspPath">Path to the nsp file</param>
    /// <returns>RomMetadataParserError if an error occurs, otherwise null.</returns>
    public RomMetadataParserError? LoadRootFsFromNSP(string nspPath)
    {
        return RomMetadataParserError.Unknown;
    }
}