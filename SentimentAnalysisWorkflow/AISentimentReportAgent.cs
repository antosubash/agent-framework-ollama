using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Agents.AI;

namespace SentimentAnalysisWorkflow;

/// <summary>
/// AI-powered sentiment report generation agent using Ollama
/// </summary>
internal sealed class AISentimentReportAgent(AIAgent agent) : ReflectingExecutor<AISentimentReportAgent>("AISentimentReportAgent"),
    IMessageHandler<SentimentAnalysisResult, SentimentReportResult>
{
    private readonly AIAgent _agent = agent;

    public async ValueTask<SentimentReportResult> HandleAsync(SentimentAnalysisResult input, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var prompt = "Generate a comprehensive sentiment analysis report based on the following analysis results.\n\n" +
                    $"Original Text: \"{input.OriginalText}\"\n" +
                    $"Detected Sentiment: {input.Sentiment}\n" +
                    $"Sentiment Score: {input.SentimentScore:F2}\n" +
                    $"Confidence: {input.Confidence:P1}\n" +
                    $"Positive Indicators: {string.Join(", ", input.PositiveIndicators)}\n" +
                    $"Negative Indicators: {string.Join(", ", input.NegativeIndicators)}\n" +
                    $"AI Reasoning: {input.Reasoning}\n\n" +
                    "Please respond ONLY with valid JSON in the following format (no markdown, no code fences, just the JSON):\n" +
                    "{\n" +
                    "    \"report\": \"A comprehensive, well-formatted sentiment analysis report\",\n" +
                    "    \"metrics\": {\n" +
                    "        \"PositiveWords\": number,\n" +
                    "        \"NegativeWords\": number,\n" +
                    "        \"TotalWords\": number,\n" +
                    "        \"SentimentScoreScaled\": number (score * 100)\n" +
                    "    },\n" +
                    "    \"confidence\": 0.0-1.0,\n" +
                    "    \"reasoning\": \"Brief explanation of report generation\"\n" +
                    "}\n\n" +
                    "The report should be detailed, professional, and include actionable insights.";

        var response = await _agent.RunAsync(prompt);
        var responseText = response.ToString();

        var structuredResponse = StructuredOutputParser.ParseStructuredResponse<SentimentReportResponse>(responseText);
        var report = !string.IsNullOrWhiteSpace(structuredResponse.Report)
            ? structuredResponse.Report.Replace("\\n", "\n")
            : Constants.ReportGenerationFailed;

        var metrics = MapMetrics(structuredResponse.Metrics);
        var confidence = Math.Clamp(structuredResponse.Confidence, Constants.MinConfidence, Constants.MaxConfidence);
        var reasoning = !string.IsNullOrWhiteSpace(structuredResponse.Reasoning)
            ? structuredResponse.Reasoning
            : Constants.DefaultReportReasoning;

        return new SentimentReportResult(
            input.OriginalText,
            input.SentimentScore,
            input.Sentiment,
            report,
            metrics,
            confidence,
            reasoning);
    }

    private static Dictionary<string, int> MapMetrics(SentimentReportMetrics? metrics)
    {
        if (metrics == null)
        {
            return new Dictionary<string, int>
            {
                ["PositiveWords"] = 0,
                ["NegativeWords"] = 0,
                ["TotalWords"] = 0,
                ["SentimentScoreScaled"] = 0
            };
        }

        return new Dictionary<string, int>
        {
            ["PositiveWords"] = metrics.PositiveWords,
            ["NegativeWords"] = metrics.NegativeWords,
            ["TotalWords"] = metrics.TotalWords,
            ["SentimentScoreScaled"] = metrics.SentimentScoreScaled
        };
    }
}

