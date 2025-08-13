using System.Text.Json;
using EmuNX.Core.Common.Types;
using Utils;
using TepEmulatorFamilyExtensions = EmuNX.Core.Configuration.TitleExecutionPetition.Types.TitleExecutionPetitionEmulatorFamilyExtensions;
using TepEmulatorFamily = EmuNX.Core.Configuration.TitleExecutionPetition.Types.TitleExecutionPetitionEmulatorFamily;
using TepUserPrompt = EmuNX.Core.Configuration.TitleExecutionPetition.Types.TitleExecutionPetitionUserPrompt;
using TepUserPromptExtensions = EmuNX.Core.Configuration.TitleExecutionPetition.Types.TitleExecutionPetitionUserPromptExtensions;

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
        RestartState();

        // Read file
        var jsonString = await EasyFile.ReadText(FilePath);
        if (jsonString == null) return TitleExecutionPetitionConfigError.FileReadError;

        // Open JSON abstraction
        using var document = JsonDocument.Parse(jsonString);
        var root = document.RootElement;
        
        // meta: Validate Version
        if (LoadMetaVersion(root) is { } error)
            return error;

        if (!VersionTarget.IsCompatibleWith(VersionCurrent))
            return TitleExecutionPetitionConfigError.MetaVersionNotCompatible;

        // data.emulators: Load JsonElement
        if (root.GetObject("emulators") is { } emulatorsElement)
        {
            // data.emulators.global
            if (emulatorsElement.GetObject("global") is { } globalElement)
                TepGlobal.Patch(
                    LoadTitleExecutionPetition(globalElement)
                );

            // data.emulators.families
            // TODO:
        }
        
        return null;
    }

    #region Load: Functions

    private TitleExecutionPetitionConfigError? LoadMetaVersion(JsonElement root)
    {
        // We need to have "meta.version" and it must be an array
        var versionArray = root
            .GetObject("meta")?
            .GetArray("version");

        if (versionArray is null)
            return TitleExecutionPetitionConfigError.MetaVersionNotFound;

        // Read each number from "meta.version"
        if (versionArray.Length < 2 ||
            !versionArray[0].TryGetUInt32(out var major) ||
            !versionArray[1].TryGetUInt32(out var minor))
            return TitleExecutionPetitionConfigError.MetaVersionNotFound;

        // Store the numbers in "VersionCurrent"
        VersionCurrent = new Version(major, minor);
        return null;
    }

    private TitleExecutionPetition LoadTitleExecutionPetition(JsonElement root)
    {
        var tep = new TitleExecutionPetition();

        var emulatorElement = root.GetObject("emulator");
        var userElement = root.GetObject("user");

        // emulation.family = EmulatorFamily ?? .Ask
        if (emulatorElement?.GetString("family") is { } family)
            tep.EmulatorFamily = TepEmulatorFamilyExtensions.FromString(family) ?? TepEmulatorFamily.Ask;

        // emulation.runner = string
        if (emulatorElement?.GetString("runner") is { } runner)
            tep.EmulatorRunner = runner;

        // user.prompt = UserPrompt ?? .Ask
        if (userElement?.GetString("prompt") is { } prompt)
            tep.UserPrompt = TepUserPromptExtensions.FromString(prompt) ?? TepUserPrompt.Ask;

        return tep;
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