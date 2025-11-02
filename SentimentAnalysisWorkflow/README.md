# 📝 Building AI-Powered Sentiment Analysis with Sequential Workflows

This project demonstrates how to build sophisticated AI-powered sentiment analysis workflows using the Microsoft Agent Framework. The `SentimentAnalysisWorkflow` showcases a powerful pattern of chaining multiple AI agents together to create complex analysis pipelines.

## 🎯 Overview

The Sentiment Analysis Workflow showcases a **two-stage sequential AI pipeline** that combines specialized agents to deliver comprehensive sentiment insights:

1. **AISentimentAnalysisAgent** - Performs deep sentiment analysis with contextual understanding
2. **AISentimentReportAgent** - Generates detailed, actionable reports based on analysis results

## 🏗️ Architecture

```
Input Text → AISentimentAnalysisAgent → SentimentAnalysisResult → AISentimentReportAgent → SentimentReportResult
```

The workflow follows a clean sequential pattern where:
- The first agent analyzes the input and produces structured analysis data
- The second agent consumes this structured data to generate comprehensive reports
- Each stage provides confidence scores and reasoning for transparency

## 🔍 Key Features

### 1. **Intelligent Sentiment Detection**
The `AISentimentAnalysisAgent` goes beyond simple keyword matching by:
- Analyzing context and tone
- Identifying emotional cues
- Providing sentiment scores ranging from -1.0 (very negative) to 1.0 (very positive)
- Detecting positive and negative language indicators
- Offering confidence scores for reliability assessment

### 2. **Comprehensive Report Generation**
The `AISentimentReportAgent` transforms raw analysis into actionable insights:
- Professional, well-formatted reports
- Detailed metrics (word counts, sentiment scaling)
- Actionable recommendations
- Confidence-weighted conclusions

### 3. **Structured Data Flow**
The workflow uses strongly-typed data structures:
- `SentimentAnalysisResult` - Contains analysis with sentiment type, score, indicators, and reasoning
- `SentimentReportResult` - Contains final report with metrics and insights
- `SentimentType` enum - Type-safe sentiment classification

### 4. **Structured Output Parsing**
The workflow implements robust structured output parsing using `System.Text.Json`:
- Type-safe DTOs (`SentimentAnalysisResponse`, `SentimentReportResponse`) for AI responses
- Centralized `StructuredOutputParser` for reliable JSON extraction and deserialization
- Handles variations in LLM response formatting (direct JSON, markdown code fences, etc.)
- Constants file for maintainable default values and error messages

## 💻 Code Samples

### Project Setup

First, ensure you have the necessary NuGet packages in your `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Agents.AI" Version="1.0.0-preview.251016.1" />
  <PackageReference Include="Microsoft.Agents.AI.Workflows" Version="1.0.0-preview.251016.1" />
  <PackageReference Include="OllamaSharp" Version="5.4.8" />
</ItemGroup>
```

### Data Structures

The workflow uses strongly-typed records for data transfer:

```csharp
// SentimentAnalysisResult.cs
internal sealed record SentimentAnalysisResult(
    string OriginalText,
    double SentimentScore,
    SentimentType Sentiment,
    double Confidence,
    string Reasoning,
    List<string> PositiveIndicators,
    List<string> NegativeIndicators);

internal sealed record SentimentReportResult(
    string OriginalText,
    double SentimentScore,
    SentimentType Sentiment,
    string Report,
    Dictionary<string, int> Metrics,
    double Confidence,
    string Reasoning);

internal enum SentimentType
{
    Positive,
    Neutral,
    Negative
}
```

### Main Program Setup

The main program initializes the Ollama client and creates the workflow:

```csharp
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI;
using OllamaSharp;

var endpoint = "http://localhost:11434";
var modelName = "qwen3:8b";

const string AgentName = "Sentiment Analyzer";
const string AgentInstructions = "You are an expert at analyzing sentiment in text and generating detailed reports.";

using OllamaApiClient chatClient = new(new Uri(endpoint), modelName);
AIAgent agent = new ChatClientAgent(chatClient, AgentInstructions, AgentName);

// Create and execute the workflow
var sentimentAnalysisAgent = new AISentimentAnalysisAgent(agent);
var sentimentReportAgent = new AISentimentReportAgent(agent);

var builder = new WorkflowBuilder(sentimentAnalysisAgent);
builder.AddEdge(sentimentAnalysisAgent, sentimentReportAgent)
       .WithOutputFrom(sentimentReportAgent);
var workflow = builder.Build();

var testText = "I absolutely love this product! It's fantastic!";
Run run = await InProcessExecution.RunAsync(workflow, testText);
```

