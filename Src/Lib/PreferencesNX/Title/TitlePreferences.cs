using EmuNX.Lib.PreferencesNX.Title.Types;
using Language = LibHac.Settings.Language;

namespace EmuNX.Lib.PreferencesNX.Title;

/// <summary>
/// Configurations about how a game can be run (which emulator, user...) and must look in the UI (Language of title...).
/// <param>
/// Some value might be ambiguous (i.e. "default" and "ask"), because:
/// </param>
/// <list type="number">
///     <item>
///         <b>Default values:</b> These <see cref="TitlePreferences"/> values might be "default", meaning that it must
///         inherit from a "default" <see cref="TitlePreferences"/>.
///     </item>
///     <item>
///         <b>Ask in UI:</b> I.e. The player might want to select the user before initializing the game, therefore the
///         property must be set to "ask". This works with other properties such as the emulator family, runner, etc...
///     </item>
/// </list>
/// </summary>
public class TitlePreferences
{
    public TitleExecutionEmulatorFamilyOptions TitleExecutionEmulatorFamily;
    public TitleExecutionEmulatorRunnerConfig TitleExecutionEmulatorRunner;
    public TitleExecutionUserPromptOptions TitleExecutionUserPrompt;
    public Language? PresentationLocaleIcon;
    public Language? PresentationLocaleName;
}