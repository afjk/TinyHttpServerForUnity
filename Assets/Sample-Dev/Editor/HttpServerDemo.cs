using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

    private string GenerateHtml()
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html>");
        sb.AppendLine("<head><title>Tiny HTTP Server for Unity</title></head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<h1>Hello Tiny HTTP Server for Unity</h1>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }
}