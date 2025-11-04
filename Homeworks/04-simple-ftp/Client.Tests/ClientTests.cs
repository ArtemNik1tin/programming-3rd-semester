// <copyright file="ClientTests.cs" company="ArtemNikit1n">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace Client.Tests;

using System.Net;
using System.Net.Sockets;

#pragma warning disable SA1600
public class ClientTests
{
    private int freePort;
    private TcpListener? testServer;
    private CancellationTokenSource? cts;

    [SetUp]
    public void Setup()
    {
        this.freePort = FindFreePort();
        this.cts = new CancellationTokenSource();
        this.testServer = new TcpListener(IPAddress.Loopback, this.freePort);
        this.testServer.Start();
    }

    [TearDown]
    public void TearDown()
    {
        this.testServer?.Stop();
        this.cts?.Cancel();
        this.testServer?.Dispose();
        this.cts?.Dispose();
    }

    [Test]
    public void ListDirectoryAsync_Should_ThrowInvalidOperationException_When_ClientNotConnected()
    {
        var client = new Client("localhost", this.freePort);
        const string path = "./test";

        Assert.ThrowsAsync<InvalidOperationException>(() => client.ListDirectoryAsync(path));
    }

    [Test]
    public async Task ListDirectoryAsync_Should_ReturnBadRequest_When_PathIsEmpty()
    {
        var client = new Client("localhost", this.freePort);
        await client.ConnectAsync();

        var result = await client.ListDirectoryAsync(string.Empty);

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("400"));
            Assert.That(result.ErrorMessage, Does.Contain("Path cannot be empty"));
        });
    }

    [Test]
    public async Task ListDirectoryAsync_Should_ReturnBadRequest_When_PathContainsInvalidCharacters()
    {
        var client = new Client("localhost", this.freePort);
        await client.ConnectAsync();

        var result = await client.ListDirectoryAsync("invalid|path*");

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("400"));
            Assert.That(result.ErrorMessage, Does.Contain("Invalid path characters"));
        });
    }

    [Test]
    public async Task ListDirectoryAsync_Should_ReturnDirectoryNotFound_When_ServerReturnsMinusOne()
    {
        _ = Task.Run(async () =>
        {
            var client = await this.testServer!.AcceptTcpClientAsync();
            var stream = client.GetStream();
            var writer = new StreamWriter(stream);
            await writer.WriteLineAsync("-1");
            await writer.FlushAsync();
        });

        var client = new Client("localhost", this.freePort);
        await client.ConnectAsync();

        var result = await client.ListDirectoryAsync("./nonexistent");

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.DirectoryExists, Is.False);
            Assert.That(result.Items, Is.Empty);
        });
    }

    [Test]
    public async Task ListDirectoryAsync_Should_ParseSuccessfulResponse_When_ServerReturnsValidData()
    {
        _ = Task.Run(async () =>
        {
            var client = await this.testServer!.AcceptTcpClientAsync();
            var stream = client.GetStream();
            var writer = new StreamWriter(stream);
            await writer.WriteLineAsync("2 file1.txt false directory true");
            await writer.FlushAsync();
        });

        var client = new Client("localhost", this.freePort);
        await client.ConnectAsync();

        var result = await client.ListDirectoryAsync("./test");

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.True);
            Assert.That(result.DirectoryExists, Is.True);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Items, Has.Count.EqualTo(2));

            Assert.That(result.Items[0].Name, Is.EqualTo("file1.txt"));
            Assert.That(result.Items[0].IsDirectory, Is.False);

            Assert.That(result.Items[1].Name, Is.EqualTo("directory"));
            Assert.That(result.Items[1].IsDirectory, Is.True);
        });
    }

    [Test]
    public async Task ListDirectoryAsync_Should_ReturnError_When_ServerReturnsMismatchedPairs()
    {
        _ = Task.Run(async () =>
        {
            var client = await this.testServer!.AcceptTcpClientAsync();
            var stream = client.GetStream();
            var writer = new StreamWriter(stream);
            await writer.WriteLineAsync("2 file1.txt false directory");
            await writer.FlushAsync();
            client.Close();
        });

        var client = new Client("localhost", this.freePort);
        await client.ConnectAsync();

        var result = await client.ListDirectoryAsync("./test");

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("mismatched name/isDir pairs"));
        });
    }

    [Test]
    public async Task ListDirectoryAsync_Should_ReturnError_When_ServerReturnsInvalidIsDirValue()
    {
        _ = Task.Run(async () =>
        {
            var client = await this.testServer!.AcceptTcpClientAsync();
            var stream = client.GetStream();
            var writer = new StreamWriter(stream);
            await writer.WriteLineAsync("1 file1.txt invalid");
            await writer.FlushAsync();
            await Task.Delay(100);
            client.Close();
        });

        var client = new Client("localhost", this.freePort);
        await client.ConnectAsync();

        var result = await client.ListDirectoryAsync("./test");

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("invalid isDir value"));
        });
    }

    [Test]
    public async Task ListDirectoryAsync_Should_ReturnError_When_ServerReturnsInvalidCountFormat()
    {
        _ = Task.Run(async () =>
        {
            var client = await this.testServer!.AcceptTcpClientAsync();
            var stream = client.GetStream();
            var writer = new StreamWriter(stream);
            await writer.WriteLineAsync("invalid file1.txt false");
            await writer.FlushAsync();
            await Task.Delay(100);
            client.Close();
        });

        var client = new Client("localhost", this.freePort);
        await client.ConnectAsync();

        var result = await client.ListDirectoryAsync("./test");

        Assert.Multiple(() =>
        {
            Assert.That(result.Success, Is.False);
            Assert.That(result.ErrorMessage, Does.Contain("Invalid the number of files and folders"));
        });
    }

    [Test]
    public async Task ListDirectoryAsync_Should_HandleServerErrorResponses()
    {
        var errorResponses = new[]
        {
            "400 Bad Request: Invalid path",
            "403 Forbidden: Access denied",
            "500 Internal Server Error: Please, try again later",
        };

        foreach (var errorResponse in errorResponses)
        {
            _ = Task.Run(async () =>
            {
                var client = await this.testServer!.AcceptTcpClientAsync();
                var stream = client.GetStream();
                var writer = new StreamWriter(stream);
                await writer.WriteLineAsync(errorResponse);
                await writer.FlushAsync();
                await Task.Delay(100);
                client.Close();
            });

            var client = new Client("localhost", this.freePort);
            await client.ConnectAsync();

            var result = await client.ListDirectoryAsync("./test");

            Assert.Multiple(() =>
            {
                Assert.That(result.Success, Is.False);
                Assert.That(result.ErrorMessage, Is.EqualTo(errorResponse));
            });
        }
    }

    private static int FindFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}