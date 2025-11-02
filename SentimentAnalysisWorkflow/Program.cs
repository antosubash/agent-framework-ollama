using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI;
using OllamaSharp;

namespace SentimentAnalysisWorkflow;

/// <summary>
/// Sentiment Analysis Workflow Demo
/// This demonstrates AI-powered sequential workflows with two agents:
/// 1. AISentimentAnalysisAgent - Analyzes sentiment in input text using LLM
/// 2. AISentimentReportAgent - Generates a detailed sentiment report using LLM
/// </summary>
internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("=== Sentiment Analysis Workflow Demo ===");
        Console.WriteLine("This demo shows AI-powered workflow execution using LLM");
        Console.WriteLine();

        var endpoint = "http://localhost:11434";
        var modelName = "qwen3:8b";

        const string AgentName = "Sentiment Analyzer";
        const string AgentInstructions = "You are an expert at analyzing sentiment in text and generating detailed reports.";

        using OllamaApiClient chatClient = new(new Uri(endpoint), modelName);

        AIAgent agent = new ChatClientAgent(chatClient, AgentInstructions, AgentName);

        await RunWorkflowDemo(agent);
    }

    /// <summary>
    /// Demonstrates sentiment analysis workflow execution using AI agents
    /// </summary>
    private static async Task RunWorkflowDemo(AIAgent agent)
    {
        Console.WriteLine("🤖 SENTIMENT ANALYSIS WORKFLOW DEMO");
        Console.WriteLine("====================================");
        Console.WriteLine();

        var testCases = new[]
        {
            "I absolutely love this product! It's fantastic and exceeded all my expectations!",
            "This is the worst experience I've ever had. I'm extremely disappointed and frustrated.",
            "The service was adequate. Nothing particularly good or bad about it.",
            "I'm thrilled and overjoyed! This is the best day ever!",
            "I'm really concerned about this issue. It's problematic and needs immediate attention."
        };

        foreach (var testCase in testCases)
        {
            Console.WriteLine($"🤖 Processing: \"{testCase}\"");
            Console.WriteLine();

            try
            {
                var sentimentAnalysisAgent = new AISentimentAnalysisAgent(agent);
                var sentimentReportAgent = new AISentimentReportAgent(agent);

                var builder = new WorkflowBuilder(sentimentAnalysisAgent);
                builder.AddEdge(sentimentAnalysisAgent, sentimentReportAgent).WithOutputFrom(sentimentReportAgent);
                var workflow = builder.Build();

                Run run = await InProcessExecution.RunAsync(workflow, testCase);

                foreach (WorkflowEvent evt in run.NewEvents)
                {
                    if (evt is ExecutorCompletedEvent executorComplete)
                    {
                        Console.WriteLine($"🤖 Agent '{executorComplete.ExecutorId}' completed:");

                        if (executorComplete.Data is SentimentAnalysisResult analysisResult)
                        {
                            Console.WriteLine($"  📝 Original Text: {analysisResult.OriginalText}");
                            Console.WriteLine($"  😊 Sentiment: {analysisResult.Sentiment}");
                            Console.WriteLine($"  📊 Sentiment Score: {analysisResult.SentimentScore:F2}");
                            Console.WriteLine($"  🧠 Confidence: {analysisResult.Confidence:P1}");
                            if (analysisResult.PositiveIndicators.Count > 0)
                            {
                                Console.WriteLine($"  ✅ Positive Indicators: {string.Join(", ", analysisResult.PositiveIndicators)}");
                            }
                            if (analysisResult.NegativeIndicators.Count > 0)
                            {
                                Console.WriteLine($"  ❌ Negative Indicators: {string.Join(", ", analysisResult.NegativeIndicators)}");
                            }
                            Console.WriteLine($"  💭 Reasoning: {analysisResult.Reasoning}");
                        }
                        else if (executorComplete.Data is SentimentReportResult reportResult)
                        {
                            Console.WriteLine($"  📄 Sentiment: {reportResult.Sentiment}");
                            Console.WriteLine($"  📊 Sentiment Score: {reportResult.SentimentScore:F2}");
                            Console.WriteLine($"  🧠 Confidence: {reportResult.Confidence:P1}");
                            Console.WriteLine($"  📈 Metrics: {string.Join(", ", reportResult.Metrics.Select(m => $"{m.Key}: {m.Value}"))}");
                            Console.WriteLine($"  💭 Reasoning: {reportResult.Reasoning}");
                            Console.WriteLine($"  📋 Report:\n{reportResult.Report}");
                        }
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in processing: {ex.Message}");
                Console.WriteLine("💡 Make sure Ollama is running with the qwen3:8b model");
            }

            Console.WriteLine("---");
            Console.WriteLine();
        }

        Console.WriteLine("✅ Sentiment analysis workflow execution completed!");
    }
}
