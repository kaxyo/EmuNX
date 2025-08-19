namespace Utils;

/// <summary>
/// Reads and writes to a file easily. 
/// </summary>
public class EasyFile
{
    /// <summary>
    /// Reads the entire content of a text file asynchronously.
    /// </summary>
    /// <returns>String if it read, null if the file couldn't be read.</returns>
    public static async Task<string?> ReadText(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        try
        {
            return await File.ReadAllTextAsync(filePath);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Writes the given text to a file asynchronously, overwriting if it already exists.
    /// </summary>
    /// <returns>True if succeeded, else false.</returns>
    public static async Task<bool> WriteText(string filePath, string content)
    {
        try
        {
            await File.WriteAllTextAsync(filePath, content);
            return true;
        }
        catch
        {
            return false;
        }
    }
}