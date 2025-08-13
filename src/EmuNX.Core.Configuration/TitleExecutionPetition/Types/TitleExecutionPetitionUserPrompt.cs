namespace EmuNX.Core.Configuration.TitleExecutionPetition.Types;

public enum TitleExecutionPetitionUserPrompt
{
    /// The system will ask which user from the emulator runner to play with.
    Ask,
    /// The system will execute the emulator runner without specifying the user, so it will default to the default set
    /// by the runner's settings.
    None,
}

public static class TitleExecutionPetitionUserPromptExtensions
{
    public static string ToString(this TitleExecutionPetitionUserPrompt value)
    {
        return value switch
        {
            TitleExecutionPetitionUserPrompt.Ask => "ask",
            TitleExecutionPetitionUserPrompt.None => "none",
            _ => "ask"
        };
    }
    
    public static TitleExecutionPetitionUserPrompt? FromString(string family)
    {
        return family.ToLower() switch
        {
            "ask" => TitleExecutionPetitionUserPrompt.Ask,
            "none" => TitleExecutionPetitionUserPrompt.None,
            _ => null
        };
    }
}