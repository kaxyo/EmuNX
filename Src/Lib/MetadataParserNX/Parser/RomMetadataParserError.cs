namespace EmuNX.Lib.MetadataParserNX.Parser;

public enum RomMetadataParserError
{
    Unknown,
    KeysProdNotFound,
    KeysProdInvalid,
    KeysMissing,
    RomUnknownFormat,
    RomReadDependenciesNotComplete,
    XciLoadRootFsError,
    NspLoadRootFsError,
    CnmtNcaNotFound,
    CnmtNcaReadError,
    CnmtReadDependenciesNotComplete,
    CnmtNotFound,
    CnmtReadError,
}