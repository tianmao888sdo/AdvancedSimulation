using UnityEngine;
using System;

/// <summary>
/// 车辆控制中心，所有对车的操作，全部集中到这里，最终将操作作用于4个轮子
/// </summary>
public class CarControlCenter:MonoBase
{
    /// <summary>
    /// 属性
    /// </summary>
    [SerializeField]
    private CarAttributes m_carAttributes;
    /// <summary>
    /// 发动机
    /// </summary>
    [SerializeField]
    private EngineSystem m_engineSystem;
    /// <summary>
    /// 转向系统
    /// </summary>
    [SerializeField]
    private SteeringSystem m_steeringSystem;

    /// <summary>
    /// 变速箱
    /// </summary>
    [SerializeField]
    private GearBoxSystem m_gearBoxSystem;

    /// <summary>
    /// 减速器
    /// </summary>
    [SerializeField]
    private GearReducerSystem m_gearReducerSystem;

    /// <summary>
    /// 刹车系统
    /// </summary>
    [SerializeField]
    private BrakeSystem m_brakeSystem;

    /// <summary>
    /// 传动系统
    /// </summary>
    [SerializeField]
    private TransmissionSystem m_transmissionSystem;

    void OnGUI()
    {
        m_carAttributes.brake = GUI.HorizontalSlider(new Rect(0, 0, 200, 30), m_carAttributes.brake, 0, 1);
    }

    public override void Init()
    {
        base.Init();

        KeyInputManager.Instance.AddAxisDelegate(KeyInputManager.InputMode.CarControl, "Horizontal", (number) =>{m_carAttributes.steering = Mathf.Clamp(number, -1f, 1f);});
        KeyInputManager.Instance.AddAxisDelegate(KeyInputManager.InputMode.CarControl, "Vertical", (number) => { m_carAttributes.accelerator = Mathf.Clamp(number, 0, 1); });
  //      KeyInputManager.Instance.AddAxisDelegate(KeyInputManager.InputMode.CarControl, "Jump", (number) => { m_carAttributes.brake = Mathf.Clamp(number, 0, 1); });

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
		KeyInputManager.Instance.Play ();

		SignalInputManager.Instance.carAttributes = m_carAttributes;
		SignalInputManager.Instance.Init ();	
		SignalInputManager.Instance.Play ();	

        //全部子部件初始化
        m_engineSystem.Init();
        m_gearBoxSystem.Init();
        m_steeringSystem.Init();
        m_brakeSystem.Init();
        m_transmissionSystem.Init();
    }

    protected override void DUpdate()
    {
        m_engineSystem.SetThrottleInput(m_carAttributes.accelerator);
        m_gearBoxSystem.SetGear(1);
        m_gearBoxSystem.SetTorque(m_engineSystem.Torque);
        m_gearBoxSystem.SetClutch(0);
        m_steeringSystem.SetSteeringInput(m_carAttributes.steering);

        //暂时用刹车比例代替刹车力矩
        m_transmissionSystem.Move(m_carAttributes.motorMode, m_gearBoxSystem.OutputTorque);
        m_brakeSystem.Brake(m_carAttributes.brakeMode, m_carAttributes.brake);
    }

    public float GetAccelerator()
    {
        return 0;
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

    public override void Release(bool destroy = false)
    {
        base.Release(destroy);
        throw new NotImplementedException();
    }

}
