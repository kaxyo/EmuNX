using EmuNX.Core.Common.Types;

namespace EmuNX.Core.Configuration.TitleExecutionPetition.Types;

public enum TepEmulatorFamily
{
    /// The system will ask which emulator family to use.
    Ask,
    /// If the runner id is empty or wrong, the system will prompt <b>Yuzu</b> runners.
    Yuzu,
    /// If the runner id is empty or wrong, the system will prompt <b>Ryujinx</b> runners.
    Ryujinx,
}

public static class TitleExecutionPetitionEmulatorFamilyExtensions
{
    public static EmulatorFamily? ToEmulatorFamily(this TepEmulatorFamily tepEmulatorFamily)
    {
        return tepEmulatorFamily switch
        {
            TepEmulatorFamily.Yuzu => EmulatorFamily.Yuzu,
            TepEmulatorFamily.Ryujinx => EmulatorFamily.Ryujinx,
            _ => null
        };
    }

    public static TepEmulatorFamily ToTitleExecutionPetitionEmulatorFamily(this EmulatorFamily emulatorFamily)
    {
        return emulatorFamily switch
        {
            EmulatorFamily.Yuzu => TepEmulatorFamily.Yuzu,
            EmulatorFamily.Ryujinx => TepEmulatorFamily.Ryujinx,
            _ => TepEmulatorFamily.Yuzu
        };
    }

    /// <returns>The enum as a string with <b>kebab-case</b>.</returns>
    public static string ToKeyString(this TepEmulatorFamily value)
    {
        return value switch
        {
            TepEmulatorFamily.Ask => "ask",
            TepEmulatorFamily.Yuzu => "yuzu",
            TepEmulatorFamily.Ryujinx => "ryujinx",
            _ => "ask"
        };
    }
    
    public static TepEmulatorFamily? FromString(string family)
    {
        return family.ToLower() switch
        {
            "ask" => TepEmulatorFamily.Ask,
            "yuzu" => TepEmulatorFamily.Yuzu,
            "ryujinx" => TepEmulatorFamily.Ryujinx,
            _ => null
        };
    }
}