using UnityEngine;
using System.Collections;

namespace BundleLoader
{
    ///<summary>
    ///类主要功能：资源加载项
    ///注意事项：
    ///创建日期：2015/8/7
    ///修改日期：2015/9/19
    ///修改人：lijunfeng
    /// </summary>
    public sealed class ResLoader:IRelease
    {
        private bool m_hasScript = false;
        private System.Object m_scriptObj;//是否有脚本调用
        private string m_srcPath;
        private string m_srcName;
        private ResourceType m_type;
        private Order m_order;//加载顺序
        private bool m_isInstance;//是否实例化
        private bool m_isNeedDownload=true;//是否需要下载，如果只用来保存信息，则为false,主要用来等待同路径的正在下载的资源
        private LoadingProgressCallback m_progressCallback = null;
        private LoadCompleteCallback m_completeCallback = null;
        private string[] m_customData;

        private WWW m_Www = null;
        private AssetBundle m_assetBundle = null;
        private AssetBundleRequest m_loaderRequest = null;//异步加载请求
        private AsyncOperation m_LoaderOperation = null;//异步加载场景

        private int m_loaded = 0;
        private bool m_isLoading = false;
        private LoadType m_loadType = LoadType.fromWWW;
        private bool m_isDestroyed = false;

        public ResLoader()
        {
        }

        /// <summary>
        /// 开始加载
        /// </summary>
        /// <param name="scriptObj">请求的脚本</param>
        /// <param name="srcPath">资源路径</param>
        /// <param name="srcName">资源名</param>
        /// <param name="type">ResourceType</param>
        /// <param name="order">Order</param>
        /// <param name="isInstance">是否实例化</param>
        /// <param name="isDownload">是否需要下载</param>
        /// <param name="progress">进度回调</param>
        /// <param name="complete">结束回调</param>
        /// <param name="args">自定义参数</param>
        public void LoadFromWWW(System.Object scriptObj, string srcPath, string srcName, ResourceType type, Order order, bool isInstance, bool isDownload,LoadingProgressCallback progress,LoadCompleteCallback complete, params string[] args)
        {
            if (scriptObj != null)
            {
                m_scriptObj = scriptObj;
                m_hasScript = true;
            }
            else
            {
                m_hasScript = false;
            }

            m_scriptObj = scriptObj;
            m_srcPath = srcPath;
            m_srcName = srcName;
            m_type = type;
            m_order = order;
            m_isInstance = isInstance;
            m_isNeedDownload = isDownload;
            m_progressCallback = progress;
            m_completeCallback = complete;
            m_customData = args;
            m_isDestroyed = false;

            if (m_Www == null)
            {
                if(m_isNeedDownload)
                {
                    m_Www = new WWW(GetPath(m_srcPath));
                    m_isLoading = true;
                }
            }

            m_loadType = LoadType.fromWWW;
        }

        /// <summary>
        /// 从本地缓存中加载
        /// </summary>
        /// <param name="assetBundle"></param>
        /// <param name="scriptObj"></param>
        /// <param name="srcPath">资源路径</param>
        /// <param name="srcName"></param>
        /// <param name="type"></param>
        /// <param name="order"></param>
        /// <param name="isInstance"></param>
        /// <param name="progress">进度回调</param>
        /// <param name="complete"></param>
        /// <param name="args"></param>
        public void LoadFromAssetBundle(AssetBundle assetBundle, System.Object scriptObj, string srcPath, string srcName, ResourceType type, Order order, bool isInstance, LoadingProgressCallback progress, LoadCompleteCallback complete, params string[] args)
        {
            m_assetBundle = assetBundle;

            if (scriptObj != null)
            {
                m_scriptObj = scriptObj;
                m_hasScript = true;
            }
            else
            {
                m_hasScript = false;
            }

            m_scriptObj = scriptObj;
            m_srcPath = srcPath;
            m_srcName = srcName;
            m_type = type;
            m_order = order;
            m_isInstance = isInstance;
            m_progressCallback = progress;
            m_completeCallback = complete;
            m_customData = args;
            m_isDestroyed = false;
            m_isLoading = true;
            m_loadType = LoadType.fromAssetBundle;
        }

        /// <summary>
        /// 由非下载加载器转变为bundle加载器
        /// </summary>
        /// <param name="assetBundle"></param>
        public void LoadFromAssetBundle(AssetBundle assetBundle)
        {
            m_assetBundle = assetBundle;
            m_loadType = LoadType.fromAssetBundle;
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

                if(m_loadType==LoadType.fromWWW)
                {
                    if (m_Www == null)
                        return false;

                    if (!string.IsNullOrEmpty(m_Www.error))
                    {
                        Debug.LogError(string.Format("{0} {1}", m_Www.url, m_Www.error));
                        m_isLoading = false;
                        return true;
                    }

                    m_loaded = m_Www.bytesDownloaded;

                    if (m_assetBundle == null && m_Www.isDone)
                    {
                        m_assetBundle = m_Www.assetBundle;
                    }

                    return CheckComplete(); 
                }
                else //fromAssetBundle
                {
                    return CheckComplete();
                }
            }
        }

