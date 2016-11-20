using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 轮胎,负责，驱动轮胎转动，刹车和转向
/// </summary>
public class Wheel : ScriptBase, IWheel, IRelease
{
    [SerializeField]
    private WheelCollider wheelCollider;
    [SerializeField]
    private GameObject wheelMesh;

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
        wheelCollider.motorTorque = val;
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
