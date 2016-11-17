using UnityEngine;
using System.Collections.Generic;

public delegate void LoadCompleteCallback(string path, string name, Object obj, params string[] args);
public delegate void LoadingProgressCallback(string path, string name,float progress, params string[] args);

///<summary>
///资源管理器，加载管理器
///注意事项：
///获取实例化后的资源，默认类型为gameobject，默认优先级
///场景不会返回对象资源，文本是同步加载，并且它们都不缓存
///获取gameobject对象一定要实例化，不允许直接使用未实例化的原始对象
///销毁对象时，必须用DiscardUsage方法回收
///加载的对象不存在多次尝试加载的情况，错误则忽略
///创建日期：2015/8/7
///修改日期：2015/9/19
///修改人：lijunfeng
///<summary>
public sealed class LocalResManager : MonoBehaviour,IRelease
{
    private Dictionary<string, ResInfo> m_dictLocalResource = new Dictionary<string, ResInfo>();
    private Dictionary<int, ResInfo> m_dictGameObjectRes = new Dictionary<int, ResInfo>();//保存游戏对象与ResInfo的对应关系，以便用来销毁
    private Dictionary<int, List<ResLoader>> m_dictDownload = new Dictionary<int, List<ResLoader>>();
    private int m_loaderCount = 0;//统计所有加载器，！！！需要与实际加载器数同步？

    /// <summary>
    /// 获取实例化后的资源，默认类型为gameobject，默认优先级
    /// 场景不会返回对象资源，文本是同步加载，并且它们都不缓存
    /// </summary>
    /// <param name="scriptObject">引用资源的脚本</param>
    /// <param name="path">资源包全路径（带名字）</param>
    /// <param name="name">要请求的资源名</param>
    /// <param name="progress"></param>
    /// <param name="complete"></param>
    /// <param name="args"></param>
    public void GetLocalResource(System.Object scriptObject, string path, string name, LoadingProgressCallback progress,LoadCompleteCallback complete, params string[] args)
    {
        GetLocalResource(scriptObject, path, name, progress, complete, ResourceType.gameobject, Order.o_default,args);
    }

    /// <summary>
    /// 获取实例化后的资源，加载优先级为默认优先级
    /// 场景不会返回对象资源，文本是同步加载，并且它们都不缓存
    /// </summary>
    /// <param name="scriptObject">引用资源的脚本</param>
    /// <param name="path">资源包全路径（带名字）</param>
    /// <param name="name">要请求的资源名</param>
    /// <param name="progress"></param>
    /// <param name="complete"></param>
    /// <param name="type">ResourceType</param>
    /// <param name="args"></param>
    public void GetLocalResource(System.Object scriptObject, string path, string name, LoadingProgressCallback progress, LoadCompleteCallback complete, ResourceType type, params string[] args)
    {
        GetLocalResource(scriptObject, path, name, progress,complete, type, Order.o_default,args);
    }

    /// <summary>
    /// 初始化资源，会实例化
    /// 场景不会返回对象资源，文本是同步加载，并且它们都不缓存
    /// </summary>
    /// <param name="scriptObject">引用资源的脚本</param>
    /// <param name="path">资源包全路径（带名字）</param>
    /// <param name="name">要请求的资源名</param>
    /// <param name="progress"></param>
    /// <param name="complete"></param>
    /// <param name="type">ResourceType</param>
    /// <param name="order">优先级</param>
    /// <param name="args"></param>
    /// <returns></returns>
    public void GetLocalResource(System.Object scriptObject, string path, string name, LoadingProgressCallback progress,LoadCompleteCallback complete, ResourceType type, Order order, params string[] args)
    {
        GetLocalResource(scriptObject, path, name, progress, complete, type, order, true, args);
    }

