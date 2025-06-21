namespace EmuNX.Lib.MetadataParserNX;

using System;
using System.IO;

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

    public string IdString { get; private set; }

    private MemoryStream _icon;
    public Stream Icon
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

    public RomMetadata() : this(null, 0, null) {}
    
    public RomMetadata(string name, ulong id, Stream icon)
    {
        Name = name;
        Id = id;
        Icon = icon;
    }

    private void DisposeIcon()
    {
        _icon?.Dispose();
        _icon = null;
    }
    
    public void Dispose()
    {
        DisposeIcon();
    }
}