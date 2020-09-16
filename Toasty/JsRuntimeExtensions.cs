using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Toasty
{
    public static class JsRuntimeExtensions
    {
        public static async Task<JsObjectReferenceDynamic> Import<T>(
            this IJSRuntime jsRuntime,
            string pathFromWwwRoot)
        {
            var libraryName = typeof(T).Assembly.GetName().Name; 
            var module = await jsRuntime.InvokeAsync<JSObjectReference>(
                "import", 
                Path.Combine($"./_content/{libraryName}/", pathFromWwwRoot)
            );
            return new JsObjectReferenceDynamic(module);
        }
    }

    public class JsObjectReferenceDynamic : DynamicObject
    {
        public JSObjectReference Module { get; }

        public JsObjectReferenceDynamic(JSObjectReference module)
        {
            Module = module;
        }
        
        public override bool TryInvokeMember(InvokeMemberBinder binder, object?[]? args, out object? result)
        {
            var csharpBinder = binder.GetType().GetInterface("Microsoft.CSharp.RuntimeBinder.ICSharpInvokeOrInvokeMemberBinder");
            var typeArgs = 
                csharpBinder!.GetProperty("TypeArguments")?.GetValue(binder, null) as IList<Type> ??
                Array.Empty<Type>();
            
            var jsObjectReferenceType = typeof(JSObjectReference);

            MethodInfo methodInfo;

            if (typeArgs.Any())
            {
                var method = jsObjectReferenceType
                    .GetMethods()
                    .First(x => x.Name.Contains(nameof(Module.InvokeAsync)));
            
                // only support one generic
                methodInfo = method.MakeGenericMethod(typeArgs.First());
            }
            else
            {
                methodInfo = jsObjectReferenceType
                    .GetMethods()
                    .First(x => x.Name.Contains(nameof(Module.InvokeVoidAsync)));
            }
            
            var task = methodInfo.Invoke(Module, new object[]{ binder.Name, args });
            result = task;
            return true;
        }
    }
}