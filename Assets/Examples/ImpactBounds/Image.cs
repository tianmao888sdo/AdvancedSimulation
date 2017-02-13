using UnityEngine;
using System.IO;
using System.Drawing;

namespace Ljf
{
    ///<summary>
    /// 主要功能：图片加载显示
    /// 注意事项：如果先设置图片路径，则运行时自动加载图片
    ///           运行后可以重新设置新的路径以加载新内容
    ///           运行后可以重置Image的大小，也可以清空内容
    /// 创建日期：2016/12/29
    /// 修改日期：2016/12/29
    /// 修改人：lijunfeng
    /// 修改日期：2017/2/10
    /// 修改人：lijunfeng
    /// 修改内容：增加贴图内存记录，shader改为unlit/texture
    /// 修改日期：2017/2/13
    /// 修改人：lijunfeng
    /// 修改内容：增加贴图内存记录，shader改为自发光高光,增加Load重载，获取文理，内存占用计算
    public sealed class Image : MonoBehaviour
    {
        /// <summary>
        /// 图像级别
        /// </summary>
        public int level = -1;

        [SerializeField]
        private string m_path;
        /// <summary>
        /// 只能用于初始设定
        /// </summary>
        [SerializeField]
        private float m_width = 1;
        /// <summary>
        /// 只能用于初始设定
        /// </summary>
        [SerializeField]
        private float m_height = 1;
        /// <summary>
        /// 周边采样宽度（像素）
        /// </summary>
        [SerializeField]
        private int m_samplingWidth = 4;

        private Texture2D m_tex = null;
        private FileStream m_fileStream = null;
        private MemoryStream m_memStream = null;
        private Bitmap m_bitmap = null;
        /// <summary>
        /// 原图片尺寸
        /// </summary>
        private int m_sourceWidth;
        private int m_sourceHeight;

        /// <summary>
        /// 当前加载的贴图占用的内存
        /// </summary>
        private long m_MemorySize = 0;

        private void Start()
        {
            this.GetComponent<MeshRenderer>().materials[0].shader = Shader.Find("Legacy Shaders/Self-Illumin/Specular");
            this.GetComponent<MeshRenderer>().materials[0].SetColor("_Color", UnityEngine.Color.black);
            Load(m_path);
        }

        /// <summary>
        /// 设置图片路径，绝对路径，包括后缀
        /// </summary>
        public string Path
        {
            get
            {
                return m_path;
            }
            set
            {
                if (value != m_path)
                    Load(value);

                m_path = value;
            }
        }

        /// <summary>
        /// 重新调整大小
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Resize(float width, float height)
        {
            if (width != m_width || height != m_height)
            {
                m_width = width;
                m_height = height;
                this.transform.localScale = new Vector3(m_width, m_height, 1f);
            }
        }

        /// <summary>
        /// 插值算法像素周边采样宽度（每个方向的像素数）
        /// </summary>
        public int SamplingWidth
        {
            get
            {
                return m_samplingWidth;
            }

            set
            {
                m_samplingWidth = value;
                Vector2 scale = new Vector2((float)(m_sourceWidth - 2 * m_samplingWidth) / m_sourceWidth, (float)(m_sourceHeight - 2 * m_samplingWidth) / m_sourceHeight);
                Vector2 offset = new Vector2((float)m_samplingWidth / m_sourceWidth, (float)m_samplingWidth / m_sourceHeight);
                this.GetComponent<MeshRenderer>().materials[0].mainTextureScale = scale;
                this.GetComponent<MeshRenderer>().materials[0].mainTextureOffset = offset;
            }
        }

        /// <summary>
        /// 加载内存字节数
        /// </summary>
        public long MemorySize
        {
            get
            {
                return m_MemorySize;
            }
        }

        /// <summary>
        /// 加载图片
        /// </summary>
        /// <param name="tex"></param>
        public void Load(Texture2D tex)
        {
            Dispose();
            m_sourceWidth = tex.width;
            m_sourceHeight = tex.height;
            SamplingWidth = m_samplingWidth;

            this.GetComponent<MeshRenderer>().materials[0].SetTexture("_MainTex", m_tex);
            this.GetComponent<MeshRenderer>().materials[0].SetColor("_Color", UnityEngine.Color.white);
            this.transform.localScale = new Vector3(m_width, m_height, 1f);
        }

        /// <summary>
        /// 加载图片
        /// </summary>
        /// <param name="path"></param>
        public void Load(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;

            Dispose();

            using (m_fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                m_bitmap = new Bitmap(m_fileStream);
                m_tex = new Texture2D(m_bitmap.Width, m_bitmap.Height);
                m_sourceWidth = m_bitmap.Width;
                m_sourceHeight = m_bitmap.Height;
                SamplingWidth = m_samplingWidth;

                using (m_memStream = new MemoryStream())
                {
                    m_bitmap.Save(m_memStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    byte[] bytes = m_memStream.ToArray();
                    m_MemorySize = bytes.Length;
                    m_tex.LoadImage(bytes);
                    m_memStream.Close();
                }

                this.GetComponent<MeshRenderer>().materials[0].SetTexture("_MainTex", m_tex);
                this.GetComponent<MeshRenderer>().materials[0].SetColor("_Color", UnityEngine.Color.white);
                this.transform.localScale = new Vector3(m_width, m_height, 1f);
                m_bitmap.Dispose();
                m_bitmap = null;
                m_fileStream.Close();
            }
        }

        /// <summary>
        /// 清除画布
        /// </summary>
        public void Clear()
        {
            this.GetComponent<MeshRenderer>().materials[0].SetTexture("_MainTex", null);
            this.GetComponent<MeshRenderer>().materials[0].SetColor("_Color", UnityEngine.Color.black);
            m_MemorySize = 0;
        }

        /// <summary>
        /// 画布是否为空
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty
        {
            get { return this.GetComponent<MeshRenderer>().materials[0].GetTexture("_MainTex") == null; }
        }

        /// <summary>
        /// 获取纹理
        /// </summary>
        /// <returns></returns>
        public Texture2D GetTexture()
        {
            return this.GetComponent<MeshRenderer>().materials[0].GetTexture("_MainTex") as Texture2D;
        }

        /// <summary>
        /// 中断加载
        /// </summary>
        public void Dispose()
        {
            if (m_memStream != null)
            {
                m_memStream.Close();
                m_memStream.Dispose();
                m_memStream = null;
            }

            if (m_fileStream != null)
            {
                m_fileStream.Close();
                m_fileStream.Dispose();
                m_fileStream = null;
            }

            if (m_bitmap != null)
            {
                m_bitmap.Dispose();
                m_bitmap = null;
            }

            this.GetComponent<MeshRenderer>().materials[0].SetTexture("_MainTex", null);
            m_tex = null;
        }
    }
}