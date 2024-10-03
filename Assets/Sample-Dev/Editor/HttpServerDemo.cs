using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using com.afjk.tinyhttpserver;
using UnityEngine;

public class HttpServerDemo
{
    private readonly TinyHttpServer server;
    private FileSystemWatcher fileWatcher;
    private readonly string documentDir = "html";
    private static readonly string HtmlFilePath = Path.Combine("html", "index.html");

    public int Port
    {
        get => server.Port;
        set => server.Port = value;
    }

    public string DocumentRoot => server.DocumentRoot;

    public HttpServerDemo()
    {
        server = new TinyHttpServer();
        server.Port = 8080;
        server.DocumentRoot = Path.Combine(Application.dataPath, "..", documentDir);
        server.AddRoute("/name_form", HandleNameFormPost);
    }


    private async Task HandleNameFormPost(HttpListenerContext context)
    {
        // フォームからの入力を処理
        using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
        {
            string textData = await reader.ReadToEndAsync();
            var formValues = textData.Split('&')
                .Select(value => value.Split('='))
                .ToDictionary(pair => Uri.UnescapeDataString(pair[0]), pair => Uri.UnescapeDataString(pair[1]));

            if (formValues.ContainsKey("name"))
            {
                var name = formValues["name"];
                var htmlContent = GenerateHtml(name);
                byte[] buffer = Encoding.UTF8.GetBytes(htmlContent);
                context.Response.ContentType = "text/html";
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
        }

        context.Response.Close();
    }
    public void StartServer()
    {
        GenerateStaticHtmlFile();
        server.StartServer();
    }

    public void StopServer()
    {
        server.StopServer();
    }

    private void GenerateStaticHtmlFile()
    {
        var htmlContent = GenerateHtml();

        Directory.CreateDirectory(Path.GetDirectoryName(HtmlFilePath));
        File.WriteAllText(HtmlFilePath, htmlContent);
    }

    private string GenerateHtml(string name = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html>");
        sb.AppendLine("<head><title>Tiny HTTP Server for Unity</title></head>");
        sb.AppendLine("<body>");
        sb.AppendLine($"<h1>Tiny HTTP Server for Unity</h1>");
        if (name != null)
        {
            sb.AppendLine($"<h2>Hello {name}</h2>");
        }
        sb.AppendLine("<form action='/name_form' method='post'>");
        sb.AppendLine("<input type='text' name='name' required>");
        sb.AppendLine("<input type='submit' value='Submit'>");
        sb.AppendLine("</form>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }
}