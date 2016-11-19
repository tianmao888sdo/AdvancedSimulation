using UnityEngine;
using System.Collections;

public class LoadTest : MonoBehaviour {

    //void Start()
    //{
    //    // 1.加载Manifest文件   
    //    AssetBundle manifestBundle = AssetBundle.CreateFromFile(Application.dataPath + "/ab/Assetbundle");
    //    if (manifestBundle != null)
    //    {
    //        AssetBundleManifest manifest = (AssetBundleManifest)manifestBundle.LoadAsset("AssetBundleManifest");
    //        // 2.获取依赖文件列表   
    //        string[] cubedepends = manifest.GetAllDependencies("assets/res/1.prefab");
    //        AssetBundle[] dependsAssetbundle = new AssetBundle[cubedepends.Length];
    //        for (int index = 0; index < cubedepends.Length; index++)
    //        {
    //            // 3.加载所有的依赖资源   
    //            dependsAssetbundle[index] = AssetBundle.CreateFromFile(
    //                Application.dataPath + "/../Assetbundle/" + cubedepends[index]);
    //        }

    //        // 4.加载资源   
    //        AssetBundle cubeBundle = AssetBundle.CreateFromFile(
    //            Application.dataPath + "/ab/assets/res/1.prefab");
    //        GameObject cube = cubeBundle.LoadAsset("1") as GameObject;
    //        if (cube != null)
    //        {
    //            Instantiate(cube);
    //        }
    //    }
    void Start()
    {
        //  asset manifestBundle = AssetBundle.CreateFromFile(Application.dataPath + "/ab/Assetbundle");
        AssetBundle manifestBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + "car.prefab");

        if(manifestBundle)
        {
            GameObject car = manifestBundle.LoadAsset("car.prefab") as GameObject;
        }
    }
}
