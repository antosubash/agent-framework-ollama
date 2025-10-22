# Profanity Filter Workflow with AI Agents

A comprehensive profanity filtering system built with Microsoft Agent Framework that demonstrates multiple workflow patterns including standard processing, streaming, AI-powered analysis, and real-time streaming AI.

## 🚀 Features

### Four Workflow Execution Modes

1. **📋 Standard Workflow** - Traditional rule-based profanity detection and filtering
2. **🌊 Streaming Workflow** - Chunked processing for large texts with real-time feedback
3. **🤖 AI Agent Workflow** - Intelligent AI-powered analysis with confidence scoring
4. **🌊🤖 Streaming AI Workflow** - Real-time streaming AI analysis with live output

### Key Capabilities

- **Intelligent Detection**: AI-powered contextual analysis beyond simple word matching
- **Real-time Processing**: Streaming capabilities for immediate feedback
- **Confidence Scoring**: AI provides reliability metrics for each decision
- **Transparent Reasoning**: Detailed explanations for AI decisions
- **Fallback Mechanisms**: Graceful degradation to rule-based processing
- **Configurable Chunking**: Adjustable chunk sizes for different use cases
- **Comprehensive Analytics**: Detailed metrics and statistics

## 🛠️ Prerequisites

- .NET 10.0 or later
- Ollama running locally with the `qwen3:8b` model
- Microsoft Agent Framework packages

## 📦 Installation

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd agent-framework-ollama
   ```

2. Install dependencies:
   ```bash
cd AgentWithWorkflowOllama
dotnet restore
```

3. Ensure Ollama is running with the required model:
   ```bash
ollama pull qwen3:8b
   ollama serve
   ```

## 🏃‍♂️ Running the Application

   ```bash
   dotnet run
   ```

The application will demonstrate all four workflow modes with sample test cases.

## 📁 Project Structure

```
AgentWithWorkflowOllama/
├── Program.cs                           # Main application with all demos
├── Constants.cs                         # Configuration constants
├── ProfanityDetectorExecutor.cs         # Rule-based profanity detection
├── TextFilterExecutor.cs                # Rule-based text filtering
├── StreamingTextSource.cs               # Text chunking for streaming
├── StreamingExecutors.cs               # Streaming versions of executors
├── StreamingProfanityFilterService.cs   # Streaming workflow service
├── AIAgentResults.cs                    # Data structures for AI results
├── AIProfanityDetectionAgent.cs         # AI-powered detection
├── AITextFilteringAgent.cs             # AI-powered filtering
├── StreamingAIAgents.cs                # Streaming AI agents
└── StreamingAIProfanityFilterService.cs # Streaming AI service
```

## 🔧 Configuration

### Constants Configuration

Edit `Constants.cs` to customize:

```csharp
public static readonly string[] ProfaneWords = 
[
    "damn", "hell", "crap", "stupid", "idiot", "moron", 
    "jerk", "loser", "hate", "kill", "die", "suck"
];

public const char ReplacementCharacter = '*';
public const int MinimumWordLength = 3;
```

### Ollama Configuration

Update the Ollama endpoint and model in `Program.cs`:

```csharp
var endpoint = "http://localhost:11434";
var modelName = "qwen3:8b";
```

## 📊 Workflow Comparison

| Feature | Standard | Streaming | AI Agent | Streaming AI |
|---------|----------|-----------|----------|--------------|
| **Detection Accuracy** | Good | Good | Excellent | Excellent |
| **Context Understanding** | Limited | Limited | Advanced | Advanced |
| **Memory Efficiency** | High | High | Medium | High |
| **Real-time Processing** | ❌ | ✅ | ❌ | ✅ |
| **Confidence Scoring** | ❌ | ❌ | ✅ | ✅ |
| **Reasoning Transparency** | ❌ | ❌ | ✅ | ✅ |
| **Scalability** | High | High | Medium | High |
| **Resource Usage** | Low | Medium | High | Medium |

## 🎯 Usage Examples

### Standard Workflow
```csharp
var profanityDetector = new ProfanityDetectorExecutor();
var textFilter = new TextFilterExecutor();

var builder = new WorkflowBuilder(profanityDetector);
builder.AddEdge(profanityDetector, textFilter).WithOutputFrom(textFilter);
var workflow = builder.Build();

var run = await InProcessExecution.RunAsync(workflow, "This is a damn stupid message");
```

### Streaming Workflow
```csharp
await StreamingProfanityFilterService.ProcessStreamingTextAsync(
    "Long text with profanity...", 
    chunkSize: 50
);
```

### AI Agent Workflow
```csharp
var profanityDetectionAgent = new AIProfanityDetectionAgent(agent);
var textFilteringAgent = new AITextFilteringAgent(agent);

var builder = new WorkflowBuilder(profanityDetectionAgent);
builder.AddEdge(profanityDetectionAgent, textFilteringAgent).WithOutputFrom(textFilteringAgent);
var workflow = builder.Build();

