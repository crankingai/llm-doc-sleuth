using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace SemanticKernelDev.Plugins;

public class LlmDocumentationDownloaderFunction
{
    static int tryCount = 0;

    [KernelFunction("download_documentation_from_web")]
    [Description("Downloads and returns the documentation found at web_url.")]
    public async Task<bool> DownloadContentsAsync(
        Kernel kernel,
        string web_url
    )
    {
        Console.Write($"*********** [try #{++tryCount}] *> Validating URL: {web_url} *> ");

        // Retrieve URL data to local storage as a temp file
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(web_url);
        // if not successful (including 404), return false
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"❌ ***** FAIL 0! - http status code: {response.StatusCode}");
            return false;
        }

        Console.WriteLine("✅ ***** <<success>> :-)");
        return true;
    }
}
