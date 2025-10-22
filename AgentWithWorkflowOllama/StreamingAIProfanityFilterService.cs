using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI;

namespace AgentWithWorkflowOllama;

/// <summary>
/// Service for executing streaming AI-powered profanity filter workflows
/// </summary>
internal sealed class StreamingAIProfanityFilterService
{
    /// <summary>
    /// Executes a streaming AI-powered profanity filter workflow
    /// </summary>
    public static async Task ProcessStreamingAITextAsync(string text, AIAgent agent, int chunkSize = 50)
    {
        Console.WriteLine($"🤖 Starting streaming AI profanity filter for text: \"{text}\"");
        Console.WriteLine($"📦 Chunk size: {chunkSize} characters");
        Console.WriteLine();

        var textSource = new StreamingTextSource(text, chunkSize);
        var allFilteredChunks = new List<string>();
        var totalWordsFiltered = 0;
        var totalConfidence = 0.0;
        var chunkCount = 0;

        await foreach (var chunk in textSource)
        {
            Console.WriteLine($"📥 Processing chunk: \"{chunk}\"");
            
            try
            {
                // Create fresh AI agents and workflow for each chunk
                var profanityDetectionAgent = new StreamingAIProfanityDetectionAgent(agent);
                var textFilteringAgent = new StreamingAITextFilteringAgent(agent);

                // Build the AI workflow by connecting agents sequentially
                var builder = new WorkflowBuilder(profanityDetectionAgent);
                builder.AddEdge(profanityDetectionAgent, textFilteringAgent).WithOutputFrom(textFilteringAgent);
                var workflow = builder.Build();

                // Execute the AI workflow with the chunk
                var run = await InProcessExecution.RunAsync(workflow, chunk);
                
                // Process the AI results
                foreach (var evt in run.NewEvents)
                {
                    if (evt is ExecutorCompletedEvent executorComplete)
                    {
                        if (executorComplete.ExecutorId == "StreamingAIProfanityDetectionAgent")
                        {
                            if (executorComplete.Data is AIProfanityDetectionResult detectionResult)
                            {
                                Console.WriteLine($"  🤖 AI Detection: {(detectionResult.HasProfanity ? "❌ Profanity detected" : "✅ Clean")}");
                                Console.WriteLine($"  🧠 AI Confidence: {detectionResult.Confidence:P1}");
                                if (detectionResult.DetectedProfaneWords.Count > 0)
                                {
                                    Console.WriteLine($"  📝 Detected words: {string.Join(", ", detectionResult.DetectedProfaneWords)}");
                                }
                                Console.WriteLine($"  💭 AI Reasoning: {detectionResult.Reasoning}");
                            }
                        }
                        else if (executorComplete.ExecutorId == "StreamingAITextFilteringAgent")
                        {
                            if (executorComplete.Data is AITextFilterResult filterResult)
                            {
                                Console.WriteLine($"  🛡️ AI Filtered: \"{filterResult.FilteredText}\"");
                                Console.WriteLine($"  📊 Words filtered: {filterResult.WordsFiltered}");
                                Console.WriteLine($"  🧠 AI Confidence: {filterResult.Confidence:P1}");
                                Console.WriteLine($"  💭 AI Reasoning: {filterResult.Reasoning}");
                                
                                allFilteredChunks.Add(filterResult.FilteredText);
                                totalWordsFiltered += filterResult.WordsFiltered;
                                totalConfidence += filterResult.Confidence;
                                chunkCount++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ❌ AI processing failed: {ex.Message}");
                Console.WriteLine($"  🔄 Falling back to rule-based processing...");
                
                // Fallback to rule-based processing
                var fallbackResult = await ProcessChunkWithFallback(chunk);
                allFilteredChunks.Add(fallbackResult);
                chunkCount++;
            }
            
            Console.WriteLine();
        }

        // Display final results
        var finalText = string.Join(' ', allFilteredChunks);
        var averageConfidence = chunkCount > 0 ? totalConfidence / chunkCount : 0.0;

        Console.WriteLine("🎯 Streaming AI Processing Complete!");
        Console.WriteLine($"📄 Original text: \"{text}\"");
        Console.WriteLine($"🛡️ AI Filtered text: \"{finalText}\"");
        Console.WriteLine($"📊 Total words filtered: {totalWordsFiltered}");
        Console.WriteLine($"📦 Total chunks processed: {chunkCount}");
        Console.WriteLine($"🧠 Average AI confidence: {averageConfidence:P1}");
        Console.WriteLine();
    }

    /// <summary>
    /// Fallback processing using rule-based detection when AI fails
    /// </summary>
    private static async Task<string> ProcessChunkWithFallback(string chunk)
    {
        var words = chunk.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var filteredWords = new List<string>();

        foreach (var word in words)
        {
            var cleanWord = CleanWord(word);
            if (cleanWord.Length >= Constants.MinimumWordLength && 
                Constants.ProfaneWords.Contains(cleanWord.ToLowerInvariant()))
            {
                var replacement = new string(Constants.ReplacementCharacter, cleanWord.Length);
                var filteredWord = word.Replace(cleanWord, replacement, StringComparison.OrdinalIgnoreCase);
                filteredWords.Add(filteredWord);
            }
            else
            {
                filteredWords.Add(word);
            }
        }

        return string.Join(' ', filteredWords);
    }

    private static string CleanWord(string word)
    {
        return new string(word.Where(char.IsLetter).ToArray());
    }
}
