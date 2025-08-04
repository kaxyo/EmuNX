namespace EmuNX.Core.Configuration.TitleExecutionPetition.Types.Inner;

public enum TitleExecutionPetitionUserPrompt
{
    /// The system will ask which user from the emulator runner to play with.
    Ask,
    /// The system will execute the emulator runner without specifying the user, so it will default to the default set
    /// by the runner's settings.
    None,
}