using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Agents.AI;

namespace AgentWithWorkflowOllama;

/// <summary>
/// AI-powered text filtering agent using Ollama
/// </summary>
internal sealed class AITextFilteringAgent(AIAgent agent) : ReflectingExecutor<AITextFilteringAgent>("AITextFilteringAgent"),
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
                "No profanity detected - text unchanged");
        }

        try
        {
            // Create a specialized prompt for text filtering
            var prompt = "Filter the following text by replacing profane and inappropriate words with asterisks (*). " +
                        "Preserve the original structure, punctuation, and spacing.\n\n" +
                        $"Original text: \"{input.OriginalText}\"\n" +
                        $"Detected profane words: {string.Join(", ", input.DetectedProfaneWords)}\n\n" +
                        "Please respond in the following JSON format:\n" +
                        "{\n" +
                        "    \"filteredText\": \"the filtered text with asterisks\",\n" +
                        "    \"wordsFiltered\": number,\n" +
                        "    \"confidence\": 0.0-1.0,\n" +
                        "    \"reasoning\": \"Brief explanation of filtering decisions\"\n" +
                        "}\n\n" +
                        "Guidelines:\n" +
                        "- Replace each profane word with asterisks (*) matching the word length\n" +
                        "- Preserve punctuation and spacing\n" +
                        "- Maintain the original text structure\n" +
                        "- Be consistent with word boundaries";

            var response = await _agent.RunAsync(prompt);
            var responseText = response.ToString();

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
        catch (Exception ex)
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
        return "AI filtering completed";
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
