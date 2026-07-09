using System.IO.Pipes;
using System.Text;
using System.Text.Json;

namespace OmniWatch.SensorHost;

/// <summary>
/// 通过 Named Pipe 提供行分隔 JSON RPC 服务。
/// </summary>
internal sealed class SensorPipeServer
{
    private readonly string _pipeName;
    private readonly SensorSnapshotService _snapshotService;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// 初始化管道服务并保存共享的硬件采集器。
    /// </summary>
    public SensorPipeServer(string pipeName, SensorSnapshotService snapshotService, JsonSerializerOptions jsonOptions)
    {
        _pipeName = pipeName;
        _snapshotService = snapshotService;
        _jsonOptions = jsonOptions;
    }

    /// <summary>
    /// 持续接受客户端连接，直到收到关闭命令或取消信号。
    /// </summary>
    public async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        var shouldStop = false;
        while (!cancellationToken.IsCancellationRequested && !shouldStop)
        {
            await using var pipe = CreatePipe();
            await pipe.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
            shouldStop = await HandleClientAsync(pipe, cancellationToken).ConfigureAwait(false);
        }

        return 0;
    }

    /// <summary>
    /// 创建单实例入站管道。
    /// </summary>
    private NamedPipeServerStream CreatePipe()
    {
        return new NamedPipeServerStream(
            _pipeName,
            PipeDirection.InOut,
            1,
            PipeTransmissionMode.Byte,
            PipeOptions.Asynchronous);
    }

    /// <summary>
    /// 处理一个客户端连接上的全部请求。
    /// </summary>
    private async Task<bool> HandleClientAsync(Stream pipe, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(pipe, new UTF8Encoding(false), detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        await using var writer = new StreamWriter(pipe, new UTF8Encoding(false), leaveOpen: true)
        {
            AutoFlush = true,
            NewLine = "\n",
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line is null)
            {
                return false;
            }

            var response = HandleRequest(line, out var shouldStop);
            await writer.WriteLineAsync(JsonSerializer.Serialize(response, _jsonOptions)).ConfigureAwait(false);
            if (shouldStop)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 根据单条 JSON 请求生成响应对象。
    /// </summary>
    private PipeResponse HandleRequest(string line, out bool shouldStop)
    {
        shouldStop = false;
        try
        {
            var request = JsonSerializer.Deserialize<PipeRequest>(line, _jsonOptions);
            var command = request?.Command?.Trim().ToLowerInvariant();
            return command switch
            {
                "ping" => PipeResponse.Success(new { status = "ok" }),
                "snapshot" => PipeResponse.Success(_snapshotService.Collect()),
                "shutdown" => Shutdown(out shouldStop),
                _ => PipeResponse.Failure("unknown_command", "未知命令。"),
            };
        }
        catch (JsonException error)
        {
            return PipeResponse.Failure("invalid_json", error.Message);
        }
        catch (Exception error)
        {
            return PipeResponse.Failure("host_error", error.Message);
        }
    }

    /// <summary>
    /// 生成关闭响应并标记服务退出。
    /// </summary>
    private static PipeResponse Shutdown(out bool shouldStop)
    {
        shouldStop = true;
        return PipeResponse.Success(new { status = "stopping" });
    }
}
