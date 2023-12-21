
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.AI;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using FunctionCalling;
using System.Diagnostics;
using SKFunctionCalling;
// This example shows how to use OpenAI's tool calling capability via the chat completions interface.
public static class SKFunctionCallingTest
{
    static async Task Main(string[] args)
    {
        await RunAsync();
    }
    public static async Task RunAsync()
    {
        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion("MODEL", "ENDPOINT", "APIKEY");
        Kernel kernel = builder.Build();
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "plugins");
        var customHandler = new CustomHttpClientHandler();
        HttpClient azureHttpClient = new(customHandler);
        //Generate the token from Kairo DEV. Enable Azure in plugins and get the token from the header of any http call (X-Sk-Copilot-Azure-Auth)
        BearerAuthenticationProvider authenticationProvider = new(() => Task.FromResult(""));
        await kernel.ImportPluginFromOpenApiAsync(
        pluginName: "AzurePlugin",
        filePath: Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "plugins", "AzurePlugin/openapi.json"),
        new OpenApiFunctionExecutionParameters
        {
            AuthCallback = authenticationProvider.AuthenticateRequestAsync,
            HttpClient = azureHttpClient
        });


        Stopwatch stopwatch = Stopwatch.StartNew();
        Console.WriteLine("======== Example 1: Use automated function calling with a non-streaming prompt ========");
        {
            OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
            Console.WriteLine(await kernel.InvokePromptAsync("RESTART virtual machine with name vm-kairo-dev-001 for subscriptionId 'fb3a669d-9793-49f3-9607-3881a804cff6', resource group rg-genresources-dev-eastus and api-version 2023-03-1. DON´t forget to add api-version as a query parameter", new(settings)));
            Console.WriteLine();
            // Stop the stopwatch.
            stopwatch.Stop();

            // Write the time taken to execute the code.
            Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} milliseconds");
        }



        Stopwatch stopwatch1 = Stopwatch.StartNew();
        Console.WriteLine("======== Example 3: Use manual function calling with a non-streaming prompt ========");
        {
            var chat = kernel.GetRequiredService<IChatCompletionService>();
            var chatHistory = new ChatHistory();

            OpenAIPromptExecutionSettings settings = new() { ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions };
            chatHistory.AddUserMessage("Restart virtual machine vm-kairo-dev-001 for subscriptionId 'fb3a669d-9793-49f3-9607-3881a804cff6', resource group rg-genresources-dev-eastus and api version 2023-03-1");
            while (true)
            {
                var result = (OpenAIChatMessageContent)await chat.GetChatMessageContentAsync(chatHistory, settings, kernel);

                if (result.Content is not null)
                {
                    Console.Write(result.Content);
                }

                List<ChatCompletionsFunctionToolCall> toolCalls = result.ToolCalls.OfType<ChatCompletionsFunctionToolCall>().ToList();
                if (toolCalls.Count == 0)
                {
                    break;
                }

                chatHistory.Add(result);
                foreach (var toolCall in toolCalls)
                {
                    string content = kernel.Plugins.TryGetFunctionAndArguments(toolCall, out KernelFunction? function, out KernelArguments? arguments) ?
                        JsonSerializer.Serialize((await function.InvokeAsync(kernel, arguments)).GetValue<object>()) :
                        "Unable to find function. Please try again!";

                    chatHistory.Add(new ChatMessageContent(
                        AuthorRole.Tool,
                        content,
                        metadata: new Dictionary<string, object?>(1) { { OpenAIChatMessageContent.ToolIdProperty, toolCall.Id } }));
                }
            }

            Console.WriteLine();
        }
        stopwatch1.Stop();

        // Write the time taken to execute the code.
        Console.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds} milliseconds");
    }
}// See https://aka.ms/new-console-template for more information

