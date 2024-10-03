using com.afjk.tinyhttpserver;
using UnityEditor;
using UnityEngine;

public class TinyHttpServerEditor : EditorWindow
{
    private string serverUrl = "";
    private HttpServerDemo server;
    private bool serverRunning = false;

    [MenuItem("Tools/Tiny HTTP Server")]
    public static void ShowWindow()
    {
        GetWindow<TinyHttpServerEditor>("Tiny HTTP Server");
    }

    private void OnEnable()
    {
        server = new HttpServerDemo();
    }

    private void OnGUI()
    {
        GUILayout.Label("Tiny HTTP Server", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Port Number:", GUILayout.Width(100));
        server.Port = EditorGUILayout.IntField(server.Port);
        GUILayout.EndHorizontal();
        
        if (!serverRunning)
        {
            if (GUILayout.Button("Start Server"))
            {
                server.StartServer();
                serverRunning = true;

                serverUrl = $"http://{TinyHttpServer.GetHostName()}:{server.Port}/";
            }
        }
        else
        {
            if (GUILayout.Button("Stop Server"))
            {
                server.StopServer();
                serverRunning = false;
                serverUrl = "";
            }
        }

        GUILayout.Space(10);

        if (!string.IsNullOrEmpty(serverUrl))
        {
            GUILayout.Label("Server URL:", EditorStyles.boldLabel);
            EditorGUILayout.TextField(serverUrl);

            if (GUILayout.Button("Copy URL"))
            {
                EditorGUIUtility.systemCopyBuffer = serverUrl;
            }

            if (GUILayout.Button("Open in Browser"))
            {
                Application.OpenURL(serverUrl);
            }
        }
        
        // Add a button to open the Document folder
        if (GUILayout.Button("Open Document Folder"))
        {
            // Open the Document folder using the default file explorer
            System.Diagnostics.Process.Start(server.DocumentRoot);
        }
    }

    private void OnDisable()
    {
        if (serverRunning)
        {
            server.StopServer();
        }
    }
}
