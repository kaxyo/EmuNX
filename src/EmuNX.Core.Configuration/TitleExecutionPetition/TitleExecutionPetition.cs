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
public class TitleExecutionPetition(EmulatorFamily? emulatorFamily, string? emulatorRunner, UserPrompt? userPrompt)
{
    public EmulatorFamily? EmulatorFamily = emulatorFamily;
    public string? EmulatorRunner = emulatorRunner;
    public UserPrompt? UserPrompt = userPrompt;

    public TitleExecutionPetition() : this(null, null, null) {}

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
        this.EmulatorFamily = other.EmulatorFamily ?? this.EmulatorFamily;
        this.EmulatorRunner = other.EmulatorRunner ?? this.EmulatorRunner;
        this.UserPrompt = other.UserPrompt ?? this.UserPrompt;

        return this;
    }

    public TitleExecutionPetition Clone()
    {
        return new TitleExecutionPetition(this.EmulatorFamily, this.EmulatorRunner, this.UserPrompt);
    }

    protected bool Equals(TitleExecutionPetition other)
    {
        return EmulatorFamily == other.EmulatorFamily && EmulatorRunner == other.EmulatorRunner && UserPrompt == other.UserPrompt;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((TitleExecutionPetition)obj);
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
}