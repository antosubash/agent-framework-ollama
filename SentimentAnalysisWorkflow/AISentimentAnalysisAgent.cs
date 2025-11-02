using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Agents.AI;

namespace SentimentAnalysisWorkflow;

/// <summary>
/// AI-powered sentiment analysis agent using Ollama
/// </summary>
internal sealed class AISentimentAnalysisAgent(AIAgent agent) : ReflectingExecutor<AISentimentAnalysisAgent>("AISentimentAnalysisAgent"),
    IMessageHandler<string, SentimentAnalysisResult>
{
    private readonly AIAgent _agent = agent;

    public async ValueTask<SentimentAnalysisResult> HandleAsync(string input, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new SentimentAnalysisResult(
                input,
                Constants.DefaultSentimentScore,
                SentimentType.Neutral,
                Constants.EmptyInputConfidence,
                Constants.EmptyInputReasoning,
                [],
                []);
        }

        var prompt = "Analyze the sentiment of the following text. " +
                    "Determine if it is positive, negative, or neutral, and provide a sentiment score.\n\n" +
                    $"Text to analyze: \"{input}\"\n\n" +
                    "Please respond ONLY with valid JSON in the following format (no markdown, no code fences, just the JSON):\n" +
                    "{\n" +
                    "    \"sentiment\": \"positive\" | \"negative\" | \"neutral\",\n" +
                    "    \"sentimentScore\": -1.0 to 1.0 (where -1.0 is very negative, 0.0 is neutral, 1.0 is very positive),\n" +
                    "    \"confidence\": 0.0-1.0,\n" +
                    "    \"positiveIndicators\": [\"word1\", \"word2\"],\n" +
                    "    \"negativeIndicators\": [\"word1\", \"word2\"],\n" +
                    "    \"reasoning\": \"Brief explanation of your sentiment analysis\"\n" +
                    "}\n\n" +
                    "Be thorough and consider context, tone, and emotional cues.";

        var response = await _agent.RunAsync(prompt);
        var responseText = response.ToString();

        var structuredResponse = StructuredOutputParser.ParseStructuredResponse<SentimentAnalysisResponse>(responseText);

        var sentiment = MapSentimentType(structuredResponse.Sentiment);
        var sentimentScore = Math.Clamp(structuredResponse.SentimentScore, Constants.MinSentimentScore, Constants.MaxSentimentScore);
        var confidence = Math.Clamp(structuredResponse.Confidence, Constants.MinConfidence, Constants.MaxConfidence);
        var positiveIndicators = structuredResponse.PositiveIndicators ?? [];
        var negativeIndicators = structuredResponse.NegativeIndicators ?? [];
        var reasoning = !string.IsNullOrWhiteSpace(structuredResponse.Reasoning)
            ? structuredResponse.Reasoning
            : Constants.DefaultAnalysisReasoning;

        return new SentimentAnalysisResult(
            input,
            sentimentScore,
            sentiment,
            confidence,
            reasoning,
            positiveIndicators,
            negativeIndicators);
    }

    private static SentimentType MapSentimentType(string sentiment)
    {
        return sentiment.Trim().ToLowerInvariant() switch
        {
            "positive" => SentimentType.Positive,
            "negative" => SentimentType.Negative,
            _ => SentimentType.Neutral
        };
    }
}