    /// <summary>
    /// 获取资源
    /// 场景不会返回对象资源，文本是同步加载，并且它们都不缓存
    /// </summary>
    /// <param name="scriptObject">引用资源的脚本</param>
    /// <param name="path">资源包全路径（带名字）</param>
    /// <param name="name">要请求的资源名</param>
    /// <param name="progress"></param>
    /// <param name="complete"></param>
    /// <param name="type">ResourceType</param>
    /// <param name="order">优先级</param>
    /// <param name="isInstance">是否实例化</param>
    /// <param name="args"></param>
    /// <returns>原始对象，如果能直接得到则为从缓存中取出</returns>
    public Object GetLocalResource(System.Object scriptObject, string path, string name,LoadingProgressCallback progress,LoadCompleteCallback complete, ResourceType type, Order order, bool isInstance, params string[] args)
    {
        ResInfo t_info;
        ResLoader t_loader;
        bool t_isNeedDownload = true;

        if (m_dictLocalResource.TryGetValue(path,out t_info))
        {
            if(t_info.assetBundle!=null)//之前已经加载过相同路径的资源
            {
                Object t_assetObj = t_info.GetAsset(name);

                if(t_assetObj!=null)
                {
#if Debug
                    Debug.Log("from cache");
#endif
                    if(progress!=null)
                    {
                        progress(path, name, 1f,args);
                    }

                    if(complete!=null)
                    {
                        if (IsNeedIncreaseRefCount(type, isInstance))
                            t_info.AddRefCount(scriptObject);

                        return ResCallBackReady(path, name, complete, t_assetObj, isInstance, args);
                    }

                    return null;
                }
                else
                {
                    if (IsNeedIncreaseRefCount(type, isInstance))
                        t_info.AddRefCount(scriptObject);

                    t_loader = new ResLoader();
                    t_loader.LoadFromAssetBundle(t_info.assetBundle, scriptObject,path, name, type, order, isInstance, progress, complete, args);
                    m_dictDownload[(int)order].Add(t_loader);
                    ++m_loaderCount;
                    return null;
                }
            }
            else//第一个加载此路径的资源包还未加载完成，第2次已经请求了
            {
                if (IsNeedIncreaseRefCount(type, isInstance))
                    t_info.AddRefCount(scriptObject);

                t_isNeedDownload = false;
            }
        }
        else//加载资源包
        {
            t_info = new ResInfo(path);
            m_dictLocalResource.Add(path,t_info);

            if (IsNeedIncreaseRefCount(type,isInstance))
                t_info.AddRefCount(scriptObject);
        }

        t_loader = new ResLoader();
        t_loader.LoadFromWWW(scriptObject, path, name, type, order, isInstance, t_isNeedDownload, progress,complete, args);
        m_dictDownload[(int)order].Add(t_loader);
        ++m_loaderCount;
        return null;
    }

    /// <summary>
    /// 获取缓存资源
    /// </summary>
    /// <param name="scriptObject"></param>
    /// <param name="path"></param>
    /// <param name="name"></param>
    /// <param name="isInstance">false为原始资源</param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public bool GetLocalResource(System.Object scriptObject, string path, string name, bool isInstance,ResourceType type,out Object obj)
    {
        ResInfo t_info;

        if (m_dictLocalResource.TryGetValue(path, out t_info))
        {
            if (t_info.assetBundle != null)//之前已经加载过相同路径的资源
            {
                Object t_assetObj = t_info.GetAsset(name);
            
                if (t_assetObj != null)
                {
                    if (IsNeedIncreaseRefCount(type, isInstance))
                        t_info.AddRefCount(scriptObject);

                    obj = ResCallBackReady(path, name, null, t_assetObj, isInstance);
                    return true;
                }
            }
        }

        obj = null;
        return false;
    }

    /// <summary>
    /// 判断是否需要增加引用计数
    /// 场景不需要缓存，因为会占用过大内存
    /// 文本类资源一般没有连续请求多次的情况，都是由各自管理器读取一次，所以不需要缓存,也不需要垃圾回收，每次同步读取的速度都很快
    /// </summary>
    /// <param name="type"></param>
    /// <param name="isInstance">是否实例化</param>
    /// <returns></returns>
    private bool IsNeedIncreaseRefCount(ResourceType type, bool isInstance)
    {
        bool t_isNeedIncreaseRefCount= isInstance&&!(type == ResourceType.map_pak || type == ResourceType.map || type == ResourceType.table||type==ResourceType.script);
        return t_isNeedIncreaseRefCount;
    }

