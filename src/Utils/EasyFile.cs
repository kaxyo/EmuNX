namespace Utils;

/// <summary>
/// Reads and writes to a file easily. 
/// </summary>
public class EasyFile
{
    /// <summary>
    /// Reads the entire content of a text file asynchronously.
    /// </summary>
    public static async Task<string?> ReadText(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        return await File.ReadAllTextAsync(filePath);
    }

    /// <summary>
    /// Writes the given text to a file asynchronously, overwriting if it already exists.
    /// </summary>
    public static async Task WriteText(string filePath, string content)
    {
        await File.WriteAllTextAsync(filePath, content);
    }

    /// <summary>
    /// Appends text to the end of the specified file asynchronously.
    /// </summary>
    public static async Task AppendText(string filePath, string content)
    {
        await File.AppendAllTextAsync(filePath, content);
    }
}