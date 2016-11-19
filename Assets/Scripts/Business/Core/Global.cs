using UnityEngine;
using System.Collections;

public class Global : MonoBehaviour {

    public CarControlCenter car;

	// Use this for initialization
	void Start (){
        car.Init();
        car.Play();
    }
}
