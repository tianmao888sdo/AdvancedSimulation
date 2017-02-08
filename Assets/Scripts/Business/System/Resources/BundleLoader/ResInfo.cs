using UnityEngine;
using System.Collections.Generic;
using System;

namespace BundleLoader
{
    ///<summary>
    ///类主要功能：信息包，每个包针对一个目录，会缓存实例化的游戏对象和原始游戏对象
    ///注意事项：
    ///创建日期：2015/8/7
    ///修改日期：2015/9/18
    ///修改人：lijunfeng
    public class ResInfo:IRelease
    {
        public int bytes = 0;
        public AssetBundle assetBundle=null;//所属资源包
        private Dictionary<int,GameObject> m_refrenceObject;//实例化后的对象缓存
        private Dictionary<System.Object, int> m_refCount = new Dictionary<System.Object, int>();//本资源包的引用计数
        private Dictionary<string, UnityEngine.Object> m_assetRequestDic = new Dictionary<string, UnityEngine.Object>();//并缓存资源（未实例化的原始资源）
        private string m_resPath;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        public ResInfo(string path)
        {
            m_resPath = path;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ab"></param>
        /// <param name="path"></param>
        public ResInfo(AssetBundle ab, string path) : this(path)
        {
            assetBundle = ab;
        }

        /// <summary>
        /// 根据名称和类型获取资源、
        /// 并缓存资源（未实例化的原始资源）
        /// </summary>
        /// <param name="resname"></param>
        /// <returns></returns>
        public UnityEngine.Object GetAsset(string resname)
        {
            if (assetBundle == null)
                return null;

            if(m_assetRequestDic==null)
            {
                m_assetRequestDic = new Dictionary<string, UnityEngine.Object>();
                return null;
            }

            UnityEngine.Object t_resRequest;

            if (m_assetRequestDic.TryGetValue(resname,out t_resRequest))
            {
                return m_assetRequestDic[resname];
            }

            return null;
        }

        /// <summary>
        /// 加入缓存
        /// </summary>
        /// <param name="resname"></param>
        /// <param name="obj"></param>
        public void AddAsset(string resname, UnityEngine.Object obj)
        {
            if (m_assetRequestDic == null)
            {
                m_assetRequestDic = new Dictionary<string, UnityEngine.Object>();
            }

            m_assetRequestDic.Add(resname, obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resname"></param>
        /// <returns></returns>
        public bool HasAsset(string resname)
        {
            if (m_assetRequestDic == null)
            {
                return false;
            }

            return m_assetRequestDic.ContainsKey(resname);
        }

        /// <summary>
        /// 保存实例化后对象的引用
        /// 其作用并不是缓存
        /// </summary>
        /// <param name="obj"></param>
        public void AddGameObjectRef(GameObject obj)
        {
            if (obj != null)
            {
                if (m_refrenceObject == null)
                    m_refrenceObject = new Dictionary<int, GameObject>();

                m_refrenceObject.Add(obj.GetInstanceID(),obj);
                MemUtil.AddCloneObjDic(m_resPath, bytes);
            }
        }

        /// <summary>
        /// 删除保存实例化后对象的引用
        /// </summary>
        /// <param name="obj"></param>
        public void DecGameObjectRef(GameObject obj)
        {
            if (m_refrenceObject != null)
            {
                m_refrenceObject.Remove(obj.GetInstanceID());
                GameObject.Destroy(obj);
                MemUtil.DelCloneObjDic(m_resPath, bytes);
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

        public string ResPath { get { return m_resPath; }}

        /// <summary>
        /// 如果当前有正在加载的资源，则不销毁
        /// </summary>
        public bool CanRelease
        {
            get { return m_refCount.Count == 0; }
        }

        /// <summary>
        /// 强制销毁，即使有加载器正在加载
        /// </summary>
        public void Release(bool destroy = false)
        {
            //foreach (KeyValuePair<string, UnityEngine.Object> kv in m_assetRequestDic)
            //{
            //    GameObject.Destroy(kv.Value);
            //}
            //未实例化的资源不能这么清理
            m_assetRequestDic.Clear();

            if (m_refrenceObject!=null&& m_refrenceObject.Count>0)
            {
                foreach (KeyValuePair<int, GameObject> kv in m_refrenceObject)
                {
                    GameObject.Destroy(kv.Value);
                }

                m_refrenceObject.Clear();
            }

            if (assetBundle != null)
                assetBundle.Unload(false);

            Resources.UnloadUnusedAssets();
            m_assetRequestDic = null;
            m_refrenceObject = null;
            assetBundle = null;

            MemUtil.DelDownloadDic(m_resPath, bytes);
        }
    }
}
