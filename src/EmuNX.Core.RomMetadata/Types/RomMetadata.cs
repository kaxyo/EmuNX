using EmuNX.Core.Common.Types;

namespace EmuNX.Core.RomMetadata.Types;

/// <summary>
/// Contains the basic metadata of a ROM.
/// </summary>
public class RomMetadata(string name, TitleId id, byte[]? icon, bool promptsForUser)
{
    public RomMetadata() : this("", new TitleId(0), null, true) {}

    public string Name = name;
    public TitleId Id = id;
    public byte[]? Icon = icon;
    public bool PromptsForUser = promptsForUser;
}