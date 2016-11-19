using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

/// <summary>
/// 打包要求，被依赖的资源一定与主资源放在同一目录下，主要针对预制体
/// manifest文件会生成在与资源同目录下
/// </summary>
public class BuildAssetsBase : Editor
{
    //保存需要打包的资源路径，递归
    private Dictionary<string, List<string>> assetsPathListDir = new Dictionary<string, List<string>>();
    //需要打包的资源后缀
    private Dictionary<string, string> asExtensionDic = new Dictionary<string, string>();

    /// <summary>
    /// 打包指定目录
    /// </summary>
    protected static void BuildSelectedDir(BuildAssetsBase build)
    {
        var select = Selection.activeObject;
        var path = AssetDatabase.GetAssetPath(select);
        build.ListDirs(path);
        build.ExecuteAssetsBuild();
    }

    /// <summary>
    /// 打包指定目录
    /// </summary>
    protected static void BuildAll(BuildAssetsBase build)
    {
        build.ListDirs(build.GetResPath());
        build.ExecuteAssetsBuild();
    }

    /// <summary>
    /// 获取版本号
    /// </summary>
    /// <returns></returns>
    protected virtual string GetVer() { return "0"; }

    /// <summary>
    /// 资源路径
    /// </summary>
    /// <returns></returns>
    protected virtual string GetResPath(){ return "Assets/Reources";}

    /// <summary>
    /// 目标路径
    /// </summary>
    /// <returns></returns>
    protected virtual string GetDstPath() { return "Assets/StreamingAssets"; }

    /// <summary>
    /// 规则过滤,重命名依赖文件的资源名，默认情况下是打包prefab
    /// </summary>
    /// <param name="file"></param>
    /// <returns>有符合打包条件的资源返回true，其他情况false</returns>
    protected virtual bool AssetsFilter(FileInfo file)
    {
        return false;
    }

    /// <summary>
    /// 遍历制定文件夹获取需要打包的资源路径
    /// </summary>
    /// <param name="dirPath"></param>
    private void ListDirs(string dirPath)
    {
        string[] filePaths = Directory.GetFiles(dirPath);

        foreach (string filePath in filePaths)
        {
            //将需要打包的资源路径添加到打包路劲中

            if (AssetsFilter(new FileInfo(filePath)))
            {
                if (!assetsPathListDir.ContainsKey(dirPath))
                    assetsPathListDir.Add(dirPath, new List<string>());

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
    /// 执行打包
    /// </summary>
    private void ExecuteAssetsBuild()
    {
        foreach (var item in assetsPathListDir)
        {
            //创建目录
            string path = item.Key.Replace(GetResPath(), GetDstPath());

            //如果不存在到处路径文件，创建一个
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //打包
            BuildPipeline.BuildAssetBundles(path,BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        }

        //刷新资源路径,避免生成的文件不显示
        AssetDatabase.Refresh();
    }

    //清除已经打包的资源 AssetBundleNames
    [MenuItem("BuildAsset/ClearAssetBundlesName")]
    private static void ClearAssetBundlesName()
    {
        int length = AssetDatabase.GetAllAssetBundleNames().Length;

        string[] oldAssetBundleNames = new string[length];
        for (int i = 0; i < length; i++)
        {
            oldAssetBundleNames[i] = AssetDatabase.GetAllAssetBundleNames()[i];
        }

        for (int j = 0; j < oldAssetBundleNames.Length; j++)
        {
            AssetDatabase.RemoveAssetBundleName(oldAssetBundleNames[j], true);
        }
    }

    //添加需要打包资源的后缀
    private void SetExtensionDic()
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
}

//public class Filter
//{
//    public virtual string ResPath() { return "Assets/Reources"; }
//    public virtual string DstPath() { return "Assets/StreamingAssets"; }

//    /// <summary>
//    /// 规则过滤
//    /// </summary>
//    /// <param name="file"></param>
//    /// <returns>有符合打包条件的资源返回true，其他情况false</returns>
//    public virtual bool AssetsFilter(FileInfo file)
//    {
//        if (file.Exists)
//        {
//            if (file.FullName.EndsWith(".meta"))
//                return false;

//            if (file.FullName.EndsWith(".prefab"))
//            {
//                string[] dps = AssetDatabase.GetDependencies(file.ToString(), false);

//                for (int i = 0; i < dps.Length; i++)
//                {
//                    if (dps[i] == file.ToString())
//                        continue;

//                    //通过资源路径来获取需要打包的资源
//                    AssetImporter ai = AssetImporter.GetAtPath(dps[i]);
//                    ai.assetBundleName = AssetDatabase.AssetPathToGUID(dps[i]);
//                }

//                return true;
//            }
//        }

//        return false;
//    }
//}

////根据切换的平台返回相应的导出路径
//public class Plathform
//{
//    public static string GetPlatformFolder(BuildTarget target)
//    {
//        switch (target)
//        {
//            case BuildTarget.Android:   //Android平台导出到 Android文件夹中
//                return "Android";
//            case BuildTarget.iOS:
//                return "IOS";
//            case BuildTarget.WebPlayer:
//                return "WebPlayer";
//            case BuildTarget.StandaloneWindows:
//            case BuildTarget.StandaloneWindows64:
//                return "Windows";
//            case BuildTarget.StandaloneOSXIntel:
//            case BuildTarget.StandaloneOSXIntel64:
//            case BuildTarget.StandaloneOSXUniversal:
//                return "OSX";
//            default:
//                return null;
//        }
//    }

//}