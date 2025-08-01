using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EmuNX.Core.RomMetadata.Parser;
using EmuNX.Core.RomMetadata.Types;
using Godot;
using LibHac.Settings;

public partial class HelloWorld : Control
{
    #region Godot
    private TextureRect textureRect;
    private RichTextLabel richTextLabel;
    #endregion

    #region Rom

    public RomMetadataParser romMetadataParser = new RomMetadataParser();
    public RomMetadata Rom = new RomMetadata();
    #endregion

    #region Godot
    public override void _Ready()
    {
        // Godot
        textureRect = GetNode<TextureRect>("TextureRect");
        richTextLabel = GetNode<RichTextLabel>("RichTextLabel");
        // Init and start Rom parsing
        var romMetadataParser = new RomMetadataParser();
        Main();
    }

    public override void _Input(InputEvent @event)
    {
        bool wantsToReload = @event is InputEventKey eventKey && eventKey.Pressed && !eventKey.Echo && eventKey.Keycode == Key.F5;
        if (wantsToReload) Main();
    }
    #endregion

    public void Main()
    {
        // Reset console
        GD.Print("Console cleared...");
        richTextLabel.Text = "";
        Log("[outline_color=#ffffff60][outline_size=6][rainbow freq=0.5 sat=0.8 val=0.8 speed=0.5][wave]Hello from EmuNX :)[/wave][/rainbow][/outline_size][/outline_color]");

        // Load files used in testing
        string[] paths = LoadFilePaths(ProjectSettings.GlobalizePath("res://.godot/locations.txt"));
        if (paths == null)
        {
            Log("Cannot continue without valid paths");
            return;
        }

        // Start ROM reading
        try
        {
            bool success = ReadRom(
                paths[0], // NSP|XCI
                paths[1] // PROD.KEYS
            );

            Log($"[color={(success ? "green]" : "red]")}ReadRom() ended with {(success ? "success" : "failure")}[/color]");
            if (!success) return;

            Log("Opening image");
            if (!LoadStreamToTextureRect(Rom.Icon, textureRect))
            {
                Log("Couldn't open");
            }

            Log("[color=purple]Process ended[/color]");
        }
        catch (Exception e)
        {
            Log("[color=red]Process ended because of the following exception:[/color]");
            Log($"[color=maroon]{e}[/color]");
        }
    }

    #region Utils
    /// <summary>
    /// Prints a message to the Godot console and UI console.
    /// </summary>
    /// <param name="message">The message to print</param>
    public void Log(string message)
    {
        GD.Print(message);
        richTextLabel.Text += message + "\n";
    }

    public bool TrueOnError(RomMetadataParserError? metadataParserError)
    {
        if (metadataParserError == null) return false;

        Log(metadataParserError.ToString());
        return true;
    }

    public bool LoadStreamToTextureRect(Stream stream, TextureRect textureRect)
    {
        // Convert bytes from stream to array
        byte[] buffer;
        using (MemoryStream ms = new MemoryStream())
        {
            stream.CopyTo(ms);
            buffer = ms.ToArray();
        }

        // Load image
        Image image = new Image();
        Error error = image.LoadJpgFromBuffer(buffer);
        if (error != Error.Ok) return false;

        // Update TextureRect
        textureRect.Texture = ImageTexture.CreateFromImage(image);
        return true;
    }

    /// <summary>
    /// Loads the file paths from the test.txt file.
    /// The file should contain three lines that are the paths to the following files:
    /// <list type="number">
    ///     <item>NSP|XCI</item>
    ///     <item>PROD.KEYS</item>
    ///     <item>TITLE.KEYS</item>
    /// </list>
    /// </summary>
    /// <param name="locationsPath">The path to the file to read</param>
    /// <returns>An array with the three paths or null if there is some error</returns>
    public string[] LoadFilePaths(string locationsPath)
    {
        Log($"Loading filePaths from: {locationsPath}");

        // Read raw paths from file
        if (!File.Exists(locationsPath))
        {
            Log($"File does not exist");
            return null;
        }

        string[] paths = File.ReadLines(locationsPath).ToArray();

        if (paths.Length != 2)
        {
            Log($"File does not have two lines");
            return null;
        }

        // Validate paths
        var typesAndPaths = new Dictionary<string, string>
        {
            { "ROM", paths[0] },
            { "PROD.KEYS", paths[1] },
        };

        int lineNumber = 1;

        foreach (var typeAndPath in typesAndPaths)
        {
            string type = typeAndPath.Key;
            string path = typeAndPath.Value;

            bool pathExists = File.Exists(path);

            Log($"Line {lineNumber}: {type} {(pathExists ? "was" : "couldn't be")} found on {path}");

            if (!pathExists) return null;

            lineNumber++;
        }

        // Return paths
        return paths;
    }
    #endregion

    #region Rom
    /// <summary>
    /// Uses logic from https://github.com/Thealexbarney/LibHac/blob/master/src/hactoolnet/ProcessPfs.cs
    /// </summary>
    /// <param name="romPath">Software path</param>
    /// <param name="prodKeysPath">prod.keys path</param>
    /// <returns>True or false depending on success</returns>
    private bool ReadRom(string romPath, string prodKeysPath)
    {
        if (romMetadataParser.CanLoadRootFsFromRom())
        {
            Log("Keys already loaded...");
        }
        else
        {
            Log("Loading keys...");
            var keysError = romMetadataParser.LoadKeys(prodKeysPath);
            if (keysError != null)
            {
                Log(keysError.ToString());
                return false;
            }
        }

        Log("Reading rom...");
        if (TrueOnError(romMetadataParser.LoadRootFsFromRom(romPath))) return false;

        Rom = romMetadataParser.RomMetadata;
        if (TrueOnError(romMetadataParser.LoadCnmt())) return false;
        romMetadataParser.ReadId();
        Log($"[color=peru]ID:[/color] [color=green]{Rom.Id.Hex}[/color]");

        if (TrueOnError(romMetadataParser.LoadControlNca())) return false;

        if (TrueOnError(romMetadataParser.LoadNacp())) return false;
        romMetadataParser.ReadName();
        Log($"[color=peru]Name:[/color] [color=green]{Rom.Name}[/color]");

        romMetadataParser.ReadIcon();
        Log($"[color=peru]Icon:[/color] [color={(Rom.Icon == null ? "red]no" : "green]yes")}[/color]");

        return true;
    }
    #endregion
}

