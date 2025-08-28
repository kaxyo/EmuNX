using EmuNX.Core.Common.Monad;
using EmuNX.Core.Common.Types;
using EmuNX.Core.Configuration.TitleExecutionPetition;
using EmuNX.Core.Configuration.TitleExecutionPetition.IO;
using EmuNX.Core.Configuration.TitleExecutionPetition.Types;
using Utils.Json;

namespace Tests.EmuNX.Core.Configuration.IO;

public class TepConfigStorageJsonTests
{
    [Theory]
    [InlineData("file_not_found", LoadError.ResourceAccessFailed)]
    [InlineData("title_execution.err.read.1.deserialization_failed", LoadError.ResourceDeserializationFailed)]
    [InlineData("title_execution.err.meta_version.1.not_found", LoadError.MetaVersionNotFound)]
    [InlineData("title_execution.err.meta_version.2.not_an_array", LoadError.MetaVersionNotFound)]
    [InlineData("title_execution.err.meta_version.3.invalid_array", LoadError.MetaVersionNotFound)]
    [InlineData("title_execution.err.meta_version.4.invalid_numbers", LoadError.MetaVersionNotFound)]
    [InlineData("title_execution.err.meta_version.5.not_compatible", LoadError.MetaVersionNotCompatible)]
    public void Load_ShouldReturnExpectedError(string fileName, LoadError expectedError)
    {
        // Arrange
        var configStorage = new TepConfigStorageJson(new JsonStorage($"data/configuration/{fileName}.json"));

        // Act
        var result = configStorage.Load();

        // Assert
        Assert.Equal(Result<TepConfig, LoadError>.Err(expectedError), result);
    }

    [Fact]
    public void Load_IsValid()
    {
        // Arrange
        var configStorage = new TepConfigStorageJson(new JsonStorage("data/configuration/title_execution.ok.trimmed.some_tep_fields_missing.json"));

        // Act
        var result = configStorage.Load();
        Assert.True(result.IsOk);
        var config = result.Success;

        // Assert: Emulators.Global
        Assert.Equal(config.TepGlobal, new TitleExecutionPetition(
            TepEmulatorFamily.Yuzu,
            null,
            TepUserPrompt.Ask
        ));

        // Assert: Emulators.Families
        Assert.Equal(config.TepEmulatorFamilies[EmulatorFamily.Yuzu], new TitleExecutionPetition(
            TepEmulatorFamily.Yuzu,
            "eden-0.0.2",
            TepUserPrompt.Ask
        ));

        Assert.Equal(config.TepEmulatorFamilies[EmulatorFamily.Ryujinx], new TitleExecutionPetition(
            TepEmulatorFamily.Ryujinx,
            "ryubing-1.3.2",
            TepUserPrompt.None
        ));

        // Assert: Titles
        Assert.Equal(config.TepTitles[new TitleId(0x0100F2C0115B6000)], new TitleExecutionPetition(
            null,
            null,
            TepUserPrompt.None
        ));

        Assert.Equal(config.TepTitles[new TitleId(0x010093C01F256000)], new TitleExecutionPetition(
            TepEmulatorFamily.Ryujinx,
            null,
            null
        ));
    }

    [Theory]
    [InlineData("title_execution.ok.trimmed.full")]
    [InlineData("title_execution.ok.trimmed.no_data")]
    [InlineData("title_execution.ok.trimmed.no_emulator_families")]
    [InlineData("title_execution.ok.trimmed.no_emulator_family_ryujinx")]
    [InlineData("title_execution.ok.trimmed.no_emulator_family_yuzu")]
    [InlineData("title_execution.ok.trimmed.no_emulator_global")]
    [InlineData("title_execution.ok.trimmed.no_emulators")]
    [InlineData("title_execution.ok.trimmed.no_titles")]
    [InlineData("title_execution.ok.trimmed.some_tep_fields_missing")]
    public void LoadAndSave_ShouldProduceEqualJsonAsSample(string fileName)
    {
        // Arrange
        var pathToLoad = $"data/configuration/{fileName}.json";
        var pathToSave = Path.Combine(Path.GetTempPath(), "title_execution.json");

        var jsonStorage = new JsonStorage(pathToLoad, storeLastJsonNodeLoaded: true);
        var configStorage = new TepConfigStorageJson(jsonStorage);

        // Load JSON from sample
        jsonStorage.FilePath = pathToLoad;
        var resultLoadSample = configStorage.Load();
        Assert.True(resultLoadSample.IsOk);
        var tepConfigLoadedSample = resultLoadSample.Success;
        var jsonNodeLoadedSample = jsonStorage.LastJsonNodeLoaded;

        // Save loaded JSON in temp
        jsonStorage.FilePath = pathToSave;
        var wasTepConfigLoadedSampleSavedInTemp = configStorage.Save(tepConfigLoadedSample) is null;
        Assert.True(wasTepConfigLoadedSampleSavedInTemp);

        // Load JSON from temp
        var resultLoadTemp = configStorage.Load();
        Assert.True(resultLoadTemp.IsOk);
        var tepConfigLoadedTemp = resultLoadTemp.Success;
        var jsonNodeLoadedTemp = jsonStorage.LastJsonNodeLoaded;

        // Assert that both JSON contents are the same
        TestsUtils.CompareJsonNodes(jsonNodeLoadedSample, jsonNodeLoadedTemp);
        Assert.Equal(tepConfigLoadedSample, tepConfigLoadedTemp);
    }
}