using LibreHardwareMonitor.Hardware;

namespace OmniWatch.SensorHost;

/// <summary>
/// 使用 LibreHardwareMonitor 采集硬件传感器并生成 monitor 兼容快照。
/// </summary>
internal sealed class SensorSnapshotService : IDisposable
{
    private readonly Computer _computer;
    private readonly UpdateVisitor _visitor = new();
    private readonly object _syncRoot = new();
    private bool _disposed;

    /// <summary>
    /// 初始化硬件枚举器并启用常用设备类型。
    /// </summary>
    public SensorSnapshotService()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsStorageEnabled = true,
            IsNetworkEnabled = true,
            IsControllerEnabled = true,
        };
        _computer.Open();
    }

    /// <summary>
    /// 采集一次完整硬件快照。
    /// </summary>
    public SensorSnapshot Collect()
    {
        lock (_syncRoot)
        {
            ThrowIfDisposed();
            _computer.Accept(_visitor);
            var hardware = FlattenHardware().ToList();
            var sensors = hardware.SelectMany(item => item.Sensors).ToList();

            var cpu = BuildCpuSnapshot(hardware, sensors);
            var memory = BuildMemorySnapshot(hardware, sensors);
            var gpu = BuildGpuSnapshot(hardware);
            var disks = BuildDiskSnapshots(hardware);
            var power = BuildPowerSnapshot(cpu, gpu);

            return new SensorSnapshot
            {
                Version = 1,
                Timestamp = DateTimeOffset.Now.ToString("O"),
                Host = Environment.MachineName,
                Platform = Environment.OSVersion.Platform.ToString(),
                Cpu = cpu,
                Memory = memory,
                Gpu = gpu,
                Power = power,
                Disks = disks,
                Hardware = hardware.Select(BuildHardwareSnapshot).ToList(),
            };
        }
    }

    /// <summary>
    /// 释放 LibreHardwareMonitor 计算机句柄。
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _computer.Close();
        _disposed = true;
    }

    /// <summary>
    /// 确保服务未被释放。
    /// </summary>
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SensorSnapshotService));
        }
    }

    /// <summary>
    /// 扁平化当前硬件树，包含子硬件。
    /// </summary>
    private IEnumerable<IHardware> FlattenHardware()
    {
        foreach (var hardware in _computer.Hardware)
        {
            yield return hardware;
            foreach (var subHardware in FlattenSubHardware(hardware))
            {
                yield return subHardware;
            }
        }
    }

    /// <summary>
    /// 递归扁平化指定硬件的子硬件。
    /// </summary>
    private static IEnumerable<IHardware> FlattenSubHardware(IHardware hardware)
    {
        foreach (var subHardware in hardware.SubHardware)
        {
            yield return subHardware;
            foreach (var nested in FlattenSubHardware(subHardware))
            {
                yield return nested;
            }
        }
    }

    /// <summary>
    /// 构建 CPU 兼容快照片段。
    /// </summary>
    private static CpuSnapshot BuildCpuSnapshot(IReadOnlyCollection<IHardware> hardware, IReadOnlyCollection<ISensor> sensors)
    {
        var cpuHardware = hardware.Where(item => item.HardwareType == HardwareType.Cpu).ToList();
        var cpuSensors = cpuHardware.SelectMany(item => item.Sensors).ToList();
        return new CpuSnapshot
        {
            Name = cpuHardware.FirstOrDefault()?.Name,
            Percent = FindSensorValue(cpuSensors, SensorType.Load, "CPU Total")
                ?? AverageSensorValue(cpuSensors, SensorType.Load, "CPU Core"),
            FrequencyGhz = AverageSensorValue(cpuSensors, SensorType.Clock, "CPU Core") is { } mhz ? Math.Round(mhz / 1000.0, 2) : null,
            TemperatureC = FindFirstValue(cpuSensors, SensorType.Temperature, "CPU Package", "CPU Tctl/Tdie", "Core Max")
                ?? AverageSensorValue(cpuSensors, SensorType.Temperature, "CPU Core"),
            PowerWatts = FindFirstValue(cpuSensors, SensorType.Power, "CPU Package", "Package"),
            SensorCount = sensors.Count(sensor => sensor.Value.HasValue),
        };
    }

    /// <summary>
    /// 构建内存兼容快照片段。
    /// </summary>
    private static MemorySnapshot BuildMemorySnapshot(IReadOnlyCollection<IHardware> hardware, IReadOnlyCollection<ISensor> sensors)
    {
        var memorySensors = hardware.Where(item => item.HardwareType == HardwareType.Memory).SelectMany(item => item.Sensors).ToList();
        return new MemorySnapshot
        {
            Percent = FindFirstValue(memorySensors, SensorType.Load, "Memory"),
            UsedBytes = ToBytes(FindFirstValue(memorySensors, SensorType.Data, "Memory Used")),
            AvailableBytes = ToBytes(FindFirstValue(memorySensors, SensorType.Data, "Memory Available")),
            SensorCount = memorySensors.Count(sensor => sensor.Value.HasValue),
        };
    }

    /// <summary>
    /// 构建首个 GPU 的兼容快照片段。
    /// </summary>
    private static GpuSnapshot? BuildGpuSnapshot(IReadOnlyCollection<IHardware> hardware)
    {
        var gpuHardware = hardware.FirstOrDefault(IsGpuHardware);
        if (gpuHardware is null)
        {
            return null;
        }

        var sensors = gpuHardware.Sensors.ToList();
        return new GpuSnapshot
        {
            Name = gpuHardware.Name,
            Percent = FindFirstValue(sensors, SensorType.Load, "GPU Core", "D3D 3D"),
            TemperatureC = FindFirstValue(sensors, SensorType.Temperature, "GPU Core", "GPU Hot Spot"),
            CoreClockMhz = FindFirstValue(sensors, SensorType.Clock, "GPU Core"),
            MemoryClockMhz = FindFirstValue(sensors, SensorType.Clock, "GPU Memory"),
            PowerWatts = FindFirstValue(sensors, SensorType.Power, "GPU Package", "GPU Core"),
            DedicatedMemoryUsedBytes = ToBytes(FindFirstValue(sensors, SensorType.SmallData, "GPU Memory Used")),
            DedicatedMemoryTotalBytes = ToBytes(FindFirstValue(sensors, SensorType.SmallData, "GPU Memory Total")),
        };
    }

    /// <summary>
    /// 构建所有存储设备的温度和健康快照。
    /// </summary>
    private static List<DiskSnapshot> BuildDiskSnapshots(IReadOnlyCollection<IHardware> hardware)
    {
        return hardware
            .Where(item => item.HardwareType == HardwareType.Storage)
            .Select(item =>
            {
                var sensors = item.Sensors.ToList();
                return new DiskSnapshot
                {
                    Name = item.Name,
                    TemperatureC = FindFirstValue(sensors, SensorType.Temperature, "Temperature"),
                    UsedSpacePercent = FindFirstValue(sensors, SensorType.Load, "Used Space"),
                    ReadBytesPerSecond = ToBytes(FindFirstValue(sensors, SensorType.Throughput, "Read Rate")),
                    WriteBytesPerSecond = ToBytes(FindFirstValue(sensors, SensorType.Throughput, "Write Rate")),
                    HealthPercent = FindFirstValue(sensors, SensorType.Level, "Remaining Life", "Available Spare"),
                };
            })
            .ToList();
    }

    /// <summary>
    /// 构建功耗兼容快照片段。
    /// </summary>
    private static PowerSnapshot BuildPowerSnapshot(CpuSnapshot cpu, GpuSnapshot? gpu)
    {
        var watts = new[] { cpu.PowerWatts, gpu?.PowerWatts }.Where(value => value.HasValue).Sum(value => value!.Value);
        return new PowerSnapshot
        {
            Watts = watts > 0 ? Math.Round(watts, 1) : null,
            Source = watts > 0 ? "librehardwaremonitor" : "unavailable",
            Scope = watts > 0 ? "cpu_gpu" : "unavailable",
        };
    }

    /// <summary>
    /// 构建原始硬件传感器树快照。
    /// </summary>
    private static HardwareSnapshot BuildHardwareSnapshot(IHardware hardware)
    {
        return new HardwareSnapshot
        {
            Name = hardware.Name,
            Type = hardware.HardwareType.ToString(),
            Sensors = hardware.Sensors
                .Where(sensor => sensor.Value.HasValue)
                .Select(sensor => new SensorValue
                {
                    Name = sensor.Name,
                    Type = sensor.SensorType.ToString(),
                    Value = Round(sensor.Value),
                    Unit = SensorUnits.UnitFor(sensor.SensorType),
                })
                .ToList(),
        };
    }

    /// <summary>
    /// 判断硬件是否为 GPU。
    /// </summary>
    private static bool IsGpuHardware(IHardware hardware)
    {
        return hardware.HardwareType is HardwareType.GpuNvidia or HardwareType.GpuAmd or HardwareType.GpuIntel;
    }

    /// <summary>
    /// 按名称精确或模糊查找第一个传感器值。
    /// </summary>
    private static double? FindFirstValue(IEnumerable<ISensor> sensors, SensorType type, params string[] names)
    {
        foreach (var name in names)
        {
            var value = FindSensorValue(sensors, type, name);
            if (value.HasValue)
            {
                return value.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// 查找指定类型和名称片段的传感器值。
    /// </summary>
    private static double? FindSensorValue(IEnumerable<ISensor> sensors, SensorType type, string name)
    {
        var exact = sensors.FirstOrDefault(sensor => sensor.SensorType == type && sensor.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && sensor.Value.HasValue);
        if (exact?.Value is { } exactValue)
        {
            return Round(exactValue);
        }

        var partial = sensors.FirstOrDefault(sensor => sensor.SensorType == type && sensor.Name.Contains(name, StringComparison.OrdinalIgnoreCase) && sensor.Value.HasValue);
        return partial?.Value is { } partialValue ? Round(partialValue) : null;
    }

    /// <summary>
    /// 计算指定类型和名称片段传感器的平均值。
    /// </summary>
    private static double? AverageSensorValue(IEnumerable<ISensor> sensors, SensorType type, string name)
    {
        var values = sensors
            .Where(sensor => sensor.SensorType == type && sensor.Name.Contains(name, StringComparison.OrdinalIgnoreCase) && sensor.Value.HasValue)
            .Select(sensor => (double)sensor.Value!.Value)
            .ToList();
        return values.Count > 0 ? Round(values.Average()) : null;
    }

    /// <summary>
    /// 把 LibreHardwareMonitor 的 GB/MB 近似数据转换为字节。
    /// </summary>
    private static long? ToBytes(double? value)
    {
        return value.HasValue ? Convert.ToInt64(value.Value * 1024 * 1024 * 1024) : null;
    }

    /// <summary>
    /// 统一浮点数精度。
    /// </summary>
    private static double? Round(float? value)
    {
        return value.HasValue
            ? Math.Round((double)value.Value, 2)
            : null;
    }

    /// <summary>
    /// 统一普通计算结果浮点数精度。
    /// </summary>
    private static double? Round(double value)
    {
        return Math.Round(value, 2);
    }

}
