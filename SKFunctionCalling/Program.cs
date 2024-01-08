using System.Reflection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using FunctionCalling;
using System.Diagnostics;
using SKFunctionCalling;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

public static class SKFunctionCallingTest
{
    static async Task Main(string[] args)
    {
        await RunAsync();
    }
    private static async Task RunAsync()
    {
        //Kernel Builder
        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion("model-name", "endpoint", "api-key");
        Kernel kernel = builder.Build();
        var customHandler = new CustomHttpClientHandler();
        HttpClient httpClient = new(customHandler);



        //Plugins Setup
        BearerAuthenticationProvider authenticationProvider = new(() => Task.FromResult("your-azure-token"));
        await kernel.ImportPluginFromOpenApiAsync(
        pluginName: "AzurePlugin",
        filePath: Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "plugins", "AzurePlugin/openapi.json"),
        new OpenApiFunctionExecutionParameters
        {
            AuthCallback = authenticationProvider.AuthenticateRequestAsync,
            HttpClient = httpClient
        });

        var authenticationProvider1 = new BasicAuthenticationProvider(() => { return Task.FromResult("your-jira-token"); });
        await kernel.ImportPluginFromOpenApiAsync(
            pluginName: "JiraPlugin",
            filePath: Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "plugins", "JiraPlugin/openapi.json"),
            new OpenApiFunctionExecutionParameters
            {
                AuthCallback = authenticationProvider1.AuthenticateRequestAsync,
                ServerUrlOverride = new Uri("https://kmeleontech.atlassian.net/rest/api/latest/"),
                HttpClient = httpClient,
            });



        Console.WriteLine("======== Example 1: Use automated function calling with a non-streaming prompt ========");
        {
            OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                                                     ChatSystemPrompt = "This assistant will provide the user with both Azure and Jira operations" };
            // Get user input for the prompt
            Console.Write("Enter the prompt: ");
            string userPrompt = Console.ReadLine();
            Stopwatch stopwatch = Stopwatch.StartNew();
            // Invoke the prompt with user input
            Console.WriteLine(await kernel.InvokePromptAsync(userPrompt, new(settings)));
            Console.WriteLine();
            // Stop the stopwatch.
            stopwatch.Stop();
            // Write the time taken to execute the code.
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} milliseconds");        
        }
    }
}