### Structured Output DTOs

The workflow uses strongly-typed DTOs for parsing AI responses:

```csharp
// SentimentAnalysisResponse.cs
internal sealed record SentimentAnalysisResponse
{
    [JsonPropertyName("sentiment")]
    public string Sentiment { get; init; } = string.Empty;

    [JsonPropertyName("sentimentScore")]
    public double SentimentScore { get; init; }

    [JsonPropertyName("confidence")]
    public double Confidence { get; init; }

    [JsonPropertyName("positiveIndicators")]
    public List<string> PositiveIndicators { get; init; } = [];

    [JsonPropertyName("negativeIndicators")]
    public List<string> NegativeIndicators { get; init; } = [];

    [JsonPropertyName("reasoning")]
    public string Reasoning { get; init; } = string.Empty;
}

// SentimentReportResponse.cs
internal sealed record SentimentReportResponse
{
    [JsonPropertyName("report")]
    public string Report { get; init; } = string.Empty;

    [JsonPropertyName("metrics")]
    public SentimentReportMetrics? Metrics { get; init; }

    [JsonPropertyName("confidence")]
    public double Confidence { get; init; }

    [JsonPropertyName("reasoning")]
    public string Reasoning { get; init; } = string.Empty;
}
```

### Structured Output Parser

Centralized helper for parsing JSON from AI responses:

```csharp
// StructuredOutputParser.cs
internal static class StructuredOutputParser
{
    public static T ParseStructuredResponse<T>(string responseText) where T : class
    {
        // Extracts JSON from various formats (direct JSON, markdown code fences)
        // Deserializes using System.Text.Json with case-insensitive matching
        // Handles errors gracefully with meaningful exceptions
    }
}
```

### AISentimentAnalysisAgent Implementation

This agent analyzes sentiment using structured output parsing:

```csharp
using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI.Workflows.Reflection;
using Microsoft.Agents.AI;

internal sealed class AISentimentAnalysisAgent(AIAgent agent) 
    : ReflectingExecutor<AISentimentAnalysisAgent>("AISentimentAnalysisAgent"),
      IMessageHandler<string, SentimentAnalysisResult>
{
    private readonly AIAgent _agent = agent;

    public async ValueTask<SentimentAnalysisResult> HandleAsync(
        string input, 
        IWorkflowContext context, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new SentimentAnalysisResult(
                input,
                Constants.DefaultSentimentScore,
                SentimentType.Neutral,
                Constants.EmptyInputConfidence,
                Constants.EmptyInputReasoning,
                [],
                []);
        }

        var prompt = "Analyze the sentiment of the following text. " +
                    "Determine if it is positive, negative, or neutral, and provide a sentiment score.\n\n" +
                    $"Text to analyze: \"{input}\"\n\n" +
                    "Please respond ONLY with valid JSON in the following format (no markdown, no code fences, just the JSON):\n" +
                    "{\n" +
                    "    \"sentiment\": \"positive\" | \"negative\" | \"neutral\",\n" +
                    "    \"sentimentScore\": -1.0 to 1.0 (where -1.0 is very negative, 0.0 is neutral, 1.0 is very positive),\n" +
                    "    \"confidence\": 0.0-1.0,\n" +
                    "    \"positiveIndicators\": [\"word1\", \"word2\"],\n" +
                    "    \"negativeIndicators\": [\"word1\", \"word2\"],\n" +
                    "    \"reasoning\": \"Brief explanation of your sentiment analysis\"\n" +
                    "}\n\n" +
                    "Be thorough and consider context, tone, and emotional cues.";

        var response = await _agent.RunAsync(prompt);
        var responseText = response.ToString();

        // Parse structured response using System.Text.Json
        var structuredResponse = StructuredOutputParser.ParseStructuredResponse<SentimentAnalysisResponse>(responseText);

        var sentiment = MapSentimentType(structuredResponse.Sentiment);
        var sentimentScore = Math.Clamp(structuredResponse.SentimentScore, Constants.MinSentimentScore, Constants.MaxSentimentScore);
        var confidence = Math.Clamp(structuredResponse.Confidence, Constants.MinConfidence, Constants.MaxConfidence);
        var positiveIndicators = structuredResponse.PositiveIndicators ?? [];
        var negativeIndicators = structuredResponse.NegativeIndicators ?? [];
        var reasoning = !string.IsNullOrWhiteSpace(structuredResponse.Reasoning)
            ? structuredResponse.Reasoning
            : Constants.DefaultAnalysisReasoning;

        return new SentimentAnalysisResult(
            input, sentimentScore, sentiment, confidence,
            reasoning, positiveIndicators, negativeIndicators);
    }

    private static SentimentType MapSentimentType(string sentiment)
    {
        return sentiment.Trim().ToLowerInvariant() switch
        {
            "positive" => SentimentType.Positive,
            "negative" => SentimentType.Negative,
            _ => SentimentType.Neutral
        };
    }
}
```

