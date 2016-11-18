using UnityEngine;
using System.Collections;

/// <summary>
/// 车辆控制中心，所有对车的操作，全部集中到这里
/// </summary>
public class CarControlCenter : ScriptBase
{
    public CarAttributes carAttributes;
    public Engine engine;
    [SerializeField]
    private WheelCollider[] m_WheelColliders = new WheelCollider[4];
    [SerializeField]
    private GameObject[] m_WheelMeshes = new GameObject[4];

    protected override void BFixedUpdate()
    {
        this.Move(carAttributes.steering, carAttributes.accelerator, carAttributes.brake);
    }

    public void Init()
    {
        KeyInputManager.Instance.AddAxisDelegate(KeyInputManager.InputMode.CarControl, "Horizontal", (number) =>
        {
            carAttributes.steering = Mathf.Clamp(number, -1f, 1f);
        });
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
        KeyInputManager.Instance.pause = false;
    }

    public void Move(float steering, float accel, float footbrake)
    {
        for (int i = 0; i < 4; i++)
        {
            Quaternion quat;
            Vector3 position;
            m_WheelColliders[i].GetWorldPose(out position, out quat);
            //m_WheelMeshes[i].transform.position = position;
            //m_WheelMeshes[i].transform.rotation = quat;
        }

        carAttributes.currSteerAngle = steering * carAttributes.maxSteerAngle;
        m_WheelColliders[0].steerAngle = carAttributes.currSteerAngle;
        m_WheelColliders[1].steerAngle = carAttributes.currSteerAngle;

        float thrustTorque = accel*engine.maxMotorTorque / 4f;

        for (int i = 0; i < 4; i++)
        {
            m_WheelColliders[i].motorTorque = thrustTorque;
            m_WheelColliders[i].brakeTorque = footbrake * m_WheelColliders[i].GetComponent<Wheel>().maxBrake;
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
}
