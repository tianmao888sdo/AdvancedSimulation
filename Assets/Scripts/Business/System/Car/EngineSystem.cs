using UnityEngine;
using System.Collections;
using System;

public class EngineSystem: ScriptBase, IRelease
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
    /// 当前转速
    /// </summary>
    [SerializeField]
    private float rmp = 0f;

    /// <summary>
    /// 扭矩转速
    /// </summary>
    [SerializeField]
    private float rpm = 0f;

    /// <summary>
    /// 功率
    /// </summary>
    [SerializeField]
    private float watts = 0f;

    /// <summary>
    /// rpm曲线
    /// </summary>
    [SerializeField]
    private AnimationCurve rpmCurve;

    public float RMP { get { return rmp; } }
    public float RPM { get { return rpm; } }
    public float Watts { get { return watts; } }

    private float m_throttleInput = 0f;

    public override void Init()
    {
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

        rpmCurve = new AnimationCurve(keys);
        rpmCurve.SmoothTangents(0, 1);
        rpmCurve.SmoothTangents(1, 1);
        rpmCurve.SmoothTangents(2, 1);
        rpmCurve.SmoothTangents(3, 1);
        rpmCurve.SmoothTangents(4, 1);
        rpmCurve.SmoothTangents(5, 1);
        rpmCurve.SmoothTangents(6, 1);
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
            rmp += m_throttleInput * rmpAcceleration * Time.deltaTime;
        }
        else
        {
            rmp -= rmpDeceleration * Time.deltaTime;
        }

        rmp = Mathf.Clamp(rmp, 0f, maxRMP);
        rpm = rpmCurve.Evaluate(rmp)*0.1f;
    }

    public void Release(bool destroy = false)
    {
        throw new NotImplementedException();
    }
}
