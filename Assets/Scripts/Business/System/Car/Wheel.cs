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
    /// 刹车轮胎,用物理材质来模拟刹车摩擦力，只使用动态摩擦力，静态摩擦力一直保持0
    /// </summary>
    [SerializeField]
	private WheelCollider m_brakeWheelCollider;

	/// <summary>
	/// 刹车轮胎,用物理材质来模拟刹车摩擦力，只使用动态摩擦力，静态摩擦力一直保持0
	/// </summary>
	[SerializeField]
	private WheelCollider m_brakeWheelCollider2;

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
	/// 刹车轮胎上下可移动的最大距离
	/// </summary>
	[SerializeField]
	private float m_brakeWheelMaxDis=0.01f;

	/// <summary>
	/// 刹车轮胎上下移动的距离
	/// </summary>
	private float m_brakeWheelDis = 0f;

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

    public override void Init()
    {
        //查找wheelCollider和wheelMesh
		m_brakeWheelCollider2.mass=m_brakeWheelCollider.mass=m_masterWheelCollider.mass = m_mass;
		m_brakeWheelCollider2.radius=m_brakeWheelCollider.radius=m_masterWheelCollider.radius = m_radius;
		m_brakeWheelCollider.steerAngle = 45;
		m_brakeWheelCollider2.steerAngle = -45;
    }

	/// <summary>
	/// 设置驱动扭矩
	/// </summary>
	/// <param name="val"></param>
    public void SetMotorTorque(float val)
    {
        //val -= 1000*0.25f * 9.8f * Vector3.Dot(masterWheelCollider.transform.forward, Vector3.up);
		//模拟重力

        m_masterWheelCollider.motorTorque =  val;
    }

	/// <summary>
	/// 设置刹车扭矩
	/// </summary>
	/// <param name="val"></param>
    public void SetBrakeTorque(float val)
    {

			WheelFrictionCurve forwardCurve=new WheelFrictionCurve ();
			WheelFrictionCurve sidewayCurve=new WheelFrictionCurve ();
			forwardCurve.asymptoteSlip = 0.4f;
			forwardCurve.asymptoteValue = 1f;
			forwardCurve.extremumSlip = 0.8f;
			forwardCurve.extremumValue = 0.5f;
			forwardCurve.stiffness =val>1?1:0;

			sidewayCurve.asymptoteSlip = 0.2f;
			sidewayCurve.asymptoteValue = 1f;
			sidewayCurve.extremumSlip = 0.5f;
			sidewayCurve.extremumValue = 0.75f;
			sidewayCurve.stiffness = 0;

			m_brakeWheelCollider.forwardFriction = m_brakeWheelCollider2.forwardFriction = forwardCurve;
			m_brakeWheelCollider.sidewaysFriction = m_brakeWheelCollider2.sidewaysFriction = sidewayCurve;
		

		m_brakeWheelCollider.brakeTorque = val;
		m_brakeWheelCollider2.brakeTorque = val;
    }

	/// <summary>
	/// 转向
	/// </summary>
	/// <param name="val"></param>
    public void SetSteerAngle(float val)
    {
        m_masterWheelCollider.steerAngle = val;
    }

    /// <summary>
    /// 更新轮胎位置
    /// </summary>
    public void UpdateWheels()
    {
        Quaternion quat;
        Vector3 position;
        m_masterWheelCollider.GetWorldPose(out position, out quat);
        m_wheelMesh.transform.position = position;
        m_wheelMesh.transform.rotation = quat;
//
//		//刹车时让刹车轮胎接触地面，否则，离开地面
//		m_brakeWheelCollider.transform.position =new Vector3(position.x,position.y-m_brakeWheelDis,position.z);
    }

    public void Release(bool destroy = false)
    {
        throw new NotImplementedException();
    }
}
