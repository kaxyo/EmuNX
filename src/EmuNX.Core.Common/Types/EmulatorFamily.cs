namespace EmuNX.Core.Common.Types;

public enum EmulatorFamily
{
    Yuzu,
    Ryujinx,
}

public static class EmulatorFamilyExtensions
{
    public static EmulatorFamily? FromString(string family)
    {
        return family.ToLower() switch
        {
            "yuzu" => EmulatorFamily.Yuzu,
            "ryujinx" => EmulatorFamily.Ryujinx,
            _ => null
        };
    }
}