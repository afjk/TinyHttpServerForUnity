using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace com.afjk.tinyhttpserver
{
    public class TinyHttpServer
    {
        private readonly HttpListener httpListener = new HttpListener();
        private　 bool serverRunning = false;
        private Task serverTask;

        public int Port { get; set; }
        public string DocumentRoot { get; set; }

        // POSTリクエストのテキストデータ処理用のデリゲート
        public Action<string, string> OnReceiveText { get; set; }

        // POSTリクエストのバイナリデータ処理用のデリゲート
        public Action<string, byte[]> OnReceiveBinary { get; set; }

        // ルートとハンドラーをマップするディクショナリ
        public delegate Task RouteHandler(HttpListenerContext context);
        public Dictionary<string, Dictionary<string, RouteHandler>> Routes { get; } = new Dictionary<string, Dictionary<string, RouteHandler>>();

        public void AddRoute(string route, string method, RouteHandler handler)
        {
            Debug.Log($"{route} {method}");
            if (!Routes.ContainsKey(route))
            {
                Routes[route] = new Dictionary<string, RouteHandler>();
            }
            Routes[route][method] = handler;
            Debug.Log(Routes);
        }
        
        public bool? IsRunning => serverRunning;

        public void StartServer()
        {
            if (serverRunning) return;

            httpListener.Prefixes.Add($"http://*:{Port}/");
            httpListener.Start();
            serverRunning = true;

            Debug.Log("Server started.");

            serverTask = Task.Run(RunServer);
        }

        public void StopServer()
        {
            if (!serverRunning) return;

            serverRunning = false;
            httpListener.Stop();

            Debug.Log("Server stopped.");

            if (serverTask == null) return;
            try
            {
                serverTask.Wait();
            }
            catch (AggregateException ex)
            {
                Debug.LogError($"Server task ended with exception: {ex.InnerException.Message}");
            }
        }

        private async Task RunServer()
        {
            try
            {
                while (serverRunning)
                {
                    var context = await httpListener.GetContextAsync();
                    Debug.Log($"{context.Request.HttpMethod} Request path: {context.Request.RawUrl}");

                    // リクエストの処理をここで行う
                    await HandleRequest(context);
                }
            }
            catch (HttpListenerException ex)
            {
                // httpListener.Stop() を呼び出すと発生する例外を無視
                if (ex.ErrorCode != 995) // 995 は ERROR_OPERATION_ABORTED
                {
                    Debug.LogError($"HttpListenerException in RunServer: {ex.Message}");
                }
            }
            catch (ObjectDisposedException)
            {
                // `httpListener` が Dispose された場合の例外を無視
                // ログを出力せず、何もしない
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception in RunServer: {ex.Message}");
            }
        }

        private async Task HandleRequest(HttpListenerContext context)
        {
            // HTTPメソッドの取得
            string httpMethod = context.Request.HttpMethod;

            Debug.Log($"[dbg] {httpMethod}");
            // リクエストパスの取得
            string requestPath = context.Request.RawUrl;
            Debug.Log($"[dbg] {requestPath}");

            
            if (Routes.ContainsKey(requestPath) && Routes[requestPath].ContainsKey(httpMethod))
            {
                // ルートとして処理
                await Routes[requestPath][httpMethod](context);
            }
            else
            {
                // ファイルパスとして処理
                await HandleGetRequest(context);
            }
        }

        // GETリクエストの処理
        private async Task HandleGetRequest(HttpListenerContext context)
        {
            // リクエストパスを取得
            string requestPath = context.Request.RawUrl.TrimStart('/');

            // ルートパスへのアクセスの場合に index.html を返す
            if (string.IsNullOrEmpty(requestPath) || requestPath == "/")
            {
                requestPath = "index.html";
            }

            // リクエストされたファイルのフルパスを生成
            string filePath = Path.Combine(DocumentRoot, requestPath);
            Debug.Log($"file path: {filePath}");

            if (File.Exists(filePath))
            {
                context.Response.StatusCode = 200;

                // MIMEタイプを決定する
                string mimeType = GetMimeType(filePath);
                context.Response.ContentType = mimeType;

                byte[] buffer = await File.ReadAllBytesAsync(filePath);
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            else
            {
                // 404エラーメッセージの設定
                context.Response.StatusCode = 404;
                byte[] buffer = Encoding.UTF8.GetBytes("404 Not Found");
                context.Response.ContentType = "text/plain";
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }

            context.Response.Close();
        }

        private async Task HandlePostRequest(HttpListenerContext context)
        {
            try
            {
                // コンテンツタイプを取得
                string contentType = context.Request.ContentType;

                Debug.Log($"ContentType:{contentType}");
                if (contentType != null && (contentType.StartsWith("text/") || contentType == "application/json" || contentType == "application/x-www-form-urlencoded"))
                {
                    // テキストデータとして処理
                    using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                    {
                        string textData = await reader.ReadToEndAsync();
                        Debug.Log($"Received text data: {textData}");

                        // テキストデータの処理をデリゲートに委任
                        OnReceiveText?.Invoke(context.Request.RawUrl, textData);
                    }

                    // 成功レスポンスを返す
                    context.Response.StatusCode = 200;
                    byte[] buffer = Encoding.UTF8.GetBytes("POST request with text data received and processed successfully.");
                    context.Response.ContentType = "text/plain";
                    context.Response.ContentLength64 = buffer.Length;
                    await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                else
                {
                    // バイナリデータとして処理
                    long contentLength = context.Request.ContentLength64;
                    if (contentLength > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await context.Request.InputStream.CopyToAsync(memoryStream);
                            byte[] binaryData = memoryStream.ToArray();

                            Debug.Log($"Received binary data of length: {binaryData.Length}");

                            // バイナリデータの処理をデリゲートに委任
                            OnReceiveBinary?.Invoke(context.Request.RawUrl, binaryData);
                        }

                        // 成功レスポンスを返す
                        context.Response.StatusCode = 200;
                        byte[] buffer = Encoding.UTF8.GetBytes("POST request with binary data received and processed successfully.");
                        context.Response.ContentType = "text/plain";
                        context.Response.ContentLength64 = buffer.Length;
                        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        // コンテンツがない場合のエラーレスポンス
                        context.Response.StatusCode = 400; // Bad Request
                        byte[] buffer = Encoding.UTF8.GetBytes("No data received in POST request.");
                        context.Response.ContentType = "text/plain";
                        context.Response.ContentLength64 = buffer.Length;
                        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                // エラーレスポンスを返す
                Debug.LogError($"Error processing POST request: {ex.Message}");
                context.Response.StatusCode = 500; // Internal Server Error
                byte[] buffer = Encoding.UTF8.GetBytes("500 Internal Server Error");
                context.Response.ContentType = "text/plain";
                context.Response.ContentLength64 = buffer.Length;
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }

            context.Response.Close();
        }
        
        private string GetMimeType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            switch (extension)
            {
                case ".html":
                case ".htm":
                    return "text/html";
                case ".css":
                    return "text/css";
                case ".js":
                    return "application/javascript";
                case ".json":
                    return "application/json";
                case ".png":
                    return "image/png";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".gif":
                    return "image/gif";
                case ".svg":
                    return "image/svg+xml";
                case ".ico":
                    return "image/x-icon";
                case ".xml":
                    return "application/xml";
                case ".pdf":
                    return "application/pdf";
                case ".zip":
                    return "application/zip";
                case ".txt":
                    return "text/plain";
                // その他の拡張子についても必要に応じて追加
                default:
                    return "application/octet-stream"; // デフォルトはバイナリデータ
            }
        }

        public static string GetHostName()
        {
            string hostname = Dns.GetHostName();
            IPAddress[] adrList = Dns.GetHostAddresses(hostname);

            var usableIps = adrList
                .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .Where(ip => !ip.Equals(IPAddress.Parse("127.0.0.1")));

            var ipAddresses = usableIps as IPAddress[] ?? usableIps.ToArray();
            foreach (var ip in ipAddresses)
            {
                Debug.Log(ip);
            }

            return ipAddresses.FirstOrDefault()?.ToString();
        }
    }
}
