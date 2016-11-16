using System;
using System.Collections.Generic;

///<summary>
///资源管理器，栈缓存
///注意事项：
///创建日期：2015/9/10
///修改日期：2015/9/21
///修改人：lijunfeng
///<summary>
public class StackPoolManager<T>:IRelease
{
    private Stack<T> m_stack = new Stack<T>();
    private Func<T> m_createInstance=null;
    private Action<T> m_onPoolTo = null;//回收前要做的处理

    /// <summary>
    /// 
    /// </summary>
    /// <param name="initialReserve">初始预存个数</param>
    /// <param name="createInstance">实例化方法</param>
    public StackPoolManager(int initialReserve = 0, Func<T> createInstance =null, Action<T> onPoolTo =null)
    {
        m_createInstance = createInstance;
        m_onPoolTo = onPoolTo;

        if (initialReserve>0&&m_createInstance == null)
            return;

        for (int i = 0; i < initialReserve; i++)
        {
            m_stack.Push(m_createInstance());
        }
    }

    /// <summary>
    /// 获取
    /// </summary>
    /// <param name="autoCreate">是否自动创建</param>
    /// <returns></returns>
    public T PoolFrom(bool autoCreate=false)
    {
        if (m_stack.Count > 0)
        {
            return m_stack.Pop();
        }
        else
        {
            if(autoCreate&& m_createInstance!=null)
            {
                return m_createInstance();
            }
        }

        return default(T);
    }

    /// <summary>
    /// 回收
    /// </summary>
    public void PoolTo(T obj)
    {
        if (m_onPoolTo != null)
            m_onPoolTo(obj);

        m_stack.Push(obj);
    }

    /// <summary>
    /// 回收
    /// </summary>
    public void PoolTo(IEnumerable<T> list)
    {
        foreach(var item in list)
        {
            if (m_onPoolTo != null)
                m_onPoolTo(item);

            m_stack.Push(item);
        }
    }

    public void Release(bool destroy = false)
    {
        m_stack.Clear();

        if (destroy)
        {
            m_stack = null;
            m_createInstance = null;
            m_onPoolTo = null;
        }
    }
}

///<summary>
///资源管理器，缓存
///注意事项：
///创建日期：2015/9/10
///修改日期：2015/9/21
///修改人：lijunfeng
///<summary>
public class ListPoolManager<T> : IRelease
{
    private List<T> m_list = new List<T>();
    private Func<T> m_createInstance = null;
    private Action<T> m_onPoolTo = null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="initialReserve">初始预存个数</param>
    /// <param name="createInstance">实例化方法</param>
    public ListPoolManager(int initialReserve = 0, Func<T> createInstance = null, Action<T> onPoolTo = null)
    {
        m_createInstance = createInstance;
        m_onPoolTo = onPoolTo;

        if (initialReserve > 0 && m_createInstance == null)
            return;

        for (int i = 0; i < initialReserve; i++)
        {
            m_list.Add(m_createInstance());
        }
    }

    /// <summary>
    /// 获取
    /// </summary>
    /// <param name="autoCreate">是否自动创建</param>
    /// <returns></returns>
    public T PoolFrom(bool autoCreate = false)
    {
        if (m_list.Count > 0)
        {
            T t= m_list[0];
            m_list.Remove(t);
            return t;
        }
        else
        {
            if (autoCreate && m_createInstance != null)
            {
                return m_createInstance();
            }
        }

        return default(T);
    }

    /// <summary>
    /// 回收
    /// </summary>
    public void PoolTo(T obj)
    {
        if (m_onPoolTo != null)
            m_onPoolTo(obj);

        m_list.Add(obj);
    }

    /// <summary>
    /// 回收
    /// </summary>
    public void PoolTo(IEnumerable<T> list)
    {
        foreach (var item in list)
        {
            if (m_onPoolTo != null)
                m_onPoolTo(item);
        }

        m_list.AddRange(list);
    }

    public void Release(bool destroy = false)
    {
        m_list.Clear();

        if (destroy)
        {
            m_list = null;
            m_createInstance = null;
            m_onPoolTo = null;
        }
    }
}