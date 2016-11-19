using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 离合器
/// </summary>
public class ClutchSystem : ScriptBase,IRelease {

    private float m_clutchInput = 0f;

    /// <summary>
    /// 
    /// </summary>
    public float Clutch { get { return m_clutchInput; } }

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
    /// 外部离合输入
    /// </summary>
    public void SetClutchInput(float val)
    {
        m_clutchInput = val;
    }

    public void Release(bool destroy = false)
    {
        throw new NotImplementedException();
    }
}
