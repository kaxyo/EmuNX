using EmuNX.Core.Configuration.TitleExecutionPetition;
using EmuNX.Core.Configuration.TitleExecutionPetition.Types;

namespace Tests.EmuNX.Core.Configuration;

public class TitleExecutionPetitionTests
{
    [Fact]
    public void Clone_CreatesExactCopy()
    {
        // Arrange
        var original = new TitleExecutionPetition(
            TepEmulatorFamily.Yuzu,
            "yuzu-stable",
            TepUserPrompt.Ask
        );

        // Act
        var clone = original.Clone();

        // Assert
        Assert.Equal(original.EmulatorFamily, clone.EmulatorFamily);
        Assert.Equal(original.EmulatorRunner, clone.EmulatorRunner);
        Assert.Equal(original.UserPrompt, clone.UserPrompt);
        Assert.NotSame(original, clone); // Ensure it's a different instance
    }

    [Fact]
    public void Patch_OverridesOnlyNonNullFields()
    {
        // Arrange
        var basePetition = new TitleExecutionPetition(
            TepEmulatorFamily.Ryujinx,
            "ryubing-1.3.2",
            TepUserPrompt.Ask
        );

        var patch = new TitleExecutionPetition(
            null,
            null,
            TepUserPrompt.None
        );

        // Act
        var result = basePetition.Clone().Patch(patch);

        // Assert
        Assert.Equal(TepEmulatorFamily.Ryujinx, result.EmulatorFamily);
        Assert.Equal("ryubing-1.3.2", result.EmulatorRunner);
        Assert.Equal(TepUserPrompt.None, result.UserPrompt);
    }

    [Fact]
    public void Patch_DoesNothingIfAllFieldsAreNull()
    {
        // Arrange
        var original = new TitleExecutionPetition(
            TepEmulatorFamily.Yuzu,
            "yuzu-ea-latest",
            TepUserPrompt.Ask
        );

        var patch = new TitleExecutionPetition(null, null, null);

        // Act
        var result = original.Clone().Patch(patch);

        // Assert
        Assert.Equal(original.EmulatorFamily, result.EmulatorFamily);
        Assert.Equal(original.EmulatorRunner, result.EmulatorRunner);
        Assert.Equal(original.UserPrompt, result.UserPrompt);
    }
}