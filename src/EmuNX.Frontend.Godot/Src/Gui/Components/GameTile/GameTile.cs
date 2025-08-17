using System.IO;
using EmuNX.Core.RomMetadata.Types;
using Godot;

namespace EmuNX.Frontend.Godot.Gui.Components.GameTile;

public partial class GameTile : AspectRatioContainer
{
    private RomMetadata? _metadata;
    private TextureRect? _nodeIcon;

    public override void _Ready()
    {
        _nodeIcon = GetNode<TextureRect>("%Icon");
    }

    public void Init(RomMetadata metadata)
    {
        _metadata = metadata;
    }

    /// <summary>
    /// Loads the icon image from <see cref="_metadata"/> and overwrites the
    /// previous image.
    /// </summary>
    /// <returns>True if the Icon was loaded</returns>
    public bool LoadIcon()
    {
        // Abort if we cannot load an icon
        if (_metadata?.Icon is null || _nodeIcon is null)
            return false;

        // Start loading
        try
        {
            // Load buffer as JPG image
            var image = new Image();
            if (image.LoadJpgFromBuffer(_metadata.Icon) != Error.Ok)
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