        /// <summary>
        /// 处理异步加载完成判断
        /// </summary>
        /// <returns></returns>
        private bool CheckComplete()
        {
            if (m_assetBundle == null)
                return false;

            switch (m_type)
            {
                case ResourceType.map:
                case ResourceType.table:
                case ResourceType.script:
                    m_isLoading = false;
                    return true;
                case ResourceType.map_pak:
                    if (m_LoaderOperation == null)
                    {
                        m_LoaderOperation = Application.LoadLevelAsync(m_srcName);
                    }
                    break;
                default:
                    if (m_loaderRequest == null)
                    {
                        m_loaderRequest = m_assetBundle.LoadAssetAsync(m_srcName, GetSystemType(m_type));
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
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        private IEnumerator LoadLevelAsync()
        {
            m_LoaderOperation = Application.LoadLevelAsync(m_srcName);

            yield return m_LoaderOperation;
        }

        public static System.Type GetSystemType(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.audio:
                    return typeof(AudioClip);
                case ResourceType.gameobject:
                    return typeof(GameObject);
                case ResourceType.texture:
                    return typeof(Texture2D);
                case ResourceType.material:
                    return typeof(Material);
                case ResourceType.controller:
                    return typeof(RuntimeAnimatorController);
                case ResourceType.map:
                case ResourceType.table:
                case ResourceType.script:
                    return typeof(TextAsset);
                case ResourceType.map_pak:
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
            string t_path = "file:///" + Application.streamingAssetsPath + "/" + path;
    #elif UNITY_STANDALONE_WIN
            string t_path = Application.streamingAssetsPath + "/" + path;
    #elif UNITY_IPHONE
            string t_path = Application.dataPath + "/Raw/" + path;
    #elif UNITY_ANDROID
            string t_path = "jar:file://"+Application.dataPath + "!/assets/" + path;
    #endif
            return t_path;
        }

        public bool IsLoading{ get{return m_isLoading;}}

        /// <summary>
        /// 是否有网络错误
        /// </summary>
        public bool HasWWWError
        {
            get
            {
                return m_Www != null && !string.IsNullOrEmpty(m_Www.error);
            }
        }

        /// <summary>
        /// 是否有脚本引用
        /// </summary>
        public bool HasScript{ get { return m_hasScript; }}

        /// <summary>
        /// 引用的脚本
        /// </summary>
        public System.Object ScriptObject { get { return m_scriptObj; }}

        /// <summary>
        /// 资源路径
        /// </summary>
        public string SrcPath { get { return m_srcPath; } }

        public string SrcName { get { return m_srcName; } }

        public ResourceType SrcType { get { return m_type; } }

        public Order LoadOrder { get { return m_order; } }

        public bool IsInstanate { get { return m_isInstance; }}

        /// <summary>
        /// 是否需要下载，只有WWW方式时才关心此问题
        /// </summary>
        public bool IsNeedDownload { get { return m_isNeedDownload; } }

        public string[] CustomData { get { return m_customData; } }

        public LoadingProgressCallback ProgressCallback { get { return m_progressCallback; } }

        public LoadCompleteCallback CompleteCallback { get { return m_completeCallback; } }

        /// <summary>
        /// 普通异步加载请求
        /// </summary>
        public AssetBundleRequest NormalRequest
        {
            get { return m_loaderRequest; }
            set { m_loaderRequest=value; }
        }

        /// <summary>
        /// 异步加载场景请求
        /// </summary>
        public AsyncOperation LevelRequest {get { return m_LoaderOperation; }}

        /// <summary>
        /// 资源库引用
        /// </summary>
        public AssetBundle AssetBundleRef { get { return m_assetBundle; } }

        /// <summary>
        /// 加载类型
        /// </summary>
        public LoadType LoadingType { get { return m_loadType; } }

        /// <summary>
        /// 已加载的字节
        /// </summary>
        public int Loaded {get { return m_loaded; }}

        /// <summary>
        /// 获取当前进度 1为100%
        /// </summary>
        public float Progress
        {
            get
            {
                switch (m_type)
                {
                    case ResourceType.map:
                    case ResourceType.table:
                        m_isLoading = false;
                        return m_Www==null?1f:m_Www.progress;
                    case ResourceType.map_pak:
                   
                        if(m_LoaderOperation!=null)
                        {
                            if(m_LoaderOperation.isDone)
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
        public void Release(bool destory=false)
        {
            m_loaded = 0;
            m_isLoading = false;
            m_isDestroyed = true;
 
            m_assetBundle = null;

            if (m_Www != null)
            {
                if (destory && string.IsNullOrEmpty(m_Www.error) && m_Www.assetBundle != null)
                    m_Www.assetBundle.Unload(true);

                m_Www.Dispose();
                m_Www = null;
            }

            m_progressCallback = null;
            m_completeCallback = null;
            m_customData = null;
        }
    }

    public enum ResourceType
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

    public enum LoadType
    {
        fromWWW,
        fromAssetBundle
    }
}


