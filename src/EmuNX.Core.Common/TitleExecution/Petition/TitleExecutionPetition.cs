using EmulatorFamily = EmuNX.Core.Common.TitleExecution.Petition.Types.TitleExecutionPetitionEmulatorFamily;
using UserPrompt = EmuNX.Core.Common.TitleExecution.Petition.Types.TitleExecutionPetitionUserPrompt;

namespace EmuNX.Core.Common.TitleExecution.Petition;

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
public class TitleExecutionPetition
{
    public EmulatorFamily? emulatorFamily;
    public string? emulatorRunner;
    public UserPrompt? userPrompt;
}