namespace EmuNX.Core.Common.Types;

public enum EmulatorFamily
{
    Yuzu,
    Ryujinx,
}

public static class EmulatorFamilyExtensions
{
    /// <returns>The enum as a string with <b>kebab-case</b>.</returns>
    public static string ToKeyString(this EmulatorFamily value)
    {
        return value switch
        {
            EmulatorFamily.Yuzu => "yuzu",
            EmulatorFamily.Ryujinx => "ryujinx",
            _ => "yuzu"
        };
    }
    
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