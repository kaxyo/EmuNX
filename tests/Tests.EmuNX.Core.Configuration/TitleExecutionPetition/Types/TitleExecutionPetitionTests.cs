using EmuNX.Core.Configuration.TitleExecutionPetition.Types.Inner;
using A = EmuNX.Core.Configuration.TitleExecutionPetition.Types;

namespace Tests.EmuNX.Core.Configuration.TitleExecutionPetition.Types;

public class TitleExecutionPetitionTests
{
    [Fact]
    public void Clone_CreatesExactCopy()
    {
        // Arrange
        var original = new A.TitleExecutionPetition(
            TitleExecutionPetitionEmulatorFamily.Yuzu,
            "yuzu-stable",
            TitleExecutionPetitionUserPrompt.Ask
        );

        // Act
        var clone = original.Clone();

        // Assert
        Assert.Equal(original.emulatorFamily, clone.emulatorFamily);
        Assert.Equal(original.emulatorRunner, clone.emulatorRunner);
        Assert.Equal(original.userPrompt, clone.userPrompt);
        Assert.NotSame(original, clone); // Ensure it's a different instance
    }

    [Fact]
    public void Patch_OverridesOnlyNonNullFields()
    {
        // Arrange
        var basePetition = new A.TitleExecutionPetition(
            TitleExecutionPetitionEmulatorFamily.Ryujinx,
            "ryubing-1.3.2",
            TitleExecutionPetitionUserPrompt.Ask
        );

        var patch = new A.TitleExecutionPetition(
            null,
            null,
            TitleExecutionPetitionUserPrompt.None
        );

        // Act
        var result = basePetition.Clone().Patch(patch);

        // Assert
        Assert.Equal(TitleExecutionPetitionEmulatorFamily.Ryujinx, result.emulatorFamily);
        Assert.Equal("ryubing-1.3.2", result.emulatorRunner);
        Assert.Equal(TitleExecutionPetitionUserPrompt.None, result.userPrompt);
    }

    [Fact]
    public void Patch_DoesNothingIfAllFieldsAreNull()
    {
        // Arrange
        var original = new A.TitleExecutionPetition(
            TitleExecutionPetitionEmulatorFamily.Yuzu,
            "yuzu-ea-latest",
            TitleExecutionPetitionUserPrompt.Ask
        );

        var patch = new A.TitleExecutionPetition(null, null, null);

        // Act
        var result = original.Clone().Patch(patch);

        // Assert
        Assert.Equal(original.emulatorFamily, result.emulatorFamily);
        Assert.Equal(original.emulatorRunner, result.emulatorRunner);
        Assert.Equal(original.userPrompt, result.userPrompt);
    }
}