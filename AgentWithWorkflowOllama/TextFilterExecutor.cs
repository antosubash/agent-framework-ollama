using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;

namespace AgentWithWorkflowOllama;

/// <summary>
/// Data structure to hold the filtered text result
/// </summary>
internal sealed record FilteredTextResult(string OriginalText, string FilteredText, int WordsFiltered);

/// <summary>
/// Second executor: filters profane words from the text and outputs the final result.
/// </summary>
internal sealed class TextFilterExecutor() : ReflectingExecutor<TextFilterExecutor>("TextFilterExecutor"),
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
