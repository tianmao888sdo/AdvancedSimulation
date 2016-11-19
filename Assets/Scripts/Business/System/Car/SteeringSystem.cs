using UnityEngine;
using System.Collections;

/// <summary>
/// 负责根据方向盘的输入改变轮胎转向，要求有前束角
/// </summary>
public class SteeringSystem : MonoBehaviour {

    [SerializeField]
    private WheelCollider[] m_WheelColliders = new WheelCollider[2];

    public float maxSteerAngle = 30f;
    public float currSteerAngle = 0f;
    public float steeringInput = 0f;

    private float m_steeringInput = 0f;
    /// <summary>
    /// 外部转向输入
    /// </summary>
    public void SetSteeringInput(float val)
    {
        m_steeringInput = val;
        currSteerAngle = m_steeringInput * maxSteerAngle;
        m_WheelColliders[0].steerAngle = currSteerAngle;
        m_WheelColliders[1].steerAngle = currSteerAngle;
    }
}
