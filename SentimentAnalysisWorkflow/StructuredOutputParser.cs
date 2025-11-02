using System.Text.Json;
using System.Text.RegularExpressions;

namespace SentimentAnalysisWorkflow;

/// <summary>
/// Helper class for parsing structured output from AI agent responses
/// </summary>
internal static class StructuredOutputParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Parses a structured response from AI agent text output
    /// </summary>
    /// <typeparam name="T">The type to deserialize to</typeparam>
    /// <param name="responseText">The raw response text from the AI agent</param>
    /// <returns>The deserialized structured response</returns>
    /// <exception cref="JsonException">Thrown when JSON parsing fails</exception>
    public static T ParseStructuredResponse<T>(string responseText) where T : class
    {
        if (string.IsNullOrWhiteSpace(responseText))
        {
            throw new JsonException("Response text is empty");
        }

        var jsonText = ExtractJsonFromResponse(responseText);

        try
        {
            return JsonSerializer.Deserialize<T>(jsonText, JsonOptions)
                ?? throw new JsonException("Failed to deserialize response");
        }
        catch (JsonException ex)
        {
            throw new JsonException($"Failed to parse structured response: {ex.Message}", ex);
        }
    }

    private static string ExtractJsonFromResponse(string responseText)
    {
        var trimmed = responseText.Trim();

        if (trimmed.StartsWith('{') && trimmed.EndsWith('}'))
        {
            return trimmed;
        }

        var jsonMatch = Regex.Match(trimmed, @"\{[\s\S]*\}", RegexOptions.Multiline);
        if (jsonMatch.Success)
        {
            return jsonMatch.Value;
        }

        var codeBlockMatch = Regex.Match(trimmed, @"```(?:json)?\s*(\{[\s\S]*?\})\s*```", RegexOptions.Multiline);
        if (codeBlockMatch.Success)
        {
            return codeBlockMatch.Groups[1].Value;
        }

        throw new JsonException("Could not extract JSON from response");
    }
}

