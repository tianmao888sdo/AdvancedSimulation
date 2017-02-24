using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 所有脚本基类，禁止使用Awake,Start
/// </summary>
public class MonoBase:MonoBehaviour,IRelease
{
    private bool m_isInit;

    public bool IsInit {
        get { return m_isInit; }
    }

    public virtual void Init()
    {
        m_isInit = true;
    }

    public virtual void DInit()
    {
        m_isInit = false;
    }

    private void Update()
    {
        if (!m_isInit)
            return;

        DUpdate();
    }

    private void FixedUpdate()
    {
        if (!m_isInit)
            return;

        DFixedUpdate();
    }

    private void LateUpdate()
    {
        if (!m_isInit)
            return;

        DLateUpdate();
    }

    protected virtual void DUpdate()
    {

    }

    protected virtual void DFixedUpdate()
    {

    }

    protected virtual void DLateUpdate()
    {

    }

    public virtual void Release(bool destroy = false)
    {
        m_isInit = false;
    }
}

public abstract class ScriptBase
{
	public abstract void Init();
	public abstract void Play ();
	public abstract void Stop();
}
