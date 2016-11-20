
public interface ISteering
{
    /// <summary>
    /// 外部转向输入,计算转向角，考虑前束角
    /// </summary>
    void SetSteeringInput(float val);
}