    /// <summary>
    /// 回收实例化的游戏对象，或整个资源包
    /// 如果想中断加载，则让obj参数为null即可
    /// </summary>
    /// <param name="scriptObject"></param>
    /// <param name="path"></param>
    /// <param name="obj">实例化后的对象，如果为空则清除该path下相关资源</param>
    public void DiscardUsage(System.Object scriptObject, string path, Object obj=null)
    {
        if (!string.IsNullOrEmpty(path) && !m_dictLocalResource.ContainsKey(path))
        {
            return;
        }

        ResInfo t_resInfo = null;

        if (obj != null)
        {
            int t_objID = obj.GetInstanceID();

            if (m_dictGameObjectRes.ContainsKey(t_objID))
            {
                t_resInfo = m_dictGameObjectRes[t_objID];
                m_dictGameObjectRes.Remove(t_objID);
                t_resInfo.DecGameObjectRef(obj as GameObject);
                t_resInfo.DecRefCount(scriptObject);
                Destroy(obj);
            }
            else
            {
                Debug.LogWarning("discardusage obj not created by InitLocalResource:" + path);
                return;
            }
        }
        else
        {
            t_resInfo = m_dictLocalResource[path];
        }

        if (t_resInfo.CanRelease)
        {
  //          MDebug.Log(t_resInfo.ResPath + " is released");
            t_resInfo.Release(true);
            m_dictLocalResource.Remove(path);
        }
    }

    /// <summary>
    /// 中断所有加载
    /// </summary>
    public void StopAllLoading(bool destory = false)
    {
        for (int order = (int)Order.o_ui; order < (int)Order.o_max; order++)
        {
            List<ResLoader> t_list = m_dictDownload[order];

            foreach (var item in t_list)
            {
                item.Release(destory);
            }

            t_list.Clear();
        }
    }

    /// <summary>
    /// 销毁所有资源包，中断所有加载
    /// </summary>
    public void Release(bool destory=false)
    {
        foreach (KeyValuePair<string, ResInfo> kv in m_dictLocalResource)
        {
            kv.Value.Release(true);
        }

        m_dictLocalResource.Clear();
        m_dictGameObjectRes.Clear();
        StopAllLoading(destory);
        m_loaderCount = 0;

        if(destory)
        {
            m_dictLocalResource = null;
            m_dictGameObjectRes = null;
        }
    }

    /// <summary>
    /// 当前加载器数目
    /// </summary>
    public int LoadingCount{get{ return m_loaderCount; } }

    void Awake()
    {
        m_dictLocalResource = new Dictionary<string, ResInfo>();
        m_dictGameObjectRes = new Dictionary<int, ResInfo>();

        for (int order = (int)Order.o_ui; order < (int)Order.o_max; order++)
        {
            m_dictDownload[order] = new List<ResLoader>();
        }
    }

