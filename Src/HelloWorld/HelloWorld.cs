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

    private void OnFileSelected(string path)
    {
        string globalizedPath = ProjectSettings.GlobalizePath(path);
        GD.Print(globalizedPath);

        using var file = new LocalStorage(globalizedPath, System.IO.FileAccess.Read);

        IFileSystem fs = null;
        using UniqueRef<PartitionFileSystem> pfs = new UniqueRef<PartitionFileSystem>();
        using UniqueRef<Sha256PartitionFileSystem> hfs = new UniqueRef<Sha256PartitionFileSystem>();

        pfs.Reset(new PartitionFileSystem());
        Result result = pfs.Get.Initialize(file);

        GD.Print(result.IsSuccess() ? "NSP has been open!" : "Couldn't open NSP");
        if (!result.IsSuccess()) return;

        GD.Print("Listing contents of NSP...");
        fs = pfs.Get;
        foreach (DirectoryEntryEx entry in fs.EnumerateEntries())
        {
            GD.Print($"\t{entry.Name}");
        }
        GD.Print("Done!");
    }
}
