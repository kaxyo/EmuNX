namespace EmuNX.Lib.PreferencesNX.Title.Types;

public class TitleExecutionEmulatorRunnerConfig
{
    public TitleExecutionEmulatorRunnerOptions Type { get; private set; }
    public string EmulatorId { get; private set; }

    private TitleExecutionEmulatorRunnerConfig(TitleExecutionEmulatorRunnerOptions type, string emulatorId = null)
    {
        Type = type;
        EmulatorId = emulatorId;
    }

    public static TitleExecutionEmulatorRunnerConfig Default() => new(TitleExecutionEmulatorRunnerOptions.Default);
    public static TitleExecutionEmulatorRunnerConfig Ask() => new(TitleExecutionEmulatorRunnerOptions.Ask);
    public static TitleExecutionEmulatorRunnerConfig Id(string value) => new(TitleExecutionEmulatorRunnerOptions.Id, value);
}

public enum TitleExecutionEmulatorRunnerOptions
{
    Default,
    Ask,
    Id
}