	void Update ()
    {
        if (m_loaderCount == 0)
            return;

        ResInfo t_info=null;

        for (int order = 0; order < (int)Order.o_max; order++)
        {
            List<ResLoader> t_downloadList = m_dictDownload[order];
            ResLoader t_loader=null;
            int t_loaderListCount = t_downloadList.Count;
            int t_toDeleteCount = 0;//要删除的加载器个数

            for(int i=0;i< t_loaderListCount - t_toDeleteCount;)
            {
                t_loader = t_downloadList[i];

                if (!m_dictLocalResource.ContainsKey(t_loader.SrcPath))
                {
                    Debug.LogWarning(t_loader.SrcPath + " already discard from loading list");
                    t_loader.Release(true);
                    CollectionUtil.SwapDestroyObject<ResLoader>(t_downloadList, i, t_loaderListCount - t_toDeleteCount - 1);//把错误的loader都移动到尾部
                    ++t_toDeleteCount;
                    continue;
                }

                t_info = m_dictLocalResource[t_loader.SrcPath];

                if (t_loader.LoadingType == LoadType.fromWWW)
                {
                    if (t_loader.IsNeedDownload)
                    {
                        if(t_loader.HasWWWError)
                        {
                            //   Debug.LogError(t_loader.SrcPath + ":loaded error");
                         //   MDebug.Log(t_loader.SrcPath + ":loaded error");
                            t_loader.Release(true);
                            CollectionUtil.SwapDestroyObject<ResLoader>(t_downloadList,i, t_loaderListCount - t_toDeleteCount - 1);//把错误的loader都移动到尾部
                            ++t_toDeleteCount;
                            continue;
                        }
                    }
                    else
                    {
                        ++i;

                        if (t_info.assetBundle == null)//正在下载的加载器还没完成下载
                            continue;

                        t_loader.LoadFromAssetBundle(t_info.assetBundle);//下一个优先级里的等待loader可能并不会被本次回调操作遍历到，所以需要转换，并且在下一帧判断加载完成
                    }
                }
                else
                {

                }

                if(t_loader.ProgressCallback!=null)
                {
                    t_loader.ProgressCallback(t_loader.SrcPath, t_loader.SrcName, t_loader.Progress, t_loader.CustomData);
                }

                if (t_loader.IsComplete)
                {
                //    MDebug.Log(t_info.ResPath + " is loaded");
                    ProcessLoaded(t_info, t_loader, t_downloadList,i,ref t_toDeleteCount);
                    continue;
                }

                ++i;
            }

            if (t_toDeleteCount > 0)
            {
                t_downloadList.RemoveRange(t_loaderListCount - t_toDeleteCount, t_toDeleteCount);
                 m_loaderCount -= t_toDeleteCount;
            }
        }   
	}

    /// <summary>
    /// 执行此方法的条件有2个
    /// www方式并且需要下载，如果是不同名的不需要下载的情况，那此时一定存在一个需要下载的加载器，需要等到需要加载的加载器加载完成对(resInfo.assetBundle的赋值
    /// assetbundle方式
    /// </summary>
    /// <param name="resInfo"></param>
    /// <param name="loaded"></param>
    /// <param name="loadingList"></param>
    /// <param name="loadedIndex">要交换的loader索引</param>
    /// <param name="deleteCount">之前因为错误要删除的个数，只遍历此处之前的loader,主要需要删除就增加</param>
    /// <returns>true表示游戏业务层有错误，需要删除所有对该资源包的加载器</returns>
    private bool ProcessLoaded(ResInfo resInfo, ResLoader loaded, List<ResLoader> loadingList,int loadedIndex,ref int deleteCount)
    {
        if (resInfo.assetBundle == null)
        {
            resInfo.assetBundle = loaded.AssetBundleRef;
            resInfo.bytes = loaded.Loaded;
            MemUtil.AddDownloadDic(loaded.SrcPath, loaded.Loaded);
        }

        int t_realCount = loadingList.Count;

        try
        {
            ProcessLoadedOfType(resInfo, loaded, loadingList, true, false);//先缓存已加载完成的资源
        }
        catch (System.Exception e)
        {
        //    MDebug.Log(loaded.SrcName + e.StackTrace.ToString()+"error");
            loaded.Release(true);
            CollectionUtil.SwapDestroyObject<ResLoader>(loadingList, loadedIndex, t_realCount - deleteCount - 1);//先移动到deleteCount之前的位置
            ++deleteCount;
            return true;
        }

        CollectionUtil.SwapDestroyObject<ResLoader>(loadingList, loadedIndex, t_realCount - deleteCount - 1);//先移动到deleteCount之前的位置
        ++deleteCount;

        for (int i = 0; i < t_realCount - deleteCount; )
        {
            //将已加载完成的资源信息同步到等待中的加载器
            if (loadingList[i].SrcPath == loaded.SrcPath)
            {
                loadingList[i].NormalRequest = loaded.NormalRequest;

                try
                {
                    ProcessLoadedOfType(resInfo, loadingList[i], loadingList, false, true);
                }
                catch (System.Exception e)
                {
                    loadingList[i].Release(true);
                    CollectionUtil.SwapDestroyObject<ResLoader>(loadingList, i, t_realCount - deleteCount - 1);
                    ++deleteCount;
            //        MDebug.Log(loaded.SrcName + e.StackTrace.ToString() + "error");
                    return true;
                }

                CollectionUtil.SwapDestroyObject<ResLoader>(loadingList, i, t_realCount - deleteCount - 1);
                ++deleteCount;
            }
            else
            {
                ++i;
            }
        }

        loaded.Release(false);
        return false;
    }

