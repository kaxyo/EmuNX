using EmuNX.Core.Common.Types;
using EmuNX.Core.Configuration.TitleExecutionPetition.Types;

namespace EmuNX.Core.Configuration.TitleExecutionPetition;

/// <summary>
/// <para>
/// Stores every <see cref="TitleExecutionPetition"/> parsed from an <see cref="IO.ITepConfigParser"/>.
/// </para>
///
/// <para>
/// This configuration is made of multiple <see cref="TitleExecutionPetition"/> layers that can <b>patch</b> previous
/// layers based on the following priority order: (The first item <b>patches</b> the next and so on)
/// <list type="number">
///     <item><see cref="TepBase"/> - Base configuration for everything</item>
///     <item><see cref="TepGlobal"/> - Overrides for all titles</item>
///     <item><see cref="TepEmulatorFamilies"/> - Overrides for specific emulator families</item>
///     <item><see cref="TepTitles"/> - Overrides for specific titles applied to one of <see cref="TepEmulatorFamilies"/></item>
/// </list>
/// </para>
///
/// <para>
/// To get some <see cref="TitleExecutionPetition"/> with previous layers patched to it, you can use the getters
/// <b>GetFullTep...</b>.
/// </para>
/// </summary>
public class TepConfig
{
    public readonly TitleExecutionPetition TepBase = new (
        TepEmulatorFamily.Ask,
        "default",
        TepUserPrompt.Ask
    );

    #region Stored and retrieved patches
    public TitleExecutionPetition TepGlobal = new();
    public Dictionary<EmulatorFamily, TitleExecutionPetition> TepEmulatorFamilies = new();
    public Dictionary<TitleId, TitleExecutionPetition> TepTitles = new();
    #endregion

    #region Utilities
    /// <summary>
    /// Gets the merged <see cref="TitleExecutionPetition"/> for the given <paramref name="emulatorFamily"/>,
    /// applying overrides on top of <see cref="TepGlobal"/>.
    /// </summary>
    /// <param name="emulatorFamily">The emulator family to retrieve its patch.</param>
    /// <returns>The resulting <see cref="TitleExecutionPetition"/> after applying relevant patches.</returns>
    public TitleExecutionPetition GetFullTepOfEmulatorFamily(EmulatorFamily emulatorFamily)
    {
        var tepFinal = TepGlobal.Clone();

        if (TepEmulatorFamilies.TryGetValue(emulatorFamily, out var tepEmulatorFamily))
            tepFinal.Patch(tepEmulatorFamily);

        return tepFinal;
    }
    
    /// <summary>
    /// Gets the merged <see cref="TitleExecutionPetition"/> for the given <paramref name="titleId"/>,
    /// applying overrides from <see cref="TepGlobal"/> and <see cref="TepEmulatorFamilies"/> based on the properties of
    /// <paramref name="titleId"/>.
    /// </summary>
    /// <param name="titleId">The title ID to retrieve its patch.</param>
    /// <returns>The resulting <see cref="TitleExecutionPetition"/> after applying all relevant patches.</returns>
    public TitleExecutionPetition GetFullTepOfTitleId(TitleId titleId)
    {
        throw new NotImplementedException();
    }
    #endregion
}