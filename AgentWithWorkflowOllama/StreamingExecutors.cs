using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace AgentWithWorkflowOllama;

/// <summary>
/// Streaming version of the profanity detector executor
/// Processes text chunks and yields detection results
/// </summary>
internal sealed class StreamingProfanityDetectorExecutor() : ReflectingExecutor<StreamingProfanityDetectorExecutor>("StreamingProfanityDetectorExecutor"),
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

/// <summary>
/// Streaming version of the text filter executor
/// Processes profanity detection results and yields filtered text
/// </summary>
internal sealed class StreamingTextFilterExecutor() : ReflectingExecutor<StreamingTextFilterExecutor>("StreamingTextFilterExecutor"),
    IMessageHandler<ProfanityDetectionResult, FilteredTextResult>
{
    public ValueTask<FilteredTextResult> HandleAsync(ProfanityDetectionResult input, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        if (!input.HasProfanity)
        {
            return ValueTask.FromResult(new FilteredTextResult(input.OriginalText, input.OriginalText, 0));
        }

        var filteredText = input.OriginalText;
        var wordsFiltered = 0;

        foreach (var profaneWord in input.DetectedProfaneWords)
        {
            // Replace the profane word with asterisks, preserving original case
            var replacement = new string(Constants.ReplacementCharacter, profaneWord.Length);
            filteredText = ReplaceWord(filteredText, profaneWord, replacement);
            wordsFiltered++;
        }

        var result = new FilteredTextResult(input.OriginalText, filteredText, wordsFiltered);
        return ValueTask.FromResult(result);
    }

    /// <summary>
    /// Replaces a word in the text while preserving word boundaries and case
    /// </summary>
    private static string ReplaceWord(string text, string wordToReplace, string replacement)
    {
        var words = text.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            var cleanWord = CleanWord(words[i]);
            if (string.Equals(cleanWord, wordToReplace, StringComparison.OrdinalIgnoreCase))
            {
                // Preserve the original punctuation and case structure
                var originalWord = words[i];
                var newWord = originalWord.Replace(cleanWord, replacement, StringComparison.OrdinalIgnoreCase);
                words[i] = newWord;
            }
        }
        return string.Join(' ', words);
    }

    /// <summary>
    /// Removes punctuation from a word for comparison
    /// </summary>
    private static string CleanWord(string word)
    {
        return new string(word.Where(char.IsLetter).ToArray());
    }
}
