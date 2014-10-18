﻿using System;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Swashbuckle.WebAssets;

namespace Swashbuckle.Application
{
    public class SwaggerUi20Config
    {
        private readonly Dictionary<string, string> _replacements;
        private readonly Dictionary<string, EmbeddedResourceDescriptor> _customWebAssets;

        public SwaggerUi20Config()
        {
            _replacements = new Dictionary<string, string>
            {
                { "%(StylesheetIncludes)", "" },
                { "%(SupportHeaderParams)", "false" },
                { "%(SupportedSubmitMethods)", "['get', 'post', 'put', 'delete']" },
                { "%(CustomScripts)", "[]" },
                { "%(DocExpansion)", "\"none\"" }
            };
            _customWebAssets = new Dictionary<string, EmbeddedResourceDescriptor>();

            // Use Swashbuckle specific index.html
            CustomWebAsset("index.html", GetType().Assembly, "Swashbuckle.SwaggerExtensions.index.html");
        }

        public SwaggerUi20Config InjectStylesheet(Assembly resourceAssembly, string resourceName)
        {
            var path = "ext/" + resourceName;
            var stringBuilder = new StringBuilder(_replacements["%(StylesheetIncludes)"]);

            stringBuilder.AppendLine("<link href='" + path + "' media='screen' rel='stylesheet' type='text/css' />");
            _replacements["%(StylesheetIncludes)"] = stringBuilder.ToString();

            _customWebAssets[path] = new EmbeddedResourceDescriptor(resourceAssembly, resourceName);
            return this;
        }

        public SwaggerUi20Config SupportHeaderParams()
        {
            _replacements["%(SupportHeaderParams)"] = "true";
            return this;
        }

        public SwaggerUi20Config SupportedSubmitMethods(HttpMethod[] httpMethods)
        {
            var httpMethodsString = String.Join(",",
                httpMethods.Select(method => "'" + method.ToString().ToLower() + "'"));

            _replacements["%(SupportedSubmitMethods)"] = "[" + httpMethodsString + "]";
            return this;
        }

        public SwaggerUi20Config DocExpansion(DocExpansion docExpansion)
        {
            _replacements["%(DocExpansion)"] = "\"" + docExpansion.ToString().ToLower() + "\"";
            return this;
        }

        public SwaggerUi20Config InjectJavaScript(Assembly resourceAssembly, string resourceName)
        {
            var path = "ext/" + resourceName;
            var stringBuilder = new StringBuilder(_replacements["%(CustomScripts)"]);

            if (stringBuilder.Length == 2)
                stringBuilder.Replace("[]", "[ '" + path + "' ]");
            else
                stringBuilder.Replace(" ]", ", '" + path + "' ]");

            _replacements["%(CustomScripts)"] = stringBuilder.ToString();

            _customWebAssets[path] = new EmbeddedResourceDescriptor(resourceAssembly, resourceName);
            return this;
        }

        public SwaggerUi20Config CustomWebAsset(string path, Assembly resourceAssembly, string resourceName)
        {
            _customWebAssets[path] = new EmbeddedResourceDescriptor(resourceAssembly, resourceName);
            return this;
        }

        internal IWebAssetProvider GetSwaggerUiProvider(HttpRequestMessage request)
        {
            return new EmbeddedWebAssetProvider(_replacements, _customWebAssets);
        }
    }
}