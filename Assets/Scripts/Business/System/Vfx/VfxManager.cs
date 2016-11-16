using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 类主要功能：全局粒子特效管理器
/// 注意事项：切换场景时，需要清除缓存的粒子特效
/// 创建日期：2015.9.9
/// 最后修改日期：2015.10,20
/// 最后修改人（包括创建人）：lijunfeng
/// </summary>
public sealed class VfxManager:IRelease
{
    private static VfxManager _instance = null;

    private List<BattleEffect> m_effects = null;
    private Dictionary<int, Stack<BattleEffect>> m_pool = null;
    private GameObject m_effectRoot = null;

    public VfxManager()
    {

    }

    public static VfxManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new VfxManager();

            return _instance;
        }
    }

    public void Init()
    {
        if(m_pool==null)
            m_pool = new Dictionary<int, Stack<BattleEffect>>();

        if(m_effects==null)
            m_effects = new List<BattleEffect>();

        if(m_effectRoot==null)
            m_effectRoot = new GameObject("EffectRoot");
    }

    public List<QueueLoaderDesc> GetPreLoadList()
    {
        List<QueueLoaderDesc> t_list = new List<QueueLoaderDesc>();
        Dictionary<int, VfxConfig> t_config = ConfigManager.Instance.vfxConfig;

        foreach (var config in t_config)
        {
            t_list.Add(new QueueLoaderDesc(this, config.Value.path+ GlobalConfig.BUNDLE_EXT_NAME, config.Value.name, ResourceType.gameobject, Order.o_default, false));
        }

        return t_list;
    }

    /// <summary>
    /// 预加载资源
    /// </summary>
    /// <param name="id"></param>
    public bool PreloadResource(int id)
    {
        return false;
    }

    /// <summary>
    /// 播放完毕需要回收
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pos"></param>
    /// <param name="rotation"></param>
    /// <returns></returns>
    public BattleEffect CreateEffect(int id)
    {
        BattleEffect t_effect = GetFreeEffectByID(id);

        if(t_effect==null)
        {
            VfxConfig t_config = ConfigManager.Instance.GetConfigByID<VfxConfig>(id);
            UnityEngine.Object t_obj = null;

            if (Global.It.LocalResMgr.GetLocalResource(this, t_config.path + GlobalConfig.BUNDLE_EXT_NAME, t_config.name, true,ResourceType.gameobject, out t_obj))
            {
                t_effect = (t_obj as GameObject).AddComponent<BattleEffect>();
                t_effect.id = id;
                t_effect.transform.parent = m_effectRoot.transform;
                m_effects.Add(t_effect);
                return t_effect;
            }
        }

        return t_effect;
    }

    /// <summary>
    /// 在固定坐标播放非持续特效,播放完毕需要回收
    /// 播放时长无法控制
    /// 如果想更自由的控制特效请使用CreateEffect
    /// </summary>
    /// <param name="id"></param>
    /// <param name="pos"></param>
    public void PlayEffectAt(int id,Vector3 pos,Quaternion rotation)
    {
        BattleEffect t_effect = GetFreeEffectByID(id);

        if (t_effect == null)
        {
            VfxConfig t_config = ConfigManager.Instance.GetConfigByID<VfxConfig>(id);
            UnityEngine.Object t_obj = null;

            if (Global.It.LocalResMgr.GetLocalResource(this, t_config.path + GlobalConfig.BUNDLE_EXT_NAME, t_config.name, true,ResourceType.gameobject, out t_obj))
            {
                t_effect = (t_obj as GameObject).AddComponent<BattleEffect>();
                t_effect.id = id;
                t_effect.transform.parent = m_effectRoot.transform;
                m_effects.Add(t_effect);
            }
        }

        if(t_effect!=null)
        {
            t_effect.transform.position = pos;
            t_effect.transform.rotation = rotation;
            t_effect.duration=PlayAllParticlesInChildren(t_effect.gameObject);
            PlayAllAnimationInChildern(t_effect.gameObject);
        }
    }

    /// <summary>
    /// 回收特效
    /// </summary>
    /// <param name="effect"></param>
    public void RecycleEffect(BattleEffect effect)
    {
        effect.target = null;
        effect.offset = Vector3.zero;
        effect.gameObject.transform.position = Vector3.zero;
        ResetAllEffect(effect.gameObject);
        effect.gameObject.SetActive(false);
        m_effects.Remove(effect);
        Stack<BattleEffect> t_effects = null;

        if (!m_pool.TryGetValue(effect.id, out t_effects))
        {
            t_effects = new Stack<BattleEffect>();
            m_pool.Add(effect.id,t_effects);
        }

        t_effects.Push(effect);
    }

    /// <summary>
    /// 从缓存中获取特效
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public BattleEffect GetFreeEffectByID(int id)
    {
        Stack<BattleEffect> t_effects = null;

        if(m_pool.TryGetValue(id,out t_effects))
        {
            if(t_effects.Count>0)
            {
                BattleEffect t_obj = t_effects.Pop();
                t_obj.gameObject.SetActive(true);
                m_effects.Add(t_obj);
                return t_obj;
            }   
        }

        return null;
    }

    public void PauseAllEffect()
    {
        foreach(var item in m_effects)
        {
            ParticleSystem[] particles = item.GetComponentsInChildren<ParticleSystem>();

            foreach (ParticleSystem p in particles)
            {
                p.Pause();
            }
        }
    }

    public void UnPauseAllEffect()
    {
        foreach (var item in m_effects)
        {
            ParticleSystem[] particles = item.GetComponentsInChildren<ParticleSystem>();

            foreach (ParticleSystem p in particles)
            {
                p.Play();
            }
        }
    }

    public void ResetAllEffect(GameObject o)
    {
        ParticleSystem[] particles = o.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem p in particles)
        {
            p.Stop();
            p.Clear();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="o"></param>
    /// <param name="loop"></param>
    /// <param name="liftTime">生存时间，0表示不限制时间</param>
    /// <returns></returns>
    static public float PlayAllParticlesInChildren(GameObject o,bool loop=false,float liftTime=0)
    {
        float maxDuration = 0.0f;
        ParticleSystem[] particles = o.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem p in particles)
        {
            if(liftTime>0)
                p.startLifetime = liftTime;

            p.Stop();
            p.Clear();
            p.loop = loop;
            p.Play(false);

            float duration = 0.0f;

            if (p.emissionRate<=0)
            {
                duration = p.startDelay + p.startLifetime;
            }
            else
            {
                duration = p.startDelay + Mathf.Max(duration, p.startLifetime);
            }

            if (duration > maxDuration)
                maxDuration = duration;
        }

        return maxDuration;
    }

    static public float PlayAllAnimationInChildern(GameObject o)
    {
        float duration = 0.0f;
        Animation[] anims = o.GetComponentsInChildren<Animation>();
        foreach (Animation anim in anims)
        {
            anim.Play();
            if (anim.clip != null && duration < anim.clip.length)
                duration = anim.clip.length;
        }

        return duration;
    }

    /// <summary>
    /// 销毁所有资源
    /// </summary>
    public void Release(bool destory = false)
    {
        VfxConfig t_config = null;

        foreach (var item in m_effects)
        {
            t_config = ConfigManager.Instance.GetConfigByID<VfxConfig>(item.id);
            Global.It.LocalResMgr.DiscardUsage(this, t_config.path + GlobalConfig.BUNDLE_EXT_NAME, item.gameObject);
        }

        m_effects.Clear();
        m_effects = null;

        foreach (var item in m_pool)
        {
            foreach(var item2 in item.Value)
            {
                t_config = ConfigManager.Instance.GetConfigByID<VfxConfig>(item2.id);
                Global.It.LocalResMgr.DiscardUsage(this, t_config.path + GlobalConfig.BUNDLE_EXT_NAME, item2.gameObject);
            }

            item.Value.Clear();
        }

        m_pool.Clear();
        m_pool = null;

        Dictionary<int, VfxConfig> t_configs = ConfigManager.Instance.vfxConfig;

        foreach (var item in t_configs)
        {
            Global.It.LocalResMgr.DiscardUsage(this, item.Value.path + GlobalConfig.BUNDLE_EXT_NAME);
        }

        if (m_effectRoot != null)
        {
            GameObject.Destroy(m_effectRoot);
            m_effectRoot = null;
        }

        if (destory)
        {
            _instance = null;
        }
    }
}
