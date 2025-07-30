using EmuNX.Core.Common.Types;

namespace Tests.EmuNX.Core.Common.Types;

using Xunit;
using EmuNX.Core.Common.Types;
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
    public void Constructor_WithValidHexString_ParsesCorrectly(string hex, ulong expectedNum)
    {
        var titleId = new TitleId(hex);

        Assert.Equal(expectedNum, titleId.Num);
        Assert.Equal(hex, titleId.Hex);
    }

    [Theory]
    [InlineData("Garbage")]
    [InlineData("1")]
    [InlineData("FFFFFFFFFFFFFFFFa")]
    [InlineData("")]
    public void Constructor_WithInvalidHexString_ThrowsArgumentException(string invalidHex)
    {
        Assert.Throws<ArgumentException>(() => new TitleId(invalidHex));
    }
}
