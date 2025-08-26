using System.Text.Json.Nodes;
using FluentAssertions.Json;
using Newtonsoft.Json.Linq;
using Utils.Json;

namespace Tests.EmuNX;

public static class TestsUtils
{
    /// <summary>
    /// Asserts that two <see cref="JsonNode"/> are equal.
    /// </summary>
    /// <param name="expectedJsonNode">The desired JSON</param>
    /// <param name="actualJsonNode">The JSON analyzed</param>
    /// <remarks>
    /// When using <see cref="JsonStorage"/>, a <see cref="JsonNode"/> can be retrieved from
    /// <see cref="JsonStorage.LastJsonNodeLoaded"/> or <see cref="JsonStorage.LastJsonNodeSaved"/>.
    /// </remarks>
    /// <seealso cref="JsonStorage"/>
    public static void CompareJsonNodes(JsonNode? expectedJsonNode, JsonNode? actualJsonNode)
    {
        Assert.NotNull(expectedJsonNode);
        Assert.NotNull(actualJsonNode);

        var expected = JToken.Parse(expectedJsonNode.ToJsonString());
        var actual = JToken.Parse(actualJsonNode.ToJsonString());

        actual.Should().BeEquivalentTo(expected);
    }
}