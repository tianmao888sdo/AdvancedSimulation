using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 负责根据方向盘的输入改变轮胎转向，要求有前束角
/// </summary>
public class SteeringSystem : MonoBase, ISteering
{
    /// <summary>
    /// 轮胎,前右，前左，后右，后左
    /// </summary>
    [SerializeField]
    private Wheel[] m_wheels = new Wheel[4];

    /// <summary>
    /// 内转向轮最大转向角度
    /// </summary>
    [SerializeField][Range(0,90f)]
    private float m_maxSteerAngle = 30f;

    /// <summary>
    /// 当前内轮转向角度
    /// </summary>
    [SerializeField]
    private float m_steerAngleInside = 0f;

    /// <summary>
    /// 当前外轮转向角度
    /// </summary>
    [SerializeField]
    private float m_steerAngleOutside = 0f;

    /// <summary>
    /// 方向盘输入
    /// </summary>
    private float m_steeringInput = 0f;

    public override void Init()
    {
        base.Init();
        //获取2个轮胎
        //读取配置文件获得参数
    }

    /// <summary>
    /// 外部转向输入
    /// </summary>
    /// <param name="val">方向盘输入，范围-1到1</param>
    public void SetSteeringInput(float val)
    {
        if (m_steeringInput != val)
            m_steeringInput = val;
        else
            return;

        m_steerAngleInside = m_steeringInput * m_maxSteerAngle;

        float L = Mathf.Abs(m_wheels[0].MasterWheelCollider.transform.localPosition.z - m_wheels[2].MasterWheelCollider.transform.localPosition.z);//前后轮距
        float B= Mathf.Abs(m_wheels[0].MasterWheelCollider.transform.localPosition.x - m_wheels[1].MasterWheelCollider.transform.localPosition.x);//左右轮距

        m_steerAngleOutside = AuxiliaryCalculation.CalculateWheelSteeringAngle(m_steerAngleInside, L, B);//外转向轮转向角

        if (val > 0)//右轮为内轮
        {
            //计算前束角
            m_wheels[0].SetSteerAngle(m_steerAngleInside);
            m_wheels[1].SetSteerAngle(m_steerAngleOutside);
        }
        else
        {
            //计算前束角
            m_wheels[1].SetSteerAngle(m_steerAngleInside);
            m_wheels[0].SetSteerAngle(m_steerAngleOutside);
        }
    }

    public override void Release(bool destroy = false)
    {
        base.Release(destroy);
        throw new NotImplementedException();
    }
}
