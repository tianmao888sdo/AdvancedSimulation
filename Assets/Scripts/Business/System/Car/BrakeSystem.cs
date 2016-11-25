using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 刹车系统
/// </summary>
public class BrakeSystem : MonoBase,IRelease {

    /// <summary>
    /// 最大刹车力矩
    /// </summary>
    [SerializeField]
    private float m_maxBrakeTorque = 5000;

    /// <summary>
    /// 机械效率
    /// </summary>
    [SerializeField][Range(0,1f)]
    private float m_mechanicalEfficiency = 1f;

    /// <summary>
    /// 刹车输入
    /// </summary>
    private float m_brakeInput = 0f;

    /// <summary>
    /// 刹车力矩
    /// </summary>
    public float BrakeTorque {get { return m_brakeInput * m_maxBrakeTorque; }}


    public override void Init()
    {

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
