using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Godot;
using LibHac;
using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Ns;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using LibHac.Tools.Ncm;
using LibHac.Util;
using ContentType = LibHac.Ncm.ContentType;

public partial class HelloWorld : Control
{
    #region Godot
    private TextureRect textureRect;
    private RichTextLabel richTextLabel;
    #endregion

    #region Rom
    private string titleId = "???";
    private Stream titleIcon;
    private string titlename = "???";
    #endregion

    #region Godot
    public override void _Ready()
    {
        textureRect = GetNode<TextureRect>("TextureRect");
        richTextLabel = GetNode<RichTextLabel>("RichTextLabel");
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
        string[] paths = LoadFilePaths(ProjectSettings.GlobalizePath("res://Target/locations.txt"));
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
                paths[1], // PROD.KEYS
                paths[2]  // TITLE.KEYS
            );

            Log($"[color={(success ? "green]" : "red]")}ReadRom() ended with {(success ? "success" : "failure")}[/color]");
            if (!success) return;

            Log("Opening image");
            if (!LoadStreamToTextureRect(titleIcon, textureRect))
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

        if (paths.Length != 3)
        {
            Log($"File does not have three lines");
            return null;
        }

        // Validate paths
        var typesAndPaths = new Dictionary<string, string>
        {
            { "ROM", paths[0] },
            { "PROD.KEYS", paths[1] },
            { "TITLE.KEYS", paths[2] }
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
    /// <param name="titleKeysPath">title.keys path</param>
    /// <returns>True or false depending on success</returns>
    private bool ReadRom(string romPath, string prodKeysPath, string titleKeysPath)
    {
        Result result;

        /* Read keys*/
        //Load titlekeys
        Log("Loading prod.keys and title.keys...");
        KeySet keyset = new KeySet();
        ExternalKeyReader.ReadKeyFile(keyset, prodKeysPath, titleKeysPath, null, null);

        /* Read ROM file */
        // Read extension
        string romExtension = romPath.Split(".").Last();

        // Check extension
        switch (romExtension)
        {
            case "nsp":
                Log("This rom will be read as NSP");
                break;

            case "xci":
                Log("This rom will be read as XCI");
                break;

            default:
                Log("This file doesn't have NSP or XCI extension");
                return false;
        }

        // Load file
        using var file = new LocalStorage(romPath, System.IO.FileAccess.Read);

        // Get fs
        IFileSystem fs = null;

        switch (romExtension)
        {
            case "nsp":
                fs = OpenNspFileSystem(file, keyset);
                break;

            case "xci":
                fs = OpenXciFileSystem(file, keyset);
                break;
        }

        if (fs == null)
        {
            Log("Couldn't open FileSystem");
            return false;
        }

        /* Search CNMT */
        // Search CNMT inside ROM
        Log("Searching for CNMT inside ROM...");
        DirectoryEntryEx cnmtNcaEntry = null;
        foreach (DirectoryEntryEx entry in fs.EnumerateEntries()) // 👀 fs.EnumerateEntries("*.nca", SearchOptions.Default).Any()
        {
            string line = $"\t{entry.Name}";
            if (entry.Name.EndsWith(".cnmt.nca"))
            {
                cnmtNcaEntry = entry;
                line += " 👈";
            }
            ;
            Log(line);

        }
        Log("Done!");

        Log(cnmtNcaEntry == null ? "No CNTM.NCA was found..." : "CNTM.NCA was found!");
        if (cnmtNcaEntry == null) return false;

        /* Read CNMT */
        // Prepares CNMT NCA reader
        Log("Opening CNMT.NCA file...");
        using var cnmtNcaFile = new UniqueRef<IFile>();
        result = fs.OpenFile(ref cnmtNcaFile.Ref, (U8Span)cnmtNcaEntry.FullPath, OpenMode.All);

        Log(result.IsSuccess() ? "CNMT.NCA has been open!" : "Couldn't open CNMT.NCA");
        if (!result.IsSuccess()) return false;

        // Open CNMT NCA FyleSystem
        Log("Parsing CNMT.NCA FileSystem...");
        var cnmtNca = new Nca(keyset, cnmtNcaFile.Get.AsStorage());
        IFileSystem cnmtNcaFs = cnmtNca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.ErrorOnInvalid);

        // Get the first CNMT file stream
        Log("Searching internal CNMT file...");
        U8Span cnmtPath = (U8Span)cnmtNcaFs.EnumerateEntries("/", "*.cnmt").Single().FullPath;
        using var cnmtFile = new UniqueRef<IFile>();
        result = cnmtNcaFs.OpenFile(ref cnmtFile.Ref, cnmtPath, OpenMode.Read);

        Log(result.IsSuccess() ? "CNMT has been open!" : "Couldn't open CNMT");
        if (!result.IsSuccess()) return false;

        // Parse CNMT from previously opened stream
        Log("Parsing CNMT...");
        var cnmt = new Cnmt(cnmtFile.Get.AsStream());

        titleId = $"{cnmt.ApplicationTitleId:X}";
        Log($"[color=yellow]ApplicationTitleId: {cnmt.ApplicationTitleId:X}[/color]");

        // Search control entry
        CnmtContentEntry control = cnmt.ContentEntries.FirstOrDefault(e => e.Type == ContentType.Control);

        Log(control == null ? "Control entry was not found" : "Control entry was found!");
        if (control == null) return false;

        /* Read CONTROL NCA */
        // Generate CONTROL NCA path
        string controlNcaPath = $"{control.NcaId.ToHexString().ToLower()}.nca";

        // Search CONTROL NCA
        Log($"Searching for {controlNcaPath} inside NSP...");
        DirectoryEntryEx controlNcaEntry = fs.EnumerateEntries().FirstOrDefault(e => e.Name == controlNcaPath);

        Log(controlNcaEntry == null ? "No CONTROL NCA was found..." : "CONTROL NCA was found!");
        if (controlNcaEntry == null) return false;

        // Prepares CONTROL NCA reader
        Log("Opening CONTROL.NCA file...");
        using var controlNcaFile = new UniqueRef<IFile>();
        result = fs.OpenFile(ref controlNcaFile.Ref, (U8Span)controlNcaEntry.FullPath, OpenMode.All);

        Log(result.IsSuccess() ? "CONTROL.NCA has been open!" : "Couldn't open CONTROL.NCA");
        if (!result.IsSuccess()) return false;

        // Open CNMT NCA FyleSystem
        Log("Parsing CONTROL.NCA FileSystem...");
        var controlNca = new Nca(keyset, controlNcaFile.Get.AsStorage());
        IFileSystem controlNcaFs = controlNca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.ErrorOnInvalid);

        Log("[color=yellow]Listing CONTROL.NCA entries...[/color]");
        foreach (var entry in controlNcaFs.EnumerateEntries("/", "*"))
        {
            Log($"\t[color=peru]{entry.FullPath}[/color]");
        }

        /* Parse icon */
        // Open file
        using var iconFile = new UniqueRef<IFile>();
        result = controlNcaFs.OpenFile(ref iconFile.Ref, (U8Span)"/icon_AmericanEnglish.dat", OpenMode.Read);

        Log(result.IsSuccess() ? "ICON has been open!" : "Couldn't open ICON");
        if (!result.IsSuccess()) return false;

        // Copy stream
        Log("Copying ICON stream for later use...");
        using (var tempStream = iconFile.Get.AsStream())
        {
            titleIcon = new MemoryStream();
            tempStream.CopyTo(titleIcon);
            titleIcon.Position = 0;
        }

        /* Parse CONTROL NCAP */
        Log("Opening control.nacp file...");
        using var nacpFile = new UniqueRef<IFile>();
        controlNcaFs.OpenFile(ref nacpFile.Ref, (U8Span)"/control.nacp", OpenMode.Read);

        Log(result.IsSuccess() ? "NACP has been open!" : "Couldn't open NACP");
        if (!result.IsSuccess()) return false;

        Log("Parsing control.nacp as ApplicationControlProperty...");

        const int nacpSize = 0x4000;
        byte[] nacpBuffer = new byte[nacpSize];

        using (var nacpStream = nacpFile.Get.AsStream())
        {
            int bytesRead = nacpStream.Read(nacpBuffer, 0, nacpSize);
            if (bytesRead != nacpSize)
            {
                Log($"Couldn't read first {nacpSize} bytes from control.nacp");
                return false;
            }

            GCHandle handle = GCHandle.Alloc(nacpBuffer, GCHandleType.Pinned);
            try
            {
                ApplicationControlProperty nacp = Marshal.PtrToStructure<ApplicationControlProperty>(handle.AddrOfPinnedObject());

                titlename = nacp.Title.Items[(int)ApplicationControlProperty.Language.AmericanEnglish].NameString.ToString();
                Log($"[color=yellow]Game title (AmericanEnglish): {titlename}[/color]");
            }
            catch (Exception)
            {
                Log($"Marshal failed to parse ApplicationControlProperty from bytes :(");
                return false;
            }
            finally
            {
                handle.Free();
            }

            return true;
        }
    }

    private IFileSystem OpenNspFileSystem(LocalStorage file, KeySet keyset)
    {
        using var pfs = new UniqueRef<PartitionFileSystem>();
        pfs.Reset(new PartitionFileSystem());
        Result result = pfs.Get.Initialize(file);

        bool success = result.IsSuccess();

        if (success)
        {
            return pfs.Release();
        }
        else
        {
            Log($"Couldn't open NSP");
            return null;
        }
    }

    private IFileSystem OpenXciFileSystem(LocalStorage file, KeySet keyset)
    {
        try
        {
            var xci = new Xci(keyset, file);
            return xci.OpenPartition(XciPartitionType.Secure);
        }
        catch (Exception ex)
        {
            Log($"Couldn't open XCI: {ex.Message}");
            return null;
        }
    }
    #endregion
}
