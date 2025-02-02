using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace NeutronSharpWebview.Content;

public sealed class WebContent : IWebviewContent
{
    private readonly WebApplication webApp;

    public WebContent(string path, int port = 0, bool activateLog = false, IDictionary<string, string> additionalMimeTypes = null)
    {
        WebApplicationBuilder webApplicationBuilder = WebApplication.CreateBuilder();
        webApplicationBuilder.WebHost.UseKestrel((options) => options.Listen(IPAddress.Loopback, port));

        if (!activateLog)
        {
            webApplicationBuilder.Logging.ClearProviders();
        }

        webApp = webApplicationBuilder.Build();

        FileServerOptions fileServerOptions = new FileServerOptions
        {
            FileProvider = new PhysicalFileProvider(Path.Combine(Environment.CurrentDirectory, path)),
            RequestPath = "",
            EnableDirectoryBrowsing = true
        };

        if (additionalMimeTypes != null)
        {
            FileExtensionContentTypeProvider fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();

            foreach (KeyValuePair<string, string> additionalMimeType in additionalMimeTypes)
            {
                fileExtensionContentTypeProvider.Mappings.Add(additionalMimeType);
            }

            fileServerOptions.StaticFileOptions.ContentTypeProvider = fileExtensionContentTypeProvider;
        }

        webApp.UseFileServer(fileServerOptions);
        webApp.Start();
    }

    public string ToWebviewUrl()
    {
        return webApp.Urls.First();
    }
}
