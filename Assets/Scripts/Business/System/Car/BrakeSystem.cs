using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 刹车系统
/// </summary>
public class BrakeSystem : MonoBase, IBrake
{
    /// <summary>
    /// 轮胎,前右，前左，后右，后左
    /// </summary>
    [SerializeField]
    private Wheel[] m_Wheels = new Wheel[4];

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
    private float m_brakeInput = -1f;

    /// <summary>
    /// 刹车力矩
    /// </summary>
    private float BrakeTorque {get { return m_brakeInput * m_maxBrakeTorque; }}

    public override void Init()
    {
        base.Init();
        ExecuteBrake(CarAttributes.BrakeMode.FourBrake, 0);
    }

    /// <summary>
    /// 驱动和刹车
    /// </summary>
    /// <param name="brakeMode"></param>
    /// <param name="brakeTorque"></param>
    public void Brake(CarAttributes.BrakeMode brakeMode, float brakeInput)
    {
        if (m_brakeInput != brakeInput)
            m_brakeInput = brakeInput;
        else
            return;

        ExecuteBrake(brakeMode, brakeInput);
    }

    private void ExecuteBrake(CarAttributes.BrakeMode brakeMode, float brakeInput)
    {
        float t_brakeTorque = BrakeTorque;

        switch (brakeMode)
        {
            case CarAttributes.BrakeMode.RearTwoBrake:
                m_Wheels[2].SetBrakeTorque(t_brakeTorque);
                m_Wheels[3].SetBrakeTorque(t_brakeTorque);
                break;
            case CarAttributes.BrakeMode.FourBrake:
                m_Wheels[0].SetBrakeTorque(t_brakeTorque);
                m_Wheels[1].SetBrakeTorque(t_brakeTorque);
                m_Wheels[2].SetBrakeTorque(t_brakeTorque);
                m_Wheels[3].SetBrakeTorque(t_brakeTorque);
                break;
        }
    }

    public override void Release(bool destroy = false)
    {
        base.Release(destroy);
        throw new NotImplementedException();
    }
}
