using System.Text.Json;

namespace Utils;

public static class JsonElementExtensions
{
    /// <returns>
    /// The specified property as a <see cref="string"/>, or <b>null</b> if it doesn't exist or is not a
    /// <see cref="string"/>.
    /// </returns>
    public static string? GetString(this JsonElement element, string propertyName)
    {
        var hasProperty = element.TryGetProperty(propertyName, out var property);

        return hasProperty && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;
    }

    /// <returns>
    /// The specified property as a <b>Json Object</b>, or <b>null</b> if it doesn't exist or is not a
    /// <b>Json Object</b>.
    /// </returns>
    public static JsonElement? GetObject(this JsonElement element, string propertyName)
    {
        var hasProperty = element.TryGetProperty(propertyName, out var property);

        return hasProperty && property.ValueKind == JsonValueKind.Object
            ? property
            : null;
    }

    /// <returns>
    /// The specified property as an <b>array</b> of <see cref="JsonElement"/>, or <b>null</b> if it doesn't exist or is
    /// not an <b>array</b>.
    /// </returns>
    public static JsonElement[]? GetArray(this JsonElement element, string propertyName)
    {
        var hasProperty = element.TryGetProperty(propertyName, out var property);
        
        return hasProperty && property.ValueKind == JsonValueKind.Array
            ? property.EnumerateArray().ToArray()
            : null;
    }
}