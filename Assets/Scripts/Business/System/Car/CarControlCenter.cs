using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 车辆控制中心，所有对车的操作，全部集中到这里，最终将操作作用于4个轮子
/// </summary>
public class CarControlCenter : MonoBase,IRelease
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
    /// 轮胎
    /// </summary>
    [SerializeField]
    private Wheel[] m_Wheels = new Wheel[4];

//    void OnGUI()
//    {
//        m_carAttributes.brake=GUI.HorizontalSlider(new Rect(0, 0, 200, 30), m_carAttributes.brake,0,1);
//    }

    public override void Init()
    {
        KeyInputManager.Instance.AddAxisDelegate(KeyInputManager.InputMode.CarControl, "Horizontal", (number) =>{m_carAttributes.steering = Mathf.Clamp(number, -1f, 1f);});
        KeyInputManager.Instance.AddAxisDelegate(KeyInputManager.InputMode.CarControl, "Vertical", (number) => { m_carAttributes.accelerator = Mathf.Clamp(number, 0, 1); });
   //     KeyInputManager.Instance.AddAxisDelegate(KeyInputManager.InputMode.CarControl, "Jump", (number) => { m_carAttributes.brake = Mathf.Clamp(number, 0, 1); });

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

        for (int i = 0; i < 4; i++)
            m_Wheels[i].Init();
    }

	private void Update()
    {
        m_engineSystem.SetThrottleInput(m_carAttributes.accelerator);
        m_gearBoxSystem.SetGear(1);
        m_gearBoxSystem.SetTorque(m_engineSystem.Torque);
        m_gearBoxSystem.SetClutch(0);
        m_steeringSystem.SetSteeringInput(m_carAttributes.steering);

        for (int i = 0; i < 4; i++)
        {
            JointSpring spring = new JointSpring();
            spring.spring = 70000f * (1 - m_carAttributes.brake);
            spring.damper = 0f;
            spring.targetPosition = 0.1f;

            JointSpring spring2 = new JointSpring();
            spring2.spring = 70000f * m_carAttributes.brake;
            spring2.damper = 0f;
            spring2.targetPosition = 0.1f;

            m_Wheels[i].MasterWheelCollider.suspensionSpring = spring;
            m_Wheels[i].HelpWheelCollider.suspensionSpring = spring2;
        }

        m_brakeSystem.SetBrakeInput(m_carAttributes.brake);
        Move(m_gearBoxSystem.OutputTorque, m_brakeSystem.BrakeTorque);

        for (int i = 0; i < 4; i++)
            m_Wheels[i].UpdateWheels();
    }

    /// <summary>
    /// 驱动和刹车
    /// </summary>
    /// <param name="motorTorque"></param>
    /// <param name="brakeTorque"></param>
    private void Move(float motorTorque,float brakeTorque)
    {
        float t_motorTorque = 0f;

        switch (m_carAttributes.motorMode)
        {
            case CarAttributes.MotorMode.FrontTwoDrive:
                t_motorTorque = m_gearBoxSystem.OutputTorque / 2f;
                m_Wheels[0].SetMotorTorque(t_motorTorque);
                m_Wheels[1].SetMotorTorque(t_motorTorque);
                break;
            case CarAttributes.MotorMode.RearTwoDrive:
                t_motorTorque = m_gearBoxSystem.OutputTorque / 2f;
                m_Wheels[2].SetMotorTorque(t_motorTorque);
                m_Wheels[3].SetMotorTorque(t_motorTorque);
                break;
            case CarAttributes.MotorMode.FourDrive:
                t_motorTorque = m_gearBoxSystem.OutputTorque / 4f;
                m_Wheels[0].SetMotorTorque(t_motorTorque);
                m_Wheels[1].SetMotorTorque(t_motorTorque);
                m_Wheels[2].SetMotorTorque(t_motorTorque);
                m_Wheels[3].SetMotorTorque(t_motorTorque);
                break;
        }

        switch (m_carAttributes.brakeMode)
        {
            case CarAttributes.BrakeMode.RearTwoBrake:
                //if(brakeSystem.BrakeTorque > 0)
                //{
                //    m_Wheels2[2].gameObject.SetActive(true);
                //    m_Wheels2[3].gameObject.SetActive(true);
                //    m_Wheels2[0].gameObject.SetActive(true);
                //    m_Wheels2[1].gameObject.SetActive(true);
                //    m_Wheels2[2].SetBrakeTorque(brakeSystem.BrakeTorque);
                //    m_Wheels2[3].SetBrakeTorque(brakeSystem.BrakeTorque);
                //    m_Wheels2[0].SetBrakeTorque(brakeSystem.BrakeTorque);
                //    m_Wheels2[1].SetBrakeTorque(brakeSystem.BrakeTorque);
                //}
                //else
                //{
                //    m_Wheels2[2].gameObject.SetActive(false);
                //    m_Wheels2[3].gameObject.SetActive(false);
                //    m_Wheels2[0].gameObject.SetActive(false);
                //    m_Wheels2[1].gameObject.SetActive(false);
                //}
                m_Wheels[2].SetBrakeTorque(m_brakeSystem.BrakeTorque);
                m_Wheels[3].SetBrakeTorque(m_brakeSystem.BrakeTorque);
                break;
            case CarAttributes.BrakeMode.FourBrake:
                m_Wheels[0].SetBrakeTorque(m_brakeSystem.BrakeTorque);
                m_Wheels[1].SetBrakeTorque(m_brakeSystem.BrakeTorque);
                m_Wheels[2].SetBrakeTorque(m_brakeSystem.BrakeTorque);
                m_Wheels[3].SetBrakeTorque(m_brakeSystem.BrakeTorque);
                break;
        }
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

    public void Release(bool destroy = false)
    {
        throw new NotImplementedException();
    }
}
