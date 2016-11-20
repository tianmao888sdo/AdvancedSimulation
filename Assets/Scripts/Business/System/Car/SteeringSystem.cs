using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 负责根据方向盘的输入改变轮胎转向，要求有前束角
/// </summary>
public class SteeringSystem : ScriptBase, ISteering,IRelease
{
    [SerializeField]
    private Wheel[] wheels = new Wheel[2];

    /// <summary>
    /// 最大转向角度
    /// </summary>
    [SerializeField]
    private float maxSteerAngle = 30f;

    /// <summary>
    /// 当前转向角度
    /// </summary>
    [SerializeField]
    private float currSteerAngle = 0f;

    private float m_steeringInput = 0f;

    public override void Init()
    {
        //获取2个轮胎
        //读取配置文件获得参数
    }

    public override void Play()
    {
        throw new NotImplementedException();
    }

    public override void Stop()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 外部转向输入
    /// </summary>
    public void SetSteeringInput(float val)
    {
        m_steeringInput = val;
        currSteerAngle = m_steeringInput * maxSteerAngle;

        //计算前束角
        wheels[0].SetSteerAngle(currSteerAngle);
        wheels[1].SetSteerAngle(currSteerAngle);
    }

    public void Release(bool destroy = false)
    {
        throw new NotImplementedException();
    }
}