    /// <summary>
    /// 处理不同类型资源
    /// 进入此方法的都是同名资源，不同名的资源请求要等到下一帧处理
    /// </summary>
    /// <param name="resInfo">资源包</param>
    /// <param name="loaded">已经加载完成的loader</param>
    /// <param name="loadingList"></param>
    /// <param name="isCache">是否缓存</param>
    /// <param name="isRemove">是否移除</param>
    private void ProcessLoadedOfType(ResInfo resInfo, ResLoader loaded, List<ResLoader> loadingList,bool isCache,bool isRemove)
    {
        switch (loaded.SrcType)
        {
            case ResourceType.map_pak://场景不需要缓存，占用内存过多，此时回调后需要调用DiscardUsage方法清除缓存
                {
                    if (loaded.HasScript && !loaded.ScriptObject.Equals(null))
                        ResCallBackReady(loaded.SrcPath, loaded.SrcName, loaded.CompleteCallback, null, loaded.IsInstanate, loaded.CustomData);

                //    DiscardUsage(loaded.ScriptObject, loaded.SrcPath);
                    System.GC.Collect();
                }
                break;
            case ResourceType.map://文本类资源一般没有连续请求多次的情况，都是由各自管理器读取一次，所以不需要缓存
            case ResourceType.table:
                {
                    if (loaded.HasScript && !loaded.ScriptObject.Equals(null))
                        ResCallBackReady(loaded.SrcPath, loaded.SrcName, loaded.CompleteCallback, resInfo.assetBundle.LoadAsset(loaded.SrcName,ResLoader.GetSystemType(loaded.SrcType)), loaded.IsInstanate, loaded.CustomData);
                }
                break;
            default://脚本也需要缓存
                {
                    if (isCache&&!resInfo.HasAsset(loaded.SrcName))
                    {
                        if(loaded.NormalRequest.asset != null)
                        {
                            resInfo.AddAsset(loaded.SrcName, loaded.NormalRequest.asset);
                        }
                        else
                        {
#if Debug
                            Debug.Log("无此资源");
#endif
                        }

                    }

                    if (loaded.HasScript && !loaded.ScriptObject.Equals(null))
                    {
                        ResCallBackReady(loaded.SrcPath, loaded.SrcName, loaded.CompleteCallback, resInfo.GetAsset(loaded.SrcName), loaded.IsInstanate, loaded.CustomData);
                    }
                }
                break;
        }

        if(isRemove)
        {
            loaded.Release(false);
        }
    }

    /// <summary>
    /// 加载完成回调
    /// </summary>
    /// <param name="path"></param>
    /// <param name="name"></param>
    /// <param name="callback"></param>
    /// <param name="obj">当为场景时可以为null</param>
    /// <param name="isInstantiate"></param>
    /// <param name="args"></param>
    private Object ResCallBackReady(string path,string name, LoadCompleteCallback callback,Object obj,bool isInstantiate, params string[] args)
    {
        if (isInstantiate)
        {
            if (obj == null)
                return null;

            Object t_obj = GameObject.Instantiate(obj);
            m_dictLocalResource[path].AddGameObjectRef(t_obj as GameObject);
            m_dictGameObjectRes[t_obj.GetInstanceID()] = m_dictLocalResource[path];

            if (callback != null)
                callback(path, name, t_obj, args);

            return t_obj;
        }
        else
        {
            if (callback != null)
                callback(path, name, obj, args);

            return obj;
        }
    }
}

public enum Order
{
    o_ui,
    o_default,
    o_back,
    o_max
}
