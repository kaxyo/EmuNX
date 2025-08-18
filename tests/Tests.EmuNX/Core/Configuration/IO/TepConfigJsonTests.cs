using EmuNX.Core.Common.Types;
using EmuNX.Core.Configuration.TitleExecutionPetition;
using EmuNX.Core.Configuration.TitleExecutionPetition.IO;
using EmuNX.Core.Configuration.TitleExecutionPetition.Types;
using Version = EmuNX.Core.Configuration.Version;

namespace Tests.EmuNX.Core.Configuration.IO;

public class TepConfigJsonTests
{
    [Theory]
    [InlineData("file_not_found", TepConfigError.FileReadError)]
    [InlineData("title_execution.err.1.meta_version.not_found", TepConfigError.MetaVersionNotFound)]
    [InlineData("title_execution.err.2.meta_version.not_an_array", TepConfigError.MetaVersionNotFound)]
    [InlineData("title_execution.err.3.meta_version.invalid_array", TepConfigError.MetaVersionNotFound)]
    [InlineData("title_execution.err.4.meta_version.invalid_numbers", TepConfigError.MetaVersionNotFound)]
    [InlineData("title_execution.err.5.meta_version.not_compatible", TepConfigError.MetaVersionNotCompatible)]
    public async Task Load_ShouldReturnExpectedError(string fileName, TepConfigError? expectedError)
    {
        // Arrange
        var config = new TepConfigJson($"data/configuration/{fileName}.json");

        // Act
        var result = await config.Load();

        // Assert
        Assert.Equal(expectedError, result);
    }

    [Fact]
    public async Task Load_IsValid()
    {
        // Arrange
        var config = new TepConfigJson("data/configuration/title_execution.ok.json");

        // Act
        var result = await config.Load();

        // Assert: Errors
        Assert.Null(result);

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
}