using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Agents.AI;

namespace AgentWithWorkflowOllama;

/// <summary>
/// AI-powered profanity detection agent using Ollama
/// </summary>
internal sealed class AIProfanityDetectionAgent(AIAgent agent) : ReflectingExecutor<AIProfanityDetectionAgent>("AIProfanityDetectionAgent"),
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
            // Create a specialized prompt for profanity detection
            var prompt = "Analyze the following text for profanity and inappropriate language. " +
                        "Consider words that are commonly considered offensive, vulgar, or inappropriate in professional settings.\n\n" +
                        $"Text to analyze: \"{input}\"\n\n" +
                        "Please respond in the following JSON format:\n" +
                        "{\n" +
                        "    \"hasProfanity\": true/false,\n" +
                        "    \"detectedWords\": [\"word1\", \"word2\"],\n" +
                        "    \"confidence\": 0.0-1.0,\n" +
                        "    \"reasoning\": \"Brief explanation of your analysis\"\n" +
                        "}\n\n" +
                        "Only include words that are clearly profane or inappropriate. Be conservative in your assessment.";

            var response = await _agent.RunAsync(prompt);
            var responseText = response.ToString();

            // Parse the AI response (simplified parsing - in production, use proper JSON parsing)
            var hasProfanity = responseText.Contains("\"hasProfanity\": true", StringComparison.OrdinalIgnoreCase);
            var confidence = ExtractConfidence(responseText);
            var detectedWords = ExtractDetectedWords(responseText);
            var reasoning = ExtractReasoning(responseText);

            return new AIProfanityDetectionResult(input, detectedWords, hasProfanity, confidence, reasoning);
        }
        catch (Exception ex)
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
        return "AI analysis completed";
    }

    private static string CleanWord(string word)
    {
        return new string(word.Where(char.IsLetter).ToArray());
    }
}
