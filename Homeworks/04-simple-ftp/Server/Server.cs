// <copyright file="Server.cs" company="ArtemNikit1n">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace Server;

using System.Net;
using System.Net.Sockets;

public class Server(int port) : IDisposable
{
    private const string DirectoryNotFoundResponse = "-1";
    private const string BadRequestCode = "400";
    private const string ForbiddenCode = "403";
    private const string InternalServerErrorCode = "500";
    private readonly Lazy<TcpListener> listener = new(() => new TcpListener(IPAddress.Any, port));
    private readonly CancellationTokenSource serverCts = new();
    private readonly List<Task> currentTasks = [];

    public async Task RunAsync()
    {
        ObjectDisposedException.ThrowIf(this.serverCts.Token.IsCancellationRequested, nameof(Server));

        var listenerInstance = this.listener.Value;
        listenerInstance.Start();
        while (!this.serverCts.Token.IsCancellationRequested)
        {
            try
            {
                var socket = await listenerInstance.AcceptSocketAsync(this.serverCts.Token);
                var task = Task.Run(async () =>
                {
                    await using var stream = new NetworkStream(socket);
                    using var reader = new StreamReader(stream);
                    var data = await reader.ReadLineAsync();
                    if (data is not null)
                    {
                        var response = await ProcessRequestAsync(data);
                        await using var writer = new StreamWriter(stream);
                        await writer.WriteLineAsync(response);
                        await writer.FlushAsync();
                    }

                    socket.Close();
                });
                this.currentTasks.Add(task);
                this.currentTasks.RemoveAll(t => t.IsCompleted);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    public void Dispose()
    {
        if (this.serverCts.Token.IsCancellationRequested || !this.listener.IsValueCreated)
        {
            return;
        }

        this.listener.Value.Stop();
        this.serverCts.Cancel();
        Task.WaitAll(this.currentTasks, this.serverCts.Token);
        this.listener.Value.Dispose();
        this.serverCts.Dispose();
    }

    private static async Task<string> ProcessRequestAsync(string request)
    {
        if (string.IsNullOrEmpty(request))
        {
            return $"{BadRequestCode} Bad Request: Empty request";
        }

        var commandAndPath = request.Split(' ');
        if (commandAndPath.Length < 2)
        {
            return $"{BadRequestCode} Bad Request: Invalid request format";
        }

        var command = commandAndPath[0];
        var path = commandAndPath[1];

        if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            return $"{BadRequestCode} Bad Request: Invalid path characters";
        }

        return command switch
        {
            "1" => await HandleListCommandAsync(path),
            "2" => await HandleGetCommandAsync(path),

            // ReSharper disable once ArrangeTrailingCommaInMultilineLists
            _ => $"{BadRequestCode} Bad Request: Unknown command",
        };
    }

    private static Task<string> HandleListCommandAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
            {
                return Task.FromResult($"{BadRequestCode} Bad Request: Path cannot be empty");
            }

            var fullPath = Path.GetFullPath(path);
            var currentDirectory = Directory.GetCurrentDirectory();

            if (!fullPath.StartsWith(currentDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult($"{ForbiddenCode} Forbidden: Access denied");
            }

            if (!Directory.Exists(fullPath))
            {
                return Task.FromResult(DirectoryNotFoundResponse);
            }

            var entries = Directory.GetFileSystemEntries(fullPath);
            var result = new System.Text.StringBuilder();
            result.Append(entries.Length);
            foreach (var entry in entries)
            {
                var name = Path.GetFileName(entry);
                var isDirectory = Directory.Exists(entry);
                if (string.IsNullOrEmpty(name) || name.Contains(' '))
                {
                    continue;
                }

                result.AppendLine($" {name} {(isDirectory ? "true" : "false")}");
            }

            result.Append('\n');
            return Task.FromResult(result.ToString());
        }
        catch (UnauthorizedAccessException)
        {
            return Task.FromResult($"{ForbiddenCode} Forbidden: Access denied");
        }
        catch (PathTooLongException)
        {
            return Task.FromResult($"{BadRequestCode} Bad Request: Path too long");
        }
        catch (Exception ex) when (ex is IOException or ArgumentException)
        {
            return Task.FromResult($"{BadRequestCode} Bad Request: Invalid path");
        }
        catch (Exception)
        {
            return Task.FromResult($"{InternalServerErrorCode} Internal Server Error: Please, try again later");
        }
    }

    private static Task<string> HandleGetCommandAsync(string path)
    {
        throw new NotImplementedException();
    }
}