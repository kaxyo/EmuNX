using EmuNX.Core.Common.Types;
using EmuNX.Core.Configuration.TitleExecutionPetition.Types;

namespace EmuNX.Core.Configuration.TitleExecutionPetition;

/// <summary>
/// <para>
/// Defines how a <see cref="TitleExecutionPetition"/> configuration <b>file/source</b> is structured and managed.
/// </para>
/// 
/// <para>
/// Each implementation must be able to <see cref="Load"/> all existing data, allow modification (add/update/delete)
/// of the loaded <see cref="TitleExecutionPetition"/> entries, and finally <see cref="Save"/> the result.
/// </para>
/// 
/// <para>
/// This configuration supports multiple <see cref="TitleExecutionPetition"/> layers that can <b>patch</b> or override 
/// previous layers based on the following priority order:
/// <list type="number">
///     <item><see cref="TepGlobal"/> - Base configuration applied to all titles</item>
///     <item><see cref="TepEmulatorFamilies"/> - Overrides for specific emulator families</item>
///     <item><see cref="TepTitles"/> - Overrides for specific titles</item>
/// </list>
/// </para>
/// </summary>
public abstract class TitleExecutionPetitionConfig
{
    public abstract Version VersionTarget { get; }
    public Version VersionCurrent = new(0, 0);

    #region Storage
    /// <summary>
    /// Loads every <see cref="TitleExecutionPetition"/> stored in the <b>file/source</b>.
    /// </summary>
    /// <seealso cref="TepGlobal"/>
    /// <seealso cref="TepEmulatorFamilies"/>
    /// <seealso cref="TepTitles"/>
    /// <returns>If the <b>loading</b> succeeds <c>null</c>, if it fails <see cref="TitleExecutionPetitionConfigError"/>.</returns>
    public abstract Task<TitleExecutionPetitionConfigError?> Load();
    
    /// <summary>
    /// Saves the current state of every <see cref="TitleExecutionPetition"/> loaded into a <b>file/source</b>.
    /// </summary>
    /// <seealso cref="TepGlobal"/>
    /// <seealso cref="TepEmulatorFamilies"/>
    /// <seealso cref="TepTitles"/>
    /// <returns>If the <b>saving</b> succeeds <c>null</c>, if it fails <see cref="TitleExecutionPetitionConfigError"/>.</returns>
    public abstract Task<TitleExecutionPetitionConfigError?> Save();
    #endregion

    #region Current state

    public TitleExecutionPetition TepGlobal = new();
    public Dictionary<EmulatorFamily, TitleExecutionPetition> TepEmulatorFamilies { get; private set; } = new();
    public Dictionary<TitleId, TitleExecutionPetition> TepTitles { get; } = new();
    #endregion
    
    #region Restart
    public readonly TitleExecutionPetition TepDefaultGlobal = new (
        TitleExecutionPetitionEmulatorFamily.Ask,
        "default",
        TitleExecutionPetitionUserPrompt.Ask
    );

    /// <summary>
    /// Restarts <see cref="TepGlobal"/>, <see cref="TepEmulatorFamilies"/> and <see cref="TepTitles"/> to their
    /// default values.
    /// </summary>
    /// <remarks>
    /// This is <b>not executed at instantiation</b>, meaning that unless you call <see cref="Load"/> or
    /// <see cref="RestartState"/>, the state-values will not be correct/unexpected.
    /// </remarks>
    public void RestartState()
    {
        TepGlobal = TepDefaultGlobal.Clone();

        TepEmulatorFamilies = new Dictionary<EmulatorFamily, TitleExecutionPetition>()
        {
            { EmulatorFamily.Yuzu, new TitleExecutionPetition() },
            { EmulatorFamily.Ryujinx, new TitleExecutionPetition() }
        };

        TepTitles.Clear();
    }
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