### AISentimentReportAgent Implementation

This agent generates comprehensive reports using structured output parsing:

```csharp
internal sealed class AISentimentReportAgent(AIAgent agent) 
    : ReflectingExecutor<AISentimentReportAgent>("AISentimentReportAgent"),
      IMessageHandler<SentimentAnalysisResult, SentimentReportResult>
{
    private readonly AIAgent _agent = agent;

    public async ValueTask<SentimentReportResult> HandleAsync(
        SentimentAnalysisResult input, 
        IWorkflowContext context, 
        CancellationToken cancellationToken = default)
    {
        var prompt = "Generate a comprehensive sentiment analysis report based on the following analysis results.\n\n" +
                    $"Original Text: \"{input.OriginalText}\"\n" +
                    $"Detected Sentiment: {input.Sentiment}\n" +
                    $"Sentiment Score: {input.SentimentScore:F2}\n" +
                    $"Confidence: {input.Confidence:P1}\n" +
                    $"Positive Indicators: {string.Join(", ", input.PositiveIndicators)}\n" +
                    $"Negative Indicators: {string.Join(", ", input.NegativeIndicators)}\n" +
                    $"AI Reasoning: {input.Reasoning}\n\n" +
                    "Please respond ONLY with valid JSON in the following format (no markdown, no code fences, just the JSON):\n" +
                    "{\n" +
                    "    \"report\": \"A comprehensive, well-formatted sentiment analysis report\",\n" +
                    "    \"metrics\": {\n" +
                    "        \"PositiveWords\": number,\n" +
                    "        \"NegativeWords\": number,\n" +
                    "        \"TotalWords\": number,\n" +
                    "        \"SentimentScoreScaled\": number (score * 100)\n" +
                    "    },\n" +
                    "    \"confidence\": 0.0-1.0,\n" +
                    "    \"reasoning\": \"Brief explanation of report generation\"\n" +
                    "}\n\n" +
                    "The report should be detailed, professional, and include actionable insights.";

        var response = await _agent.RunAsync(prompt);
        var responseText = response.ToString();

        // Parse structured response using System.Text.Json
        var structuredResponse = StructuredOutputParser.ParseStructuredResponse<SentimentReportResponse>(responseText);

        var report = !string.IsNullOrWhiteSpace(structuredResponse.Report)
            ? structuredResponse.Report.Replace("\\n", "\n")
            : Constants.ReportGenerationFailed;

        var metrics = MapMetrics(structuredResponse.Metrics);
        var confidence = Math.Clamp(structuredResponse.Confidence, Constants.MinConfidence, Constants.MaxConfidence);
        var reasoning = !string.IsNullOrWhiteSpace(structuredResponse.Reasoning)
            ? structuredResponse.Reasoning
            : Constants.DefaultReportReasoning;

        return new SentimentReportResult(
            input.OriginalText, input.SentimentScore, input.Sentiment,
            report, metrics, confidence, reasoning);
    }

    private static Dictionary<string, int> MapMetrics(SentimentReportMetrics? metrics)
    {
        if (metrics == null)
        {
            return new Dictionary<string, int>
            {
                ["PositiveWords"] = 0,
                ["NegativeWords"] = 0,
                ["TotalWords"] = 0,
                ["SentimentScoreScaled"] = 0
            };
        }

        return new Dictionary<string, int>
        {
            ["PositiveWords"] = metrics.PositiveWords,
            ["NegativeWords"] = metrics.NegativeWords,
            ["TotalWords"] = metrics.TotalWords,
            ["SentimentScoreScaled"] = metrics.SentimentScoreScaled
        };
    }
}
```

