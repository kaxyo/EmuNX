using EmuNX.Core.Common.Types;

namespace EmuNX.Core.Configuration.TitleExecutionPetition.Types;

public enum TitleExecutionPetitionEmulatorFamily
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
    public static EmulatorFamily? ToEmulatorFamily(this TitleExecutionPetitionEmulatorFamily tepEmulatorFamily)
    {
        return tepEmulatorFamily switch
        {
            TitleExecutionPetitionEmulatorFamily.Yuzu => EmulatorFamily.Yuzu,
            TitleExecutionPetitionEmulatorFamily.Ryujinx => EmulatorFamily.Ryujinx,
            _ => null
        };
    }

    public static string ToString(this TitleExecutionPetitionEmulatorFamily value)
    {
        return value switch
        {
            TitleExecutionPetitionEmulatorFamily.Ask => "ask",
            TitleExecutionPetitionEmulatorFamily.Yuzu => "yuzu",
            TitleExecutionPetitionEmulatorFamily.Ryujinx => "ryujinx",
            _ => "ask"
        };
    }
    
    public static TitleExecutionPetitionEmulatorFamily? FromString(string family)
    {
        return family.ToLower() switch
        {
            "ask" => TitleExecutionPetitionEmulatorFamily.Ask,
            "yuzu" => TitleExecutionPetitionEmulatorFamily.Ask,
            "ryujinx" => TitleExecutionPetitionEmulatorFamily.Ask,
            _ => null
        };
    }
}