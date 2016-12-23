using UnityEngine;
using System.Collections;
using LocalAssets;

public class Global : MonoBase {

    public static AssetsManager AssetMgr;
    public static TestManager TestMgr;
    ResourceRequest r = null;
    // Use this for initialization
    void Start (){
        AssetMgr = this.gameObject.AddComponent<AssetsManager>();
        TestMgr = this.gameObject.AddComponent<TestManager>();
        TestMgr.Init();

    }

    void Update()
    {
 
    }

	public override void Init()
	{
	        
	}
}
