namespace EmuNX.Core.Configuration;

/// <summary>
/// Represents a version for a configuration file/source that can be easily compared with other versions.
/// </summary>
public struct Version(uint major, uint minor)
{
    /// <summary>
    /// Represents the changes of structure or content.
    /// </summary>
    public uint Major = major;
    /// <summary>
    /// Represents the number of additions.
    /// </summary>
    public uint Minor = minor;

    public bool IsCompatibleWith(Version other)
    {
        return this.Major == other.Major && this.Minor <= other.Minor;
    }
}