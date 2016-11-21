using UnityEngine;
using System.Collections;

/// <summary>
/// 汽车属性
/// </summary>
public class CarAttributes : MonoBehaviour
{
    public enum MotorMode
    {
        FrontTwoDrive,
        RearTwoDrive,
        FourDrive
    }

    public enum BrakeMode
    {
        RearTwoBrake,
        FourBrake
    }

    public MotorMode motorMode = MotorMode.FrontTwoDrive;
    public BrakeMode brakeMode = BrakeMode.RearTwoBrake;
    public float mass = 1000f;//质量
    public float steering = 0.0f;//方向
    public float accelerator = 0.0f;//油门
    public float brake = 0.0f;//刹车
    public float handbrake = 0.0f;//手刹
    public float clutch = 0.0f;//离合   CarControl使用以确定换挡流程

    public float shaftMomentofInertia = 0f;//传动轴惯量
    public float shaftAngleAcc = 0f;//传动轴角加速度


    public void Init()
    {
        //读取车辆基本配置文件
    }
}
