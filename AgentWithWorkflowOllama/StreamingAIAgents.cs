using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Agents.AI;

namespace AgentWithWorkflowOllama;

/// <summary>
/// Streaming AI-powered profanity detection agent using Ollama
/// </summary>
internal sealed class StreamingAIProfanityDetectionAgent(AIAgent agent) : ReflectingExecutor<StreamingAIProfanityDetectionAgent>("StreamingAIProfanityDetectionAgent"),
    IMessageHandler<string, AIProfanityDetectionResult>
{
    private readonly AIAgent _agent = agent;

    public async ValueTask<AIProfanityDetectionResult> HandleAsync(string input, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new AIProfanityDetectionResult(input, [], false, 1.0, "Empty input - no profanity detected");
        }

        try
        {
            // Create a specialized prompt for streaming profanity detection
            var prompt = "Analyze the following text chunk for profanity and inappropriate language. " +
                        "Consider words that are commonly considered offensive, vulgar, or inappropriate in professional settings.\n\n" +
                        $"Text chunk to analyze: \"{input}\"\n\n" +
                        "Please respond in the following JSON format:\n" +
                        "{\n" +
                        "    \"hasProfanity\": true/false,\n" +
                        "    \"detectedWords\": [\"word1\", \"word2\"],\n" +
                        "    \"confidence\": 0.0-1.0,\n" +
                        "    \"reasoning\": \"Brief explanation of your analysis for this chunk\"\n" +
                        "}\n\n" +
                        "Note: This is a text chunk from a larger document. Be conservative in your assessment and consider context.";

            // Use streaming AI response for real-time processing
            var responseBuilder = new System.Text.StringBuilder();
            var stream = _agent.RunStreamingAsync(prompt);
            
            Console.WriteLine("    🤖 AI streaming response:");
            var isFirstChunk = true;
            
            await foreach (var chunk in stream)
            {
                if (isFirstChunk)
                {
                    Console.Write("    💭 ");
                    isFirstChunk = false;
                }
                responseBuilder.Append(chunk);
                Console.Write(chunk);
                await Task.Delay(30); // Small delay to show streaming effect
            }
            Console.WriteLine(); // New line after streaming
            
            var responseText = responseBuilder.ToString();

            // Parse the AI response
            var hasProfanity = responseText.Contains("\"hasProfanity\": true", StringComparison.OrdinalIgnoreCase);
            var confidence = ExtractConfidence(responseText);
            var detectedWords = ExtractDetectedWords(responseText);
            var reasoning = ExtractReasoning(responseText);

            return new AIProfanityDetectionResult(input, detectedWords, hasProfanity, confidence, reasoning);
        }
        catch (Exception)
        {
            // Fallback to rule-based detection if AI fails
            return await FallbackDetection(input);
        }
    }

    private ValueTask<AIProfanityDetectionResult> FallbackDetection(string input)
    {
        var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var detectedProfaneWords = new List<string>();
        var hasProfanity = false;

        foreach (var word in words)
        {
            var cleanWord = CleanWord(word);
            if (cleanWord.Length >= Constants.MinimumWordLength && 
                Constants.ProfaneWords.Contains(cleanWord.ToLowerInvariant()))
            {
                detectedProfaneWords.Add(cleanWord);
                hasProfanity = true;
            }
        }

        return ValueTask.FromResult(new AIProfanityDetectionResult(
            input, 
            detectedProfaneWords, 
            hasProfanity, 
            0.8, // Lower confidence for fallback
            "AI detection failed, used rule-based fallback"));
    }

    private static double ExtractConfidence(string response)
    {
        var confidenceMatch = System.Text.RegularExpressions.Regex.Match(response, @"""confidence"":\s*([0-9.]+)");
        if (confidenceMatch.Success && double.TryParse(confidenceMatch.Groups[1].Value, out var confidence))
        {
            return Math.Clamp(confidence, 0.0, 1.0);
        }
        return 0.5; // Default confidence
    }

    private static List<string> ExtractDetectedWords(string response)
    {
        var words = new List<string>();
        var wordsMatch = System.Text.RegularExpressions.Regex.Match(response, @"""detectedWords"":\s*\[(.*?)\]");
        if (wordsMatch.Success)
        {
            var wordsText = wordsMatch.Groups[1].Value;
            var wordMatches = System.Text.RegularExpressions.Regex.Matches(wordsText, @"""([^""]+)""");
            foreach (System.Text.RegularExpressions.Match match in wordMatches)
            {
                words.Add(match.Groups[1].Value);
            }
        }
        return words;
    }

    private static string ExtractReasoning(string response)
    {
        var reasoningMatch = System.Text.RegularExpressions.Regex.Match(response, @"""reasoning"":\s*""([^""]+)""");
        if (reasoningMatch.Success)
        {
            return reasoningMatch.Groups[1].Value;
        }
        return "AI analysis completed for chunk";
    }

    private static string CleanWord(string word)
    {
        return new string(word.Where(char.IsLetter).ToArray());
    }
}

