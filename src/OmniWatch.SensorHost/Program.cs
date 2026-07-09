using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using OmniWatch.SensorHost;

Console.OutputEncoding = new UTF8Encoding(false);

var options = HostOptions.Parse(args);
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    WriteIndented = options.PrettyJson,
};

if (options.ShowHelp)
{
    Console.WriteLine(HostOptions.HelpText);
    return 0;
}

using var service = new SensorSnapshotService();
if (options.Once)
{
    var snapshot = service.Collect();
    Console.WriteLine(JsonSerializer.Serialize(snapshot, jsonOptions));
    return 0;
}

using var cancellation = new CancellationTokenSource();
ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    try
    {
        if (!cancellation.IsCancellationRequested)
        {
            cancellation.Cancel();
        }
    }
    catch (ObjectDisposedException)
    {
        // 退出阶段可能已释放取消源，此时忽略重复取消信号。
    }
};
Console.CancelKeyPress += cancelHandler;

try
{
    var server = new SensorPipeServer(options.PipeName, service, jsonOptions);
    return await server.RunAsync(cancellation.Token);
}
finally
{
    Console.CancelKeyPress -= cancelHandler;
}
