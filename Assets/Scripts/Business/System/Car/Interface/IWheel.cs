
public interface IWheel
{
    /// <summary>
    /// 设置扭矩
    /// </summary>
    /// <param name="val"></param>
    void SetMotorTorque(float val);

    /// <summary>
    /// 设置刹车扭矩
    /// </summary>
    /// <param name="val"></param>
    void SetBrakeTorque(float val);

    /// <summary>
    /// 转向
    /// </summary>
    /// <param name="val"></param>
    void SetSteerAngle(float val);

    /// <summary>
    /// 更新轮胎位置
    /// </summary>
    void UpdateWheels();
}
