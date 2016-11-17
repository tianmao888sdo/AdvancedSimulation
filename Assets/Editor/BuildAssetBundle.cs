using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

public class BuildAssetBundle : Editor
{
    //需要打包的路径
    private static string resPath = "Assets/Reources";
    //导出包路径
    private static string dstPath = "Assets/StreamingAssets";
    //保存需要打包的资源路径，递归
    private static Dictionary<string,List<string>> assetsPathListDir = new  Dictionary<string,List<string>>();
    //需要打包的资源后缀
    private static Dictionary<string, string> asExtensionDic = new Dictionary<string, string>();

    /// <summary>
    /// 打包指定目录
    /// </summary>
    [MenuItem("BuildAsset/BuildSelectedDir")]
    private static void BuildSelectedDir()
    {
        var select = Selection.activeObject;
        var path = AssetDatabase.GetAssetPath(select);
        SetExtensionDic();
        ListDirs(path);
        ExecuteAssetsBuild();
    }

    /// <summary>
    /// 打包指定目录
    /// </summary>
    [MenuItem("BuildAsset/BuildAll")]
    private static void BuildAll()
    {
        SetExtensionDic();
        ListDirs(resPath);
        ExecuteAssetsBuild();
    }

    /// <summary>
    /// 遍历制定文件夹获取需要打包的资源路径
    /// </summary>
    /// <param name="dirPath"></param>
    private static void ListDirs(string dirPath)
    {
        if (!assetsPathListDir.ContainsKey(dirPath))
            assetsPathListDir.Add(dirPath, new List<string>());

        foreach (string filePath in Directory.GetFiles(dirPath))
        {
            //将需要打包的资源路径添加到打包路劲中

            if(AssetsFilter(new FileInfo(filePath)))
            {
                assetsPathListDir[dirPath].Add(filePath);
            }
        }

        foreach (string path in Directory.GetDirectories(dirPath))
        {
            //使用递归方法遍历所有文件夹
            ListDirs(path);
        }
    }

    /// <summary>
    /// 规则过滤
    /// </summary>
    /// <param name="file"></param>
    private static bool AssetsFilter(FileInfo file)
    {
        if (file.Exists)
        {
            if (file.FullName.EndsWith(".prefab"))
            {
                string[] dps = AssetDatabase.GetDependencies(file.ToString());

                for(int i=0;i<dps.Length;i++)
                {
                    // 在Windows 平台 要从 Assets/ 开始，和 Mac 上不同，此处也是一个大坑
                    string path = "";

                    if (Application.platform == RuntimePlatform.WindowsEditor)
                    {
                        path = dps[i].Substring(dps[i].IndexOf("Assets/"));
                    }

                    //通过资源路径来获取需要打包的资源
                    AssetImporter ai = AssetImporter.GetAtPath(path);
                    ai.assetBundleName = AssetDatabase.AssetPathToGUID(path);
                }

                return true;
            }
            else if(file.FullName.EndsWith(".controller"))
            {
                // 在Windows 平台 要从 Assets/ 开始，和 Mac 上不同，此处也是一个大坑
                string path = "";

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    path = file.FullName.Substring(file.FullName.IndexOf("Assets/"));
                }

                //通过资源路径来获取需要打包的资源
                AssetImporter ai = AssetImporter.GetAtPath(path);
                ai.assetBundleName = AssetDatabase.AssetPathToGUID(path);

                return true;
            }
            else if (file.FullName.EndsWith(".mat"))//有依赖
            {
                string[] dps = AssetDatabase.GetDependencies(file.FullName);

                for (int i = 0; i < dps.Length; i++)
                {
                    // 在Windows 平台 要从 Assets/ 开始，和 Mac 上不同，此处也是一个大坑
                    string path = "";

                    if (Application.platform == RuntimePlatform.WindowsEditor)
                    {
                        path = dps[i].Substring(dps[i].IndexOf("Assets/"));
                    }

                    //通过资源路径来获取需要打包的资源
                    AssetImporter ai = AssetImporter.GetAtPath(path);
                    ai.assetBundleName = AssetDatabase.AssetPathToGUID(path);
                }

                return true;
            }
            else if (file.FullName.EndsWith(".jpg") || file.FullName.EndsWith(".png"))
            {
                return true;
            }
            else if (file.FullName.EndsWith(".txt") || file.FullName.EndsWith(".xml") || file.FullName.EndsWith(".shader"))
            {
                return true;
            }
            else if (file.FullName.EndsWith(".cs"))
            {
                return true;
            }

            return false;
        }

        return false;
    }

    /// <summary>
    /// 执行打包
    /// </summary>
    private static void ExecuteAssetsBuild()
    {
        foreach (var item in assetsPathListDir)
        {
            //创建目录
            string path = item.Key.Replace(resPath, dstPath);

            //如果不存在到处路径文件，创建一个
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //打包
            BuildPipeline.BuildAssetBundles(path, 0, EditorUserBuildSettings.activeBuildTarget);
        }

        //刷新资源路径,避免生成的文件不显示
        AssetDatabase.Refresh();
    }

    //添加需要打包资源的后缀
    private static void SetExtensionDic()
    {
        asExtensionDic.Clear();

        asExtensionDic.Add(".prefab", ".assetbundle");//mesh ui
        asExtensionDic.Add(".controller", ".assetbundle");
        asExtensionDic.Add(".mat", ".assetbundle");//atlas
        asExtensionDic.Add(".jpg", ".assetbundle");
        asExtensionDic.Add(".png", ".assetbundle");
        asExtensionDic.Add(".txt", ".assetbundle");
        asExtensionDic.Add(".xml", ".assetbundle");
        asExtensionDic.Add(".shader", ".assetbundle");
        asExtensionDic.Add(".cs", ".assetbundle");
    }

    ////清除已经打包的资源 AssetBundleNames
    //private static void ClearAssetBundlesName()
    //{
    //    int length = AssetDatabase.GetAllAssetBundleNames().Length;
    //    Debug.Log(length);
    //    string[] oldAssetBundleNames = new string[length];
    //    for (int i = 0; i < length; i++)
    //    {
    //        oldAssetBundleNames[i] = AssetDatabase.GetAllAssetBundleNames()[i];
    //    }

    //    for (int j = 0; j < oldAssetBundleNames.Length; j++)
    //    {
    //        AssetDatabase.RemoveAssetBundleName(oldAssetBundleNames[j], true);
    //    }
    //}
}


//根据切换的平台返回相应的导出路径
public class Plathform
{
    public static string GetPlatformFolder(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.Android:   //Android平台导出到 Android文件夹中
                return "Android";
            case BuildTarget.iOS:
                return "IOS";
            case BuildTarget.WebPlayer:
                return "WebPlayer";
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return "Windows";
            case BuildTarget.StandaloneOSXIntel:
            case BuildTarget.StandaloneOSXIntel64:
            case BuildTarget.StandaloneOSXUniversal:
                return "OSX";
            default:
                return null;
        }
    }

}