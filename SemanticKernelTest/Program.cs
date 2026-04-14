using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.MistralAI;
using Microsoft.SemanticKernel.Connectors.Ollama;

Console.WriteLine("=== LLM API Key Tester ===");
Console.WriteLine("Select provider:");
Console.WriteLine("1) Google Gemini");
Console.WriteLine("2) OpenAI");
Console.WriteLine("3) Azure OpenAI");
Console.WriteLine("4) Mistral AI");
Console.WriteLine("5) Ollama (local)");
Console.Write("Option (1-5): ");

var providerOption = Console.ReadLine()?.Trim();
if (providerOption is not ("1" or "2" or "3" or "4" or "5"))
{
    Console.Error.WriteLine("Invalid option. Use 1, 2, 3, 4, or 5.");
    return;
}

Console.Write("Model (exact string): ");
var modelId = Console.ReadLine()?.Trim();
if (string.IsNullOrWhiteSpace(modelId))
{
    Console.Error.WriteLine("Model is required.");
    return;
}

var builder = Kernel.CreateBuilder();
switch (providerOption)
{
    case "1":
    {
        Console.Write("API key: ");
        var apiKey = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.Error.WriteLine("API key is required.");
            return;
        }
        builder.AddGoogleAIGeminiChatCompletion(modelId: modelId, apiKey: apiKey);
        break;
    }
    case "2":
    {
        Console.Write("API key: ");
        var apiKey = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.Error.WriteLine("API key is required.");
            return;
        }
        builder.AddOpenAIChatCompletion(modelId: modelId, apiKey: apiKey);
        break;
    }
    case "3":
    {
        Console.Write("Azure endpoint (e.g. https://my-resource.openai.azure.com/): ");
        var endpoint = Console.ReadLine()?.Trim();
        Console.Write("API key: ");
        var apiKey = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey))
        {
            Console.Error.WriteLine("Endpoint and API key are required.");
            return;
        }

        // In Azure OpenAI, the deployment name is passed as "modelId".
        builder.AddAzureOpenAIChatCompletion(deploymentName: modelId, endpoint: endpoint, apiKey: apiKey);
        break;
    }
    case "4":
    {
        Console.Write("API key: ");
        var apiKey = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.Error.WriteLine("API key is required.");
            return;
        }

        builder.AddMistralChatCompletion(modelId: modelId, apiKey: apiKey);
        break;
    }
    case "5":
    {
        Console.Write("Ollama endpoint (default http://localhost:11434): ");
        var endpointInput = Console.ReadLine()?.Trim();
        if (string.IsNullOrWhiteSpace(endpointInput))
        {
            endpointInput = "http://localhost:11434";
        }

        if (!Uri.TryCreate(endpointInput, UriKind.Absolute, out var endpointUri))
        {
            Console.Error.WriteLine("Invalid Ollama endpoint URL.");
            return;
        }

        builder.AddOllamaChatCompletion(modelId: modelId, endpoint: endpointUri);
        break;
    }
}

var kernel = builder.Build();
kernel.ImportPluginFromObject(new TextUtilsPlugin(), "TextUtils");

Console.WriteLine();
Console.WriteLine("Write the input text to evaluate.");
Console.WriteLine("The program will summarize your text in a single sentence with a maximum of 20 words.");
Console.Write("Input text: ");
var inputTask = Console.ReadLine()?.Trim();
if (string.IsNullOrWhiteSpace(inputTask))
{
    Console.Error.WriteLine("Input text is required.");
    return;
}

var wordCountResult = await kernel.InvokeAsync("TextUtils", "CountWords", new() { ["text"] = inputTask });
var wordCount = wordCountResult.GetValue<int>();

var prompt = $@"
Resume el siguiente texto en una oración de maximo 20 palabras.
Información adicional: El texto original tiene {wordCount} palabras.
Texto: {inputTask}";

Console.WriteLine("Sending request to the selected provider... please wait.");
var result = await kernel.InvokePromptAsync(prompt);

Console.WriteLine();
Console.WriteLine($"Provider: {providerOption switch
{
    "1" => "Google Gemini",
    "2" => "OpenAI",
    "3" => "Azure OpenAI",
    "4" => "Mistral AI",
    _ => "Ollama"
}}");
Console.WriteLine($"Model: {modelId}");
Console.WriteLine($"Word count: {wordCount}");
Console.WriteLine($"--- Summary ---\n{result}");