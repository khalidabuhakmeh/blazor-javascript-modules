using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Toasty.Core
{
    public static class ExampleJsInterop
    {
        public static async ValueTask<string> Prompt(this IJSRuntime jsRuntime, string message)
        {
            // Implemented in exampleJsInterop.js
            var module = await jsRuntime.InvokeAsync<JSObjectReference>("import", "./_content/Toasty.Core/exampleJsInterop.js");
            return await module.InvokeAsync<string>("showPrompt", message);
        }
    }
    
    public class Example {}
}