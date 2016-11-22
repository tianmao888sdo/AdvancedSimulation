using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 负责根据方向盘的输入改变轮胎转向，要求有前束角
/// </summary>
public class SteeringSystem : ScriptBase, ISteering,IRelease
{
    [SerializeField]
    private Wheel[] m_wheels = new Wheel[2];

    /// <summary>
    /// 最大转向角度
    /// </summary>
    [SerializeField][Range(0,90f)]
    private float m_maxSteerAngle = 30f;

    /// <summary>
    /// 当前转向角度
    /// </summary>
    [SerializeField]
    private float m_currSteerAngle = 0f;

    /// <summary>
    /// 方向盘输入
    /// </summary>
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
        m_currSteerAngle = m_steeringInput * m_maxSteerAngle;

        //计算前束角
        m_wheels[0].SetSteerAngle(m_currSteerAngle);
        m_wheels[1].SetSteerAngle(m_currSteerAngle);
    }

    public void Release(bool destroy = false)
    {
        throw new NotImplementedException();
    }
}
