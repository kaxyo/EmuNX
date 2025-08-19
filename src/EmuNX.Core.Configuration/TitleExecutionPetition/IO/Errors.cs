namespace EmuNX.Core.Configuration.TitleExecutionPetition.IO;

/// Errors that might arise when <b>loading</b> a <see cref="TepConfig"/>.
public enum LoadError
{
    /// The resource (e.g. file) couldn't be read.
    ResourceAccessFailed,
    /// The resource (e.g. file) couldn't be deserialized.
    ResourceDeserializationFailed,
    /// A <see cref="Version"/> couldn't be found in the resource.
    MetaVersionNotFound,
    /// The <see cref="Version"/> found in the resource is not compatible.
    MetaVersionNotCompatible,
}
    
/// Errors that might arise when <b>saving</b> a <see cref="TepConfig"/>.
public enum SaveError
{
    /// This should never arise as it is a placeholder.
    Unknown,
    /// The resource (e.g. file) couldn't be written.
    ResourceWriteFailed,
}