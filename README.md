# TinyHttpServer for Unity

`TinyHttpServer` is a simple HTTP server implemented in C#. It uses the `HttpListener` class to listen for HTTP requests and handle them asynchronously.

## Features

- **GET and POST Request Handling**: The server can handle GET and POST requests. For GET requests, it serves files from the specified document root. For POST requests, it can process both text and binary data.

- **Customizable Port and Document Root**: You can set the port and document root for the server.

- **Delegates for Data Processing**: You can assign delegates to process the received text and binary data from POST requests.

- **Server Status**: You can check if the server is running.

## Usage

1. Create an instance of `TinyHttpServer`.
2. Set the `Port` and `DocumentRoot` properties.
3. Assign delegates to `OnReceiveText` and `OnReceiveBinary` if you want to process POST data.
4. Call `StartServer()` to start the server.
5. Call `StopServer()` to stop the server.

## Example

```csharp
var server = new TinyHttpServer();
server.Port = 8080;
server.DocumentRoot = "/path/to/your/document/root";
server.OnReceiveText = textData => Debug.Log($"Received text: {textData}");
server.OnReceiveBinary = binaryData => Debug.Log($"Received binary data of length: {binaryData.Length}");
server.StartServer();
```

Please note that you need to handle exceptions and ensure that the server is stopped properly when your application is closing.
```
