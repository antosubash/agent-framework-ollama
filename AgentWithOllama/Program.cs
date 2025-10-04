using Microsoft.Agents.AI;
using OllamaSharp;

var endpoint = "http://localhost:11434";
var modelName = "qwen3:8b";

const string JokerName = "Joker";
const string JokerInstructions = "You are good at telling jokes.";

// Get a chat client for Ollama and use it to construct an AIAgent.
using OllamaApiClient chatClient = new(new Uri(endpoint), modelName);
AIAgent agent = new ChatClientAgent(chatClient, JokerInstructions, JokerName);

// Display a nice header
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine($"🤖 {JokerName} is thinking...");
Console.WriteLine("=" + new string('=', 50));
Console.ResetColor();

// Track streaming progress
var fullResponse = "";
var isFirstUpdate = true;

await foreach (var update in agent.RunStreamingAsync("Tell me a joke about a pirate."))
{
    if (isFirstUpdate)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"{JokerName}: ");
        Console.ResetColor();
        isFirstUpdate = false;
    }
    
    // Add the update to our full response
    fullResponse += update;
    
    // Write the update with a subtle color
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write(update);
    Console.ResetColor();
    
    // Small delay to make streaming more visible
    await Task.Delay(10);
}

// Add a nice footer
Console.WriteLine();
Console.WriteLine();
Console.ForegroundColor = ConsoleColor.DarkGray;
Console.WriteLine("=" + new string('=', 50));
Console.WriteLine("✨ Response complete!");
Console.ResetColor();