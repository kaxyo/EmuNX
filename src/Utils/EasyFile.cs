namespace Utils;

/// <summary>
/// Reads and writes to a file easily. 
/// </summary>
public static class EasyFile
{
    /// <summary>
    /// Reads the entire content of a text file.
    /// </summary>
    /// <returns>String if it read, null if the file couldn't be read.</returns>
    public static string? ReadText(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        try
        {
            return File.ReadAllText(filePath);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Writes the given text to a file, overwriting if it already exists.
    /// </summary>
    /// <returns>True if succeeded, else false.</returns>
    public static bool WriteText(string filePath, string content)
    {
        try
        {
            File.WriteAllText(filePath, content);
            return true;
        }
        catch
        {
            return false;
        }
    }
}