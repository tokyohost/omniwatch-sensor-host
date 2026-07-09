namespace OmniWatch.SensorHost;

/// <summary>
/// 表示传感器宿主进程的命令行配置。
/// </summary>
internal sealed class HostOptions
{
    /// <summary>
    /// 默认命名管道名称。
    /// </summary>
    public const string DefaultPipeName = "omniwatch.sensorhost";

    /// <summary>
    /// 帮助文本。
    /// </summary>
    public const string HelpText =
        """
        OmniWatch.SensorHost

        用法:
          OmniWatch.SensorHost.exe --pipe omniwatch.sensorhost
          OmniWatch.SensorHost.exe --once --pretty

        参数:
          --pipe <name>   指定 Named Pipe 名称，默认 omniwatch.sensorhost。
          --once          采集一次快照并写到标准输出。
          --pretty        使用缩进 JSON 输出，便于调试。
          --help          显示帮助。
        """;

    /// <summary>
    /// 获取或初始化命名管道名称。
    /// </summary>
    public string PipeName { get; init; } = DefaultPipeName;

    /// <summary>
    /// 获取或初始化是否只采集一次。
    /// </summary>
    public bool Once { get; init; }

    /// <summary>
    /// 获取或初始化是否输出格式化 JSON。
    /// </summary>
    public bool PrettyJson { get; init; }

    /// <summary>
    /// 获取或初始化是否显示帮助。
    /// </summary>
    public bool ShowHelp { get; init; }

    /// <summary>
    /// 解析命令行参数并返回宿主配置。
    /// </summary>
    public static HostOptions Parse(IReadOnlyList<string> args)
    {
        var pipeName = DefaultPipeName;
        var once = false;
        var pretty = false;
        var help = false;

        for (var index = 0; index < args.Count; index++)
        {
            var argument = args[index];
            switch (argument)
            {
                case "--pipe" when index + 1 < args.Count:
                    pipeName = args[++index];
                    break;
                case "--once":
                    once = true;
                    break;
                case "--pretty":
                    pretty = true;
                    break;
                case "--help":
                case "-h":
                case "/?":
                    help = true;
                    break;
            }
        }

        return new HostOptions
        {
            PipeName = string.IsNullOrWhiteSpace(pipeName) ? DefaultPipeName : pipeName.Trim(),
            Once = once,
            PrettyJson = pretty,
            ShowHelp = help,
        };
    }
}
