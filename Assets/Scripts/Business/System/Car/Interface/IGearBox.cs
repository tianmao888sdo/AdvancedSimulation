public interface IGearBox
{
    /// <summary>
    /// 设置挡位
    /// </summary>
    /// <param name="val"></param>
    void SetGear(int val);

    /// <summary>
    /// 设置离合
    /// </summary>
    /// <param name="val"></param>
    void SetClutch(float val);

    /// <summary>
    /// 设置扭矩
    /// </summary>
    /// <param name="val"></param>
    void SetTorque(float val);

    /// <summary>
    /// 输出扭矩
    /// </summary>
    float OutputTorque { get; }
}
