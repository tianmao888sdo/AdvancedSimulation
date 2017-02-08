using UnityEngine;
using System.Collections;
using ResourcesLoader;

public class TestManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
        
	}

    public void Init()
    {
        Global.AssetMgr.GetResource(this, "Cube", "cube", null, Callback, AssetType.gameobject, true, true);
    }

    void Callback(string path, string name, Object obj, params string[] args)
    {
        Cubemap cube = obj as Cubemap;
       
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
