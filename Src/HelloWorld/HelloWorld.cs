using Godot;

using LibHac;
using LibHac.Common;
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
        // Converts the select path into the user's OS format
        string globalizedPath = ProjectSettings.GlobalizePath(path);
        GD.Print(globalizedPath);

        // Prepares NSP reader
        using var file = new LocalStorage(globalizedPath, System.IO.FileAccess.Read);

        // Starts NSP reader
        using UniqueRef<PartitionFileSystem> pfs = new UniqueRef<PartitionFileSystem>();
        pfs.Reset(new PartitionFileSystem());
        Result result = pfs.Get.Initialize(file);

        GD.Print(result.IsSuccess() ? "NSP has been open!" : "Couldn't open NSP");
        if (!result.IsSuccess()) return;

        // Debug the NSP
        GD.Print("Listing contents of NSP...");
        IFileSystem fs = pfs.Get;
        foreach (DirectoryEntryEx entry in fs.EnumerateEntries())
        {
            GD.Print($"\t{entry.Name}");
        }
        GD.Print("Done!");
    }
}
