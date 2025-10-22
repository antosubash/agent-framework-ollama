using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace AgentWithWorkflowOllama;

/// <summary>
/// Profanity Filter Workflow Demo
/// This demonstrates both standard and streaming sequential workflows with two executors:
/// 1. ProfanityDetectorExecutor - Identifies profane words in input text
/// 2. TextFilterExecutor - Replaces profane words with asterisks
/// </summary>
internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("=== Profanity Filter Workflow Demo ===");
        Console.WriteLine("This demo shows both standard and streaming workflow execution");
        Console.WriteLine();

        var endpoint = "http://localhost:11434";
        var modelName = "qwen3:8b";

        const string AgentName = "Profanity Checker";
        const string AgentInstructions = "You are good at checking for profanity in text.";

        using OllamaApiClient chatClient = new(new Uri(endpoint), modelName);

        AIAgent agent = new ChatClientAgent(chatClient, AgentInstructions, AgentName);

        // Run standard workflow demo
        await RunStandardWorkflowDemo();

        Console.WriteLine();
        Console.WriteLine(new string('=', 60));
        Console.WriteLine();

        // Run streaming workflow demo
        await RunStreamingWorkflowDemo();

        Console.WriteLine();
        Console.WriteLine(new string('=', 60));
        Console.WriteLine();

        // Run AI agent workflow demo
        await RunAgentWorkflowDemo(agent);

        Console.WriteLine();
        Console.WriteLine(new string('=', 60));
        Console.WriteLine();

        // Run streaming AI agent workflow demo
        await RunStreamingAgentWorkflowDemo(agent);

        Console.WriteLine();
        Console.WriteLine(new string('=', 60));
        Console.WriteLine();

        // Run real-time streaming AI demo
        await RunRealTimeStreamingAIDemo(agent);
    }

    /// <summary>
    /// Demonstrates standard workflow execution
    /// </summary>
    private static async Task RunStandardWorkflowDemo()
    {
        Console.WriteLine("📋 STANDARD WORKFLOW DEMO");
        Console.WriteLine("=========================");
        Console.WriteLine();

        // Test cases with different levels of profanity
        var testCases = new[]
        {
            "Hello, this is a clean message!",
            "This is a damn stupid message with profanity",
            "What the hell is going on here?",
            "You are such an idiot and moron!",
            "I hate this crap so much",
            "This message is completely clean and appropriate"
        };

        foreach (var testCase in testCases)
        {
            Console.WriteLine($"Input: {testCase}");
            Console.WriteLine("Processing...");
            Console.WriteLine();

            // Create fresh executors and workflow for each test case
            ProfanityDetectorExecutor profanityDetector = new();
            TextFilterExecutor textFilter = new();

            // Build the workflow by connecting executors sequentially
            WorkflowBuilder builder = new(profanityDetector);
            builder.AddEdge(profanityDetector, textFilter).WithOutputFrom(textFilter);
            var workflow = builder.Build();

            // Execute the workflow with input data
            Run run = await InProcessExecution.RunAsync(workflow, testCase);

            // Display executor events
            foreach (WorkflowEvent evt in run.NewEvents)
            {
                if (evt is ExecutorCompletedEvent executorComplete)
                {
                    Console.WriteLine($"Executor '{executorComplete.ExecutorId}' completed:");

                    if (executorComplete.Data is ProfanityDetectionResult detectionResult)
                    {
                        Console.WriteLine($"  - Original Text: {detectionResult.OriginalText}");
                        Console.WriteLine($"  - Has Profanity: {detectionResult.HasProfanity}");
                        if (detectionResult.DetectedProfaneWords.Count > 0)
                        {
                            Console.WriteLine($"  - Detected Words: {string.Join(", ", detectionResult.DetectedProfaneWords)}");
                        }
                    }
                    else if (executorComplete.Data is FilteredTextResult filterResult)
                    {
                        Console.WriteLine($"  - Original Text: {filterResult.OriginalText}");
                        Console.WriteLine($"  - Filtered Text: {filterResult.FilteredText}");
                        Console.WriteLine($"  - Words Filtered: {filterResult.WordsFiltered}");
                    }
                    Console.WriteLine();
                }
            }

            Console.WriteLine("---");
            Console.WriteLine();
        }

        Console.WriteLine("✅ Standard workflow execution completed!");
    }

    /// <summary>
    /// Demonstrates streaming workflow execution
    /// </summary>
    private static async Task RunStreamingWorkflowDemo()
    {
        Console.WriteLine("🌊 STREAMING WORKFLOW DEMO");
        Console.WriteLine("==========================");
        Console.WriteLine();

        // Test cases for streaming (longer text to demonstrate chunking)
        var streamingTestCases = new[]
        {
            "This is a damn stupid message with profanity that needs to be filtered out completely",
            "What the hell is going on here? This is such a crap situation and I hate it so much",
            "You are such an idiot and moron! This is completely stupid and I damn well know it",
            "Hello world! This is a completely clean message with no profanity at all, just normal text"
        };

        foreach (var testCase in streamingTestCases)
        {
            Console.WriteLine($"📝 Testing streaming with: \"{testCase}\"");
            Console.WriteLine();

            await StreamingProfanityFilterService.ProcessStreamingTextAsync(testCase, chunkSize: 30);

            Console.WriteLine(new string('=', 50));
            Console.WriteLine();
        }

        Console.WriteLine("✅ Streaming workflow execution completed!");
    }

    /// <summary>
    /// Demonstrates AI agent workflow execution
    /// </summary>
    private static async Task RunAgentWorkflowDemo(AIAgent agent)
    {
        Console.WriteLine("🤖 AI AGENT WORKFLOW DEMO");
        Console.WriteLine("=========================");
        Console.WriteLine();

        // Test cases for AI agent processing
        var agentTestCases = new[]
        {
            "Hello, this is a clean message!",
            "You are such an idiot and moron!",
            "I hate this crap so much",
        };

        foreach (var testCase in agentTestCases)
        {
            Console.WriteLine($"🤖 AI Agent Processing: \"{testCase}\"");
            Console.WriteLine();

            try
            {
                // Create AI-powered profanity detection agent
                var profanityDetectionAgent = new AIProfanityDetectionAgent(agent);
                var textFilteringAgent = new AITextFilteringAgent(agent);

                // Build agent-based workflow
                var builder = new WorkflowBuilder(profanityDetectionAgent);
                builder.AddEdge(profanityDetectionAgent, textFilteringAgent).WithOutputFrom(textFilteringAgent);
                var workflow = builder.Build();

                // Execute the workflow with input data
                Run run = await InProcessExecution.RunAsync(workflow, testCase);

                // Display executor events
                foreach (WorkflowEvent evt in run.NewEvents)
                {
                    if (evt is ExecutorCompletedEvent executorComplete)
                    {
                        Console.WriteLine($"🤖 Agent '{executorComplete.ExecutorId}' completed:");

                        if (executorComplete.Data is AIProfanityDetectionResult detectionResult)
                        {
                            Console.WriteLine($"  📝 Original Text: {detectionResult.OriginalText}");
                            Console.WriteLine($"  🔍 AI Detection: {(detectionResult.HasProfanity ? "❌ Profanity detected" : "✅ Clean")}");
                            Console.WriteLine($"  🧠 AI Confidence: {detectionResult.Confidence:P1}");
                            if (detectionResult.DetectedProfaneWords.Count > 0)
                            {
                                Console.WriteLine($"  📋 Detected Words: {string.Join(", ", detectionResult.DetectedProfaneWords)}");
                            }
                            Console.WriteLine($"  💭 AI Reasoning: {detectionResult.Reasoning}");
                        }
                        else if (executorComplete.Data is AITextFilterResult filterResult)
                        {
                            Console.WriteLine($"  📄 Original Text: {filterResult.OriginalText}");
                            Console.WriteLine($"  🛡️ Filtered Text: {filterResult.FilteredText}");
                            Console.WriteLine($"  📊 Words Filtered: {filterResult.WordsFiltered}");
                            Console.WriteLine($"  🧠 AI Confidence: {filterResult.Confidence:P1}");
                            Console.WriteLine($"  💭 AI Reasoning: {filterResult.Reasoning}");
                        }
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in AI agent processing: {ex.Message}");
                Console.WriteLine("💡 Make sure Ollama is running with the qwen3:8b model");
            }

            Console.WriteLine("---");
            Console.WriteLine();
        }

        Console.WriteLine("✅ AI Agent workflow execution completed!");
    }

    /// <summary>
    /// Demonstrates streaming AI agent workflow execution
    /// </summary>
    private static async Task RunStreamingAgentWorkflowDemo(AIAgent agent)
    {
        Console.WriteLine("🌊🤖 STREAMING AI AGENT WORKFLOW DEMO");
        Console.WriteLine("====================================");
        Console.WriteLine();

        // Test cases for streaming AI processing (longer text to demonstrate chunking)
        var streamingAITestCases = new[]
        {
            "This is a damn stupid message with profanity that needs intelligent AI filtering",
            "What the hell is going on here? This is such a crap situation and I hate it so much",
            "You are such an idiot and moron! This is completely stupid and I damn well know it",
            "Hello world! This is a completely clean message with no profanity at all, just normal text that should pass through",
            "The word profanity itself is not profane, but damn it, this sentence contains actual profanity that needs filtering"
        };

        foreach (var testCase in streamingAITestCases)
        {
            Console.WriteLine($"🤖🌊 Testing streaming AI with: \"{testCase}\"");
            Console.WriteLine();
            
            try
            {
                await StreamingAIProfanityFilterService.ProcessStreamingAITextAsync(testCase, agent, chunkSize: 40);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in streaming AI processing: {ex.Message}");
                Console.WriteLine("💡 Make sure Ollama is running with the qwen3:8b model");
            }
            
            Console.WriteLine(new string('=', 50));
            Console.WriteLine();
        }

        Console.WriteLine("✅ Streaming AI Agent workflow execution completed!");
    }

    /// <summary>
    /// Demonstrates real-time streaming AI analysis
    /// </summary>
    private static async Task RunRealTimeStreamingAIDemo(AIAgent agent)
    {
        Console.WriteLine("🌊🤖 REAL-TIME STREAMING AI DEMO");
        Console.WriteLine("=================================");
        Console.WriteLine();

        // Test cases for real-time streaming AI processing
        var realTimeTestCases = new[]
        {
            "This is a damn stupid message that needs real-time AI analysis",
            "What the hell is going on here? This needs immediate streaming analysis",
            "Hello world! This is a clean message for real-time processing"
        };

        foreach (var testCase in realTimeTestCases)
        {
            Console.WriteLine($"🌊🤖 Real-time streaming analysis for: \"{testCase}\"");
            Console.WriteLine();
            
            try
            {
                await StreamingAIProfanityFilterService.ProcessStreamingAITextAsync(testCase, agent, chunkSize: 30);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in real-time streaming: {ex.Message}");
                Console.WriteLine("💡 Make sure Ollama is running with the qwen3:8b model");
            }
            
            Console.WriteLine(new string('=', 50));
            Console.WriteLine();
        }

        Console.WriteLine("✅ Real-time streaming AI demo completed!");
    }
}

