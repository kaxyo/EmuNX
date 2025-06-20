namespace EmuNX.Lib.MetadataParserNX.Parser;

public enum RomMetadataParserError
{
    Unknown,
    KeysProdNotFound,
    KeysProdInvalid,
    KeysMissing,
    RomUnknownFormat,
    XciLoadRootFsError,
    NspLoadRootFsError,
}