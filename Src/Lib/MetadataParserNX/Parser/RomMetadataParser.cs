namespace EmuNX.Lib.MetadataParserNX.Parser;

/// <summary>
/// Provides functionality to parse metadata from ROM files (XCI/NSP),
/// this includes TitleID, Name, and Icon, regardless of language.
/// This class is designed for efficient ROM parsing by avoiding redundant
/// operations. For example, decryption keys are loaded only once and then
/// reused to analyze multiple ROMs without the need to reload them.
/// </summary>
public class RomMetadataParser
{
    /// <summary>
    /// Loads the encryption keys from the title.keys and prod.keys
    /// files. These are required to read the metadata.
    /// </summary>
    /// <returns>RomMetadataParserError if an error occurs, otherwise null.</returns>
    public RomMetadataParserError? LoadKeys()
    {
        return RomMetadataParserError.Unknown;
    }
}