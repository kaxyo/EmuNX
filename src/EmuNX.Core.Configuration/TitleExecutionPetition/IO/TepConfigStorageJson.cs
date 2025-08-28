using System.Text.Json;
using System.Text.Json.Nodes;
using EmuNX.Core.Common.Monad;
using EmuNX.Core.Common.Types;
using EmuNX.Core.Configuration.TitleExecutionPetition.Types;
using Utils.Json;

namespace EmuNX.Core.Configuration.TitleExecutionPetition.IO;

using ResultLoad = Result<TepConfig, LoadError>;

/// <summary>
/// Implements <see cref="TepConfig"/> to manage the configuration inside a <b>JSON</b> file.
/// To change the <b>file path</b> after instantiation, please change <see cref="FilePath"/>.
/// </summary>
public class TepConfigStorageJson(JsonStorage jsonStorage) : ITepConfigStorage
{
    public Version VersionRequired { get; } = new(1, 0);

    public ResultLoad Load()
    {
        // Read file
        var result = jsonStorage.Load();

        if (result.IsErr)
            return ResultLoad.Err(
                result.Error
                    ? LoadError.ResourceDeserializationFailed
                    : LoadError.ResourceAccessFailed
            );

        // Open JSON abstraction
        var root = result.Ok;

        // {meta}: Validate Version
        if (LoadAndValidateMetaVersion(root) is { } error)
            return ResultLoad.Err(error);

        // From here, every TitleExecutionPetition will be parsed
        // Missing entries or properties will be set to null
        var config = new TepConfig();
        var resultConfig = ResultLoad.Ok(config);

        // {data}: Load JsonElement with every TitleExecutionPetition
        if (root.Dig("data") is not { } dataElement)
            return resultConfig;

        // {data.emulators}
        if (dataElement.Dig("emulators") is { } emulatorsElement)
        {
            // {data.emulators.global}
            if (emulatorsElement.Dig("global") is { } globalElement)
                config.TepGlobal = DeserializeTitleExecutionPetition(globalElement);

            // {data.emulators.families}
            if (emulatorsElement.Dig("families") is { } familiesElement)
            {
                LoadTepFromEmulatorsFamily(familiesElement, EmulatorFamily.Yuzu, config);
                LoadTepFromEmulatorsFamily(familiesElement, EmulatorFamily.Ryujinx, config);
            }
        }

        // {data.titles}
        if (dataElement.Dig("titles") is { } titleElements)
            LoadTepFromTitles(titleElements, config);

        // End
        return resultConfig;
    }

    #region Load: Functions
    private LoadError? LoadAndValidateMetaVersion(JsonNode root)
    {
        // We need to have "meta.version" and it must be an array
        var versionArray = root.Dig("meta", "version")?.IntoArray();

        
        if (versionArray is null)
            return LoadError.MetaVersionNotFound;

        // Read each number from "meta.version"
        if (versionArray.Length < 2 
            || !versionArray[0].TryGetValue(out uint major)
            || !versionArray[1].TryGetValue(out uint minor))
            return LoadError.MetaVersionNotFound;

        // Validate the file's version with the parser's
        var version = new Version(major, minor);

        return VersionRequired.IsCompatibleWith(version)
            ? null
            : LoadError.MetaVersionNotCompatible;
    }

    private static TitleExecutionPetition DeserializeTitleExecutionPetition(JsonNode root)
    {
        var tep = new TitleExecutionPetition();

        // emulation.family = EmulatorFamily ?? .Ask
        if (root.Dig("emulator", "family").IntoValue<string>() is {} family)
            tep.EmulatorFamily = family.ToLower() switch
            {
                "yuzu" => TepEmulatorFamily.Yuzu,
                "ryujinx" => TepEmulatorFamily.Ryujinx,
                // "ask" => TepEmulatorFamily.Ask,
                _ => TepEmulatorFamily.Ask
            };

        // emulation.runner = string
        if (root.Dig("emulator", "runner").IntoValue<string>() is {} runner)
            tep.EmulatorRunner = runner;

        // user.prompt = UserPrompt ?? .Ask
        if (root.Dig("user", "prompt").IntoValue<string>() is {} prompt)
            tep.UserPrompt = prompt.ToLower() switch
            {
                "none" => TepUserPrompt.None,
                // "ask" => TepUserPrompt.Ask,
                _ => TepUserPrompt.Ask
            };

        return tep;
    }

    private static void LoadTepFromEmulatorsFamily(JsonNode root, EmulatorFamily family, TepConfig config)
    {
        // data.emulators.families.<family>
        var familyString = family switch
        {
            EmulatorFamily.Yuzu => "yuzu",
            EmulatorFamily.Ryujinx => "ryujinx",
            _ => "yuzu"
        };

        if (root.Dig(familyString) is not { } familyElement)
            return;

        var tep = DeserializeTitleExecutionPetition(familyElement);
        tep.EmulatorFamily = family.ToTitleExecutionPetitionEmulatorFamily();
        config.TepEmulatorFamilies[family] = tep;
    }

