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
    public static TepEmulatorFamily ToTitleExecutionPetitionEmulatorFamily(this EmulatorFamily emulatorFamily)
    {
        return emulatorFamily switch
        {
            EmulatorFamily.Yuzu => TepEmulatorFamily.Yuzu,
            EmulatorFamily.Ryujinx => TepEmulatorFamily.Ryujinx,
            _ => TepEmulatorFamily.Yuzu
        };
    }
}