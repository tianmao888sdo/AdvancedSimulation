using UnityEngine;
using System.Collections;

public class ShowAttr : MonoBehaviour {

    private WheelCollider collider;
    public float motor;
    public float brake;
	// Use this for initialization
	void Start () {
        collider = this.GetComponent<WheelCollider>();
        motor = collider.motorTorque;
        brake = collider.brakeTorque;
    }
	
	// Update is called once per frame
	void Update () {
        motor = collider.motorTorque;
        brake = collider.brakeTorque;
    }
}
