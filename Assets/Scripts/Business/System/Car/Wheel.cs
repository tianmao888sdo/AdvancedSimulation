using UnityEngine;
using System.Collections;

public class Wheel : MonoBehaviour
{
    public WheelCollider wheelCollider;
    public GameObject wheelMesh;
    public float maxBrake = 0f;

    // Update is called once per frame
    public void UpdateWheels()
    {
        Quaternion quat;
        Vector3 position;
        wheelCollider.GetWorldPose(out position, out quat);
        //wheelCollider.transform.position = position;
        //wheelCollider.transform.rotation = quat;
    }
}
