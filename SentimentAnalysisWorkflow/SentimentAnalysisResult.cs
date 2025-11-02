namespace SentimentAnalysisWorkflow;

/// <summary>
/// Result of sentiment analysis containing the analysis details
/// </summary>
internal sealed record SentimentAnalysisResult(
    string OriginalText,
    double SentimentScore,
    SentimentType Sentiment,
    double Confidence,
    string Reasoning,
    List<string> PositiveIndicators,
    List<string> NegativeIndicators);
