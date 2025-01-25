// Import packages
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SemanticKernelDev.Config;

using SemanticKernelDev.Plugins;

#region Load configuration (including secrets) from environment variables
// Load the Azure OpenAI keys and config from the environment variables
string azureOpenAIModelId, azureOpenAIEndpoint, azureOpenAIAPIKey;
(azureOpenAIModelId, azureOpenAIEndpoint, azureOpenAIAPIKey) = EnvVarReader.GetAzureOpenAIConfig();

// Load the OpenAI API keys and config from the environment variables
string openAIModelId, openAIAPIKey;
(openAIModelId, openAIAPIKey) = EnvVarReader.GetOpenAIConfig();
#endregion

#region Commandline arguments
// At least one commandline parameter is expected
if (args.Length == 0)
{
    // Console.WriteLine("Usage: dotnet run <brand>");
    // return 1;
}

string llm = string.Join(" ", args);
llm = "Azure OpenAI";
string param = "Temperature";
Console.WriteLine($"Hello, LLM Param Sleuth Agent here! Beginning processing for LLM: ❰{llm}❱");
#endregion

// NOTE: if I reverse these, doing AzureOpenAI first FAILS - but that was because there was insufficient BALANCE on OpenAI account! Fixed.
// DEMO: second one used by default, but both are available via serviceId
var builder = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(azureOpenAIModelId, azureOpenAIEndpoint, azureOpenAIAPIKey, serviceId: "AzureOpenAI")
    .AddOpenAIChatCompletion(openAIModelId, openAIAPIKey, serviceId: "OpenAI")
    // .AddAzureOpenAIChatCompletion(azureOpenAIModelId, azureOpenAIEndpoint, azureOpenAIAPIKey, serviceId: "AzureOpenAI")
    ;

var resourceBuilder = ResourceBuilder.CreateDefault().AddService("LlmParamSleuth");

#region OpenTelemetry
// DEMO: on/off OpenTelemetry SK - includes token counts, function calls, and more
resourceBuilder = OTelEnabler.EnableOTelSK(resourceBuilder);

AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

using var loggerFactory = LoggerFactory.Create(builder =>
{
// DEMO: on/off OpenTelemetry generally
#if true
    builder.AddOpenTelemetry(options =>
    {
        options.SetResourceBuilder(resourceBuilder);
        options.AddConsoleExporter();
    });
#endif
    // DEMO: on/off logging verbosity levels
    // builder.SetMinimumLevel(LogLevel.Information);
    builder.SetMinimumLevel(LogLevel.Trace);
    // builder.SetMinimumLevel(LogLevel.Warning);
    // builder.SetMinimumLevel(LogLevel.None);
});
#endregion


Kernel kernel = builder.Build();
builder.Services.AddSingleton(loggerFactory);

var logger = loggerFactory.CreateLogger<Program>();
logger.LogInformation($"✅ Starting the Sleuthing Agent for ❰{llm}❱ for param ❰{param}❱");

var chat = kernel.GetRequiredService<IChatCompletionService>();
// Which is chat completion service type (OpenAI or AzureOpenAI)?
logger.LogWarning($"Chat completion service: {chat.GetType().Name}");

Console.WriteLine($"Chat completion service: {chat.GetType().Name}");


var history = new ChatHistory();

#region Basic AI Chat app
#if false
// DEMO: on/off Basic chat app demo

PromptExecutionSettings promptExecutionSettings = new()
{
};

history.AddUserMessage($"Hello, LLM Param Sleuth Agent here! Here's an interesting technical detail about: \"{llm}\"");

var result = await chat.GetChatMessageContentAsync(
        history,
        executionSettings: promptExecutionSettings,
        kernel: kernel);

// logger.LogInformation($"✅ LLM Param Default Sleuth Agent says ❰❰begin❱❱\n{result}\n❰❰end❱❱");
Console.WriteLine($"✅ LLM Param Default Sleuth Agent says ❰❰begin❱❱\n{result}\n❰❰end❱❱");

