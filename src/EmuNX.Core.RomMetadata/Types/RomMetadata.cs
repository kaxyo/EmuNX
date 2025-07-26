namespace EmuNX.Core.RomMetadata.Types;

/// <summary>
/// Contains the basic metadata of a ROM.
/// </summary>
public class RomMetadata : IDisposable
{
    public string Name;

    private ulong _id;
    public ulong Id
    {
        get => _id;
        set
        {
            _id = value;
            IdString = $"{_id:X}";
        }
    }

    public string IdString { get; private set; } = "";

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

    public RomMetadata() : this("", 0, null, true) {}
    
    public RomMetadata(string name, ulong id, Stream? icon, bool promptsForUser)
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