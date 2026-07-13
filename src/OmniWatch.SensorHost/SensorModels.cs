namespace OmniWatch.SensorHost;

/// <summary>
/// 表示一次传感器采集的完整快照。
/// </summary>
internal sealed class SensorSnapshot
{
    /// <summary>
    /// 获取或初始化快照协议版本。
    /// </summary>
    public int Version { get; init; }

    /// <summary>
    /// 获取或初始化采集时间。
    /// </summary>
    public string Timestamp { get; init; } = "";

    /// <summary>
    /// 获取或初始化主机名称。
    /// </summary>
    public string Host { get; init; } = "";

    /// <summary>
    /// 获取或初始化平台名称。
    /// </summary>
    public string Platform { get; init; } = "";

    /// <summary>
    /// 获取或初始化 CPU 快照。
    /// </summary>
    public CpuSnapshot Cpu { get; init; } = new();

    /// <summary>
    /// 获取或初始化内存快照。
    /// </summary>
    public MemorySnapshot Memory { get; init; } = new();

    /// <summary>
    /// 获取或初始化 GPU 快照。
    /// </summary>
    public GpuSnapshot? Gpu { get; init; }

    /// <summary>
    /// 获取或初始化功耗快照。
    /// </summary>
    public PowerSnapshot Power { get; init; } = new();

    /// <summary>
    /// 获取或初始化磁盘快照。
    /// </summary>
    public List<DiskSnapshot> Disks { get; init; } = [];

    /// <summary>
    /// 获取或初始化原始硬件传感器列表。
    /// </summary>
    public List<HardwareSnapshot> Hardware { get; init; } = [];
}

/// <summary>
/// 表示 CPU 传感器快照。
/// </summary>
internal sealed class CpuSnapshot
{
    /// <summary>
    /// 获取或初始化 CPU 名称。
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// 获取或初始化 CPU 使用率百分比。
    /// </summary>
    public double? Percent { get; init; }

    /// <summary>
    /// 获取或初始化 CPU 频率 GHz。
    /// </summary>
    public double? FrequencyGhz { get; init; }

    /// <summary>
    /// 获取或初始化 CPU 温度摄氏度。
    /// </summary>
    public double? TemperatureC { get; init; }

    /// <summary>
    /// 获取或初始化 CPU 功耗瓦数。
    /// </summary>
    public double? PowerWatts { get; init; }

    /// <summary>
    /// 获取或初始化有效传感器数量。
    /// </summary>
    public int SensorCount { get; init; }
}

/// <summary>
/// 表示内存传感器快照。
/// </summary>
internal sealed class MemorySnapshot
{
    /// <summary>
    /// 获取或初始化物理内存快照。
    /// </summary>
    public MemoryUsageSnapshot? Physical { get; init; }

    /// <summary>
    /// 获取或初始化虚拟内存快照。
    /// </summary>
    public MemoryUsageSnapshot? Virtual { get; init; }

    /// <summary>
    /// 获取或初始化有效传感器数量。
    /// </summary>
    public int SensorCount { get; init; }
}

/// <summary>
/// 表示一种内存空间的使用情况。
/// </summary>
internal sealed class MemoryUsageSnapshot
{
    /// <summary>
    /// 获取或初始化内存使用率百分比。
    /// </summary>
    public double? Percent { get; init; }

    /// <summary>
    /// 获取或初始化已用内存字节数。
    /// </summary>
    public long? UsedBytes { get; init; }

    /// <summary>
    /// 获取或初始化可用内存字节数。
    /// </summary>
    public long? AvailableBytes { get; init; }
}

/// <summary>
/// 表示 GPU 传感器快照。
/// </summary>
internal sealed class GpuSnapshot
{
    /// <summary>
    /// 获取或初始化 GPU 名称。
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// 获取或初始化 GPU 使用率百分比。
    /// </summary>
    public double? Percent { get; init; }

    /// <summary>
    /// 获取或初始化 GPU 温度摄氏度。
    /// </summary>
    public double? TemperatureC { get; init; }

    /// <summary>
    /// 获取或初始化 GPU 核心频率 MHz。
    /// </summary>
    public double? CoreClockMhz { get; init; }

    /// <summary>
    /// 获取或初始化 GPU 显存频率 MHz。
    /// </summary>
    public double? MemoryClockMhz { get; init; }

    /// <summary>
    /// 获取或初始化 GPU 功耗瓦数。
    /// </summary>
    public double? PowerWatts { get; init; }

    /// <summary>
    /// 获取或初始化已用专用显存字节数。
    /// </summary>
    public long? DedicatedMemoryUsedBytes { get; init; }

    /// <summary>
    /// 获取或初始化总专用显存字节数。
    /// </summary>
    public long? DedicatedMemoryTotalBytes { get; init; }
}

/// <summary>
/// 表示磁盘传感器快照。
/// </summary>
internal sealed class DiskSnapshot
{
    /// <summary>
    /// 获取或初始化磁盘名称。
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// 获取或初始化磁盘温度摄氏度。
    /// </summary>
    public double? TemperatureC { get; init; }

    /// <summary>
    /// 获取或初始化磁盘已用空间百分比。
    /// </summary>
    public double? UsedSpacePercent { get; init; }

    /// <summary>
    /// 获取或初始化磁盘读取速率字节每秒。
    /// </summary>
    public long? ReadBytesPerSecond { get; init; }

    /// <summary>
    /// 获取或初始化磁盘写入速率字节每秒。
    /// </summary>
    public long? WriteBytesPerSecond { get; init; }

    /// <summary>
    /// 获取或初始化磁盘健康剩余百分比。
    /// </summary>
    public double? HealthPercent { get; init; }
}

/// <summary>
/// 表示功耗传感器快照。
/// </summary>
internal sealed class PowerSnapshot
{
    /// <summary>
    /// 获取或初始化功耗瓦数。
    /// </summary>
    public double? Watts { get; init; }

    /// <summary>
    /// 获取或初始化功耗来源。
    /// </summary>
    public string Source { get; init; } = "unavailable";

    /// <summary>
    /// 获取或初始化功耗统计范围。
    /// </summary>
    public string Scope { get; init; } = "unavailable";
}

/// <summary>
/// 表示原始硬件快照。
/// </summary>
internal sealed class HardwareSnapshot
{
    /// <summary>
    /// 获取或初始化硬件名称。
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// 获取或初始化硬件类型。
    /// </summary>
    public string Type { get; init; } = "";

    /// <summary>
    /// 获取或初始化传感器列表。
    /// </summary>
    public List<SensorValue> Sensors { get; init; } = [];
}

/// <summary>
/// 表示单个传感器值。
/// </summary>
internal sealed class SensorValue
{
    /// <summary>
    /// 获取或初始化传感器名称。
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// 获取或初始化传感器类型。
    /// </summary>
    public string Type { get; init; } = "";

    /// <summary>
    /// 获取或初始化传感器数值。
    /// </summary>
    public double? Value { get; init; }

    /// <summary>
    /// 获取或初始化传感器单位。
    /// </summary>
    public string Unit { get; init; } = "";
}
