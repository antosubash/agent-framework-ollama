using Microsoft.Agents.AI.Workflows;

namespace AgentWithWorkflowOllama;

/// <summary>
/// Service for executing streaming profanity filter workflows
/// </summary>
internal sealed class StreamingProfanityFilterService
{
    /// <summary>
    /// Executes a streaming profanity filter workflow
    /// </summary>
    public static async Task ProcessStreamingTextAsync(string text, int chunkSize = 50)
    {
        Console.WriteLine($"🔄 Starting streaming profanity filter for text: \"{text}\"");
        Console.WriteLine($"📦 Chunk size: {chunkSize} characters");
        Console.WriteLine();

        var textSource = new StreamingTextSource(text, chunkSize);
        var allFilteredChunks = new List<string>();
        var totalWordsFiltered = 0;

        await foreach (var chunk in textSource)
        {
            Console.WriteLine($"📥 Processing chunk: \"{chunk}\"");
            
            // Create fresh executors and workflow for each chunk
            var profanityDetector = new StreamingProfanityDetectorExecutor();
            var textFilter = new StreamingTextFilterExecutor();

            // Build the workflow by connecting executors sequentially
            var builder = new WorkflowBuilder(profanityDetector);
            builder.AddEdge(profanityDetector, textFilter).WithOutputFrom(textFilter);
            var workflow = builder.Build();

            // Execute the workflow with the chunk
            var run = await InProcessExecution.RunAsync(workflow, chunk);
            
            // Process the results
            foreach (var evt in run.NewEvents)
            {
                if (evt is ExecutorCompletedEvent executorComplete)
                {
                    if (executorComplete.ExecutorId == "StreamingProfanityDetectorExecutor")
                    {
                        if (executorComplete.Data is ProfanityDetectionResult detectionResult)
                        {
                            Console.WriteLine($"  🔍 Detection: {(detectionResult.HasProfanity ? "❌ Profanity detected" : "✅ Clean")}");
                            if (detectionResult.DetectedProfaneWords.Count > 0)
                            {
                                Console.WriteLine($"  📝 Detected words: {string.Join(", ", detectionResult.DetectedProfaneWords)}");
                            }
                        }
                    }
                    else if (executorComplete.ExecutorId == "StreamingTextFilterExecutor")
                    {
                        if (executorComplete.Data is FilteredTextResult filterResult)
                        {
                            Console.WriteLine($"  🛡️ Filtered: \"{filterResult.FilteredText}\"");
                            Console.WriteLine($"  📊 Words filtered: {filterResult.WordsFiltered}");
                            
                            allFilteredChunks.Add(filterResult.FilteredText);
                            totalWordsFiltered += filterResult.WordsFiltered;
                        }
                    }
                }
            }
            
            Console.WriteLine();
        }

        // Display final results
        var finalText = string.Join(' ', allFilteredChunks);
        Console.WriteLine("🎯 Streaming Processing Complete!");
        Console.WriteLine($"📄 Original text: \"{text}\"");
        Console.WriteLine($"🛡️ Filtered text: \"{finalText}\"");
        Console.WriteLine($"📊 Total words filtered: {totalWordsFiltered}");
        Console.WriteLine($"📦 Total chunks processed: {allFilteredChunks.Count}");
        Console.WriteLine();
    }
}
