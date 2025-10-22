namespace AgentWithWorkflowOllama;

/// <summary>
/// Data structure to hold AI-powered profanity detection results
/// </summary>
internal sealed record AIProfanityDetectionResult(
    string OriginalText, 
    List<string> DetectedProfaneWords, 
    bool HasProfanity, 
    double Confidence, 
    string Reasoning);

/// <summary>
/// Data structure to hold AI-powered text filtering results
/// </summary>
internal sealed record AITextFilterResult(
    string OriginalText, 
    string FilteredText, 
    int WordsFiltered, 
    double Confidence, 
    string Reasoning);
