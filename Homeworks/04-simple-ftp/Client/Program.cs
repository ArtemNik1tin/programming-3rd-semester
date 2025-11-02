// <copyright file="Program.cs" company="ArtemNikit1n">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

using System.Net.Sockets;

const int port = 8888;
using var client = new TcpClient("localhost", port);
Console.WriteLine($"Sending \"Hello!\" to port {port}...");
var stream = client.GetStream();
var writer = new StreamWriter(stream);
writer.WriteLine("Hello!");
writer.Flush();
var reader = new StreamReader(stream);
var data = reader.ReadToEnd();
Console.WriteLine($"Received: {data}");