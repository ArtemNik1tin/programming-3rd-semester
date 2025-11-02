// <copyright file="Server.cs" company="ArtemNikit1n">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace Server;

using System.Net;
using System.Net.Sockets;

public class Server(int port) : IDisposable
{
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
                var task = Task.Run(() => Task.CompletedTask);
                this.currentTasks.Add(task);
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
}