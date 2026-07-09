namespace OmniWatch.SensorHost;

/// <summary>
/// 描述 monitor 通过 Named Pipe 发送的请求。
/// </summary>
internal sealed class PipeRequest
{
    /// <summary>
    /// 获取或设置请求命令。
    /// </summary>
    public string? Command { get; set; }
}
