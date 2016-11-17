using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// 内存统计工具
/// </summary>
public class MemUtil
{
    public static Dictionary<string, int> downloadDic = new Dictionary<string, int>();//下载的原始资源字节数
    public static Dictionary<string, int> cloneObjDic = new Dictionary<string, int>();//克隆资源的总字节数
    public static Dictionary<string, int> newObjDic = new Dictionary<string, int>();//运行中创建的对象个数
    public static bool isStart = false;

    public static void AddDownloadDic(string fullPath,int bytes)
    {
        if (!isStart)
            return;

        if (downloadDic.ContainsKey(fullPath))
            downloadDic[fullPath] += bytes;
        else
            downloadDic.Add(fullPath, bytes);
    }

    public static void DelDownloadDic(string fullPath, int bytes)
    {
        if (!isStart)
            return;

        if (downloadDic.ContainsKey(fullPath))
            downloadDic[fullPath] -= bytes;
    }

    public static void AddCloneObjDic(string fullPath,int bytes)
    {
        if (!isStart)
            return;

        if (cloneObjDic.ContainsKey(fullPath))
            cloneObjDic[fullPath] += bytes;
        else
            cloneObjDic.Add(fullPath, bytes);
    }

    public static void DelCloneObjDic(string fullPath, int bytes)
    {
        if (!isStart)
            return;

        if (cloneObjDic.ContainsKey(fullPath))
            cloneObjDic[fullPath] -= bytes;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="objName">对象名</param>
    /// <param name="count"></param>
    public static void SetNewObjDic(string objName,int count)
    {
        if (!isStart)
            return;

        if (newObjDic.ContainsKey(objName))
            newObjDic[objName] += count;
        else
            newObjDic.Add(objName, count);
    }

    public static void Clear()
    {
        downloadDic.Clear();
        cloneObjDic.Clear();
        newObjDic.Clear();
    }
}
