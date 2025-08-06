using EmuNX.Core.Configuration.TitleExecutionPetition.Types;
using EmulatorFamily = EmuNX.Core.Configuration.TitleExecutionPetition.Types.TitleExecutionPetitionEmulatorFamily;
using UserPrompt = EmuNX.Core.Configuration.TitleExecutionPetition.Types.TitleExecutionPetitionUserPrompt;

namespace EmuNX.Core.Configuration.TitleExecutionPetition;

/// <summary>
/// <para>
/// Defines a possibly <b>ambiguous petition</b> on how a <b>title should be executed</b>.
/// </para>
/// 
/// <para>
/// <b>Ambiguous</b> means that a petition <b>can store concrete</b> information like concrete emulator runners <b>as
/// well as undefined</b> like the user must be prompted before playing.
/// </para>
///
/// <para>
/// This <see cref="TitleExecutionPetition"/> is used for <b>generating formularies</b> in which the user completes to
/// generate a <see cref="TitleExecutionCommand"/> used to play a title.
/// (e.g., The petition wants to use Ryujinx-1.0 and prompt the user, so because the user is undefined we ask it)
/// </para>
///
/// <para>
/// In short, this is the middle step to generate the final <see cref="TitleExecutionCommand"/>.
/// </para>
/// </summary>
/// 
/// <remarks>
/// This class supports <b>patching</b> operations between two <see cref="TitleExecutionPetition"/>.
/// See <see cref="Patch"/> method for more information.
/// </remarks>
public class TitleExecutionPetition(TitleExecutionPetitionEmulatorFamily? emulatorFamily, string? emulatorRunner, TitleExecutionPetitionUserPrompt? userPrompt)
{
    public TitleExecutionPetitionEmulatorFamily? emulatorFamily = emulatorFamily;
    public string? emulatorRunner = emulatorRunner;
    public TitleExecutionPetitionUserPrompt? userPrompt = userPrompt;

    /// <summary>
    /// Overrides values from <b>this</b> with the <b>non-null</b> values from <paramref name="other"/>.
    /// </summary>
    /// 
    /// <example>
    /// We will create two <see cref="TitleExecutionPetition"/> and generate a final one.
    /// <code>
    /// var global = new TitleExecutionPetition(
    ///     EmulatorFamily.Ryujinx,
    ///     "ryubing-1.3.2",
    ///     UserPrompt.Ask
    /// );
    ///
    /// var title = new TitleExecutionPetition(
    ///     null,
    ///     null,
    ///     UserPrompt.None
    /// );
    ///
    /// var final = global.Clone().Patch(title);
    /// // effective.emulatorFamily == EmulatorFamily.Ryujinx
    /// // effective.emulatorRunner == "ryubing-1.3.2"
    /// // effective.userPrompt == UserPrompt.None
    /// </code>
    /// </example>
    /// 
    /// <param name="other">The patch with overriding values.</param>
    /// <returns>This object but with the patch applied</returns>
    public TitleExecutionPetition Patch(TitleExecutionPetition other)
    {
        this.emulatorFamily = other.emulatorFamily ?? this.emulatorFamily;
        this.emulatorRunner = other.emulatorRunner ?? this.emulatorRunner;
        this.userPrompt = other.userPrompt ?? this.userPrompt;

        return this;
    }

    public TitleExecutionPetition Clone()
    {
        return new TitleExecutionPetition(this.emulatorFamily, this.emulatorRunner, this.userPrompt);
    }
}