### Processing Workflow Events

After running the workflow, process the events to extract results:

```csharp
Run run = await InProcessExecution.RunAsync(workflow, testText);

foreach (WorkflowEvent evt in run.NewEvents)
{
    if (evt is ExecutorCompletedEvent executorComplete)
    {
        Console.WriteLine($"Agent '{executorComplete.ExecutorId}' completed:");

        if (executorComplete.Data is SentimentAnalysisResult analysisResult)
        {
            Console.WriteLine($"  Original Text: {analysisResult.OriginalText}");
            Console.WriteLine($"  Sentiment: {analysisResult.Sentiment}");
            Console.WriteLine($"  Sentiment Score: {analysisResult.SentimentScore:F2}");
            Console.WriteLine($"  Confidence: {analysisResult.Confidence:P1}");
            if (analysisResult.PositiveIndicators.Count > 0)
            {
                Console.WriteLine($"  Positive Indicators: {string.Join(", ", analysisResult.PositiveIndicators)}");
            }
            if (analysisResult.NegativeIndicators.Count > 0)
            {
                Console.WriteLine($"  Negative Indicators: {string.Join(", ", analysisResult.NegativeIndicators)}");
            }
            Console.WriteLine($"  Reasoning: {analysisResult.Reasoning}");
        }
        else if (executorComplete.Data is SentimentReportResult reportResult)
        {
            Console.WriteLine($"  Sentiment: {reportResult.Sentiment}");
            Console.WriteLine($"  Sentiment Score: {reportResult.SentimentScore:F2}");
            Console.WriteLine($"  Confidence: {reportResult.Confidence:P1}");
            Console.WriteLine($"  Metrics: {string.Join(", ", reportResult.Metrics.Select(m => $"{m.Key}: {m.Value}"))}");
            Console.WriteLine($"  Reasoning: {reportResult.Reasoning}");
            Console.WriteLine($"  Report:\n{reportResult.Report}");
        }
    }
}
```

## 📊 Real-World Applications

This workflow pattern is ideal for:

- **Customer Feedback Analysis** - Process reviews and support tickets at scale
- **Social Media Monitoring** - Track brand sentiment across platforms
- **Content Moderation** - Assess emotional tone of user-generated content
- **Market Research** - Analyze public opinion from surveys and interviews
- **Product Development** - Understand user reactions to features and updates

## 🎓 Learning Outcomes

Building this workflow demonstrates:

1. **Sequential Workflow Design** - How to chain specialized agents for complex tasks
2. **Type-Safe Data Contracts** - Using records and enums for structured communication
3. **Structured Output Parsing** - Using `System.Text.Json` with strongly-typed DTOs for reliable AI response parsing
4. **AI Integration Patterns** - Connecting local LLMs (Ollama) with the Agent Framework
5. **Prompt Engineering** - Crafting prompts that yield structured, parseable JSON responses
6. **Error Resilience** - Handling LLM response variations gracefully with robust JSON extraction

## 🚀 Running the Demo

### Prerequisites

- .NET 10.0 or later
- Ollama running locally with the `qwen3:8b` model

### Setup

1. Start Ollama and pull the required model:
```bash
ollama pull qwen3:8b
ollama serve
```

2. Navigate to the project directory:
```bash
cd SentimentAnalysisWorkflow
```

3. Restore dependencies:
```bash
dotnet restore
```

4. Run the application:
```bash
dotnet run
```

The demo processes multiple test cases showcasing:
- Highly positive sentiment ("I absolutely love this product!")
- Negative sentiment ("This is the worst experience...")
- Neutral sentiment ("The service was adequate...")
- Emotional extremes ("I'm thrilled and overjoyed!")
- Concerned sentiment ("I'm really concerned about this issue...")

## 📈 Sample Output

