namespace AgentWithWorkflowOllama;

/// <summary>
/// Constants for the Profanity Filter Workflow
/// </summary>
internal static class Constants
{
    /// <summary>
    /// List of profane words to filter out
    /// </summary>
    public static readonly string[] ProfaneWords = 
    [
        "damn", "hell", "crap", "stupid", "idiot", "moron", 
        "jerk", "loser", "hate", "kill", "die", "suck"
    ];

    /// <summary>
    /// Character used to replace profane words
    /// </summary>
    public const char ReplacementCharacter = '*';

    /// <summary>
    /// Minimum length of word to be considered for profanity filtering
    /// </summary>
    public const int MinimumWordLength = 3;
}
