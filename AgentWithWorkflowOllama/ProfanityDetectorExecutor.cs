using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace AgentWithWorkflowOllama;

/// <summary>
/// Data structure to hold profanity detection results
/// </summary>
internal sealed record ProfanityDetectionResult(string OriginalText, List<string> DetectedProfaneWords, bool HasProfanity);

/// <summary>
/// First executor: detects profane words in the input text.
/// </summary>
internal sealed class ProfanityDetectorExecutor() : ReflectingExecutor<ProfanityDetectorExecutor>("ProfanityDetectorExecutor"),
    IMessageHandler<string, ProfanityDetectionResult>
{
    public ValueTask<ProfanityDetectionResult> HandleAsync(string input, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return ValueTask.FromResult(new ProfanityDetectionResult(input, [], false));
        }

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

        var result = new ProfanityDetectionResult(input, detectedProfaneWords, hasProfanity);
        return ValueTask.FromResult(result);
    }

    /// <summary>
    /// Removes punctuation from a word for comparison
    /// </summary>
    private static string CleanWord(string word)
    {
        return new string(word.Where(char.IsLetter).ToArray());
    }
}