```
🤖 SENTIMENT ANALYSIS WORKFLOW DEMO
====================================

🤖 Processing: "I absolutely love this product! It's fantastic and exceeded all my expectations!"

🤖 Agent 'AISentimentAnalysisAgent' completed:
  📝 Original Text: I absolutely love this product! It's fantastic and exceeded all my expectations!
  😊 Sentiment: Positive
  📊 Sentiment Score: 0.85
  🧠 Confidence: 95.0%
  ✅ Positive Indicators: love, absolutely, fantastic, exceeded, expectations
  💭 Reasoning: Strong positive language with enthusiastic tone...

🤖 Agent 'AISentimentReportAgent' completed:
  📄 Sentiment: Positive
  📊 Sentiment Score: 0.85
  🧠 Confidence: 92.0%
  📈 Metrics: PositiveWords: 8, NegativeWords: 0, TotalWords: 12, SentimentScoreScaled: 85
  💭 Reasoning: Generated comprehensive report with actionable insights...
  📋 Report:
  Sentiment Analysis Report
  ========================
  Overall sentiment: Highly Positive (Score: 0.85)
  The text demonstrates strong positive sentiment with enthusiastic language...
```

## 🔧 Customization Tips

1. **Adjust Sentiment Thresholds** - Modify score ranges in `AISentimentAnalysisAgent`
2. **Enhance Reporting** - Extend `AISentimentReportAgent` prompts for domain-specific insights
3. **Add More Stages** - Chain additional agents for categorization, summarization, or action items
4. **Stream Processing** - Adapt for real-time sentiment monitoring of live data streams
5. **Multi-Language Support** - Extend prompts to handle multiple languages

## 🎯 Key Takeaways

The Sentiment Analysis Workflow exemplifies how **specialized AI agents working in sequence** can produce more sophisticated results than a single monolithic agent. By breaking complex tasks into focused stages, we achieve:

- **Better Accuracy** - Each agent focuses on what it does best
- **Greater Flexibility** - Easy to modify individual stages without affecting others
- **Improved Maintainability** - Clear separation of concerns
- **Enhanced Transparency** - Each stage provides reasoning and confidence scores

This pattern scales beautifully to more complex workflows where you might chain 5, 10, or even more specialized agents together to solve intricate business problems.

## 🛠️ Implementation Highlights

### Agent Communication Pattern

The workflow builder creates a clean pipeline where data flows from one agent to the next:

```csharp
var builder = new WorkflowBuilder(sentimentAnalysisAgent);
builder.AddEdge(sentimentAnalysisAgent, sentimentReportAgent)
       .WithOutputFrom(sentimentReportAgent);
var workflow = builder.Build();
```

### AI Prompt Engineering

Both agents use carefully crafted prompts that:
- Request structured JSON responses for reliable parsing
- Include examples and formatting requirements
- Emphasize thoroughness and context awareness
- Specify confidence scoring requirements

### Structured Output Parsing

The workflow implements robust structured output parsing that:
- Uses `System.Text.Json` for type-safe deserialization
- Handles variations in LLM response formatting (direct JSON, markdown code fences, multiline)
- Centralizes parsing logic in `StructuredOutputParser` for reusability
- Uses strongly-typed DTOs (`SentimentAnalysisResponse`, `SentimentReportResponse`) for reliability
- Includes fallback values for missing fields using constants
- Validates and clamps numeric values to valid ranges
- Provides meaningful error messages through `JsonException`

## 📁 Project Structure

```
SentimentAnalysisWorkflow/
├── Program.cs                         # Main application entry point
├── AISentimentAnalysisAgent.cs        # First stage: sentiment analysis
├── AISentimentReportAgent.cs          # Second stage: report generation
├── SentimentAnalysisResult.cs         # Workflow data structures and types
├── SentimentAnalysisResponse.cs       # DTO for AI sentiment analysis response
├── SentimentReportResponse.cs         # DTO for AI report generation response
├── StructuredOutputParser.cs          # Centralized JSON parsing helper
└── Constants.cs                       # Constants for defaults and error messages
```

## 🔧 Troubleshooting

### Ollama Connection Error
```
❌ Error in processing: Connection refused
💡 Make sure Ollama is running with the qwen3:8b model
```

**Solution**: Start Ollama and ensure the model is available:
```bash
ollama serve
ollama pull qwen3:8b
```

### Build Errors
If you encounter build errors, ensure you're using the correct Microsoft Agent Framework version specified in the `.csproj` file.

### Model Not Found
If the model isn't available, pull it first:
```bash
ollama pull qwen3:8b
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License.

---

**Built with ❤️ using Microsoft Agent Framework and Ollama**

