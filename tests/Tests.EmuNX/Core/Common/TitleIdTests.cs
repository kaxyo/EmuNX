using EmuNX.Core.Common.Types;

namespace Tests.EmuNX.Core.Common;

public class TitleIdTests
{
    [Theory]
    [InlineData(0x01004D300C5AE000, "01004D300C5AE000")]
    [InlineData(0x01007E3006DDA000, "01007E3006DDA000")]
    [InlineData(0x01006B601380E000, "01006B601380E000")]
    public void Constructor_WithNum_SetsProperties(ulong num, string expectedHex)
    {
        var titleId = new TitleId(num);

        Assert.Equal(num, titleId.Num);
        Assert.Equal(expectedHex, titleId.Hex);
    }
    
    
    
    [Theory]
    [InlineData("01004D300C5AE000", 0x01004D300C5AE000)]
    [InlineData("01007E3006DDA000", 0x01007E3006DDA000)]
    [InlineData("01006B601380E000", 0x01006B601380E000)]
    [InlineData("01004d300c5ae000", 0x01004D300C5AE000)]
    [InlineData("01007e3006dda000", 0x01007E3006DDA000)]
    [InlineData("01006b601380e000", 0x01006B601380E000)]
    public void TryParseHex_WithValidHexString_ParsesCorrectly(string hex, ulong expectedNum)
    {
        var success = TitleId.TryParseHex(hex, out var titleId);

        Assert.True(success);
        Assert.Equal(new TitleId(expectedNum), titleId);
    }

    [Theory]
    [InlineData("Garbage")]
    [InlineData("1")]
    [InlineData("FFFFFFFFFFFFFFFFa")]
    [InlineData("")]
    public void TryParseHex_WithInvalidHexString_ReturnsFailure(string invalidHex)
    {
        var success = TitleId.TryParseHex(invalidHex, out var titleId);

        Assert.Equal(0UL, titleId.Num);
        Assert.False(success);
    }
}
