using UnityEngine;
using System.Collections.Generic;
using ResourcesLoader;

public class Global : MonoBase {

    public static AssetsManager AssetMgr;
    public static TestManager TestMgr;
    public CarControlCenter carControl;

    // Use this for initialization
    void Start (){
        AssetMgr = this.gameObject.AddComponent<AssetsManager>();
        //   TestMgr = this.gameObject.AddComponent<TestManager>();
        //    TestMgr.Init();

        carControl.Init();

    }

    void Update()
    {
 
    }

	public override void Init()
	{
	        
	}
}
