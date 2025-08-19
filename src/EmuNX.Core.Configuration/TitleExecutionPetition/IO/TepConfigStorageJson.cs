using System.Text.Json;
using EmuNX.Core.Common.Monad;
using EmuNX.Core.Common.Types;
using EmuNX.Core.Configuration.TitleExecutionPetition.Types;
using Utils;
using TepEmulatorFamilyExtensions = EmuNX.Core.Configuration.TitleExecutionPetition.Types.TitleExecutionPetitionEmulatorFamilyExtensions;
using TepUserPromptExtensions = EmuNX.Core.Configuration.TitleExecutionPetition.Types.TitleExecutionPetitionUserPromptExtensions;

namespace EmuNX.Core.Configuration.TitleExecutionPetition.IO;

using ResultLoad = Result<TepConfig, LoadError>;

/// <summary>
/// Implements <see cref="TepConfig"/> to manage the configuration inside a <b>JSON</b> file.
/// To change the <b>file path</b> after instantiation, please change <see cref="FilePath"/>.
/// </summary>
public class TepConfigStorageJson(string filePath) : ITepConfigStorage
{
    public Version VersionRequired { get; } = new(1, 0);

    #region Storage
    public string FilePath = filePath;

    public Result<TepConfig, LoadError> Load()
    {
        // Read file
        var jsonString = EasyFile.ReadText(FilePath);
        if (jsonString == null) return ResultLoad.Err(LoadError.ResourceAccessFailed);

        // Open JSON abstraction
        // TODO: Handle exception
        using var document = JsonDocument.Parse(jsonString);
        var root = document.RootElement;

        // {meta}: Validate Version
        if (LoadAndValidateMetaVersion(root) is { } error)
            return ResultLoad.Err(error);

        // From here, every TitleExecutionPetition will be parsed
        // Missing entries or properties will be set to null
        var config = new TepConfig();
        var resultConfig = ResultLoad.Ok(config);

        // {data}: Load JsonElement with every TitleExecutionPetition
        if (root.GetObject("data") is not { } dataElement)
            return resultConfig;

        // {data.emulators}
        if (dataElement.GetObject("emulators") is { } emulatorsElement)
        {
            // {data.emulators.global}
            if (emulatorsElement.GetObject("global") is { } globalElement)
                config.TepGlobal = DeserializeTitleExecutionPetition(globalElement);

            // {data.emulators.families}
            if (emulatorsElement.GetObject("families") is { } familiesElement)
            {
                LoadTepFromEmulatorsFamily(familiesElement, EmulatorFamily.Yuzu, config);
                LoadTepFromEmulatorsFamily(familiesElement, EmulatorFamily.Ryujinx, config);
            }
        }

        // {data.titles}
        if (dataElement.GetObject("titles") is { } titleElements)
            LoadTepFromTitles(titleElements, config);

        // End
        return resultConfig;
    }

    #region Load: Functions
    private LoadError? LoadAndValidateMetaVersion(JsonElement root)
    {
        // We need to have "meta.version" and it must be an array
        var versionArray = root
            .GetObject("meta")?
            .GetArray("version");

        if (versionArray is null)
            return LoadError.MetaVersionNotFound;

        // Read each number from "meta.version"
        if (versionArray.Length < 2 ||
            !versionArray[0].TryGetUInt32(out var major) ||
            !versionArray[1].TryGetUInt32(out var minor))
            return LoadError.MetaVersionNotFound;

        // Validate the file's version with the parser's
        var version = new Version(major, minor);

        return VersionRequired.IsCompatibleWith(version)
            ? null
            : LoadError.MetaVersionNotCompatible;
    }

    private TitleExecutionPetition DeserializeTitleExecutionPetition(JsonElement root)
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

    private void LoadTepFromEmulatorsFamily(JsonElement root, EmulatorFamily family, TepConfig config)
    {
        // data.emulators.families.<family>
        if (root.GetObject(family.ToKeyString()) is { } familyElement)
        {
            var tep = DeserializeTitleExecutionPetition(familyElement);
            tep.EmulatorFamily = family.ToTitleExecutionPetitionEmulatorFamily();
            config.TepEmulatorFamilies[family] = tep;
        }
    }

    private void LoadTepFromTitles(JsonElement root, TepConfig config)
    {
        foreach (var titleElement in root.EnumerateObject())
        {
            if (TitleId.TryParseHex(titleElement.Name, out var titleId))
                config.TepTitles[titleId] = DeserializeTitleExecutionPetition(titleElement.Value);
        }
    }
    #endregion

    public SaveError? Save(TepConfig config)
    {
        var jsonString = "";

        // Save file
        if (EasyFile.WriteText(FilePath, jsonString) is false)
            return SaveError.ResourceWriteFailed;

        return null;
    }

    #region Save: Functions
    #endregion
    #endregion
}