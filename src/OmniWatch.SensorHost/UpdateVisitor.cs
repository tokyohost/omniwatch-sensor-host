using LibreHardwareMonitor.Hardware;

namespace OmniWatch.SensorHost;

/// <summary>
/// 递归刷新 LibreHardwareMonitor 硬件树。
/// </summary>
internal sealed class UpdateVisitor : IVisitor
{
    /// <summary>
    /// 访问计算机节点并刷新所有硬件。
    /// </summary>
    public void VisitComputer(IComputer computer)
    {
        computer.Traverse(this);
    }

    /// <summary>
    /// 访问硬件节点并刷新子硬件。
    /// </summary>
    public void VisitHardware(IHardware hardware)
    {
        TryUpdateHardware(hardware);
        foreach (var subHardware in hardware.SubHardware)
        {
            subHardware.Accept(this);
        }
    }

    /// <summary>
    /// 传感器节点无需额外处理。
    /// </summary>
    public void VisitSensor(ISensor sensor)
    {
    }

    /// <summary>
    /// 参数节点无需额外处理。
    /// </summary>
    public void VisitParameter(IParameter parameter)
    {
    }

    /// <summary>
    /// 安全刷新单个硬件节点，忽略驱动层偶发读取异常。
    /// </summary>
    private static void TryUpdateHardware(IHardware hardware)
    {
        try
        {
            hardware.Update();
        }
        catch (Exception)
        {
            // 部分硬件驱动会偶发抛出读取异常，保留其余传感器继续采集。
        }
    }
}
