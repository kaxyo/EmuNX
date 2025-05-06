using System;
using Godot;

using LibHac.FsSystem;

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
    }
}
