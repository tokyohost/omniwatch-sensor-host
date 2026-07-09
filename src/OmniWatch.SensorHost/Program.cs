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
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

var server = new SensorPipeServer(options.PipeName, service, jsonOptions);
return await server.RunAsync(cancellation.Token);
