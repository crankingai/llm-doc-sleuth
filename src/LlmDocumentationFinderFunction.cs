using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using System.Linq;


#pragma warning disable SKEXP0050 // BingTextSearch is not yet available in the public API
// Create an ITextSearch instance using Bing search

namespace SemanticKernelDev.Plugins;

public class LlmDocumentationFinderFunction
{
    static int tryCount = 0;


    [KernelFunction("search_web_for_llm_documentation")]
    // [Description("Returns details of how AI LLM APIs use color.")]
    [Description("Searches web for websites that may contain relevant documentation. Returns a list of URLs.")]
    [return: Description("List of URLs.")]
    public async Task<List<string>> SearchWebForLlmDocumentationAsync(
        Kernel kernel,
        string search_string)
    {
#if false // short circuit for debugging
        var reply = new List<string>();

        // randomly return one of the next two
        if (new Random().Next(2) == 0)
            reply.Add($"Trains are fun to ride in!");
        else
            reply.Add($"LLM's favorite color is cyan.");

        Console.Write($"¿¿¿¿¿¿¿¿¿¿¿: {reply[0]} **** ");
        return reply;
#endif

        Console.Write($"¿¿¿¿¿¿¿¿¿¿¿ [try #{++tryCount}] *> Bing Web Searching: {search_string} *> ");

        string bingSearchEndpoint, bingSearchKey;
        (bingSearchEndpoint, bingSearchKey) = Config.EnvVarReader.GetBingSearchConfig();

        var textSearch = new BingTextSearch(apiKey: bingSearchKey);
        string query = "";
        // flip a coin
        if (new Random().Next(2) == 0)
            query = $"Link to {search_string} png or jpg image";
        else
            query = $"href to {search_string} file";

#pragma warning disable SKEXP0001 // 'KernelSearchResults<string>' is for evaluation purposes only and is subject to change or removal in future updates.
        KernelSearchResults<string> searchResults = await textSearch.SearchAsync(query, new() { Top = 4 });
#pragma warning restore SKEXP0001

        var resultsList = new List<string>();
        await foreach (string result in searchResults.Results)
        {
            Console.WriteLine($"¿¿¿¿¿ - one of the bing search results: {result}");
            resultsList.Add(result);
        }

        return resultsList;
    }
}
