using System.Text.Json;
using System.Text.Json.Nodes;
using Utils.Monad;

namespace Utils.Json;

/// <summary>
/// Utility that works as middleware <see cref="Load"/> or <see cref="Save"/> a <see cref="JsonNode"/>.
/// Just work with <see cref="JsonNode"/> and let this class do everything else related to <b>files</b>.
/// </summary>
/// <param name="filePath">The location of the file that will be read or written.</param>
/// <param name="storeLastJsonNodeLoaded">If enabled, the <see cref="LastJsonNodeLoaded"/> is updated after each <see cref="Load"/>.</param>
/// <param name="storeLastJsonNodeSaved">If enabled, the <see cref="LastJsonNodeSaved"/> is updated after each <see cref="Save"/>.</param>
/// <param name="writerOptions">Settings that change the <see cref="Save"/> behaviour.</param>
public class JsonStorage(string filePath, JsonWriterOptions? writerOptions = null, bool storeLastJsonNodeLoaded = false, bool storeLastJsonNodeSaved = false)
{
    private static readonly JsonWriterOptions DefaultJsonWriterOptions = new() { Indented = false };
    private readonly JsonWriterOptions _jsonWriterOptions = writerOptions ?? DefaultJsonWriterOptions;

    #region Debug
    public JsonNode? LastJsonNodeLoaded { get; private set; } = null;
    public JsonNode? LastJsonNodeSaved { get; private set; } = null;
    #endregion

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

            if (storeLastJsonNodeLoaded)
                LastJsonNodeLoaded = node;

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
    /// <param name="node">The <see cref="JsonNode"/> that will be written to a file as JSON.</param>
    /// <returns>If it fails it returns the cause of it with <see cref="Exception"/> otherwise <see cref="Nothing"/>.</returns>
    public Result<Nothing, Exception> Save(JsonNode node)
    {
        Result<Nothing, Exception> result;

        try
        {
            using var fs = File.Open(filePath, FileMode.Create, FileAccess.Write);
            using var writer = new Utf8JsonWriter(fs, _jsonWriterOptions);
            node.WriteTo(writer);
            writer.Flush();
            result = Result<Nothing, Exception>.Success(new Nothing());
        }
        catch (Exception ex)
        {
            result = Result<Nothing, Exception>.Failure(ex);
        }

        if (storeLastJsonNodeSaved)
            LastJsonNodeLoaded = result.IsOk ? node : null;

        return result;
    }
}
