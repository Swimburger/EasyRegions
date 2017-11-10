using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace EasyRegions.HtmlHelpers
{
    public static class EasyRegionsHelpers
    {
        private const string EasyScriptKey = "EasyScript";

        public static string CaptureScript(this HtmlHelper html, Func<HelperResult> script, string group, string id)
        {
            var key = $"{group}{id}";
            var contextItems = html.ViewContext.HttpContext.Items;
            var storage = contextItems[EasyScriptKey] as Dictionary<string, string> ?? new Dictionary<string, string>();
            if (storage.ContainsKey(key))
            {
                return string.Empty;
            }

            storage[key] = script.Invoke().ToHtmlString();
            contextItems[EasyScriptKey] = storage;
            return string.Empty;
        }

        public static ViewRecorder StartCaptureRegion(this HtmlHelper html, string group, string id)
        {
            var key = $"{group}{id}";
            var contextItems = html.ViewContext.HttpContext.Items;
            var storage = contextItems[EasyScriptKey] as Dictionary<string, string> ?? new Dictionary<string, string>();

            var recorder = new ViewRecorder(
                (WebViewPage)html.ViewDataContainer,
                recordedHtml =>
                {
                    if (storage.ContainsKey(key))
                    {
                        return;
                    }

                    storage[key] = recordedHtml;
                    contextItems[EasyScriptKey] = storage;
                });
            return recorder;
        }

        public static IHtmlString RenderScripts(this HtmlHelper html, string group, string id = "")
        {
            var key = $"{group}{id}";

            var contextItems = html.ViewContext.HttpContext.Items;
            var storage = contextItems[EasyScriptKey] as Dictionary<string, string>;
            if (storage == null)
            {
                return new HtmlString(string.Empty);
            }

            var result = storage.Keys
                .Where(k => k.StartsWith(key))
                .Select(k => storage[k]);
            return html.Raw(string.Join(string.Empty, result));
        }
    }

    public class ViewRecorder : IDisposable
    {
        private readonly WebViewPage webpage;
        private readonly Action<string> onRecordEnd;

        public ViewRecorder(WebViewPage webpage, Action<string> onRecordEnd)
        {
            this.webpage = webpage;
            this.onRecordEnd = onRecordEnd;
            this.webpage.OutputStack.Push(new StringWriter());
        }

        public void Dispose()
        {
            var markup = webpage.OutputStack.Pop().ToString();
            onRecordEnd?.Invoke(markup);
        }
    }
}