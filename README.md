# Agent Framework with Ollama Sample

A .NET application demonstrating Microsoft Agents AI framework integration with Ollama for local AI model execution. This project showcases various agent capabilities including streaming responses, multi-turn conversations, function tools, and middleware integration.

## Features

- 🤖 **AI Agent Integration**: Uses Microsoft Agents AI framework with Ollama
- 💬 **Multi-turn Conversations**: Context preservation across conversation turns
- 🌊 **Streaming Responses**: Real-time character-by-character response streaming
- 🔧 **Function Tools**: Agent can call custom functions (e.g., weather API)
- ⚙️ **Middleware Support**: Custom middleware for function call logging

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Ollama](https://ollama.com/) installed and running locally
- A compatible model (e.g., `qwen3:8b`) pulled in Ollama

## Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd agent-framework-ollama
   ```

2. **Install Ollama** (if not already installed)
   ```bash
   # Windows (using winget)
   winget install Ollama.Ollama
   
   # macOS
   brew install ollama
   
3. **Pull a compatible model**
   ```bash
   ollama pull qwen3:8b
   ```

4. **Start Ollama service**
   ```bash
   ollama serve
   ```

5. **Restore dependencies and run**
   ```bash
   dotnet restore
   dotnet run
   ```

## Configuration

The application is configured to connect to Ollama running on `http://localhost:11434`. You can modify these settings in `Program.cs`:

```csharp
var endpoint = "http://localhost:11434";
var modelName = "qwen3:8b";
```

## Examples

The application demonstrates four different scenarios:

### 1. Multi-Turn Conversation (Non-Streaming)
Shows immediate complete responses with context preservation:
```csharp
AgentThread thread = agent.GetNewThread();
Console.WriteLine(await agent.RunAsync("Tell me a joke about a pirate.", thread));
Console.WriteLine(await agent.RunAsync("Now add some emojis to the joke and tell it in the voice of a pirate's parrot.", thread));
```

### 2. Multi-Turn Conversation (Streaming)
Character-by-character streaming with context preservation:
```csharp
AgentThread thread = agent.GetNewThread();
await DisplayStreamingResponse("Tell me a joke about a pirate.", thread);
await DisplayStreamingResponse("Now add some emojis to the joke and tell it in the voice of a pirate's parrot.", thread);
```

### 3. Function Tools Integration
Agent calling custom functions for real-time data:
```csharp
// Custom weather function
[Description("Get the weather for a given location.")]
static string GetWeather([Description("The location to get the weather for.")] string location)
    => $"The weather in {location} is cloudy with a high of 15°C.";

// Agent with tools
AIAgent agent = new ChatClientAgent(chatClient, instructions, name, tools: [AIFunctionFactory.Create(GetWeather)]);
```

### 4. Middleware Integration
Custom middleware for function call logging:
```csharp
async ValueTask<object?> FunctionCallMiddleware(AIAgent agent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
{
    Console.WriteLine($"🔧 Function Name: {context!.Function.Name} - Middleware Pre-Invoke");
    var result = await next(context, cancellationToken);
    Console.WriteLine($"🔧 Function Name: {context!.Function.Name} - Middleware Post-Invoke");
    return result;
}
```

## Project Structure

```
agent-framework-ollama/
├── AgentWithOllama/
│   ├── Program.cs              # Main application code
│   ├── AgentWithOllama.csproj  # Project file
│   └── bin/                    # Build output
├── agent-framework-ollama.sln  # Solution file
└── README.md                   # This file
```

## Dependencies

- **Microsoft.Agents.AI** (1.0.0-preview.251002.1): Microsoft's Agents AI framework
- **OllamaSharp** (5.4.7): .NET client for Ollama API
- **Microsoft.Extensions.AI**: AI extensions for dependency injection

## Usage Examples

### Basic Streaming
```csharp
await foreach (var update in agent.RunStreamingAsync("Tell me a joke"))
{
    Console.Write(update);
}
```

### Multi-turn with Context
```csharp
AgentThread thread = agent.GetNewThread();
await DisplayStreamingResponse("What's your name?", thread);
await DisplayStreamingResponse("What did I just ask you?", thread); // Agent remembers!
```

### Function Calling
```csharp
await DisplayStreamingResponse("What's the weather in Paris?");
// Agent will call GetWeather("Paris") and incorporate the result
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [Microsoft Agents AI](https://github.com/microsoft/agent-framework) - The AI framework
- [Ollama](https://ollama.com/) - Local AI model execution
- [OllamaSharp](https://github.com/awaescher/OllamaSharp) - .NET Ollama client