/// <summary>
/// Streaming AI-powered text filtering agent using Ollama
/// </summary>
internal sealed class StreamingAITextFilteringAgent(AIAgent agent) : ReflectingExecutor<StreamingAITextFilteringAgent>("StreamingAITextFilteringAgent"),
    IMessageHandler<AIProfanityDetectionResult, AITextFilterResult>
{
    private readonly AIAgent _agent = agent;

    public async ValueTask<AITextFilterResult> HandleAsync(AIProfanityDetectionResult input, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        if (!input.HasProfanity)
        {
            return new AITextFilterResult(
                input.OriginalText, 
                input.OriginalText, 
                0, 
                input.Confidence, 
                "No profanity detected in chunk - text unchanged");
        }

        try
        {
            // Create a specialized prompt for streaming text filtering
            var prompt = "Filter the following text chunk by replacing profane and inappropriate words with asterisks (*). " +
                        "Preserve the original structure, punctuation, and spacing.\n\n" +
                        $"Original text chunk: \"{input.OriginalText}\"\n" +
                        $"Detected profane words: {string.Join(", ", input.DetectedProfaneWords)}\n\n" +
                        "Please respond in the following JSON format:\n" +
                        "{\n" +
                        "    \"filteredText\": \"the filtered text with asterisks\",\n" +
                        "    \"wordsFiltered\": number,\n" +
                        "    \"confidence\": 0.0-1.0,\n" +
                        "    \"reasoning\": \"Brief explanation of filtering decisions for this chunk\"\n" +
                        "}\n\n" +
                        "Guidelines:\n" +
                        "- Replace each profane word with asterisks (*) matching the word length\n" +
                        "- Preserve punctuation and spacing\n" +
                        "- Maintain the original text structure\n" +
                        "- Be consistent with word boundaries\n" +
                        "- Note: This is a chunk from a larger document";

            // Use streaming AI response for real-time processing
            var responseBuilder = new System.Text.StringBuilder();
            var stream = _agent.RunStreamingAsync(prompt);
            
            Console.WriteLine("    🤖 AI streaming filter response:");
            var isFirstChunk = true;
            
            await foreach (var chunk in stream)
            {
                if (isFirstChunk)
                {
                    Console.Write("    🛡️ ");
                    isFirstChunk = false;
                }
                responseBuilder.Append(chunk);
                Console.Write(chunk);
                await Task.Delay(30); // Small delay to show streaming effect
            }
            Console.WriteLine(); // New line after streaming
            
            var responseText = responseBuilder.ToString();

            // Parse the AI response
            var filteredText = ExtractFilteredText(responseText);
            var wordsFiltered = ExtractWordsFiltered(responseText);
            var confidence = ExtractConfidence(responseText);
            var reasoning = ExtractReasoning(responseText);

            return new AITextFilterResult(
                input.OriginalText, 
                filteredText, 
                wordsFiltered, 
                confidence, 
                reasoning);
        }
        catch (Exception)
        {
            // Fallback to rule-based filtering if AI fails
            return await FallbackFiltering(input);
        }
    }

    private ValueTask<AITextFilterResult> FallbackFiltering(AIProfanityDetectionResult input)
    {
        var filteredText = input.OriginalText;
        var wordsFiltered = 0;

        foreach (var profaneWord in input.DetectedProfaneWords)
        {
            var replacement = new string(Constants.ReplacementCharacter, profaneWord.Length);
            filteredText = ReplaceWord(filteredText, profaneWord, replacement);
            wordsFiltered++;
        }

        return ValueTask.FromResult(new AITextFilterResult(
            input.OriginalText, 
            filteredText, 
            wordsFiltered, 
            0.8, // Lower confidence for fallback
            "AI filtering failed, used rule-based fallback"));
    }

    private static string ExtractFilteredText(string response)
    {
        var textMatch = System.Text.RegularExpressions.Regex.Match(response, @"""filteredText"":\s*""([^""]+)""");
        if (textMatch.Success)
        {
            return textMatch.Groups[1].Value;
        }
        return "Filtering failed";
    }

    private static int ExtractWordsFiltered(string response)
    {
        var countMatch = System.Text.RegularExpressions.Regex.Match(response, @"""wordsFiltered"":\s*(\d+)");
        if (countMatch.Success && int.TryParse(countMatch.Groups[1].Value, out var count))
        {
            return count;
        }
        return 0;
    }

    private static double ExtractConfidence(string response)
    {
        var confidenceMatch = System.Text.RegularExpressions.Regex.Match(response, @"""confidence"":\s*([0-9.]+)");
        if (confidenceMatch.Success && double.TryParse(confidenceMatch.Groups[1].Value, out var confidence))
        {
            return Math.Clamp(confidence, 0.0, 1.0);
        }
        return 0.5; // Default confidence
    }

    private static string ExtractReasoning(string response)
    {
        var reasoningMatch = System.Text.RegularExpressions.Regex.Match(response, @"""reasoning"":\s*""([^""]+)""");
        if (reasoningMatch.Success)
        {
            return reasoningMatch.Groups[1].Value;
        }
        return "AI filtering completed for chunk";
    }

    private static string ReplaceWord(string text, string wordToReplace, string replacement)
    {
        var words = text.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            var cleanWord = CleanWord(words[i]);
            if (string.Equals(cleanWord, wordToReplace, StringComparison.OrdinalIgnoreCase))
            {
                var originalWord = words[i];
                var newWord = originalWord.Replace(cleanWord, replacement, StringComparison.OrdinalIgnoreCase);
                words[i] = newWord;
            }
        }
        return string.Join(' ', words);
    }

    private static string CleanWord(string word)
    {
        return new string(word.Where(char.IsLetter).ToArray());
    }
}
