using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace EasyScript.HtmlHelpers
{
    public static class EasyScriptHelpers
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
                .Select(k => storage[k])
                .Cast<string>();
            return html.Raw(string.Join(string.Empty, result));
        }
    }
}