using System.Text.Json.Serialization;

namespace SentimentAnalysisWorkflow;

/// <summary>
/// Structured output DTO for sentiment analysis response from AI
/// </summary>
internal sealed record SentimentAnalysisResponse
{
    [JsonPropertyName("sentiment")]
    public string Sentiment { get; init; } = string.Empty;

    [JsonPropertyName("sentimentScore")]
    public double SentimentScore { get; init; }

    [JsonPropertyName("confidence")]
    public double Confidence { get; init; }

    [JsonPropertyName("positiveIndicators")]
    public List<string> PositiveIndicators { get; init; } = [];

    [JsonPropertyName("negativeIndicators")]
    public List<string> NegativeIndicators { get; init; } = [];

    [JsonPropertyName("reasoning")]
    public string Reasoning { get; init; } = string.Empty;
}

