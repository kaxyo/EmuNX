using System.IO;
using EmuNX.Lib.MetadataParserNX;
using Godot;

namespace EmuNX.Gui.Components.GameTile;

public partial class GameTile : AspectRatioContainer
{
    private RomMetadata Metadata;
    private TextureRect _nodeIcon;

    public override void _Ready()
    {
        _nodeIcon = GetNode<TextureRect>("%Icon");
    }

    public void Init(RomMetadata metadata)
    {
        Metadata = metadata;
    }

    /// <summary>
    /// Loads the icon image from <see cref="Metadata"/> and overwrites the
    /// previous image.
    /// </summary>
    /// <returns>True if the Icon was loaded</returns>
    public bool LoadIcon()
    {
        // Validate icon stream from Data
        if (Metadata?.Icon is not { CanRead: true })
            return true;

        try
        {
            // Convert bytes from stream to array
            using var ms = new MemoryStream();
            Metadata.Icon.CopyTo(ms);
            byte[] buffer = ms.ToArray();

            // Load stream as JPG image
            var image = new Image();
            if (image.LoadJpgFromBuffer(buffer) != Error.Ok)
                return false;

            // Change the displayed Icon
            _nodeIcon.Texture = ImageTexture.CreateFromImage(image);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
