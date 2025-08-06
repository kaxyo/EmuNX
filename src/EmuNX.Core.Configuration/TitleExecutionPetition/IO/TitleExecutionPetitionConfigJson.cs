using Utils;

namespace EmuNX.Core.Configuration.TitleExecutionPetition.IO;

/// <summary>
/// Implements <see cref="TitleExecutionPetitionConfig"/> to manage the configuration inside a <b>JSON</b> file.
/// To set the <b>file path</b>, please change 
/// </summary>
public class TitleExecutionPetitionConfigJson(string filePath) : TitleExecutionPetitionConfig
{
    public string FilePath = filePath;

    public override async Task<TitleExecutionPetitionConfigError?> Load()
    {
        throw new NotImplementedException();
        // Read file
        var jsonString = await EasyFile.ReadText(FilePath);
        if (jsonString == null) return TitleExecutionPetitionConfigError.Unknown;
    }

    public override async Task<TitleExecutionPetitionConfigError?> Save()
    {
        var jsonString = "";
        throw new NotImplementedException();
        // Read file
        var success = await EasyFile.WriteText(FilePath, jsonString);
        if (!success) return TitleExecutionPetitionConfigError.Unknown;
    }
}