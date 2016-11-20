
public interface IEngine
{
    /// <summary>
    /// 外部油门输入
    /// </summary>
    void SetThrottleInput(float val);

    /// <summary>
    /// 当前转速
    /// </summary>
    float RMP { get; }

    /// <summary>
    /// 当前扭矩
    /// </summary>
    float Torque { get ;}

    /// <summary>
    /// 功率
    /// </summary>
    float Watts { get; }

    /// <summary>
    /// 公制马力
    /// </summary>
    float Ps { get; }

    /// <summary>
    /// 曲轴输出力
    /// </summary>
    float Force { get; }
}