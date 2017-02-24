using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 轮胎,负责，驱动轮胎转动，刹车和转向
/// </summary>
[ExecuteInEditMode]
public class Wheel : MonoBase, IWheel
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
    /// 刹车曲线
    /// </summary>
    [SerializeField]
    private AnimationCurve m_brakeCurve;

    /// <summary>
    /// 刹车轮胎上下可移动的最大距离
    /// </summary>
    [SerializeField]
	private float m_brakeWheelMaxDis=0.01f;

	/// <summary>
	/// 刹车轮胎上下移动的距离
	/// </summary>
	private float m_brakeWheelDis = 0;

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
        base.Init();
        //查找wheelCollider和wheelMesh
        m_brakeWheelCollider2.mass=m_brakeWheelCollider.mass=m_masterWheelCollider.mass = m_mass;
		m_brakeWheelCollider2.radius=m_brakeWheelCollider.radius=m_masterWheelCollider.radius = m_radius;
        m_brakeWheelCollider.steerAngle = 45;
        m_brakeWheelCollider2.steerAngle = -45;

        JointSpring spring = m_masterWheelCollider.suspensionSpring;
        spring.spring = 70000;
        m_masterWheelCollider.suspensionSpring = spring;
        spring.spring = 0;
        spring.damper = 0f;
        m_brakeWheelCollider.suspensionSpring = m_brakeWheelCollider2.suspensionSpring = spring;

        //读取配置文件获得参数
        Keyframe[] keys = new Keyframe[]
        {
            new Keyframe(0,0),
            new Keyframe(0.1f,1.5f),
            new Keyframe(0.2f,1.6f),
            new Keyframe(0.3f,1.7f),
            new Keyframe(0.4f,1.8f),
            new Keyframe(0.5f,1.9f),
            new Keyframe(0.6f,2.0f),
            new Keyframe(0.7f,2.5f),
            new Keyframe(0.8f,2.8f),
            new Keyframe(0.9f,3.0f),
            new Keyframe(1.0f,4.0f)
        };

        m_brakeCurve = new AnimationCurve(keys);

        for(int i=0;i<10;i++)
        {
            m_brakeCurve.SmoothTangents(i, 1);
        }
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
        //m_masterWheelCollider.center= new Vector3(0, 0.1f * val, 0);
    //    m_brakeWheelCollider.center = m_brakeWheelCollider2.center = new Vector3(0, 0.05f * (1 - val), 0);

        //JointSpring spring = m_masterWheelCollider.suspensionSpring;
        //spring.spring = 70000*(1-val);

        //m_masterWheelCollider.suspensionSpring = spring;
        //spring.spring = 70000 * val/2;
        //m_brakeWheelCollider.suspensionSpring = m_brakeWheelCollider2.suspensionSpring=spring;

        WheelFrictionCurve forwardCurve= m_brakeWheelCollider.forwardFriction;
		WheelFrictionCurve sidewayCurve= m_brakeWheelCollider.sidewaysFriction;
		forwardCurve.stiffness =0;
		sidewayCurve.stiffness = m_brakeCurve.Evaluate(val);

		m_brakeWheelCollider.forwardFriction = m_brakeWheelCollider2.forwardFriction = forwardCurve;
        m_brakeWheelCollider.sidewaysFriction = m_brakeWheelCollider2.sidewaysFriction = sidewayCurve;
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
    }

    public override void Release(bool destroy = false)
    {
        base.Release(destroy);
        throw new NotImplementedException();
    }
}
