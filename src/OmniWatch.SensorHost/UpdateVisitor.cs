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
        hardware.Update();
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
}
