using UnityEngine;
using System.Collections;

public class Global : MonoBehaviour {

    public CarControlCenter car;

	// Use this for initialization
	void Start () {

        car.Init();

        SignalInputManager.Instance.carAttributes = car.carAttributes;
        SignalInputManager.Instance.Init();

        car.pause = false;
        SignalInputManager.Instance.pause = false;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
