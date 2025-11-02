using System.Net.Sockets;

namespace Client;

public class Client(string host = "localhost", int port = 8080) : IDisposable
{
    private const string DirectoryNotFoundResponse = "-1";
    private const string BadRequestCode = "400";
    private const string ForbiddenCode = "403";
    private const string InternalServerErrorCode = "500";

    private TcpClient? client;
    private NetworkStream? stream;
    private StreamReader? reader;
    private StreamWriter? writer;

    public async Task ConnectAsync()
    {
        this.client = new TcpClient(host, port);
        await this.client.ConnectAsync(host, port);
        this.stream = this.client.GetStream();
        this.reader = new StreamReader(this.stream);
        this.writer = new StreamWriter(this.stream);
    }

    public async Task<ListResult> ListDirectoryAsync(string path)
    {
        if (this.reader == null || this.writer == null)
        {
            throw new InvalidOperationException("Client not connected");
        }

        await this.writer.WriteLineAsync($"1 {path}");
        await this.writer.FlushAsync();

        var response = await this.reader.ReadLineAsync();
        return response == null ? new ListResult { Success = false, ErrorMessage = "No response from server" } : ParseListResponse(response);
    }

    public void Dispose()
    {
        this.reader?.Dispose();
        this.writer?.Dispose();
        this.stream?.Dispose();
        this.client?.Dispose();
    }

    private static ListResult ParseListResponse(string response)
    {
        if (response.StartsWith(BadRequestCode) || response.StartsWith(ForbiddenCode) || response.StartsWith(InternalServerErrorCode))
        {
            return new ListResult { Success = false, ErrorMessage = response };
        }

        if (response == DirectoryNotFoundResponse)
        {
            return new ListResult { Success = true, Items = [], DirectoryExists = false };
        }

        try
        {
            var parts = response.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return new ListResult { Success = false, ErrorMessage = "Invalid response format" };
            }

            if (!int.TryParse(parts[0], out var count))
            {
                return new ListResult { Success = false, ErrorMessage = "Invalid the number of files and folders in the directory in response" };
            }

            var items = new List<FileSystemItem>();

            for (var i = 1; i < parts.Length; i += 2)
            {
                if (i + 1 >= parts.Length)
                {
                    break;
                }

                var name = parts[i];
                var isDir = parts[i + 1] == "true";

                items.Add(new FileSystemItem { Name = name, IsDirectory = isDir });
            }

            return new ListResult
            {
                Success = true,
                Items = items,
                DirectoryExists = true,
                Count = count,
            };
        }
        catch (Exception ex)
        {
            return new ListResult { Success = false, ErrorMessage = $"Failed to parse response: {ex.Message}" };
        }
    }

    public class ListResult
    {
        public bool Success { get; set; }

        public string? ErrorMessage { get; set; }

        public bool DirectoryExists { get; set; }

        public int Count { get; set; }

        public List<FileSystemItem> Items { get; set; }
    }

    public class FileSystemItem
    {
        public string Name { get; set; } = string.Empty;

        public bool IsDirectory { get; set; }
    }
}