using UnityEngine;
using System.Collections;
using System;

public class EngineSystem: ScriptBase, IEngine,IRelease
{
    /// <summary>
    /// 最大转速，转速/分钟
    /// </summary>
    [SerializeField]
    private float maxRMP = 7000f;

    /// <summary>
    /// 转速加速性，转速/分钟
    /// 需要通过变速箱传回的力来计算真正的加速
    /// </summary>
    [SerializeField]
    private float rmpAcceleration = 1000f;

    /// <summary>
    /// 转速减速性，转速/分钟
    /// 需要通过变速箱传回的力来计算真正的减速
    /// </summary>
    [SerializeField]
    private float rmpDeceleration=1000f;

    /// <summary>
    /// 发动机曲轴半径，单位米
    /// </summary>
    [SerializeField]
    private float crankShaftR = 0.1f;

    /// <summary>
    /// torque/rpm曲线
    /// </summary>
    [SerializeField]
    private AnimationCurve torqueRpmCurve;

    /// <summary>
    /// 机械效率
    /// </summary>
    [SerializeField]
    private float mechanicalEfficiency = 1f;

    /// <summary>
    /// 质量
    /// </summary>
    [SerializeField]
    private float mass = 20f;

    /// <summary>
    /// 油门输入
    /// </summary>
    private float m_throttleInput = 0f;

    /// <summary>
    /// 当前转速,转/分
    /// </summary>
    private float m_rmp = 0f;

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
    public float RMP { get { return m_rmp; } }

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
    public float Ps { get { return m_watts* 0.735f; } }

    /// <summary>
    /// 曲轴输出力
    /// </summary>
    public float Force { get { return m_force; } }

    public override void Init()
    {
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

        torqueRpmCurve = new AnimationCurve(keys);
        torqueRpmCurve.SmoothTangents(0, 1);
        torqueRpmCurve.SmoothTangents(1, 1);
        torqueRpmCurve.SmoothTangents(2, 1);
        torqueRpmCurve.SmoothTangents(3, 1);
        torqueRpmCurve.SmoothTangents(4, 1);
        torqueRpmCurve.SmoothTangents(5, 1);
        torqueRpmCurve.SmoothTangents(6, 1);

        mechanicalEfficiency = 1f;
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
    /// 外部油门输入
    /// </summary>
    public void SetThrottleInput(float val)
    {
        m_throttleInput = val;

        if (m_throttleInput > 0)
        {
            m_rmp += m_throttleInput * rmpAcceleration * Time.deltaTime;
        }
        else
        {
            m_rmp -= rmpDeceleration * Time.deltaTime;
        }

        m_rmp = Mathf.Clamp(m_rmp, 0f, maxRMP);
        m_torque= torqueRpmCurve.Evaluate(m_rmp) * mechanicalEfficiency;
        //m_watts= m_torque * m_rmp * 2 * Mathf.PI* mechanicalEfficiency / 60;
        m_watts = m_torque * m_rmp*mechanicalEfficiency / 9549;
        m_force = m_torque * mechanicalEfficiency / crankShaftR;
    }

    public void Release(bool destroy = false)
    {
        throw new NotImplementedException();
    }
}
