using System.ComponentModel;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;

var endpoint = "http://localhost:11434";
var modelName = "qwen3:8b";

const string JokerName = "Joker";
const string JokerInstructions = "You are good at telling jokes.";

[Description("Get the weather for a given location.")]
static string GetWeather([Description("The location to get the weather for.")] string location)
    => $"The weather in {location} is cloudy with a high of 15°C.";

// Get a chat client for Ollama and use it to construct an AIAgent.
using OllamaApiClient chatClient = new(new Uri(endpoint), modelName);
AIAgent agent = new ChatClientAgent(chatClient, JokerInstructions, JokerName, tools: [AIFunctionFactory.Create(GetWeather)]);

// Function invocation middleware that logs before and after function calls.
async ValueTask<object?> FunctionCallMiddleware(AIAgent agent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"🔧 Function Name: {context!.Function.Name} - Middleware Pre-Invoke");
    Console.ResetColor();
    
    var result = await next(context, cancellationToken);
    
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"🔧 Function Name: {context!.Function.Name} - Middleware Post-Invoke");
    Console.ResetColor();

    return result;
}

var agentWithMiddleware = agent.AsBuilder()
    .Use(FunctionCallMiddleware)
    .Build();

// Helper method to display streaming response with formatting
async Task DisplayStreamingResponse(string prompt, AgentThread? thread = null, AIAgent? agentToUse = null)
{
    var agentInstance = agentToUse ?? agent;
    
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"🤖 {JokerName} is thinking...");
    Console.ResetColor();
    
    var isFirstUpdate = true;
    
    var stream = thread != null 
        ? agentInstance.RunStreamingAsync(prompt, thread)
        : agentInstance.RunStreamingAsync(prompt);
    
    await foreach (var update in stream)
    {
        if (isFirstUpdate)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{JokerName}: ");
            Console.ResetColor();
            isFirstUpdate = false;
        }
        
        Console.Write(update);
        await Task.Delay(10);
    }
    
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine("✨ Response complete!");
    Console.ResetColor();
}

// Example 1: Non-streaming multi-turn conversation
Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("🎭 MULTI-TURN CONVERSATION (Non-Streaming)");
Console.WriteLine("=" + new string('=', 60));
Console.ResetColor();

AgentThread thread1 = agent.GetNewThread();
Console.WriteLine(await agent.RunAsync("Tell me a joke about a pirate.", thread1));
Console.WriteLine(await agent.RunAsync("Now add some emojis to the joke and tell it in the voice of a pirate's parrot.", thread1));

// Example 2: Streaming multi-turn conversation
Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("\n🎭 MULTI-TURN CONVERSATION (Streaming)");
Console.WriteLine("=" + new string('=', 60));
Console.ResetColor();

AgentThread thread2 = agent.GetNewThread();
await DisplayStreamingResponse("Tell me a joke about a pirate.", thread2);
await DisplayStreamingResponse("Now add some emojis to the joke and tell it in the voice of a pirate's parrot.", thread2);

// Example 3: Streaming agent interaction with function tools
Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("\n🔧 FUNCTION TOOLS INTERACTION (Streaming)");
Console.WriteLine("=" + new string('=', 60));
Console.ResetColor();

await DisplayStreamingResponse("What is the weather like in Amsterdam?");

Console.ForegroundColor = ConsoleColor.DarkGray;
Console.WriteLine("\n" + "=" + new string('=', 60));
Console.WriteLine("🎉 All conversations completed!");
Console.ResetColor();

// Example 4: Agent with middleware
Console.ForegroundColor = ConsoleColor.Magenta;
Console.WriteLine("\n🔧 AGENT WITH MIDDLEWARE (Streaming)");
Console.WriteLine("=" + new string('=', 60));
Console.ResetColor();

await DisplayStreamingResponse("What is the weather like in Amsterdam?", agentToUse: agentWithMiddleware);

Console.ForegroundColor = ConsoleColor.DarkGray;
Console.WriteLine("\n" + "=" + new string('=', 60));
Console.WriteLine("🎉 All conversations completed!");
Console.ResetColor();