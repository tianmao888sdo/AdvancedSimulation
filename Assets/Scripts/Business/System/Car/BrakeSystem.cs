using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 刹车系统
/// </summary>
public class BrakeSystem : ScriptBase,IRelease {

    /// <summary>
    /// 最大刹车力矩
    /// </summary>
    [SerializeField]
    private float maxBrakeTorque = 1000f;

    private float m_brakeInput = 0f;

    /// <summary>
    /// 刹车力矩
    /// </summary>
    public float BrakeTorque {get { return m_brakeInput * maxBrakeTorque; }}

    public override void Init()
    {
        throw new NotImplementedException();
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
    /// 外部刹车输入
    /// </summary>
    public void SetBrakeInput(float val)
    {
        m_brakeInput = val;
    }

    public void Release(bool destroy = false)
    {
        throw new NotImplementedException();
    }
}