    private static void LoadTepFromTitles(JsonNode root, TepConfig config)
    {
        if (root.IntoObject() is not { } obj)
            return;

        foreach (var titleElement in obj)
        {
            if (TitleId.TryParseHex(titleElement.Key, out var titleId) && titleElement.Value is not null)
                config.TepTitles[titleId] = DeserializeTitleExecutionPetition(titleElement.Value);
        }
    }
    #endregion

    public SaveError? Save(TepConfig config)
    {
        // Generate JSON abstraction using Set method for automatic cleanup
        var root = new JsonObject()
            .Set("meta", new JsonObject()
                .Set("version", new JsonArray { VersionRequired.Major, VersionRequired.Minor }))
            .Set("data", new JsonObject()
                .Set("emulators", new JsonObject()
                    .Set("global", SerializeTitleExecutionPetition(config.TepGlobal))
                    .Set("families", BuildEmulatorFamiliesObject(config)))
                .Set("titles", BuildTitlesObject(config.TepTitles)));

        // Serialize it into FilePath
        return jsonStorage.Save(root).IsOk ? null : SaveError.ResourceWriteFailed;
    }

    #region Save: Functions
    private static JsonObject BuildEmulatorFamiliesObject(TepConfig config)
    {
        var families = new JsonObject();

        InjectEmulatorFamily("yuzu", EmulatorFamily.Yuzu);
        InjectEmulatorFamily("ryujinx", EmulatorFamily.Ryujinx);

        return families;

        void InjectEmulatorFamily(string key, EmulatorFamily family)
        {
            if (config.TepEmulatorFamilies.TryGetValue(family, out var familyTep))
                families.Set(key, SerializeTitleExecutionPetition(familyTep.Clone().NullifyEmulatorFamily()));
        }
    }

    private static JsonObject BuildTitlesObject(Dictionary<TitleId, TitleExecutionPetition> tepTitles)
    {
        var titlesObj = new JsonObject();

        foreach (var kv in tepTitles)
            titlesObj.Set(kv.Key.Hex, SerializeTitleExecutionPetition(kv.Value));

        return titlesObj;
    }

    private static JsonObject? SerializeTitleExecutionPetition(TitleExecutionPetition tep)
    {
        if (tep.IsEmpty)
            return null;

        var root = new JsonObject()
            .Set("emulator", new JsonObject()
                .Set("family", TepEmulatorFamilyToJsonString(tep.EmulatorFamily))
                .Set("runner", tep.EmulatorRunner))
            .Set("user", new JsonObject()
                .Set("prompt", TepUserPromptToJsonString(tep.UserPrompt)));

        return root;
    }

    /// <summary>
    /// Transforms an <see cref="TepEmulatorFamily"/> into a <c>string</c> with the required format for <c>title_execution.json</c>.
    /// </summary>
    /// <param name="family">The <see cref="TepEmulatorFamily"/> to transform into <c>string</c>.</param>
    /// <returns>A <c>string</c> that represents a <see cref="TepEmulatorFamily"/> with the required format for <c>title_execution.json</c>.</returns>
    private static string? TepEmulatorFamilyToJsonString(TepEmulatorFamily? family)
    {
        return family switch
        {
            TepEmulatorFamily.Yuzu => "yuzu",
            TepEmulatorFamily.Ryujinx => "ryujinx",
            TepEmulatorFamily.Ask => "ask",
            _ => null
        };
    }

    /// <summary>
    /// Transforms an <see cref="TepUserPrompt"/> into a <c>string</c> with the required format for <c>title_execution.json</c>.
    /// </summary>
    /// <param name="prompt">The <see cref="TepUserPrompt"/> to transform into <c>string</c>.</param>
    /// <returns>A <c>string</c> that represents a <see cref="TepUserPrompt"/> with the required format for <c>title_execution.json</c>.</returns>
    private static string? TepUserPromptToJsonString(TepUserPrompt? prompt)
    {
        return prompt switch
        {
            TepUserPrompt.Ask => "ask",
            TepUserPrompt.None => "none",
            _ => null
        };
    }
    #endregion
}

/// <summary>
/// Extensions that helps <b>serialize</b> and <b>deserialize</b> <see cref="TitleExecutionPetition"/>.
/// </summary>
file static class TitleExecutionPetitionStorageExtensions
{
    /// <summary>
    /// Sets to <c>null</c> its <see cref="TitleExecutionPetition.EmulatorFamily"/>. It's meant to be used when
    /// <b>serializing</b> the <see cref="TitleExecutionPetition"/> for each <b>emulator family</b>, because while they
    /// are stored without defining its <b>emulator family</b> because it's redundant; The moment
    /// they <b>deserialized</b> the <see cref="TepConfigStorageJson.LoadTepFromEmulatorsFamily"/> injects
    /// <see cref="TepEmulatorFamily"/>.
    /// </summary>
    /// <param name="tep">The <see cref="TitleExecutionPetition"/> that we want to <b>nullify</b> its <see cref="TitleExecutionPetition.EmulatorFamily"/>.</param>
    /// <returns>The same <see cref="TitleExecutionPetition"/> as input.</returns>
    public static TitleExecutionPetition NullifyEmulatorFamily(this TitleExecutionPetition tep)
    {
        tep.EmulatorFamily = null;
        return tep;
    }
}