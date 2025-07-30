using EmuNX.Core.Common.Types;

namespace EmuNX.Core.RomMetadata.Types;

/// <summary>
/// Contains the basic metadata of a ROM.
/// </summary>
public class RomMetadata : IDisposable
{
    public string Name;

    public TitleId Id;

    private MemoryStream? _icon;
    public Stream? Icon
    {
        get => _icon;
        set
        {
            DisposeIcon();
            if (value == null) return;
            _icon = new MemoryStream();
            value.CopyTo(_icon);
            _icon.Position = 0;
        }
        
    }

    public bool PromptsForUser;

    public RomMetadata() : this("", new TitleId(0), null, true) {}
    
    public RomMetadata(string name, TitleId id, Stream? icon, bool promptsForUser)
    {
        Name = name;
        Id = id;
        Icon = icon;
        PromptsForUser = promptsForUser;
    }

    #region Resource management
    private void DisposeIcon()
    {
        _icon?.Dispose();
        _icon = null;
    }
    
    public void Dispose()
    {
        DisposeIcon();
    }
    #endregion
}