using System.Text.Json;
using System.Text.Json.Nodes;
using Utils.Monad;

namespace Utils.Json;

/// <summary>
/// Utility that works as middleware <see cref="Load"/> or <see cref="Save"/> a <see cref="JsonNode"/>.
/// Just work with <see cref="JsonNode"/> and let this class do everything else related to <b>files</b>.
/// </summary>
/// <param name="filePath">The location of the file that will be read or written.</param>
/// <param name="writerOptions">Settings that change the <see cref="Save"/> behaviour.</param>
public class JsonStorage(string filePath, JsonWriterOptions? writerOptions = null)
{
    private static readonly JsonWriterOptions DefaultJsonWriterOptions = new() { Indented = false };
    private readonly JsonWriterOptions _jsonWriterOptions = writerOptions ?? DefaultJsonWriterOptions;

    /// <summary>
    /// Reads a <b>file</b> and tries to parse it as <b>JSON</b>.
    /// </summary>
    /// <returns>A <see cref="JsonNode"/> if <b>loading</b> and <b>parsing</b> succeed or an <see cref="Exception"/> otherwise.</returns>
    public Result<JsonNode, Exception> Load()
    {
        try
        {
            using var fs = File.OpenRead(filePath);
            var buffer = new byte[fs.Length];
            fs.ReadExactly(buffer);

            var reader = new Utf8JsonReader(buffer);
            var node = JsonNode.Parse(ref reader);

            return node is null
                ? Result<JsonNode, Exception>.Failure(new Exception("JsonNode.Parse returned null."))
                : Result<JsonNode, Exception>.Success(node);
        }
        catch (Exception ex)
        {
            return Result<JsonNode, Exception>.Failure(ex);
        }
    }

    /// <summary>
    /// Tries to <b>serialize</b> <see cref="JsonNode"/> and write it to a <b>file</b>.
    /// </summary>
    /// <returns>If it fails it returns the cause of it with <see cref="Exception"/> otherwise <see cref="Nothing"/>.</returns>
    public Result<Nothing, Exception> Save(JsonNode data)
    {
        try
        {
            using var fs = File.Open(filePath, FileMode.Create, FileAccess.Write);
            using var writer = new Utf8JsonWriter(fs, _jsonWriterOptions);
            data.WriteTo(writer);
            writer.Flush();
            return Result<Nothing, Exception>.Success(new Nothing());
        }
        catch (Exception ex)
        {
            return Result<Nothing, Exception>.Failure(ex);
        }
    }
}
