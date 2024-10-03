# TinyHttpServer for Unity

`TinyHttpServer` is a simple HTTP server implemented in C#. It uses the `HttpListener` class to listen for HTTP requests and handle them asynchronously.

## Features

- **GET and POST Request Handling**: The server can handle GET and POST requests.

- **Customizable Port and Document Root**: You can set the port and document root for the server.

- **Delegates for Data Processing**: You can assign delegates to process the received text and binary data from POST requests.

## Installation

To install the TinyHttpServer package using OpenUPM, run this command:

```
openupm add com.afjk.tinyhttpserver
```

or

To install the TinyHttpServer package using Unity Package Manager, follow these steps:

1.	Open the Unity Editor and go to Window -> Package Manager.
2.	Click the + button in the top-left corner.
3.	Select Add package from git URL....
4.	Enter the following URL to install the latest version:

```
https://github.com/afjk/TinyHttpServerForUnity.git?path=/Packages/com.afjk.tinyhttpserver#v0.0.1
```

## Importing and Running Samples

After installing the TinyHttpServer package, you can use the built-in Editor extension to launch a sample HTTP server:

1.	Open the Unity Editor and go to the menu bar.
2.	Navigate to Tools -> Tiny HTTP Server to open the server control panel.
3.	In the Port Number field, specify the desired port (e.g., 8080).
4.	Click the Start Server button to launch the HTTP server.

Verifying the Server

Once the server is started, the control panel will display the Server URL. You can:

* Copy URL: Copy the server URL to your clipboard.
* Open in Browser: Launch your default web browser to view the server’s content.
* Open Document Folder: Open the folder that serves as the document root for the server.

The attached screenshots below show the sample UI for starting and managing the server:

<img width="319" alt="image" src="https://github.com/user-attachments/assets/a463ad6b-4e19-4199-84bb-c0f0a26c6ad5">

<img width="319" alt="image" src="https://github.com/user-attachments/assets/a403412e-fbab-4d5a-b5ab-7961d4c70887">

## Usage

To use the server, you need to create an instance of the `TinyHttpServer` class, define your routes and start the server. Here is a basic example:

```csharp
var server = new TinyHttpServer();
server.AddGetRoute("/example", HandleExampleGet);
server.AddPostRoute("/example", HandleExamplePost);
server.StartServer();