history.Clear();

#endif
#endregion



#region Basic AI Agent
#if true
// DEMO: on/off Basic AI Agent demo

#region Functions
#region Plugins
// Add plugins
#pragma warning disable SKEXP0040
// find websites with docs that appear to be authoritative and applicable to question at hand
// RUNTIME ERROR var func1 = kernel.Plugins.AddFromType<LlmDocumentationFinderFunction>("search_web_for_llm_param_default_documentation_urls"); // ###
var func1 = kernel.Plugins.AddFromType<LlmDocumentationFinderFunction>("search_web_for_llm_documentation"); // ###
// download the documentation from the website
var func2 = kernel.Plugins.AddFromType<LlmDocumentationDownloaderFunction>("download_documentation_from_web"); // ***
// analyze the documentation to ascertain answer to question, or to discard it
var func3 = kernel.CreateFunctionFromPromptyFile("./LlmParamDocumentationAnalyzer.prompty");
// var func3 = kernel.CreateFunctionFromPromptyFile("./Testy.prompty");
#pragma warning restore SKEXP0040
//// var func2 = kernel.Plugins.AddFromType<DocumentationUrlValidatorFunction>("url_validator"); // ***
// var func3 = kernel.Plugins.AddFromType<ImageDescriberFunction>("describe_image_at_url"); // ###
// DEMO: on/off ImageWebSearchFunction
// var func4 = kernel.Plugins.AddFromType<SearchWebForLlmDocumentation>("search_for_llm_param_default"); // ###
//// var func5 = kernel.CreateFunctionFromPromptyFile("./IntrospectForLlmDocumentation.prompty");
#endregion
#endregion

#region AI Agent Prompt Execution Settings
// PromptExecutionSettings → https://learn.microsoft.com/en-us/dotnet/api/microsoft.semantickernel.promptexecutionsettings?view=semantic-kernel-dotnet
//    ↑
// OpenAIPromptExecutionSettings → https://learn.microsoft.com/en-us/dotnet/api/microsoft.semantickernel.connectors.openai.openaipromptexecutionsettings?view=semantic-kernel-dotnet
//    ↑
// AzureOpenAIPromptExecutionSettings → https://learn.microsoft.com/en-us/dotnet/api/microsoft.semantickernel.connectors.azureopenai.azureopenaipromptexecutionsettings?view=semantic-kernel-dotnet

OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
{
#pragma warning disable SKEXP0001
    // ★☆★ 💖💖💖 ☆★☆ 💖💖💖 ★☆★
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
#pragma warning restore SKEXP0001

    MaxTokens = 1000,
    // Temperature = 0.70, // higher values make the model more creative
    Temperature = 0.10, // lower values make the model more precise
};
#endregion

// represents the GOAL
var retryBudget = 23;
var promptTemplate = 
   $"The overall goal is to find high quality (authorative) documentation about {llm} on the web " +
    "and analyze it to answer specific questions about the default behavior when {parameter} is not specified. " +
    "If you encounter errors along the way or find content that does not reveal the needed data, " + 
    $"keep trying up find the right content. You can try up to {retryBudget} documentation sources.";

Console.WriteLine($"LENGTH of `promptTemplate`: {promptTemplate.Length}");

// promptTemplate = $"What is favorite color for LLM API? If you don't success on first try, keep trying up to 5 times.";

var response = await kernel.InvokePromptAsync(promptTemplate, new(openAIPromptExecutionSettings));

Console.WriteLine(response); 


// var response = await kernel.InvokePromptAsync(promptTemplate,
// Parameters = new Dictionary<string, string> { { "brand", brand } } }
// new(openAIPromptExecutionSettings));

Console.WriteLine("---------------------------------------------------------------");
Console.WriteLine(response);

#endif
#endregion

return 0;
