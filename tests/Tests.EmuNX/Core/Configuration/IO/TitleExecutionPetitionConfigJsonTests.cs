using EmuNX.Core.Configuration.TitleExecutionPetition;
using EmuNX.Core.Configuration.TitleExecutionPetition.IO;

namespace Tests.EmuNX.Core.Configuration.IO;

public class TitleExecutionPetitionConfigJsonTests
{
    [Theory]
    [InlineData("file_not_found", TitleExecutionPetitionConfigError.FileReadError)]
    [InlineData("title_execution.err.1.meta_version.not_found", TitleExecutionPetitionConfigError.MetaVersionNotFound)]
    [InlineData("title_execution.err.2.meta_version.not_an_array", TitleExecutionPetitionConfigError.MetaVersionNotFound)]
    [InlineData("title_execution.err.3.meta_version.invalid_array", TitleExecutionPetitionConfigError.MetaVersionNotFound)]
    [InlineData("title_execution.err.4.meta_version.invalid_numbers", TitleExecutionPetitionConfigError.MetaVersionNotFound)]
    [InlineData("title_execution.err.5.meta_version.not_compatible", TitleExecutionPetitionConfigError.MetaVersionNotCompatible)]
    public async Task Load_ShouldReturnExpectedError(string fileName, TitleExecutionPetitionConfigError? expectedError)
    {
        // Arrange
        var config = new TitleExecutionPetitionConfigJson($"data/configuration/{fileName}.json");

        // Act
        var result = await config.Load();

        // Assert
        Assert.Equal(expectedError, result);
    }
}