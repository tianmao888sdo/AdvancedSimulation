using UnityEngine;
using System.Collections.Generic;

public sealed class GameManager : MonoBehaviour,IRelease
{
    #region 单例
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    private List<IUpdate> m_updateList = null;
    private List<IUpdate> m_removedUpdateList = null;
    private List<IFixedUpdate> m_fixedUpdateList = null;
    private List<IFixedUpdate> m_removedFixedUpdateList = null;

    public void AddUpdater(IUpdate updater)
    {
        if (m_updateList == null)
            m_updateList = new List<IUpdate>() { updater };
        else
            m_updateList.Add(updater);
    }

    public void AddFixedUpdater(IFixedUpdate fixedUpdater)
    {
        if (m_fixedUpdateList == null)
            m_fixedUpdateList = new List<IFixedUpdate>() { fixedUpdater };
        else
            m_fixedUpdateList.Add(fixedUpdater);
    }

    public void RemoveUpdater(IUpdate updater)
    {
        if (m_updateList == null)
            return;

        if(m_updateList.Contains(updater))
        {
            if (m_removedUpdateList == null)
                m_removedUpdateList = new List<IUpdate>();

            m_removedUpdateList.Add(updater);
        }
    }

    public void RemoveFixedUpdater(IFixedUpdate fixedUpdater)
    {
        if (m_fixedUpdateList == null)
            return;

        if (m_fixedUpdateList.Contains(fixedUpdater))
        {
            if (m_removedFixedUpdateList == null)
                m_removedFixedUpdateList = new List<IFixedUpdate>();

            m_removedFixedUpdateList.Add(fixedUpdater);
        }
    }

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (m_updateList != null && m_updateList.Count > 0)
        {
            foreach (var item in m_updateList)
            {
                item.Update();
            }
        }

        if (m_removedUpdateList != null && m_removedUpdateList.Count > 0)
        {
            int t_toDeleteCount = 0;
            int t_listCount = m_updateList.Count;

            foreach (var item in m_removedUpdateList)
            {
                int t_index = m_updateList.IndexOf(item);
                CollectionUtil.SwapDestroyObject<IUpdate>(m_updateList, t_index, t_listCount - t_toDeleteCount - 1);
                t_toDeleteCount++;
            }

            m_updateList.RemoveRange(t_listCount - t_toDeleteCount, t_toDeleteCount);
            m_removedUpdateList.Clear();
        }
    }

    void FixedUpdate()
    {
        if (m_fixedUpdateList != null && m_fixedUpdateList.Count > 0)
        {
            foreach (var item in m_fixedUpdateList)
            {
                item.FixedUpdate();
            }
        }

        if(m_removedFixedUpdateList!=null&& m_removedFixedUpdateList.Count>0)
        {
            int t_toDeleteCount = 0;
            int t_listCount = m_fixedUpdateList.Count;

            foreach (var item in m_removedFixedUpdateList)
            {
                int t_index = m_fixedUpdateList.IndexOf(item);
                CollectionUtil.SwapDestroyObject<IFixedUpdate>(m_fixedUpdateList, t_index, t_listCount - t_toDeleteCount - 1);
                t_toDeleteCount++;
            }

            m_fixedUpdateList.RemoveRange(t_listCount - t_toDeleteCount, t_toDeleteCount);
            m_removedFixedUpdateList.Clear();
        }
    }

    public void Release(bool destroy = false)
    {
        if(m_updateList!=null)
            m_updateList.Clear();

        if(m_fixedUpdateList!=null)
            m_fixedUpdateList.Clear();

        if (m_removedUpdateList != null)
            m_removedUpdateList.Clear();

        if (m_removedFixedUpdateList != null)
            m_removedFixedUpdateList.Clear();
    }
}
