﻿using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 车辆控制中心，所有对车的操作，全部集中到这里
/// </summary>
public class CarControlCenter : ScriptBase, IUpdate,IRelease
{
    /// <summary>
    /// 属性
    /// </summary>
    [SerializeField]
    private CarAttributes carAttributes;
    /// <summary>
    /// 发动机
    /// </summary>
    [SerializeField]
    private EngineSystem engineSystem;
    /// <summary>
    /// 转向系统
    /// </summary>
    [SerializeField]
    private SteeringSystem steeringSystem;
    /// <summary>
    /// 离合器
    /// </summary>
    [SerializeField]
    private ClutchSystem clutchSystem;
    /// <summary>
    /// 变速箱
    /// </summary>
    [SerializeField]
    private GearBoxSystem gearBoxSystem;
    /// <summary>
    /// 减速器
    /// </summary>
    [SerializeField]
    private GearReducerSystem gearReducerSystem;
    /// <summary>
    /// 刹车系统
    /// </summary>
    [SerializeField]
    private BrakeSystem brakeSystem;

    /// <summary>
    /// 轮胎
    /// </summary>
    [SerializeField]
    private Wheel[] m_Wheels = new Wheel[4];

    public override void Init()
    {
        KeyInputManager.Instance.AddAxisDelegate(KeyInputManager.InputMode.CarControl, "Horizontal", (number) =>{carAttributes.steering = Mathf.Clamp(number, -1f, 1f);});
        KeyInputManager.Instance.AddAxisDelegate(KeyInputManager.InputMode.CarControl, "Vertical", (number) => { carAttributes.accelerator = Mathf.Clamp(number, 0, 1); });
        KeyInputManager.Instance.AddAxisDelegate(KeyInputManager.InputMode.CarControl, "Jump", (number) => { carAttributes.brake = Mathf.Clamp(number, 0, 1); });

        KeyInputManager.Instance.AddKeyboardDelegate(KeyInputManager.InputMode.CarControl, KeyCode.C, OnKeyC);
        KeyInputManager.Instance.AddKeyboardDelegate(KeyInputManager.InputMode.CarControl, KeyCode.P, OnKeyP);
        KeyInputManager.Instance.AddKeyboardDelegate(KeyInputManager.InputMode.CarControl, KeyCode.T, OnKeyT);
        KeyInputManager.Instance.AddKeyboardDelegate(KeyInputManager.InputMode.CarControl, KeyCode.Y, OnKeyY);
        KeyInputManager.Instance.AddKeyboardDelegate(KeyInputManager.InputMode.CarControl, KeyCode.E, OnKeyE);
        KeyInputManager.Instance.AddKeyboardDelegate(KeyInputManager.InputMode.CarControl, KeyCode.Return, OnKeyReturn);
        KeyInputManager.Instance.AddKeyboardDelegate(KeyInputManager.InputMode.CarControl, KeyCode.PageDown, OnKeyPageDown);
        KeyInputManager.Instance.AddKeyboardDelegate(KeyInputManager.InputMode.CarControl, KeyCode.PageUp, OnKeyPageUp);
        KeyInputManager.Instance.AddKeyboardDelegate(KeyInputManager.InputMode.CarControl, KeyCode.O, OnKeyO);
        KeyInputManager.Instance.AddKeyboardDelegate(KeyInputManager.InputMode.CarControl, KeyCode.B, OnKeyB);
        KeyInputManager.Instance.AddKeyboardDelegate(KeyInputManager.InputMode.CarControl, KeyCode.Alpha0, OnKeyAlpha0);

        KeyInputManager.Instance.Init();
        KeyInputManager.Instance.SetInputMode(KeyInputManager.InputMode.CarControl);

        SignalInputManager.Instance.carAttributes = carAttributes;
        SignalInputManager.Instance.Init();

        //全部子部件初始化
        engineSystem.Init();
        gearBoxSystem.Init();
        clutchSystem.Init();
        steeringSystem.Init();
        brakeSystem.Init();
    }

    public override void Play()
    {
        GameManager.Instance.AddUpdater(this);
    }

    public override void Stop()
    {
        GameManager.Instance.RemoveUpdater(this);
    }

    public void Update()
    {
        engineSystem.SetThrottleInput(carAttributes.accelerator);
        gearBoxSystem.SetGear(1);
        gearBoxSystem.SetRPM(engineSystem.RPM);
        steeringSystem.SetSteeringInput(carAttributes.steering);
        brakeSystem.SetBrakeInput(carAttributes.brake);
        clutchSystem.SetClutchInput(0);

        float thrustTorque = gearBoxSystem.OutputRPM * (1- clutchSystem.Clutch) / 4f;

        for (int i = 0; i < 4; i++)
        {
            m_Wheels[i].SetMotorTorque(thrustTorque);
            m_Wheels[i].SetBrakeTorque(brakeSystem.BrakeTorque);
        }
    }

    /// <summary>
    /// 切换相机
    /// </summary>
    private void OnKeyC()
    {

    }

    /// <summary>
    /// 暂停
    /// </summary>
    private void OnKeyP()
    {

    }

    /// <summary>
    /// 时间缩放
    /// </summary>
    private void OnKeyT()
    {

    }

    /// <summary>
    /// 键盘点火
    /// </summary>
    private void OnKeyY()
    {

    }

    /// <summary>
    /// 点火加电
    /// </summary>
    private void OnKeyE()
    {

    }

    /// <summary>
    /// 返回初始位置
    /// </summary>
    private void OnKeyReturn()
    {

    }

    /// <summary>
    /// 加档
    /// </summary>
    private void OnKeyPageDown()
    {

    }

    /// <summary>
    /// 减档
    /// </summary>
    private void OnKeyPageUp()
    {

    }

    /// <summary>
    /// 雨刮器开关
    /// </summary>
    private void OnKeyO()
    {

    }

    /// <summary>
    /// 刹车
    /// </summary>
    private void OnKeyB()
    {

    }

    /// <summary>
    /// 离合
    /// </summary>
    private void OnKeyAlpha0()
    {

    }

    public void Release(bool destroy = false)
    {
        throw new NotImplementedException();
    }
}
