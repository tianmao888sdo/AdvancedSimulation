using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 
/// </summary>
public class EngineSystem: MonoBase, IEngine
{
    /// <summary>
    /// 最大转速，转速/分钟
    /// </summary>
    [SerializeField]
    private float m_maxRPM = 7000f;

    /// <summary>
    /// 转速加速性，转速/分钟
    /// 需要通过变速箱传回的力来计算真正的加速
    /// </summary>
    [SerializeField]
    private float m_rpmAcceleration = 1000f;

    /// <summary>
    /// 转速减速性，转速/分钟
    /// 需要通过变速箱传回的力来计算真正的减速
    /// </summary>
    [SerializeField]
    private float m_rpmDeceleration=1000f;

    /// <summary>
    /// torque/rpm曲线
    /// </summary>
    [SerializeField]
    private AnimationCurve m_torqueRpmCurve;

    /// <summary>
    /// 机械效率，热能，粘滞阻力带来的损失
    /// </summary>
    [SerializeField][Range(0,1f)]
    private float m_mechanicalEfficiency = 1f;

    /// <summary>
    /// 质量
    /// </summary>
    [SerializeField]
    private float m_mass = 20f;

    /// <summary>
    /// 传动轴质量
    /// </summary>
    [SerializeField]
    private float m_crankShaftMass = 2f;

    /// <summary>
    /// 发动机曲轴半径，单位米
    /// </summary>
    [SerializeField]
    private float m_crankShaftRadius = 0.1f;

    /// <summary>
    /// 油门输入
    /// </summary>
    [SerializeField]
    private float m_throttleInput = 0f;

    /// <summary>
    /// 当前转速,转/分
    /// </summary>
    private float m_rpm = 0f;

    /// <summary>
    /// 扭矩
    /// </summary>
    private float m_torque = 0f;

    /// <summary>
    /// 功率,马力
    /// </summary>
    private float m_watts = 0f;

    /// <summary>
    /// 曲轴输出力
    /// </summary>
    private float m_force = 0f;

    /// <summary>
    /// 当前转速
    /// </summary>
    public float RPM { get { return m_rpm; } }

    /// <summary>
    /// 当前扭矩
    /// </summary>
    public float Torque { get { return m_torque; } }

    /// <summary>
    /// 功率
    /// </summary>
    public float Watts { get { return m_watts; } }
    
    /// <summary>
    /// 公制马力
    /// </summary>
    public float Ps { get { return m_watts; } }

    /// <summary>
    /// 曲轴输出力
    /// </summary>
    public float Force { get { return m_force; } }

    /// <summary>
    /// 转动惯量，惯量会带啦扭矩损失
    /// </summary>
    public float MomentofInertia { get { return 0.5f* m_crankShaftMass * m_crankShaftRadius* m_crankShaftRadius; } }

    /// <summary>
    /// 角加速度
    /// </summary>
    public float ShaftAngleAcc { get { return 0f; } }

    public override void Init()
    {
        base.Init();

        //读取配置文件获得参数
        Keyframe[] keys = new Keyframe[]
        {
            new Keyframe(0,0),
            new Keyframe(2000,50),
            new Keyframe(3000,80),
            new Keyframe(4000,110),
            new Keyframe(5000,120),
            new Keyframe(6000,135),
            new Keyframe(7000,110)
        };

        m_torqueRpmCurve = new AnimationCurve(keys);
        m_torqueRpmCurve.SmoothTangents(0, 1);
        m_torqueRpmCurve.SmoothTangents(1, 1);
        m_torqueRpmCurve.SmoothTangents(2, 1);
        m_torqueRpmCurve.SmoothTangents(3, 1);
        m_torqueRpmCurve.SmoothTangents(4, 1);
        m_torqueRpmCurve.SmoothTangents(5, 1);
        m_torqueRpmCurve.SmoothTangents(6, 1);

        m_mechanicalEfficiency = 1f;
    }

    /// <summary>
    /// 外部油门输入
    /// </summary>
    public void SetThrottleInput(float val)
    {
        m_throttleInput = val;

        if (m_throttleInput > 0)
        {
            m_rpm += m_throttleInput * m_rpmAcceleration * Time.deltaTime;
        }
        else
        {
            m_rpm -= m_rpmDeceleration * Time.deltaTime;
        }

        m_rpm = Mathf.Clamp(m_rpm, 0f, m_maxRPM);
        m_torque = m_torqueRpmCurve.Evaluate(m_rpm) * m_mechanicalEfficiency;
        m_watts = m_torque * 0.735f * m_rpm * m_mechanicalEfficiency / 9549;
        m_force = m_torque * m_mechanicalEfficiency / m_crankShaftRadius;
    }

    public override void Release(bool destroy = false)
    {
        base.Release(destroy);
        throw new NotImplementedException();
    }
}
