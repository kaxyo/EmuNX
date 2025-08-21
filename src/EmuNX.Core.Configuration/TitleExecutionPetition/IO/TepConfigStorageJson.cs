using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using EmuNX.Core.Common.Monad;
using EmuNX.Core.Common.Types;
using EmuNX.Core.Configuration.TitleExecutionPetition.Types;
using Utils;

namespace EmuNX.Core.Configuration.TitleExecutionPetition.IO;

using ResultLoad = Result<TepConfig, LoadError>;

/// <summary>
/// Implements <see cref="TepConfig"/> to manage the configuration inside a <b>JSON</b> file.
/// To change the <b>file path</b> after instantiation, please change <see cref="FilePath"/>.
/// </summary>
public class TepConfigStorageJson(string filePath) : ITepConfigStorage
{
    public Version VersionRequired { get; } = new(1, 0);

    public string FilePath = filePath;

    public ResultLoad Load()
    {
        // Read file
        if (EasyFile.ReadText(FilePath) is not {} jsonString)
            return ResultLoad.Err(LoadError.ResourceAccessFailed);

        // Open JSON abstraction
        using var document = ParseJsonDocument(jsonString);
        if (document is null)
            return ResultLoad.Err(LoadError.ResourceDeserializationFailed);

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
    /// <summary>
    /// Reads a <c>jsonString</c> and tries to convert it into a <see cref="JsonDocument"/>,
    /// </summary>
    /// <param name="jsonString">The <c>string</c> that must be formatted in <b>json</b>.</param>
    /// <returns><see cref="JsonDocument"/> if the parsing succeeds, <c>null</c> if it fails.</returns>
    private static JsonDocument? ParseJsonDocument(string jsonString)
    {
        try
        {
            return JsonDocument.Parse(jsonString);
        }
        catch
        {
            return null;
        }
    }

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

    private static TitleExecutionPetition DeserializeTitleExecutionPetition(JsonElement root)
    {
        var tep = new TitleExecutionPetition();

        var emulatorElement = root.GetObject("emulator");
        var userElement = root.GetObject("user");

        // emulation.family = EmulatorFamily ?? .Ask
        if (emulatorElement?.GetString("family") is { } family)
            tep.EmulatorFamily = JsonStringToTepEmulatorFamily(family);

        // emulation.runner = string
        if (emulatorElement?.GetString("runner") is { } runner)
            tep.EmulatorRunner = runner;

        // user.prompt = UserPrompt ?? .Ask
        if (userElement?.GetString("prompt") is { } prompt)
            tep.UserPrompt = JsonStringToTepUserPrompt(prompt);

        return tep;
    }

    private static void LoadTepFromEmulatorsFamily(JsonElement root, EmulatorFamily family, TepConfig config)
    {
        // data.emulators.families.<family>
        var familyString = family switch
        {
            EmulatorFamily.Yuzu => "yuzu",
            EmulatorFamily.Ryujinx => "ryujinx",
            _ => "yuzu"
        };

        if (root.GetObject(familyString) is not { } familyElement)
            return;

        var tep = DeserializeTitleExecutionPetition(familyElement);
        tep.EmulatorFamily = family.ToTitleExecutionPetitionEmulatorFamily();
        config.TepEmulatorFamilies[family] = tep;
    }

    private static void LoadTepFromTitles(JsonElement root, TepConfig config)
    {
        foreach (var titleElement in root.EnumerateObject())
        {
            if (TitleId.TryParseHex(titleElement.Name, out var titleId))
                config.TepTitles[titleId] = DeserializeTitleExecutionPetition(titleElement.Value);
        }
    }

    /// <summary>
    /// Transforms n <c>string</c> into an <see cref="TepEmulatorFamily"/>.
    /// </summary>
    /// <param name="jsonString">The <c>string</c> that will be parsed into an <see cref="TepEmulatorFamily"/>.</param>
    /// <returns>An <see cref="TepEmulatorFamily"/>.</returns>
    private static TepEmulatorFamily JsonStringToTepEmulatorFamily(string jsonString)
    {
        return jsonString.ToLower() switch
        {
            "yuzu" => TepEmulatorFamily.Yuzu,
            "ryujinx" => TepEmulatorFamily.Ryujinx,
            // "ask" => TepEmulatorFamily.Ask,
            _ => TepEmulatorFamily.Ask
        };
    }

    /// <summary>
    /// Transforms n <c>string</c> into an <see cref="TepUserPrompt"/>.
    /// </summary>
    /// <param name="jsonString">The <c>string</c> that will be parsed into an <see cref="TepUserPrompt"/>.</param>
    /// <returns>An <see cref="TepUserPrompt"/>.</returns>
    private static TepUserPrompt JsonStringToTepUserPrompt(string jsonString)
    {
        return jsonString.ToLower() switch
        {
            "none" => TepUserPrompt.None,
            // "ask" => TepUserPrompt.Ask,
            _ => TepUserPrompt.Ask
        };
    }
    #endregion

    public SaveError? Save(TepConfig config)
    {
        // Generate JSON abstraction
        // TODO: Don't add keys with null values
        // TODO: Don't add objects with every key being null values
        var root = new JsonObject
        {
            ["meta"] = new JsonObject
            {
                ["version"] = new JsonArray { VersionRequired.Major, VersionRequired.Minor }
            },
            ["data"] = new JsonObject
            {
                ["emulators"] = new JsonObject
                {
                    ["global"] = SerializeTitleExecutionPetition(config.TepGlobal),
                    ["families"] = new JsonObject
                    {
                        ["yuzu"] = SerializeTitleExecutionPetition(config.TepEmulatorFamilies[EmulatorFamily.Yuzu]),
                        ["ryujinx"] = SerializeTitleExecutionPetition(config.TepEmulatorFamilies[EmulatorFamily.Ryujinx])
                    }
                },
                ["titles"] = new JsonObject(
                    config.TepTitles.Select(kv =>
                        new KeyValuePair<string, JsonNode?>(
                            kv.Key.Hex,
                            SerializeTitleExecutionPetition(kv.Value)
                        )
                    )
                )
            }
        };

        // Serialize it into FilePath
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
    
        root.WriteTo(writer);
        writer.Flush();
    
        var jsonString = Encoding.UTF8.GetString(stream.ToArray());

        if (EasyFile.WriteText(FilePath, jsonString) is false)
            return SaveError.ResourceWriteFailed;

        return null;
    }

    #region Save: Functions
    private static JsonObject? SerializeTitleExecutionPetition(TitleExecutionPetition tep)
    {
        if (tep.IsEmpty)
            return null;

        // Create object with null and non-null values
        var root = new JsonObject
        {
            ["emulator"] = new JsonObject
            {
                ["family"] = TepEmulatorFamilyToJsonString(tep.EmulatorFamily),
                ["runner"] = tep.EmulatorRunner
            },
            ["user"] = new JsonObject
            {
                ["prompt"] = TepUserPromptToJsonString(tep.UserPrompt)
            }
        };
        
        // Delete null objects/values

        // {emulator.family}: Delete "family" if it is null
        if (root.Go("emulator")?.IsNull("family") ?? false)
            root.Go("emulator")?.Remove("family");

        // {emulator.runner}: Delete "runner" if it is null
        if (root.Go("emulator")?.IsNull("runner") ?? false)
            root.Go("emulator")?.Remove("runner");

        // {user.prompt}: Delete "prompt" if it is null
        if (root.Go("user")?.IsNull("prompt") ?? false)
            root.Go("user")?.Remove("prompt");

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

file static class JsonExtensions
{
    /// <summary>
    /// Gets the <see cref="JsonObject"/> from <c>jsonNode</c> in a safe and one-liner way.
    /// </summary>
    /// <param name="jsonNode">The <see cref="JsonObject"/> that will be read to access <c>key's</c> <see cref="JsonObject"/>.</param>
    /// <param name="key">The name of the <b>key</b> to enter.</param>
    /// <returns>The <c>key's</c> <see cref="JsonObject"/> or null if it couldn't enter it.</returns>
    public static JsonObject? Go(this JsonNode jsonNode, string key) =>
        jsonNode[key]?.GetValueKind() is JsonValueKind.Object
            ? jsonNode[key]?.AsObject()
            : null;

    public static bool IsNull(this JsonObject? jsonObject, string key) =>
        jsonObject?[key] is null;
}