using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using LibHac;
using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using LibHac.Tools.Ncm;
using LibHac.Util;
using ContentType = LibHac.Ncm.ContentType;

public partial class HelloWorld : Control
{
    private RichTextLabel richTextLabel;

    public override void _Ready()
    {
        richTextLabel = GetNode<RichTextLabel>("RichTextLabel");
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

            Log($"[color={(success ? "green]" : "red]")}Process ended with {(success ? "success" : "failure")}[/color]");
        }
        catch (Exception e)
        {
            Log("[color=red]Process ended because of the following exception:[/color]");
            Log($"[color=maroon]{e}[/color]");
        }
    }

    /// <summary>
    /// Prints a message to the Godot console and UI console.
    /// </summary>
    /// <param name="message">The message to print</param>
    public void Log(string message)
    {
        GD.Print(message);
        richTextLabel.Text += message + "\n";
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

        /* Read NSP*/
        // Prepares NSP reader
        using var file = new LocalStorage(romPath, System.IO.FileAccess.Read);

        // Starts NSP reader
        using UniqueRef<PartitionFileSystem> pfs = new UniqueRef<PartitionFileSystem>();
        pfs.Reset(new PartitionFileSystem());
        result = pfs.Get.Initialize(file);

        Log(result.IsSuccess() ? "NSP has been open!" : "Couldn't open NSP");
        if (!result.IsSuccess()) return false;

        // Search CNMT inside NSP
        Log("Searching for CNMT inside NSP...");
        DirectoryEntryEx cnmtNcaEntry = null;
        IFileSystem fs = pfs.Get;
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
        var nca = new Nca(keyset, cnmtNcaFile.Get.AsStorage());
        IFileSystem ncaFs = nca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.ErrorOnInvalid);

        // Get the first CNMT file stream
        Log("Searching internal CNMT file...");
        U8Span cnmtPath = (U8Span)ncaFs.EnumerateEntries("/", "*.cnmt").Single().FullPath;
        using var cnmtFile = new UniqueRef<IFile>();
        result = ncaFs.OpenFile(ref cnmtFile.Ref, cnmtPath, OpenMode.Read);

        Log(result.IsSuccess() ? "CNMT has been open!" : "Couldn't open CNMT");
        if (!result.IsSuccess()) return false;

        // Parse CNMT from previously opened stream
        Log("Parsing CNMT...");
        var cnmt = new Cnmt(cnmtFile.Get.AsStream());

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

        return true;
    }
}
