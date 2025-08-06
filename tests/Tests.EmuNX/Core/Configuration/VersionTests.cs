namespace Tests.EmuNX.Core.Configuration;

using Version = global::EmuNX.Core.Configuration.Version;

public class VersionTests
{
    [Fact]
    public void DefaultConstructor_ShouldInitializeTo_ZeroZero()
    {
        var version = new Version();

        Assert.Equal((uint)0, version.Major);
        Assert.Equal((uint)0, version.Minor);
    }

    [Theory]
    [InlineData(1, 0, 1, 0, true)] // Equal, no problems
    [InlineData(1, 1, 1, 0, true)] // The 2nd is the same as 1st except it lacks features from 1st
    [InlineData(1, 0, 1, 1, false)] // The 2nd hsa more additions than 1st
    [InlineData(2, 0, 1, 9, false)] // Different major, this newer
    [InlineData(1, 5, 2, 0, false)] // Different major, other newer
    public void IsCompatibleWith_WorksAsExpected(uint major1, uint minor1, uint major2, uint minor2, bool isCompatible)
    {
        var thisVersion = new Version(major1, minor1);
        var otherVersion = new Version(major2, minor2);

        var result = thisVersion.IsCompatibleWith(otherVersion);

        Assert.Equal(isCompatible, result);
    }
}