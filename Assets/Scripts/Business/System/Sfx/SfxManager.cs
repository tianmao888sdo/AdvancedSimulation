using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 类主要功能：全局音频管理器
/// 注意事项：所有音效分为loop和非loop，loop的需要手动销毁，不受音效时长控制，非loop的会受实际设置的时间或音效实际时间销毁
/// 创建日期：2015.9.9
/// 最后修改日期：2015.11.23
/// 最后修改人（包括创建人）：lijunfeng
/// </summary>
public sealed class SfxManager
{
    struct RequestInfo:IRelease
    {
        public int id;
        public GameObject target;//请求目标
        public bool loop;//是否循环播放
        public float volume;
        public float delay;
        public float duration;//时长

        public void Release(bool destroy = false)
        {
            target = null;
        }
    }

    private static SfxManager _instance = null;
    private List<AudioSource> m_audios = null;
    private List<string> m_preloadResource = null;//记录预加载的资源，用于将来销毁
    private Dictionary<string, RequestInfo> m_requestQueue = null;//请求队列

    public SfxManager()
    {

    }

    public static SfxManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SfxManager();

            return _instance;
        }
    }

    public void Init()
    {
        m_audios = new List<AudioSource>();
        m_preloadResource = new List<string>();
        m_requestQueue = new Dictionary<string, RequestInfo>();
    }

    /// <summary>
    /// 预加载资源
    /// </summary>
    /// <param name="id"></param>
    public void PreloadResource(int id)
    {
        SfxConfig t_config = ConfigManager.Instance.GetConfigByID<SfxConfig>(id);
        Global.It.LocalResMgr.GetLocalResource(this, t_config.path + GlobalConfig.BUNDLE_EXT_NAME, t_config.name, null, PreloadResourceReadyCallback, ResourceType.audio, Order.o_default, false);

        if (!m_preloadResource.Contains(t_config.path))
            m_preloadResource.Add(t_config.path);
    }

    private void PreloadResourceReadyCallback(string path, string name, UnityEngine.Object obj, params string[] arg)
    {
        m_preloadResource.Add(path);
    }

    /// <summary>
    /// 调节所有正在播放的声音大小
    /// </summary>
    /// <param name="volume"></param>
    public void SetVolume(float volume)
    {
        foreach(var item in m_audios)
        {
            item.volume= volume;
        }
    }

    public void Pause()
    {
        foreach (var item in m_audios)
        {
            item.Pause();
        }
    }

    public void Play()
    {
        foreach (var item in m_audios)
        {
            item.Play();
        }
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="id"></param>
    /// <param name="obj">AudioSource所在的对象</param>
    /// <param name="loop"></param>
    /// <param name="over">如果该对象上已经有音效,并且在播放中，是否覆盖</param>
    /// <param name="delay"></param>
    /// <param name="duration"></param>
    /// <param name="volume"></param>
    public void PlaySoundAt(int id, GameObject obj, bool loop = false,bool over=true,float delay = 0f, float duration = 0f, float volume = 1f)
    {
        AudioSource t_source = obj.GetComponent<AudioSource>();

        if (t_source == null)
            return;

        if (t_source.isPlaying && !over)
            return;

        SfxConfig t_config = ConfigManager.Instance.GetConfigByID<SfxConfig>(id);

        if (t_config == null)
            return;

        RequestInfo t_target;

        //如果obj已经存在另一个id的音乐请求，并且另一个请求正在加载中，则中断该加载
        if (m_requestQueue.TryGetValue(t_config.path, out t_target))
        {
            if (t_target.id != id)
                m_requestQueue.Remove(t_config.path);
        }

        UnityEngine.Object t_obj = null;

        if (Global.It.LocalResMgr.GetLocalResource(this, t_config.path + GlobalConfig.BUNDLE_EXT_NAME, t_config.name, false,ResourceType.audio, out t_obj))
        {
            t_source.clip = t_obj as AudioClip;
            t_source.loop = loop;
            t_source.volume = volume;
            t_source.PlayDelayed(delay);
            m_audios.Add(t_source);
        }
        else
        {
            if (m_requestQueue.ContainsKey(t_config.path + GlobalConfig.BUNDLE_EXT_NAME))//放置请求过快
                return;

            RequestInfo t_request = new RequestInfo();
            t_request.id = id;
            t_request.target = obj;
            t_request.loop = loop;
            t_request.delay = delay;
            t_request.duration = duration;
            t_request.volume = volume;
            m_requestQueue.Add(t_config.path + GlobalConfig.BUNDLE_EXT_NAME, t_request);
            Global.It.LocalResMgr.GetLocalResource(this, t_config.path + GlobalConfig.BUNDLE_EXT_NAME, t_config.name, null, ResourceReadyCallback, ResourceType.audio, Order.o_default, false);
        }
    }

    private void ResourceReadyCallback(string path, string name, UnityEngine.Object obj, params string[] arg)
    {
        RequestInfo t_request;

        if (m_requestQueue.TryGetValue(path, out t_request))
        {
            if (t_request.target==null||!t_request.target.activeSelf)
            {
                t_request.target = null;
                m_requestQueue.Remove(path);
                return;
            }

            AudioSource t_source = t_request.target.GetComponent<AudioSource>();

            if (t_source == null)
            {
                t_request.target = null;
                m_requestQueue.Remove(path);
                return;
            }

            t_source.clip = obj as AudioClip;
            t_source.loop = t_request.loop;
            t_source.volume = t_request.volume;
            t_source.PlayDelayed(t_request.delay);

            m_audios.Add(t_source);
            t_request.target = null;
            m_requestQueue.Remove(path);

            if (!m_preloadResource.Contains(path))
                m_preloadResource.Add(path);
        }
        else
        {
            //说明被下个音乐请求中断了，销毁资源
            Global.It.LocalResMgr.DiscardUsage(this, path);
        }
    }

    /// <summary>
    /// 销毁所有资源
    /// </summary>
    public void Release(bool destory = false)
    {
        foreach (var item in m_requestQueue)
        {
            item.Value.Release();
        }

        m_requestQueue.Clear();
        m_requestQueue = null;

        foreach (var item in m_audios)
        {
            if (item != null)
                item.Stop();
        }

        m_audios.Clear();
        m_audios = null;

        foreach (var item in m_preloadResource)
        {
            Global.It.LocalResMgr.DiscardUsage(this, item, null);
        }

        m_preloadResource.Clear();
        m_preloadResource = null;

        if (destory)
        {
            _instance = null;
        }
    }
}

