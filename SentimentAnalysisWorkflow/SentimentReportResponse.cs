using System.Text.Json.Serialization;

namespace SentimentAnalysisWorkflow;

/// <summary>
/// Structured output DTO for sentiment report response from AI
/// </summary>
internal sealed record SentimentReportResponse
{
    [JsonPropertyName("report")]
    public string Report { get; init; } = string.Empty;

    [JsonPropertyName("metrics")]
    public SentimentReportMetrics? Metrics { get; init; }

    [JsonPropertyName("confidence")]
    public double Confidence { get; init; }

    [JsonPropertyName("reasoning")]
    public string Reasoning { get; init; } = string.Empty;
}

/// <summary>
/// Metrics structure for sentiment report
/// </summary>
internal sealed record SentimentReportMetrics
{
    [JsonPropertyName("PositiveWords")]
    public int PositiveWords { get; init; }

    [JsonPropertyName("NegativeWords")]
    public int NegativeWords { get; init; }

    [JsonPropertyName("TotalWords")]
    public int TotalWords { get; init; }

    [JsonPropertyName("SentimentScoreScaled")]
    public int SentimentScoreScaled { get; init; }
}

