using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace ResourcesLoader
{
    ///<summary>
    ///类主要功能：资源加载项
    ///注意事项：可以缓存使用，此加载器只加载相同路径的多个资源
    ///创建日期：2016/12/22
    ///修改日期：2016/12/23
    ///修改人：lijunfeng
    /// </summary>
    public sealed class AssetLoader : IRelease
    {
        private string m_srcPath;
        private string m_srcName;
        private AssetType m_type;
        private bool m_isInstance;//是否实例化
        private bool m_isCache;//是否缓存

        private Dictionary<System.Object, LoaderInfo> m_loaderInfoDic = null;
        private List<LoaderInfo> m_loaderInfoList = null;
        private Object m_originalObj;
        private ResourceRequest m_loaderRequest = null;//异步加载请求
        private AsyncOperation m_LoaderOperation = null;//异步加载场景

        private bool m_isLoading = false;
        private bool m_isDestroyed = false;

        public AssetLoader()
        {
        }

        /// <summary>
        /// 开始加载
        /// </summary>
        public void Load(string srcPath, string srcName, AssetType type, bool isInstance, bool isCache)
        {
            if (m_loaderInfoDic == null || m_loaderInfoDic.Count == 0)
                return;

            m_srcPath = srcPath;
            m_srcName = srcName;
            m_type = type;
            m_isInstance = isInstance;
            m_isCache = isCache;
            m_loaderRequest = Resources.LoadAsync(m_srcPath);
            m_isLoading = true;
        }

        /// <summary>
        /// 创建加载信息
        /// </summary>
        /// <param name="scriptObj">请求的脚本</param>
        /// <param name="srcPath">资源路径</param>
        /// <param name="srcName">资源名</param>
        /// <param name="type">ResourceType</param>
        /// <param name="isInstance">是否实例化</param>
        /// <param name="isCache">是否缓存资源，可实例化的资源在引用计数为0时也不销毁，不可实例化资源跟引用计数无关</param>
        /// <param name="progress">进度回调</param>
        /// <param name="complete">结束回调</param>
        /// <param name="args">自定义参数</param>
        public static LoaderInfo CreateLoaderInfo(System.Object scriptObj,LoadingProgressCallback progress, LoadCompleteCallback complete, params string[] args)
        {
            LoaderInfo t_loaderInfo = new LoaderInfo();

            t_loaderInfo.scriptObj = scriptObj;
            t_loaderInfo.progressCallback = progress;
            t_loaderInfo.completeCallback = complete;
            t_loaderInfo.customData = args;

            return t_loaderInfo;
        }

        /// <summary>
        /// 加入任务
        /// </summary>
        /// <param name="loader"></param>
        public void Add(LoaderInfo loaderInfo)
        {
            if (m_loaderInfoDic==null)
                m_loaderInfoDic = new Dictionary<System.Object, LoaderInfo>();

            if (m_loaderInfoList == null)
                m_loaderInfoList = new List<LoaderInfo>();

            m_loaderInfoDic.Add(loaderInfo.scriptObj, loaderInfo);
            m_loaderInfoList.Add(loaderInfo);
        }

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="scriptObj"></param>
        public void Remove(System.Object scriptObj)
        {
            m_loaderInfoList.Remove(m_loaderInfoDic[scriptObj]);
            m_loaderInfoDic.Remove(scriptObj);
        }

        /// <summary>
        /// 返回指定的任务
        /// </summary>
        /// <param name="scriptObj"></param>
        /// <returns></returns>
        public LoaderInfo this[System.Object key]
        {
            get
            {
                return m_loaderInfoDic[key];
            }
        }

        /// <summary>
        /// 返回指定索引的任务
        /// </summary>
        /// <param name="scriptObj"></param>
        /// <returns></returns>
        public LoaderInfo this[int index]
        {
            get
            {
                return m_loaderInfoList[index];
            }
        }

        /// <summary>
        /// 从前边冒出
        /// </summary>
        /// <returns></returns>
        public LoaderInfo Shift()
        {
            LoaderInfo t_loaderInfo = m_loaderInfoList[0];
            m_loaderInfoList.RemoveAt(0);
            m_loaderInfoDic.Remove(t_loaderInfo.scriptObj);
            return t_loaderInfo;
        }

        /// <summary>
        /// 加载任务数量
        /// </summary>
        public int Count { get { return m_loaderInfoDic != null ? m_loaderInfoDic.Count:0; } }

        /// <summary>
        /// 清理加载任务
        /// </summary>
        public void Clear(bool destroy)
        {
            if(m_loaderInfoDic!=null)
                m_loaderInfoDic.Clear(); 
            
            if(m_loaderInfoList!=null)
            {
                m_loaderInfoList.Clear();
            }             

            if(destroy)
            {
                m_loaderInfoDic = null;
                m_loaderInfoList = null;
            }
        }

        /// <summary>
        /// 检查是否加载完成
        /// 出错也视为加载完成
        /// isdownload=false的情况下不检查此条件
        /// 如果销毁了，返回false
        /// </summary>
        public bool IsComplete
        {
            get
            {
                if (m_isDestroyed)
                    return false;

                switch (m_type)
                {
                    case AssetType.map_pak:
                        if (m_LoaderOperation == null)
                        {
                            m_LoaderOperation = SceneManager.LoadSceneAsync(m_srcName);
                        }
                        break;
                }

                if (m_LoaderOperation != null)
                {
                    if (m_LoaderOperation.isDone)
                    {
                        m_isLoading = false;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                if (m_loaderRequest != null)
                {
                    if (m_loaderRequest.isDone)
                    {
                        m_isLoading = false;
                        m_originalObj = m_loaderRequest.asset;

                        if (m_originalObj == null)
                            Debug.LogError("no asset is loaded");

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// 执行进度回调
        /// </summary>
        public void ProgressCallback()
        {
            for(int i=0;i<m_loaderInfoList.Count;i++)
            {
                if(m_loaderInfoList[i].progressCallback!=null)
                {
                    m_loaderInfoList[i].progressCallback(m_srcPath, m_srcName, Progress, m_loaderInfoList[i].customData);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static System.Type GetSystemType(AssetType type)
        {
            switch (type)
            {
                case AssetType.audio:
                    return typeof(AudioClip);
                case AssetType.gameobject:
                    return typeof(GameObject);
                case AssetType.texture:
                    return typeof(Texture2D);
                case AssetType.material:
                    return typeof(Material);
                case AssetType.controller:
                    return typeof(RuntimeAnimatorController);
                case AssetType.map:
                case AssetType.table:
                case AssetType.script:
                    return typeof(TextAsset);
                case AssetType.map_pak:
                    return typeof(Terrain);
                default:
                    return System.Type.EmptyTypes[0];
            }
        }

        /// <summary>
        /// 获取资源地址
        /// 优先查找程序域目录资源，然后是streamingAssets目录，然后是Resources目录，最后从网络下载
        /// </summary>
        /// <param name="path">包含了文件名的路径</param>
        /// <returns></returns>
        private string GetPath(string path)
        {
#if UNITY_EDITOR
            //string t_path = "file:///" + Application.streamingAssetsPath + "/" + path;
            string t_path = Application.streamingAssetsPath + "/" + path;
#elif UNITY_STANDALONE_WIN
        string t_path = Application.streamingAssetsPath + "/" + path;
#elif UNITY_IPHONE
        string t_path = Application.dataPath + "/Raw/" + path;
#elif UNITY_ANDROID
        string t_path = "jar:file://"+Application.dataPath + "!/assets/" + path;
#endif
            return t_path;
        }

        /// <summary>
        /// 资源路径
        /// </summary>
        public string SrcPath { get { return m_srcPath; } }

        public string SrcName { get { return m_srcName; } }

        public AssetType SrcType { get { return m_type; } }

        public bool IsInstanate { get { return m_isInstance; } }

        public bool IsCache { get { return m_isCache; } }

        /// <summary>
        /// 资源库引用
        /// </summary>
        public Object OraginalObj { get { return m_originalObj; } }

        /// <summary>
        /// 异步加载场景请求
        /// </summary>
        public AsyncOperation LevelRequest { get { return m_LoaderOperation; } }

        /// <summary>
        /// 
        /// </summary>
        public bool IsLoading { get { return m_isLoading; } }

        /// <summary>
        /// 是否可回收
        /// </summary>
        public bool IsDestroyed { get { return m_isDestroyed; } }

        /// <summary>
        /// 获取当前进度 1为100%
        /// </summary>
        public float Progress
        {
            get
            {
                switch (m_type)
                {
                    case AssetType.map_pak:

                        if (m_LoaderOperation != null)
                        {
                            if (m_LoaderOperation.isDone)
                            {
                                return 1f;
                            }
                            else
                            {
                                return m_LoaderOperation.progress;
                            }
                        }
                        break;
                    default:
                        if (m_loaderRequest != null)
                        {
                            if (m_loaderRequest.isDone)
                            {
                                return 1f;
                            }
                            else
                            {
                                return m_loaderRequest.progress;
                            }
                        }
                        break;
                }

                return 0f;
            }
        }

        /// <summary>
        /// 回收后可以再利用
        /// </summary>
        /// <param name="destory"></param>
        public void Release(bool destory = false)
        {
            m_isLoading = false;
            m_isDestroyed = true;
            m_originalObj = null;
            m_loaderRequest = null;
            m_LoaderOperation = null;
            Clear(destory);
        }
    }

    /// <summary>
    /// 资源信息
    /// </summary>
    public struct LoaderInfo
    {
        public System.Object scriptObj;
        public LoadingProgressCallback progressCallback;
        public LoadCompleteCallback completeCallback;
        public string[] customData;
    }

    public enum AssetType
    {
        gameobject,//游戏对象（预制）
        map,//地图信息
        map_pak,//地图资源包
        table,//配置表
        texture,//纹理
        material,//材质
        script,//代码dll资源
        audio,//声音
        controller,//人物控制器
    }
}
