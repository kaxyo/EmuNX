using System.Text.Json;
using Utils;

namespace EmuNX.Core.Configuration.TitleExecutionPetition.IO;

/// <summary>
/// Implements <see cref="TitleExecutionPetitionConfig"/> to manage the configuration inside a <b>JSON</b> file.
/// To set the <b>file path</b>, please change 
/// </summary>
public class TitleExecutionPetitionConfigJson(string filePath) : TitleExecutionPetitionConfig
{
    public override Version VersionTarget { get; } = new(1, 0);

    #region Storage
    public string FilePath = filePath;

    public override async Task<TitleExecutionPetitionConfigError?> Load()
    {
        // Read file
        var jsonString = await EasyFile.ReadText(FilePath);
        if (jsonString == null) return TitleExecutionPetitionConfigError.FileReadError;

        // Open JSON abstraction
        using var document = JsonDocument.Parse(jsonString);
        var root = document.RootElement;
        
        // Validate Version
        if (LoadMetaVersion(root) is { } error)
            return error;

        if (!VersionTarget.IsCompatibleWith(VersionCurrent))
            return TitleExecutionPetitionConfigError.MetaVersionNotCompatible;

        return null;
    }

    #region Load: Functions

    private TitleExecutionPetitionConfigError? LoadMetaVersion(JsonElement root)
    {
        // Enter "meta" object
        if (!root.TryGetProperty("meta", out var metaElement) || metaElement.ValueKind != JsonValueKind.Object)
            return TitleExecutionPetitionConfigError.MetaVersionNotFound;

        // "meta.version" must be an array
        if (!metaElement.TryGetProperty("version", out var versionElement) || versionElement.ValueKind != JsonValueKind.Array)
            return TitleExecutionPetitionConfigError.MetaVersionNotFound;

        // Read each number from "meta.version"
        var versionArray = versionElement.EnumerateArray().ToArray();
        if (versionArray.Length < 2 ||
            !versionArray[0].TryGetUInt32(out var major) ||
            !versionArray[1].TryGetUInt32(out var minor))
            return TitleExecutionPetitionConfigError.MetaVersionNotFound;

        // Store the numbers in "VersionCurrent"
        VersionCurrent = new Version(major, minor);
        return null;
    }
    #endregion

    public override async Task<TitleExecutionPetitionConfigError?> Save()
    {
        var jsonString = "";
        throw new NotImplementedException();
        // Read file
        var success = await EasyFile.WriteText(FilePath, jsonString);
        if (!success) return TitleExecutionPetitionConfigError.Unknown;
    }

    #region Save: Functions
    #endregion
    #endregion
}