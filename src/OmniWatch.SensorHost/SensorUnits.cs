using LibreHardwareMonitor.Hardware;

namespace OmniWatch.SensorHost;

/// <summary>
/// 提供 LibreHardwareMonitor 传感器类型到显示单位的映射。
/// </summary>
internal static class SensorUnits
{
    /// <summary>
    /// 返回指定传感器类型的常用单位。
    /// </summary>
    public static string UnitFor(SensorType type)
    {
        return type switch
        {
            SensorType.Voltage => "V",
            SensorType.Current => "A",
            SensorType.Clock => "MHz",
            SensorType.Load => "%",
            SensorType.Temperature => "C",
            SensorType.Fan => "RPM",
            SensorType.Flow => "L/h",
            SensorType.Control => "%",
            SensorType.Level => "%",
            SensorType.Power => "W",
            SensorType.Data => "GB",
            SensorType.SmallData => "MB",
            SensorType.Factor => "x",
            SensorType.Frequency => "Hz",
            SensorType.Throughput => "B/s",
            SensorType.TimeSpan => "s",
            SensorType.Energy => "Wh",
            _ => "",
        };
    }
}
