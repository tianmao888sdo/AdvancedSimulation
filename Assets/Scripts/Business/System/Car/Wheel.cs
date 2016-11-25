using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 轮胎,负责，驱动轮胎转动，刹车和转向
/// </summary>
[ExecuteInEditMode]
public class Wheel : MonoBase, IWheel, IRelease
{
    /// <summary>
    /// 主轮胎
    /// </summary>
    [SerializeField]
    private WheelCollider m_masterWheelCollider;

    /// <summary>
    /// 刹车轮胎
    /// </summary>
    [SerializeField]
    private WheelCollider m_helpWheelCollider;

    /// <summary>
    /// 轮胎外观
    /// </summary>
    [SerializeField]
    private GameObject m_wheelMesh;

    /// <summary>
    /// 质量
    /// </summary>
    [SerializeField]
    private float m_mass = 20f;

    /// <summary>
    /// 车轮半径
    /// </summary>
    [SerializeField]
    private float m_radius = 0f;

    /// <summary>
    /// 机械效率，热能，粘滞阻力带来的损失
    /// </summary>
    [SerializeField][Range(0,1f)]
    private float m_mechanicalEfficiency = 1f;

    /// <summary>
    /// 转动惯量，惯量会带啦扭矩损失
    /// </summary>
    public float MomentofInertia { get { return 0.5f * m_mass * m_radius * m_radius; } }

    /// <summary>
    /// 角加速度
    /// </summary>
    public float ShaftAngleAcc { get { return 0f; } }

    public WheelCollider MasterWheelCollider {
        get { return m_masterWheelCollider; }
    }

    public WheelCollider HelpWheelCollider
    {
        get { return m_helpWheelCollider; }
    }

    public override void Init()
    {
        //查找wheelCollider和wheelMesh
        m_helpWheelCollider.mass=m_masterWheelCollider.mass = m_mass;
        m_helpWheelCollider.radius=m_masterWheelCollider.radius = m_radius;
    }

    public void SetMotorTorque(float val)
    {
        //val -= 1000*0.25f * 9.8f * Vector3.Dot(masterWheelCollider.transform.forward, Vector3.up);
        //Rigidbody r;
     
        //Debug.Log(val);

        //if (Mathf.Abs(val) < 100f)
        //    val = 0;

        m_masterWheelCollider.motorTorque =  val;
    }

    public void SetBrakeTorque(float val)
    {
        m_helpWheelCollider.brakeTorque = val;
    }

    public void SetSteerAngle(float val)
    {
        m_masterWheelCollider.steerAngle = val;
        m_helpWheelCollider.steerAngle = val;
    }

    // Update is called once per frame
    public void UpdateWheels()
    {
        Quaternion quat;
        Vector3 position;
        m_masterWheelCollider.GetWorldPose(out position, out quat);
        m_wheelMesh.transform.position = position;
        m_wheelMesh.transform.rotation = quat;
    }

    public void Release(bool destroy = false)
    {
        throw new NotImplementedException();
    }
}
