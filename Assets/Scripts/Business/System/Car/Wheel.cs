using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 轮胎,负责，驱动轮胎转动，刹车和转向
/// </summary>
[ExecuteInEditMode]
public class Wheel : ScriptBase, IWheel, IRelease
{
    [SerializeField]
    private WheelCollider wheelCollider;
    [SerializeField]
    private GameObject wheelMesh;

    /// <summary>
    /// 车轮半径
    /// </summary>
    [SerializeField]
    private float Radius = 0f;

    /// <summary>
    /// 机械效率，热能，粘滞阻力带来的损失
    /// </summary>
    [SerializeField]
    private float mechanicalEfficiency = 1f;

    /// <summary>
    /// 转动惯量，惯量会带啦扭矩损失
    /// </summary>
    //public float MomentofInertia { get { return 00.5f * mass * crankShaftR * crankShaftR; } }

    /// <summary>
    /// 角加速度
    /// </summary>
    public float ShaftAngleAcc = 0f;

    public override void Init()
    {
        //查找wheelCollider和wheelMesh
    }

    public override void Play()
    {
        throw new NotImplementedException();
    }

    public override void Stop()
    {
        throw new NotImplementedException();
    }

    public void SetMotorTorque(float val)
    {
        val -= 1000*0.25f * 9.8f * Vector3.Dot(wheelCollider.transform.forward, Vector3.up);
        Rigidbody r;
     
        Debug.Log(val);

        if (Mathf.Abs(val) < 100f)
            val = 0;

        wheelCollider.motorTorque =  val*10;
    }

    public void SetBrakeTorque(float val)
    {
        wheelCollider.brakeTorque = val;
    }

    public void SetSteerAngle(float val)
    {
        wheelCollider.steerAngle = val;
    }

    // Update is called once per frame
    public void UpdateWheels()
    {
        Quaternion quat;
        Vector3 position;
        wheelCollider.GetWorldPose(out position, out quat);
        //wheelCollider.transform.position = position;
        //wheelCollider.transform.rotation = quat;
    }

    public void Release(bool destroy = false)
    {
        throw new NotImplementedException();
    }
}
