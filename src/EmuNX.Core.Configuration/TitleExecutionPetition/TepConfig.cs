using EmuNX.Core.Common.Types;
using EmuNX.Core.Configuration.TitleExecutionPetition.IO;
using EmuNX.Core.Configuration.TitleExecutionPetition.Types;

namespace EmuNX.Core.Configuration.TitleExecutionPetition;

/// <summary>
/// <para>
/// Stores every <see cref="TitleExecutionPetition"/> parsed from an <see cref="ITepConfigStorage"/>.
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

    #region TitleExecutionPetitions as they are stored
    public TitleExecutionPetition TepGlobal = new();
    public Dictionary<EmulatorFamily, TitleExecutionPetition> TepEmulatorFamilies = new();
    public Dictionary<TitleId, TitleExecutionPetition> TepTitles = new();
    #endregion

    #region Generate full TitleExecutionPetitions
    /// <returns>
    /// The <see cref="TepGlobal"/> with the previous layers merged.
    /// To learn about the merging order, read the documentation of <seealso cref="TepConfig"/>.
    /// </returns>
    public TitleExecutionPetition GetFullTepGlobal()
    {
        return TepBase.Clone().Patch(TepGlobal);
    }

    /// <param name="emulatorFamily">The key to select one of the <see cref="TitleExecutionPetition"/> from <see cref="TepEmulatorFamilies"/>.</param>
    /// <returns>
    /// One of the <see cref="TitleExecutionPetition"/> from <see cref="TepEmulatorFamilies"/> with the previous layers
    /// merged.
    /// To learn about the merging order, read the documentation of <seealso cref="TepConfig"/>.
    /// </returns>
    public TitleExecutionPetition GetFullTepOfEmulatorFamily(EmulatorFamily emulatorFamily)
    {
        // TODO: Fix
        var tepFinal = TepGlobal.Clone();

        if (TepEmulatorFamilies.TryGetValue(emulatorFamily, out var tepEmulatorFamily))
            tepFinal.Patch(tepEmulatorFamily);

        return tepFinal;
    }

    /// <param name="titleId">The key to select one of the <see cref="TitleExecutionPetition"/> from <see cref="TepTitles"/>.</param>
    /// <returns>
    /// One of the <see cref="TitleExecutionPetition"/> from <see cref="TepTitles"/> with the previous layers
    /// merged.
    /// To learn about the merging order, read the documentation of <seealso cref="TepConfig"/>.
    /// </returns>
    public TitleExecutionPetition GetFullTepOfTitleId(TitleId titleId)
    {
        throw new NotImplementedException();
    }
    #endregion
}

public static class TitleExecutionPetitionDictionaryExtensions
{
    // Extension method for Dictionary<TKey, TitleExecutionPetition>
    public static TitleExecutionPetition GetOrNew<TKey>(this Dictionary<TKey, TitleExecutionPetition> dictionary, TKey key) where TKey : notnull
    {
        return dictionary.TryGetValue(key, out var value)
            ? value
            : new TitleExecutionPetition();
    }

    /// Removes all <see cref="TitleExecutionPetition"/> that are <b>empty</b>.
    /// <seealso cref="TitleExecutionPetition.IsEmpty"/>
    public static void RemoveEmpty<TKey>(this Dictionary<TKey, TitleExecutionPetition> dictionary) where TKey : notnull
    {
        // Collect keys to remove (can't modify while iterating)
        var keysToRemove = dictionary
            .Where(kvp => kvp.Value.IsEmpty)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
            dictionary.Remove(key);
    }
}