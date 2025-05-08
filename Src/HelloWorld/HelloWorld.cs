using Godot;

using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;

public partial class HelloWorld : Control
{
    private FileDialog fileDialog;

    public override void _Ready()
    {
        GD.Print("Hello from C# to Godot :)");

        fileDialog = GetNode<FileDialog>("FileDialog");
        fileDialog.FileSelected += OnFileSelected;
        fileDialog.Popup();
    }

    /// <summary>
    /// Uses logic from https://github.com/Thealexbarney/LibHac/blob/master/src/hactoolnet/ProcessPfs.cs
    /// </summary>
    /// <param name="path">The user's selected path</param>
    private void OnFileSelected(string path)
    {
        Result result;

        // Converts the select path into the user's OS format
        string globalizedPath = ProjectSettings.GlobalizePath(path);
        GD.Print(globalizedPath);

        /* Read NSP*/
        // Prepares NSP reader
        using var file = new LocalStorage(globalizedPath, System.IO.FileAccess.Read);

        // Starts NSP reader
        using UniqueRef<PartitionFileSystem> pfs = new UniqueRef<PartitionFileSystem>();
        pfs.Reset(new PartitionFileSystem());
        result = pfs.Get.Initialize(file);

        GD.Print(result.IsSuccess() ? "NSP has been open!" : "Couldn't open NSP");
        if (!result.IsSuccess()) return;

        // Search CNMT inside NSP
        GD.Print("Searching for CNMT inside NSP...");
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
            GD.Print(line);

        }
        GD.Print("Done!");

        GD.Print(cnmtNcaEntry == null ? "No CNTM was found..." : "CNTM was found!");
        if (cnmtNcaEntry == null) return;

        /* Read CNMT */
        // Prepares CNMT reader
        using var cnmtNcaFile = new UniqueRef<IFile>();
        result = fs.OpenFile(ref cnmtNcaFile.Ref, (U8Span)cnmtNcaEntry.FullPath, OpenMode.All);

        GD.Print(result.IsSuccess() ? "CNMT has been open!" : "Couldn't open CNMT");
        if (!result.IsSuccess()) return;
    }
}