var run = await InProcessExecution.RunAsync(workflow, "This is a damn stupid message");
```

### Streaming AI Workflow
```csharp
await StreamingAIProfanityFilterService.ProcessStreamingAITextAsync(
    "Long text with profanity...", 
    agent, 
    chunkSize: 40
);
```

## 🔍 AI Agent Features

### Intelligent Analysis
- **Contextual Understanding**: Distinguishes between appropriate and inappropriate usage
- **Confidence Scoring**: Provides reliability metrics (0-100%)
- **Detailed Reasoning**: Explains decision-making process
- **Conservative Approach**: Suitable for professional environments

### Real-time Streaming
- **Live Output**: Shows AI responses as they're generated
- **Chunk Processing**: Handles large texts efficiently
- **Progressive Results**: Immediate feedback for each chunk
- **Aggregated Analytics**: Comprehensive metrics across all chunks

## 📈 Performance Metrics

The application tracks various metrics:

- **Words Filtered**: Total number of profane words replaced
- **Chunks Processed**: Number of text chunks analyzed
- **AI Confidence**: Average confidence across all decisions
- **Processing Time**: Real-time performance monitoring
- **Fallback Usage**: Frequency of rule-based fallbacks

## 🛡️ Error Handling

- **Graceful Degradation**: Falls back to rule-based processing if AI fails
- **Exception Handling**: Comprehensive error catching and reporting
- **Connection Resilience**: Handles Ollama connection issues
- **Input Validation**: Validates text input before processing

## 🔧 Customization

### Adding New Profane Words
Edit the `ProfaneWords` array in `Constants.cs`:

```csharp
public static readonly string[] ProfaneWords = 
[
    "damn", "hell", "crap", "stupid", "idiot", "moron", 
    "jerk", "loser", "hate", "kill", "die", "suck",
    "your-new-word"  // Add custom words here
];
```

### Adjusting Chunk Sizes
Modify chunk sizes in the demo methods:

```csharp
await StreamingAIProfanityFilterService.ProcessStreamingAITextAsync(
    text, 
    agent, 
    chunkSize: 30  // Adjust chunk size here
);
```

### Custom AI Prompts
Modify prompts in the AI agent classes for different analysis approaches.

## 🧪 Testing

The application includes comprehensive test cases:

- **Clean Messages**: Verify no false positives
- **Profane Messages**: Test detection accuracy
- **Mixed Content**: Validate contextual understanding
- **Edge Cases**: Handle special characters and formatting
- **Long Texts**: Test streaming capabilities

## 📝 Sample Output

```
🌊🤖 STREAMING AI AGENT WORKFLOW DEMO
====================================

🤖🌊 Testing streaming AI with: "This is a damn stupid message"

🤖 Starting streaming AI profanity filter for text: "This is a damn stupid message"
📦 Chunk size: 40 characters

📥 Processing chunk: "This is a damn stupid message"
    🤖 AI streaming response:
    💭 {
    "hasProfanity": true,
    "detectedWords": ["damn"],
    "confidence": 0.9,
    "reasoning": "The word 'damn' is a commonly considered profanity..."
    }
    🤖 AI streaming filter response:
    🛡️ {
    "filteredText": "This is a **** stupid message",
    "wordsFiltered": 1,
    "confidence": 1.0,
    "reasoning": "Replaced 'damn' with '****' as it is a profane word..."
    }
  🤖 AI Detection: ❌ Profanity detected
  🧠 AI Confidence: 100,0 %
  📝 Detected words: damn
  💭 AI Reasoning: The word 'damn' is a commonly considered profanity...
  🛡️ AI Filtered: "This is a **** stupid message"
  📊 Words filtered: 1
  🧠 AI Confidence: 100,0 %
  💭 AI Reasoning: Replaced 'damn' with '****' as it is a profane word...

🎯 Streaming AI Processing Complete!
📄 Original text: "This is a damn stupid message"
🛡️ AI Filtered text: "This is a **** stupid message"
📊 Total words filtered: 1
📦 Total chunks processed: 1
🧠 Average AI confidence: 100,0 %
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

- Microsoft Agent Framework for the workflow infrastructure
- Ollama for local AI model hosting
- qwen3:8b model for intelligent text analysis

## 📞 Support

For issues and questions:
1. Check the troubleshooting section below
2. Review the error messages in the console output
3. Ensure Ollama is running with the correct model
4. Verify .NET 10.0+ is installed

## 🔧 Troubleshooting

### Common Issues

**Ollama Connection Error**
```
❌ Error in AI agent processing: Connection refused
💡 Make sure Ollama is running with the qwen3:8b model
```
Solution: Start Ollama and pull the required model

**Build Errors**
```
error CS1061: 'AIAgent' does not contain a definition for 'GenerateContentAsync'
```
Solution: Ensure you're using the correct Microsoft Agent Framework version

**Performance Issues**
- Reduce chunk sizes for faster processing
- Use rule-based workflows for high-volume scenarios
- Consider model size vs. accuracy trade-offs

---

**Built with ❤️ using Microsoft Agent Framework and Ollama**