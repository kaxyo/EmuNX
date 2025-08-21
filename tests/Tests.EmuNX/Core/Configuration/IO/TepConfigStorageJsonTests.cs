using EmuNX.Core.Common.Monad;
using EmuNX.Core.Common.Types;
using EmuNX.Core.Configuration.TitleExecutionPetition;
using EmuNX.Core.Configuration.TitleExecutionPetition.IO;
using EmuNX.Core.Configuration.TitleExecutionPetition.Types;
using System.Text.Json;
using Utils;

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
        var configStorage = new TepConfigStorageJson($"data/configuration/{fileName}.json");

        // Act
        var result = configStorage.Load();

        // Assert
        Assert.Equal(Result<TepConfig, LoadError>.Err(expectedError), result);
    }

    [Fact]
    public void Load_IsValid()
    {
        // Arrange
        var configStorage = new TepConfigStorageJson("data/configuration/title_execution.ok.json");

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

    [Fact]
    public void LoadAndSave_ShouldProduceEqualJsonAsSource()
    {
        // Arrange
        var temporalNewJsonPath = Path.Combine(Path.GetTempPath(), "title_execution.json");

        // This should produce the same JSON
        var configStorage = new TepConfigStorageJson("data/configuration/title_execution.ok.json");
        var resultConfig = configStorage.Load();
        Assert.True(resultConfig.IsOk);
        configStorage.FilePath = temporalNewJsonPath;
        configStorage.Save(resultConfig.Success);

        // Act
        using var expectedDoc = JsonDocument.Parse(EasyFile.ReadText("data/configuration/title_execution.ok.json") ?? "{}");
        using var temporalDoc = JsonDocument.Parse(EasyFile.ReadText(temporalNewJsonPath) ?? "{}");

        // Assert
        Assert.Equal(expectedDoc.RootElement, temporalDoc.RootElement);
    }
}