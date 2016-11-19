using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// 打包要求，被依赖的资源一定与主资源放在同一目录下，主要针对预制体
/// manifest文件会生成在与资源同目录下
/// </summary>
public class BuildPrefab : BuildAssetsBase
{
    /// <summary>
    /// 打包指定目录
    /// </summary>
    [MenuItem("BuildAsset/BuildSelectedPrefab")]
    private static void BuildSelectedPrefab()
    {
        BuildPrefab.BuildSelectedDir(CreateInstance<BuildPrefab>());
    }

    /// <summary>
    /// 打包指定目录
    /// </summary>
    [MenuItem("BuildAsset/BuildAllPrefab")]
    private static void BuildAllPrefab()
    {
        BuildPrefab.BuildAll(CreateInstance<BuildPrefab>());
    }

    /// <summary>
    /// 规则过滤,默认情况下是打包prefab
    /// </summary>
    /// <param name="file"></param>
    /// <returns>有符合打包条件的资源返回true，其他情况false</returns>
    protected override bool AssetsFilter(FileInfo file)
    {
        if (file.Exists)
        {
            if (file.FullName.EndsWith(".meta"))
                return false;

            if (file.FullName.EndsWith(".prefab"))
            {
                string[] dps = AssetDatabase.GetDependencies(file.ToString());

                for (int i = 0; i < dps.Length; i++)
                {
                    if (Path.GetExtension(dps[i]) == ".cs")
                        continue;

                    //通过资源路径来获取需要打包的资源
                    AssetImporter ai = AssetImporter.GetAtPath(dps[i]);
                    //ai.assetBundleName = AssetDatabase.AssetPathToGUID(dps[i]) + Path.GetExtension(dps[i]);
                    ai.assetBundleName = new FileInfo(dps[i]).Name;
                }

                return true;
            }
        }

        return false;
    }
}
