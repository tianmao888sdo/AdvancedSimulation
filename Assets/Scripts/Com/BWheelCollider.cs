using UnityEngine;
using System.Collections;

[RequireComponent(typeof(HingeJoint))][ExecuteInEditMode]
public class BWheelCollider:MonoBehaviour
{
	
	private HingeJoint m_joint;
	private Rigidbody m_Body;

	[SerializeField]private Rigidbody m_connectedBody;
	[SerializeField]private float m_mass=1;
	[SerializeField]private float m_radius;
	[SerializeField]private float m_wheelDampingRate;
	[SerializeField]private float m_suspensionDistance;
	[SerializeField]private float m_forceAppPointDistance;
	[SerializeField]private Vector3 m_center;
	[SerializeField]private float m_spring;
	[SerializeField]private float m_damper;
	[SerializeField]private float m_targetPosition;
	[SerializeField]private AnimationCurve m_forwardFriction;
	[SerializeField]private AnimationCurve m_sidewaysFriction;

	public float mass{ get{ return m_mass;} set{ m_mass=value;}}
	public float radius{ get{ return m_radius;} set{ m_radius=value;}}
	public float wheelDampingRate{ get{ return m_wheelDampingRate;} set{m_wheelDampingRate=value;}}
	public float suspensionDistance{ get{ return m_suspensionDistance;} set{ m_suspensionDistance=value;}}
	public float forceAppPointDistance{ get{ return m_forceAppPointDistance;} set{ m_forceAppPointDistance=value;}}
	public Vector3 center{ get{ return m_center;} set{ m_center=value;}}
//	public float spring{get{ return m_spring;}set{ m_spring=value;}}
//	public float damper{get{ return m_damper;}set{ m_damper=value;}}
//	public float targetPosition{get{ return m_targetPosition;}set{m_targetPosition=value; }}
	public AnimationCurve forwardFriction{ get{ return m_forwardFriction;} set{ m_forwardFriction=value;}}
	public AnimationCurve sidewaysFriction{get{ return m_sidewaysFriction;}set{ m_sidewaysFriction=value;}}

	public float brakeTorque
	{ 
		get; set;
	}
	public bool isGrounded
	{ 
		get{ return false;}
	}

	public float motorTorque
	{ 
		get; set;
	}

	public float rpm
	{ 
		get
		{ 
			return m_rpm=0;
		}
	}

	public float sprungMass
	{ 
		get
		{ 
			return 0;
		}
	}

	public float steerAngle
	{ 
		get
		{
			return this.transform.localRotation.y;
		}
		set
		{
			this.transform.localRotation = Quaternion.AngleAxis (value, Vector3.up);
		}
	}


	[SerializeField]private float m_rpm=0;
	private float m_preLocalEulerAngle=float.NaN;

	void Awake()
	{
		m_Body = GetComponent<Rigidbody> ();
		m_joint=GetComponent<HingeJoint> ();
		m_joint.anchor = Vector3.zero;
		m_joint.autoConfigureConnectedAnchor = true;
		m_joint.useMotor = true;
		m_joint.useSpring = false;
	}

    // Use this for initialization
    void Start () {
		m_joint.connectedBody = m_connectedBody;
	//	m_joint.connectedAnchor = new Vector3(m_center.x,m_center.y-m_suspensionDistance,m_center.z);
	//	transform.localEulerAngles.x = transform.localEulerAngles.x % 360f;
		m_Body.mass=m_mass;
    }
	
	// Update is called once per frame
	void Update ()
	{
		m_joint.connectedBody = m_connectedBody;
	//	m_joint.connectedAnchor =new Vector3(m_center.x,m_center.y-m_suspensionDistance,m_center.z);
	//	transform.localEulerAngles.x = transform.localEulerAngles.x % 360f;
		m_Body.mass=m_mass;

		if (float.IsNaN (m_preLocalEulerAngle)) 
		{
			m_preLocalEulerAngle = transform.localEulerAngles.x;
		}
		else
		{
			m_rpm=Mathf.Abs (transform.localEulerAngles.x - m_preLocalEulerAngle)/Time.deltaTime;
			m_preLocalEulerAngle = transform.localEulerAngles.x;
		}
	}

//	void OnGUI()
//	{
//		GUI.TextField (new Rect (0, 0, 100, 10), m_rpm.ToString());
//	}

	void UpdateConnectedBody()
	{
		m_joint.connectedBody = m_connectedBody;
	}

	void UpdateConnectedAnchor()
	{
		m_joint.connectedAnchor =new Vector3(m_center.x,m_center.y-m_suspensionDistance,m_center.z);
	}

	/// <summary>
	/// Configure vehicle sub-stepping parameters.
	/// </summary>
	/// <param name="speedThreshold">The speed threshold of the sub-stepping algorithm.</param>
	/// <param name="stepsBelowThreshold">Amount of simulation sub-steps when vehicle's speed is below speedThreshold.</param>
	/// <param name="stepsAboveThreshold">Amount of simulation sub-steps when vehicle's speed is above speedThreshold.</param>
	public void ConfigureVehicleSubsteps(float speedThreshold,float stepsBelowThreshold,float stepsAboveThreshold)
	{
	}

	/// <summary>
	/// Gets ground collision data for the wheel.
	/// </summary>
	/// <param name="hit">Hit.</param>
	public void GetGroundHit(out WheelHit hit)
	{
		hit = default(WheelHit);
	}

	/// <summary>
	/// Gets the world space pose of the wheel accounting for ground contact, suspension limits, steer angle, and rotation angle (angles in degrees).
	/// </summary>
	/// <param name="pos">Position of the wheel in world space.</param>
	/// <param name="quat">Rotation of the wheel in world space.</param>
	public void GetWorldPos(out Vector3 pos, out Quaternion quat)
	{
		pos = default(Vector3);
		quat = default(Quaternion);
	}
}
