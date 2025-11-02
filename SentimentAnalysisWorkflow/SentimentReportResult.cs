namespace SentimentAnalysisWorkflow;

/// <summary>
/// Result of sentiment report generation containing the detailed report
/// </summary>
internal sealed record SentimentReportResult(
    string OriginalText,
    double SentimentScore,
    SentimentType Sentiment,
    string Report,
    Dictionary<string, int> Metrics,
    double Confidence,
    string Reasoning);

