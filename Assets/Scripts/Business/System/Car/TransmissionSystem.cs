using System;
using UnityEngine;
/// <summary>
/// 汽车动力传动系统
/// </summary>
public class TransmissionSystem : MonoBase, ITransmission
{
    /// <summary>
    /// 轮胎,前右，前左，后右，后左
    /// </summary>
    [SerializeField]
    private Wheel[] m_Wheels = new Wheel[4];

    /// <summary>
    /// 刹车扭矩输入
    /// </summary>
    [SerializeField]
    private float m_motorTorque=0;

    public override void Init()
    {
        base.Init();

        for (int i = 0; i < 4; i++)
            m_Wheels[i].Init();
    }

    protected override void DUpdate()
    {
        for (int i = 0; i < 4; i++)
            m_Wheels[i].UpdateWheels();
    }

    /// <summary>
    /// 驱动
    /// </summary>
    /// <param name="motorMode">驱动模式：两驱，四驱</param>
    /// <param name="motorTorque"></param>
    public void Move(CarAttributes.MotorMode motorMode, float motorTorque)
    {
        if (m_motorTorque != motorTorque)
            m_motorTorque = motorTorque;
        else
            return;

        if (motorTorque == 0)
            return;

        ExecuteMove(motorMode, motorTorque);
    }

    private void ExecuteMove(CarAttributes.MotorMode motorMode, float motorTorque)
    {
        switch (motorMode)
        {
            case CarAttributes.MotorMode.FrontTwoDrive:
                motorTorque = motorTorque / 2f;
                m_Wheels[0].SetMotorTorque(motorTorque);
                m_Wheels[1].SetMotorTorque(motorTorque);
                break;
            case CarAttributes.MotorMode.RearTwoDrive:
                motorTorque = motorTorque / 2f;
                m_Wheels[2].SetMotorTorque(motorTorque);
                m_Wheels[3].SetMotorTorque(motorTorque);
                break;
            case CarAttributes.MotorMode.FourDrive:
                motorTorque = motorTorque / 4f;
                m_Wheels[0].SetMotorTorque(motorTorque);
                m_Wheels[1].SetMotorTorque(motorTorque);
                m_Wheels[2].SetMotorTorque(motorTorque);
                m_Wheels[3].SetMotorTorque(motorTorque);
                break;
        }
    }

    public override void Release(bool destroy = false)
    {
        base.Release(destroy);
        throw new NotImplementedException();
    }
}
