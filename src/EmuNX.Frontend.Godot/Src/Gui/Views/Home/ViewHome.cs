using System.Collections.Generic;
using System.IO;
using System.Linq;
using EmuNX.Core.RomMetadata.Parser;
using EmuNX.Frontend.Godot.Gui.Components.GameTile;
using Godot;

namespace EmuNX.Frontend.Godot.Gui.Views.Home;

public partial class ViewHome : Control
{
	[Export]
	public PackedScene SceneGameTile { get; set; }
	private HBoxContainer _nodeGameRow;
	
	public override void _Ready()
	{
		// Load and validate godot stuff
		_nodeGameRow = GetNode<HBoxContainer>("%GameRow");

		if (SceneGameTile == null)
		{
			GD.Print("SceneGameTile is null");
			return;
		}

		// Load paths from file
		string locationsFilePath = ProjectSettings.GlobalizePath("res://.godot/locations.txt");
		string[] paths = LoadFilePaths(locationsFilePath);
		if (paths == null)
		{
			GD.Print($"{locationsFilePath} is wrong");
			return;
		}

		// Initialize rom parser
		var rmp = new RomMetadataParser();
		rmp.LoadKeys(paths[1]);

		// Get roms paths
		string[] romsPaths = Directory.GetFiles(paths[0]);
		var hello = new List<string>();
		foreach (var romPath in romsPaths)
		{
			// Parse rom
			GD.Print($"Loading \"{romPath}\"...");
			var error = rmp.LoadAndReadEverythingFromRom(romPath);
			bool parsingSuccess = error == null;
			GD.Print(parsingSuccess ? "Parsed succesfully!" : $"Parsing failed with {error}");
			if (!parsingSuccess) continue;
			// Instantiaate
			var gameTile = (GameTile)SceneGameTile.Instantiate();
			gameTile.Init(rmp.RomMetadata);
			hello.Add(rmp.RomMetadata.Id.Hex);
			_nodeGameRow.AddChild(gameTile);
			gameTile.LoadIcon();
		}

		foreach (var aaa in hello)
		{
			GD.Print(aaa);
		}
	}

	#region Temporal
	/// <summary>
	/// Loads the file paths from the locations.txt file.
	/// The file should contain three lines that are the paths to the following
	/// files/folder:
	/// <list type="number">
	///     <item>ROMS_FOLDER</item>
	///     <item>PROD.KEYS</item>
	/// </list>
	/// </summary>
	/// <param name="locationsPath">The path to the file to read</param>
	/// <returns>An array with the two paths or null if there is some error</returns>
	public string[] LoadFilePaths(string locationsPath)
	{
		GD.Print($"Loading filePaths from: {locationsPath}");

		// Read raw paths from file
		if (!File.Exists(locationsPath))
		{
			GD.Print($"File does not exist");
			return null;
		}

		string[] paths = File.ReadLines(locationsPath).ToArray();

		if (paths.Length != 2)
		{
			GD.Print($"File does not have two lines");
			return null;
		}

		// If the first line is a file (likely for testing HelloWorld) get the parent
		if (File.Exists(paths[0]))
		{
			string parentDir = Path.GetDirectoryName(paths[0]);
			if (!string.IsNullOrEmpty(parentDir)) paths[0] = parentDir;
		}

		// Validate paths
		bool firstLineIsRomsFolder = Directory.Exists(paths[0]);
		GD.Print($"Line 1: ROMS_FOLDER {(firstLineIsRomsFolder ? "was" : "couldn't be")} found on {paths[0]}");
		if (!firstLineIsRomsFolder) return null;
		
		bool secondLineIsProdKeys = File.Exists(paths[1]);
		GD.Print($"Line 2: PROD.KEYS {(secondLineIsProdKeys ? "was" : "couldn't be")} found on {paths[1]}");
		if (!secondLineIsProdKeys) return null;

		// Return paths
		return paths;
	}
	#endregion
}
