namespace SentimentAnalysisWorkflow;

/// <summary>
/// Constants for the Sentiment Analysis Workflow
/// </summary>
internal static class Constants
{
    /// <summary>
    /// Default confidence value when extraction fails
    /// </summary>
    public const double DefaultConfidence = 0.5;

    /// <summary>
    /// Default sentiment score for neutral sentiment
    /// </summary>
    public const double DefaultSentimentScore = 0.0;

    /// <summary>
    /// Maximum confidence value
    /// </summary>
    public const double MaxConfidence = 1.0;

    /// <summary>
    /// Minimum confidence value
    /// </summary>
    public const double MinConfidence = 0.0;

    /// <summary>
    /// Maximum sentiment score
    /// </summary>
    public const double MaxSentimentScore = 1.0;

    /// <summary>
    /// Minimum sentiment score
    /// </summary>
    public const double MinSentimentScore = -1.0;

    /// <summary>
    /// Error message for empty input
    /// </summary>
    public const string EmptyInputReasoning = "Empty input - neutral sentiment";

    /// <summary>
    /// Error message when report generation fails
    /// </summary>
    public const string ReportGenerationFailed = "Report generation failed";

    /// <summary>
    /// Default reasoning message for AI analysis completion
    /// </summary>
    public const string DefaultAnalysisReasoning = "AI sentiment analysis completed";

    /// <summary>
    /// Default reasoning message for AI report generation
    /// </summary>
    public const string DefaultReportReasoning = "AI report generation completed";

    /// <summary>
    /// Default confidence for empty input
    /// </summary>
    public const double EmptyInputConfidence = 1.0;
}

