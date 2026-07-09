namespace OmniWatch.SensorHost;

/// <summary>
/// 表示 Named Pipe JSON RPC 的统一响应。
/// </summary>
internal sealed class PipeResponse
{
    /// <summary>
    /// 获取或初始化请求是否成功。
    /// </summary>
    public bool Ok { get; init; }

    /// <summary>
    /// 获取或初始化成功响应数据。
    /// </summary>
    public object? Data { get; init; }

    /// <summary>
    /// 获取或初始化错误码。
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// 获取或初始化错误说明。
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// 创建成功响应。
    /// </summary>
    public static PipeResponse Success(object? data)
    {
        return new PipeResponse { Ok = true, Data = data };
    }

    /// <summary>
    /// 创建失败响应。
    /// </summary>
    public static PipeResponse Failure(string error, string message)
    {
        return new PipeResponse { Ok = false, Error = error, Message = message };
    }
}
