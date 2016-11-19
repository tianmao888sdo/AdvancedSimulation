using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 车辆控制中心，所有对车的操作，全部集中到这里
/// </summary>
public class CarControlCenter :MonoBehaviour,IUpdate,IRelease
{
    public CarAttributes carAttributes;
    public EngineSystem engineSystem;
    public SteeringSystem steeringSystem;
    public ClutchSystem clutchSystem;
    public GearBoxSystem gearBoxSystem;
    public GearReducerSystem gearReducerSystem;
    public BrakeSystem brakeSystem;

    [SerializeField]
    private Wheel[] m_Wheels = new Wheel[4];

    public void Init()
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

        gearBoxSystem.gear = 1;
    }

    public void Update()
    {
        engineSystem.SetThrottleInput(carAttributes.accelerator);
        steeringSystem .SetSteeringInput(carAttributes.steering);
        brakeSystem.SetBrakeInput(carAttributes.brake);
        clutchSystem.SetClutchInput(0);

        float thrustTorque = gearBoxSystem.GetNewRPM(engineSystem.rpm) / 4f;

        for (int i = 0; i < 4; i++)
        {
            m_Wheels[i].wheelCollider.motorTorque = thrustTorque;
            Debug.Log(thrustTorque);
            m_Wheels[i].wheelCollider.brakeTorque = brakeSystem.GetBrakeTorque();
        }
    }

    public void Play()
    {
        GameManager.Instance.AddUpdater(this);
    }

    public void Stop()
    {
        GameManager.Instance.RemoveUpdater(this);
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
