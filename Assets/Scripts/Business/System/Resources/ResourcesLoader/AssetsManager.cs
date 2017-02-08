using UnityEngine;
using System.Collections.Generic;

namespace ResourcesLoader
{
    public delegate void LoadCompleteCallback(string path, string name, Object obj, params string[] args);
    public delegate void LoadingProgressCallback(string path, string name, float progress, params string[] args);

    ///<summary>
    /// 主要功能：资源管理器，异步加载管理器
    /// 注意事项：非assetbundle加载器，只能加载Resources下的本地资源，可以加载任何类型的资源并缓存
    ///             场景不会返回对象资源，也不应该缓存
    ///             获取gameobject对象一定要实例化，不允许直接使用未实例化的原始对象
    ///             销毁对象时，必须用DiscardUsage方法回收，存在引用计数的资源，只有引用计数为0，并且未设置缓存，才可以销毁
    ///             加载的对象不存在多次尝试加载的情况，错误则忽略
    /// 创建日期：2016/12/22
    /// 修改日期：2016/12/23
    /// 修改人：lijunfeng
    ///<summary>
    public sealed class AssetsManager : MonoBehaviour, IRelease
    {
        private Dictionary<string, Asset> m_dictResource = new Dictionary<string, Asset>();//资源信息缓存
        private Dictionary<int, Asset> m_dictGameObjectRes = new Dictionary<int, Asset>();//Asset，以便用来销毁
        private Dictionary<string, AssetLoader> m_dictLoader = new Dictionary<string,AssetLoader>();//主加载器列表
        private Queue<AssetLoader> m_loaderPool = new Queue<AssetLoader>();//加载器缓存池

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
        /// <param name="isInstance">是否实例化</param>
        /// <param name="isCache">是否缓存资源，可实例化的资源在引用计数为0时也不销毁，不可实例化资源跟引用计数无关</param>
        /// <param name="args"></param>
        /// <returns>原始对象，如果能直接得到则为从缓存中取出</returns>
        public Object GetResource(System.Object scriptObject, string path, string name, LoadingProgressCallback progress, LoadCompleteCallback complete, AssetType type, bool isInstance, bool isCache, params string[] args)
        {
            Asset t_asset;
            AssetLoader t_loader;
            LoaderInfo t_loaderInfo;

            if (m_dictResource.TryGetValue(path, out t_asset))
            {
                if (t_asset.OriginalObj != null)
                {
                    if (progress != null)
                    {
                        progress(path, name, 1f, args);
                    }

                    if (complete != null)
                    {
                        if (IsNeedIncreaseRefCount(type, isInstance))
                            t_asset.AddRefCount(scriptObject);

                        return ResCallBackReady(path, name, t_asset.OriginalObj, isInstance, complete, args);
                    }

                    return null;
                }
                else//第一个加载此路径的资源包还未加载完成，第2次已经请求了
                {
                    if (IsNeedIncreaseRefCount(type, isInstance))
                        t_asset.AddRefCount(scriptObject);

                    t_loaderInfo = AssetLoader.CreateLoaderInfo(scriptObject, progress, complete, args);
                    m_dictLoader[path].Add(t_loaderInfo);
                }
            }
            else//加载资源包
            {
                t_asset = new Asset(name, path,isCache);
                m_dictResource.Add(path, t_asset);

                if (IsNeedIncreaseRefCount(type, isInstance))
                    t_asset.AddRefCount(scriptObject);

                t_loader = PoolFrom();
                t_loaderInfo = AssetLoader.CreateLoaderInfo(scriptObject, progress, complete, args);
                t_loader.Add(t_loaderInfo);
                t_loader.Load(path,name,type,isInstance,isCache);
                m_dictLoader.Add(path,t_loader);
            }

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
        public bool GetResource(System.Object scriptObject, string path, string name, bool isInstance, AssetType type, out Object obj)
        {
            Asset t_asset;

            if (m_dictResource.TryGetValue(path, out t_asset))
            {
                if (t_asset.OriginalObj != null)//之前已经加载过相同路径的资源
                {
                    if (IsNeedIncreaseRefCount(type, isInstance))
                        t_asset.AddRefCount(scriptObject);

                    obj = ResCallBackReady(path, name, t_asset.OriginalObj, isInstance, null);
                    return true;
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
        private bool IsNeedIncreaseRefCount(AssetType type, bool isInstance)
        {
            bool t_isNeedIncreaseRefCount = isInstance && !(type == AssetType.map_pak || type == AssetType.map || type == AssetType.table || type == AssetType.script);
            return t_isNeedIncreaseRefCount;
        }

        /// <summary>
        /// 回收实例化的游戏对象，或整个资源包
        /// 如果想中断加载，则让obj参数为null即可
        /// </summary>
        /// <param name="scriptObject"></param>
        /// <param name="path"></param>
        /// <param name="obj">实例化后的对象，如果为空则清除该path下相关资源</param>
        public void DiscardUsage(System.Object scriptObject, string path, Object obj = null)
        {
            if (!string.IsNullOrEmpty(path) && !m_dictResource.ContainsKey(path))
            {
                return;
            }

            Asset t_asset = null;

            if (obj != null)
            {
                int t_objID = obj.GetInstanceID();

                if (m_dictGameObjectRes.ContainsKey(t_objID))
                {
                    t_asset = m_dictGameObjectRes[t_objID];
                    m_dictGameObjectRes.Remove(t_objID);
                    t_asset.DecRefCount(scriptObject);
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
                t_asset = m_dictResource[path];
                StopLoading(scriptObject, path);
            }

            if (t_asset.CanRelease)
            {
                //          MDebug.Log(t_resInfo.ResPath + " is released");
                t_asset.Release(true);
                m_dictResource.Remove(path);
            }
        }

        /// <summary>
        /// 中断指定任务
        /// </summary>
        /// <param name="scriptObject"></param>
        /// <param name="path"></param>
        public void StopLoading(System.Object scriptObject, string path)
        {
            if (m_dictLoader.ContainsKey(path))
            {
                m_dictLoader[path].Remove(scriptObject);

                if(m_dictLoader[path].Count==0)
                {
                    m_dictLoader[path].Release(true);
                    PoolTo(m_dictLoader[path]);
                    m_dictLoader.Remove(path);
                }
            }
        }

        /// <summary>
        /// 中断所有加载
        /// </summary>
        public void StopAllLoading(bool destory = false)
        {
            foreach(var item in m_dictLoader)
            {
                item.Value.Release(destory);
                PoolTo(item.Value);
            }

            m_dictLoader.Clear();
        }

        /// <summary>
        /// 销毁所有资源包，中断所有加载
        /// </summary>
        public void Release(bool destory = false)
        {
            foreach (KeyValuePair<string, Asset> kv in m_dictResource)
            {
                kv.Value.Release(true);
            }

            m_dictResource.Clear();
            m_dictGameObjectRes.Clear();
            StopAllLoading(destory);
            m_loaderPool.Clear();

            if (destory)
            {
                m_dictResource = null;
                m_dictGameObjectRes = null;
                m_loaderPool = null;
                m_dictLoader = null;
            }
        }

        /// <summary>
        /// 当前加载器数目,包括加载器中的子任务
        /// </summary>
        public int LoadingCount
        {
            get
            {
                int count=0;

                foreach(var item in m_dictLoader)
                {
                    count += item.Value.Count;
                }
                return count;
            }
        }

        /// <summary>
        /// 从池中取出加载器
        /// </summary>
        /// <returns></returns>
        private AssetLoader PoolFrom()
        {
            if (m_loaderPool.Count > 0)
                return m_loaderPool.Dequeue();

            else
                return new AssetLoader();
        }

        /// <summary>
        /// 回收加载器
        /// </summary>
        /// <param name="loader"></param>
        private void PoolTo(AssetLoader loader)
        {
            if (loader.IsDestroyed)
                m_loaderPool.Enqueue(loader);
        }

        void Awake()
        {
            m_dictResource = new Dictionary<string, Asset>();
            m_dictGameObjectRes = new Dictionary<int, Asset>();
            m_dictLoader = new Dictionary<string, AssetLoader>();
        }

        void Update()
        {
            AssetLoader t_loader = null;
            string[] keys = new string[m_dictLoader.Count];
            m_dictLoader.Keys.CopyTo(keys, 0);//效率为每100个数据拷贝时间为0.4355毫秒，可以接受

            for (int i=0;i<keys.Length;i++)
            {
                t_loader = m_dictLoader[keys[i]];
                t_loader.ProgressCallback();

                if (t_loader.IsComplete)
                {
                    if (t_loader.Count != 0)
                        ProcessLoaded(t_loader);
                }
            }
        }

        /// <summary>
        /// 把每一帧加载到的资源保存到资源列表中
        /// 进入此方法的都是同名资源，不同名的资源请求要等到下一帧处理
        /// </summary>
        /// <param name="resInfo">资源包</param>
        /// <param name="loaded">已经加载完成的loader</param>
        /// <param name="loadingList"></param>
        private void ProcessLoaded(AssetLoader loaded)
        {
            Asset t_asset = m_dictResource[loaded.SrcPath];

            if (t_asset.OriginalObj == null)
                t_asset.OriginalObj = loaded.OraginalObj;

            LoaderInfo t_loaderInfo = loaded.Shift();
            ResCallBackReady(loaded.SrcPath, loaded.SrcName, loaded.OraginalObj, loaded.IsInstanate, t_loaderInfo.completeCallback, t_loaderInfo.customData);

            if (loaded.Count == 0)
            {
                m_dictLoader.Remove(loaded.SrcPath);
                PoolTo(loaded);
            }

            //非引用计数类型如果不缓存，则直接销毁
            if (t_asset.CanRelease)
            {
                t_asset.Release(true);
                m_dictResource.Remove(loaded.SrcPath);
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
        private Object ResCallBackReady(string path, string name, Object obj, bool isInstantiate, LoadCompleteCallback callback, params string[] args)
        {
            if (isInstantiate)
            {
                Object t_obj=null;

                if (obj!=null)
                {
                    t_obj = GameObject.Instantiate(obj);
                    m_dictGameObjectRes[t_obj.GetInstanceID()] = m_dictResource[path];
                }

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
}
