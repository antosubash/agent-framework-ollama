namespace AgentWithWorkflowOllama;

/// <summary>
/// Simulates a streaming text source by yielding chunks of text
/// </summary>
internal sealed class StreamingTextSource : IAsyncEnumerable<string>
{
    private readonly string _text;
    private readonly int _chunkSize;

    public StreamingTextSource(string text, int chunkSize = 50)
    {
        _text = text;
        _chunkSize = chunkSize;
    }

    public async IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var words = _text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var currentChunk = new List<string>();
        var currentLength = 0;

        foreach (var word in words)
        {
            currentChunk.Add(word);
            currentLength += word.Length + 1; // +1 for space

            // Yield chunk when it reaches the desired size
            if (currentLength >= _chunkSize)
            {
                var chunk = string.Join(' ', currentChunk);
                yield return chunk;
                
                // Simulate streaming delay
                await Task.Delay(100, cancellationToken);
                
                currentChunk.Clear();
                currentLength = 0;
            }
        }

        // Yield any remaining words
        if (currentChunk.Count > 0)
        {
            var chunk = string.Join(' ', currentChunk);
            yield return chunk;
        }
    }
}
