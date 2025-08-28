using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace Utils.Json;

public static class JsonExtensions
{
    #region Reading
    /// <summary>
    /// Tries to get a property from a JSON Object by key.
    /// </summary>
    /// <param name="node">The <see cref="JsonNode"/> that should be a <see cref="JsonObject"/>.</param>
    /// <param name="key">The property key to look for.</param>
    /// <returns>
    /// The <see cref="JsonNode"/> value of the property, or <c>null</c> if the key does not exist
    /// or if the node is not a <see cref="JsonObject"/>.
    /// </returns>
    public static JsonNode? Dig(this JsonNode? node, string key) =>
        node.IntoObject()?.TryGetPropertyValue(key, out var value) == true ? value : null;

    /// <summary>
    /// Tries to traverse a JSON Object hierarchy using a sequence of keys, returning the final node if it exists.
    /// </summary>
    /// <param name="node">The root <see cref="JsonNode"/> to start the traversal from.</param>
    /// <param name="keys">A sequence of keys representing the path through nested <see cref="JsonObject"/> nodes.</param>
    /// <returns>
    /// The <see cref="JsonNode"/> located at the end of the key path, or <c>null</c> if any key in the path
    /// does not exist or if a node in the path is not a <see cref="JsonObject"/>.
    /// </returns>
    /// <remarks>
    /// This method is useful for safely accessing deeply nested JSON objects without throwing exceptions
    /// if intermediate nodes are missing or not objects. It only works for traversing <see cref="JsonObject"/> nodes;
    /// if a node along the path is a <see cref="JsonArray"/> or a primitive value, traversal stops and <c>null</c> is returned.
    /// </remarks>
    public static JsonNode? Dig(this JsonNode? node, params string[] keys)
    {
        var current = node;

        foreach (var key in keys)
        {
            current = current.Dig(key);
            if (current is null) return null;
        }

        return current;
    }

    /// <summary>
    /// Tries to obtain the current JSON value and returns a value that indicates whether the operation succeeded.
    /// </summary>
    /// <typeparam name="T">The type of value to obtain.</typeparam>
    /// <param name="node">The <see cref="JsonNode"/> that might be hold a value of type <c>T</c>.</param>
    /// <param name="value">When this method returns, contains the parsed value.</param>
    /// <returns><see langword="true"/> if the value can be successfully obtained; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetValue<T>(this JsonNode? node, [NotNullWhen(true)] out T? value)
    {
        if (node is JsonValue jsonValue)
            return jsonValue.TryGetValue(out value);

        value = default;
        return false;
    }

    /// <summary>
    /// Tries to obtain the current JSON value as type <c>T</c>.
    /// </summary>
    /// <typeparam name="T">The type of value to obtain.</typeparam>
    /// <param name="node">The <see cref="JsonNode"/> that might be hold a value of type <c>T</c>.</param>
    /// <returns>The <c>T</c> value when success or <c>null</c> otherwise.</returns>
    public static T? IntoValue<T>(this JsonNode? node)
        => node.TryGetValue<T>(out var value) ? value : default;

    /// <summary>
    /// Tries to get an array of <see cref="JsonNode"/> from this <see cref="JsonNode"/>.
    /// </summary>
    /// <param name="node">The <see cref="JsonNode"/> that might be an array of <see cref="JsonNode"/>.</param>
    /// <returns>An array of <see cref="JsonNode"/> or <c>null</c>.</returns>
    /// <remarks>This method is safe because it does not throw any exceptions.</remarks>
    public static JsonNode?[]? IntoArray(this JsonNode? node) =>
        node is JsonArray array ? array.ToArray() : null;

    /// <summary>
    /// Tries to convert this <see cref="JsonNode"/> into a <see cref="JsonObject"/>.
    /// </summary>
    /// <param name="node">The <see cref="JsonNode"/> that might be a <see cref="JsonObject"/>.</param>
    /// <returns>A <see cref="JsonObject"/> if the node is an object, or <c>null</c> otherwise.</returns>
    /// <remarks>This method is safe because it does not throw any exceptions.</remarks>
    public static JsonObject? IntoObject(this JsonNode? node) =>
        node as JsonObject;
    #endregion Reading

    #region Writing

    /// <summary>
    /// Sets a value for the specified key in this <see cref="JsonObject"/>. If the value is considered "empty"
    /// (null, empty <see cref="JsonObject"/>, or empty <see cref="JsonArray"/>), the key is removed instead of being set.
    /// </summary>
    /// <typeparam name="T">The type of the value to set.</typeparam>
    /// <param name="obj">The <see cref="JsonObject"/> to modify.</param>
    /// <param name="key">The property key to set or remove.</param>
    /// <param name="value">The value to set. If null or empty, the key will be removed from the object.</param>
    /// <returns>The modified <see cref="JsonObject"/> for method chaining.</returns>
    /// <remarks>
    /// This method helps keep JSON objects clean by automatically removing keys that would have empty values.
    /// Empty values are defined as:
    /// <list type="bullet">
    /// <item><c>null</c></item>
    /// <item>Empty <see cref="JsonObject"/> (Count = 0)</item>
    /// <item>Empty <see cref="JsonArray"/> (Count = 0)</item>
    /// </list>
    /// </remarks>
    public static JsonObject Set<T>(this JsonObject obj, string key, T? value)
    {
        // Do not add incomplete data
        if (value is null || value is JsonObject { Count: 0 } || value is JsonArray { Count: 0 })
        {
            obj.Remove(key);
            return obj;
        }

        // Some values cannot be added as they are
        obj[key] = value switch
        {
            JsonObject jsonObject => jsonObject,
            JsonArray jsonArray => jsonArray,
            _ => JsonValue.Create(value)
        };

        return obj;
    }
    #endregion Writing
}