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