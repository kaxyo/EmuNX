using Godot;

using System.Collections.Generic;
using System.IO;

using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using System.Linq;

public partial class HelloWorld : Control
{
    private RichTextLabel richTextLabel;

    public override void _Ready()
    {
        richTextLabel = GetNode<RichTextLabel>("RichTextLabel");
        Log("Hello from C# to Godot :)");

        // Load files used in testing
        string[] paths = LoadFilePaths(ProjectSettings.GlobalizePath("res://Target/locations.txt"));
        if (paths == null)
        {
            Log("Cannot continue without valid paths");
            return;
        }

        // Start ROM reading
        bool success = ReadRom(
            paths[0], // NSP|XCI
            paths[1], // PROD.KEYS
            paths[2]  // TITLE.KEYS
        );

        Log($"[color={(success ? "green]" : "red]")}Process ended with {(success ? "success" : "failure")}[/color]");
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

        Log(cnmtNcaEntry == null ? "No CNTM was found..." : "CNTM was found!");
        if (cnmtNcaEntry == null) return false;

        /* Read CNMT */
        // Prepares CNMT reader
        using var cnmtNcaFile = new UniqueRef<IFile>();
        result = fs.OpenFile(ref cnmtNcaFile.Ref, (U8Span)cnmtNcaEntry.FullPath, OpenMode.All);

        Log(result.IsSuccess() ? "CNMT has been open!" : "Couldn't open CNMT");
        if (!result.IsSuccess()) return false;

        return true;
    }
}
