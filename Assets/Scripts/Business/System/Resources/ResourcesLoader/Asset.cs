using UnityEngine;
using System.Collections.Generic;
using System;

namespace ResourcesLoader
{
    ///<summary>
    ///类主要功能：信息包,对各种文件进行缓存
    ///注意事项：
    ///创建日期：2016/12/22
    ///修改日期：2016/12/23
    ///修改人：lijunfeng
    public class Asset : IRelease
    {
        public bool isCache = false;//当引用计数为0时是否自动删除，
        private UnityEngine.Object m_originalObj = null;//并缓存资源（未实例化的原始资源）包括各种类型
        private Dictionary<int, GameObject> m_refrenceObject;//实例化后的对象缓存，针对预制体类型
        private Dictionary<System.Object, int> m_refCount = new Dictionary<System.Object, int>();//本资源包的引用计数，针对预制体类型

        private string m_resName;
        private string m_resPath;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public Asset(string name, string path,bool isCache)
        {
            m_resName = name;
            m_resPath = path;
            this.isCache = isCache;
        }

        /// <summary>
        /// 原始资源获取设置原始资源
        /// </summary>
        public UnityEngine.Object OriginalObj
        {
            get
            {
                return m_originalObj;
            }
            set
            {
                m_originalObj = value;
            }
        }

        /// <summary>
        /// 增加引用计数
        /// </summary>
        /// <param name="sobj">引用的脚本</param>
        public void AddRefCount(System.Object sobj)
        {
            if (!m_refCount.ContainsKey(sobj))
            {
                m_refCount.Add(sobj, 1);
            }
            else
            {
                m_refCount[sobj]++;
            }
        }

        /// <summary>
        /// 减少引用计数
        /// </summary>
        /// <param name="sobj">引用的脚本</param>
        public void DecRefCount(System.Object sobj)
        {
            if (!m_refCount.ContainsKey(sobj))
            {
                //   Debug.LogError("Dec not exist script object");
            }
            else
            {
                m_refCount[sobj]--;

                if (m_refCount[sobj] == 0)
                    m_refCount.Remove(sobj);
            }
        }

        public string ResName { get { return m_resName; } }

        public string ResPath { get { return m_resPath; } }

        /// <summary>
        /// 如果当前有正在加载的资源，则不销毁
        /// </summary>
        public bool CanRelease
        {
            get { return m_refCount.Count == 0 && !isCache; }
        }

        /// <summary>
        /// 强制销毁，即使有加载器正在加载
        /// </summary>
        public void Release(bool destroy = false)
        {
            if (m_refrenceObject != null && m_refrenceObject.Count > 0)
            {
                foreach (KeyValuePair<int, GameObject> kv in m_refrenceObject)
                {
                    GameObject.Destroy(kv.Value);
                }

                m_refrenceObject.Clear();
            }

            Resources.UnloadUnusedAssets();
            m_refrenceObject = null;
            m_originalObj = null;
        }
    }
}
