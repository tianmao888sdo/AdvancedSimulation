using UnityEngine;
using System.Collections;

public class Global : MonoBase {

    public CarControlCenter car;

	// Use this for initialization
	void Start (){
		Init ();
    }

	public override void Init()
	{
		car.Init();
	}
}
