using UnityEngine;
using System.Collections.Generic;
using System;

namespace Ljf
{
    /// <summary>
    /// image缓存池元素
    /// 
    /// </summary>
    public sealed class TextureElement
    {
        /// <summary>
        /// 
        /// </summary>
        public string name;

        /// <summary>
        /// image
        /// </summary>
        public Texture2D tex = null;

        /// <summary>
        /// 缓存时间（秒）
        /// </summary>
        public float totalTime = 0;

        /// <summary>
        /// 存储时的时间（秒）
        /// </summary>
        public float saveTime = 0;

        /// <summary>
        /// 占用字节大小
        /// </summary>
        public long memorySize = 0;

        public TextureElement(string name, Texture2D tex, float totalTime, float saveTime, long memorySize)
        {
            this.name = name;
            this.tex = tex;
            this.totalTime = totalTime;
            this.saveTime = saveTime;
            this.memorySize = memorySize;
        }
    }

    ///<summary>
    /// 主要功能：高级Image，用于分块地图动态加载
    ///             自动加载处于视口内和视口外一圈的图块
    /// 注意事项：图片名拼写规则为x_y_lv。x,y,lv分别为横纵轴索引和图片级别索引
    ///           只支持jpg格式的图片
    /// 创建日期：2016/12/29
    /// 修改日期：2017/2/8
    /// 修改人：lijunfeng
    /// 修改内容：增加时间缓存，优化卡顿
    /// 修改日期：2017/2/13
    /// 修改人：lijunfeng
    /// 修改内容：地图格子加载方式采用tile管理自身加载和卸载图片级别的方式，暂时不适用缓存以免出现bug
    public sealed class GridImage : MonoBehaviour, IRelease
    {
        /// <summary>
        /// 对tile的操作类型
        /// </summary>
        private enum CmdType
        {
            Not,//空操作
            Add,
            Replace,
            Remove
        }

        /// <summary>
        /// tile操作命令
        /// </summary>
        private class Command
        {
            public CmdType cmdType;
            public int xIndex = -1;
            public int yIndex = -1;

            public Command(CmdType cmdType, int xIndex, int yIndex)
            {
                this.cmdType = cmdType;
                this.xIndex = xIndex;
                this.yIndex = yIndex;
            }
        }

        /// <summary>
        /// 镜头控制类
        /// </summary>
        [SerializeField]
        private ImpactBounds impactBounds;

        /// <summary>
        /// 可选的地图尺寸，宽高相同
        /// </summary>
        [SerializeField]
        private float[] m_mapSizeList = new float[6] { 64f, 128f, 256f, 512f, 1024f, 2048f };

        /// <summary>
        /// 资源加载路径
        /// </summary>
        [SerializeField]
        private string AssetPath = "MapTiles/";

        /// <summary>
        /// 最高级别图片像素
        /// </summary>
        [SerializeField]
        private float m_maxTexSize = 2048f;

        /// <summary>
        /// 地图尺寸（米）
        /// </summary>
        [SerializeField]
        private Vector2 m_mapSize = new Vector2(1024f, 1024f);

        /// <summary>
        /// 地图高与相机size之间的比例（比例尺）
        /// </summary>
        [SerializeField]
        private float m_scale = 1f;//1比1

        /// <summary>
        /// 相机缩放尺寸最小范围12.5到xxx
        /// </summary>
        [Range(12.5f, 10000f)]
        [SerializeField]
        private float m_cameraSize = 12.5f;

        /// <summary>
        /// 网格切割宽度（米）
        /// 实际是最大分辨率时的地图单位
        /// </summary>
        [SerializeField]
        private float m_tileSize = 204.8f;

        /// <summary>
        /// 图片级别
        /// </summary>
        [SerializeField]
        private int m_texLevel = -1;

        /// <summary>
        /// 每贞最大加载的图片占用内存数
        /// </summary>
        [SerializeField]
        private long m_maxMemorySize = 5000000;

        /// <summary>
        /// 最大缓存字节数
        /// </summary>
        [SerializeField]
        private long m_maxCacheMemorySize = 10000000;

        /// <summary>
        /// 当前缓存字节数
        /// </summary>
        private long m_currCacheMemorySize = 0;

        /// <summary>
        /// 保存变换后的4至点坐标
        /// </summary>
        private Vector3[] m_sides = new Vector3[4];

        /// <summary>
        /// 四边界的tile索引
        /// </summary>
        private int m_topIndex = -1;
        private int m_leftIndex = -1;
        private int m_bottomIndex = -1;
        private int m_rightIndex = -1;

        /// <summary>
        /// texture2d时间池
        /// </summary>
        private Dictionary<string, TextureElement> m_imagePool = new Dictionary<string, TextureElement>();

        /// <summary>
        /// 显示层所有地图格子
        /// </summary>
        private Image[,] m_viewGrid = null;

        /// <summary>
        /// tile操作队列
        /// </summary>
        private List<Command> m_commandQueue = new List<Command>();

        /// <summary>
        /// 格子索引x边界
        /// </summary>
        private int MaxXIndex
        {
            get
            {
                return Mathf.CeilToInt(m_mapSize.x / m_tileSize) - 1;
            }
        }

        /// <summary>
        /// 格子索引y边界
        /// </summary>
        private int MaxYIndex
        {
            get
            {
                return Mathf.CeilToInt(m_mapSize.y / m_tileSize) - 1;
            }
        }

        /// <summary>
        /// 视野（米）
        /// </summary>
        private float ViewRange
        {
            get
            {
                return m_scale * m_cameraSize * 2;
            }
        }

        /// <summary>
        /// 设置边界值
        /// </summary>
        private Vector3[] CameraSides
        {
            set
            {
                m_sides[0] = value[0];
                m_sides[1] = value[1];
                m_sides[2] = value[2];
                m_sides[3] = value[3];
            }
        }

        // Use this for initialization
        void Start()
        {
            InitGrid();
        }

        // Update is called once per frame
        void Update()
        {
            //只有变化了，才更新地图块的加载
            if (CheckInput(impactBounds.CameraSize, impactBounds.CameraSides))
            {
                m_cameraSize = impactBounds.CameraSize;
                CameraSides = impactBounds.CameraSides;

                if (CheckTexLevelAndSides())
                    CalculateDownloadAndRelease();
            }

            ExecuteElements();
            ExecuteDownloadAndRelease();

        }

        /// <summary>
        /// 初始化所有格子
        /// </summary>
        private void InitGrid()
        {
            m_viewGrid = new Image[MaxXIndex, MaxYIndex];

            for (int i = 0, iLen = m_viewGrid.GetLength(0); i < iLen; i++)
            {
                for (int j = 0, jLen = m_viewGrid.GetLength(1); j < jLen; j++)
                {
                    m_viewGrid[i, j] = CreateImage(i, j);
                }
            }
        }

        /// <summary>
        /// 获取四边界坐标
        /// </summary>
        /// <returns></returns>
        private void GetSides(Vector3[] sides, out float top, out float left, out float bottom, out float right)
        {
            top = sides[0].y;
            bottom = sides[0].y;
            left = sides[0].x;
            right = sides[0].x;

            for (int i = 1; i < 4; i++)
            {
                if (top < sides[i].y)
                    top = sides[i].y;

                if (bottom > sides[i].y)
                    bottom = sides[i].y;

                if (left > sides[i].x)
                    left = sides[i].x;

                if (right < sides[i].x)
                    right = sides[i].x;
            }
        }

        /// <summary>
        /// 根据坐标获取tile上x轴的索引
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private int GetXTileIndex(float x)
        {
            //把坐标系从屏幕中心点转换到屏幕左下角（初始化时，地图和相机都是上下左右对称，中心点对应世界坐标原点）
            return Mathf.FloorToInt((x + m_mapSize.x * 0.5f) / m_tileSize);
        }

        /// <summary>
        /// 根据坐标获取tile上y轴的索引
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        private int GetYTileIndex(float y)
        {
            //把坐标系从屏幕中心点转换到屏幕左下角（初始化时，地图和相机都是上下左右对称，中心点对应世界坐标原点）
            return Mathf.FloorToInt((y + m_mapSize.y * 0.5f) / m_tileSize);
        }

        /// <summary>
        /// 检查相机的四至点坐标和缩放是否发生变化
        /// </summary>
        /// <param name="cameraSize"></param>
        /// <param name="sides"></param>
        /// <returns>true发生变化</returns>
        private bool CheckInput(float cameraSize, Vector3[] sides)
        {
            if (m_cameraSize != cameraSize)
                return true;

            for (int i = 0; i < 4; i++)
            {
                if (m_sides[i] != sides[i])
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 检查地图级别和四边界索引是否发生变化，决定是否更新图块
        /// 并且设置tile索引和地图级别
        /// </summary>
        /// <returns></returns>
        private bool CheckTexLevelAndSides()
        {
            float t_top, t_left, t_bottom, t_right;
            GetSides(m_sides, out t_top, out t_left, out t_bottom, out t_right);

            //只有超过格子一半范围，才包含外圈格子
            int t_topIndex = GetYTileIndex(t_top);
            t_topIndex = (t_top + m_mapSize.y * 0.5f) % m_tileSize > m_tileSize * 0.5f ? t_topIndex + 1 : t_topIndex;

            int t_bottomIndex = GetYTileIndex(t_bottom);
            t_bottomIndex = (t_bottom + m_mapSize.y * 0.5f) % m_tileSize < m_tileSize * 0.5f ? t_bottomIndex - 1 : t_bottomIndex;

            int t_leftIndex = GetXTileIndex(t_left);
            t_leftIndex = (t_left + m_mapSize.y * 0.5f) % m_tileSize < m_tileSize * 0.5f ? t_leftIndex - 1 : t_leftIndex;

            int t_rightIndex = GetXTileIndex(t_right);
            t_rightIndex = (t_right + m_mapSize.y * 0.5f) % m_tileSize > m_tileSize * 0.5f ? t_rightIndex + 1 : t_rightIndex;

            //计算格子索引x边界
            int minX = 0;
            int maxX = MaxXIndex;

            //计算格子索引y边界
            int minY = 0;
            int maxY = MaxYIndex;

            //限制边界索引
            t_leftIndex = Mathf.Clamp(t_leftIndex, minX, maxX);
            t_rightIndex = Mathf.Clamp(t_rightIndex, minX, maxX);
            t_topIndex = Mathf.Clamp(t_topIndex, minY, maxY);
            t_bottomIndex = Mathf.Clamp(t_bottomIndex, minY, maxY);

            bool isChanged = false;

            if (t_topIndex != m_topIndex || t_bottomIndex != m_bottomIndex || t_leftIndex != m_leftIndex || t_rightIndex != m_rightIndex)
            {
                m_topIndex = t_topIndex;
                m_bottomIndex = t_bottomIndex;
                m_leftIndex = t_leftIndex;
                m_rightIndex = t_rightIndex;
                isChanged = true;
            }

            int t_texLevel = GetTexLevel();

            if (t_texLevel != m_texLevel)
            {
                m_texLevel = t_texLevel;
                isChanged = true;
            }

            return isChanged;
        }

        /// <summary>
        /// 计算要加载的图片的对应级别编号,范围为0到最大索引
        /// </summary>
        private int GetTexLevel()
        {
            //计算图片像素
            float px = m_tileSize * m_maxTexSize / ViewRange;

            //判断px离哪个元素更近
            int index = 0;
            float near = Mathf.Abs(m_mapSizeList[0] - px);

            for (int i = 1; i < m_mapSizeList.Length; i++)
            {
                float t_near = Mathf.Abs(m_mapSizeList[i] - px);

                if (t_near < near)
                {
                    near = t_near;
                    index = i;
                }
            }

            return index;
        }

        /// <summary>
        /// 计算要加载的地图快id列表,并且计算要卸载的id列表
        /// 地图切块坐标从左下角开始计算，从00开始
        /// </summary>
        private void CalculateDownloadAndRelease()
        {
            Command t_cmd;

            for (int i = 0, iLen = m_viewGrid.GetLength(0); i < iLen; i++)
            {
                for (int j = 0, jLen = m_viewGrid.GetLength(1); j < jLen; j++)
                {
                    if (i >= m_leftIndex && i <= m_rightIndex && j >= m_bottomIndex && j <= m_topIndex)
                        t_cmd = new Command(CmdType.Add, i, j);
                    else
                        t_cmd = new Command(CmdType.Remove, i, j);

                    Command t_cmdA = m_commandQueue.Find(item => item.xIndex == t_cmd.xIndex && item.yIndex == t_cmd.yIndex);

                    if (t_cmdA != null)
                    {
                        t_cmdA.cmdType = t_cmd.cmdType;
                    }
                    else
                    {
                        m_commandQueue.Add(t_cmd);
                    }
                }
            }
        }

        /// <summary>
        /// 处理加载和卸载
        /// </summary>
        private void ExecuteDownloadAndRelease()
        {
            long currMem = 0;

            lock (m_commandQueue)
            {
                while (m_commandQueue.Count > 0)
                {
                    Command t_cmd = m_commandQueue[0];
                    m_commandQueue.RemoveAt(0);
                    Image t_image = m_viewGrid[t_cmd.xIndex, t_cmd.yIndex];

                    if (t_cmd.cmdType == CmdType.Add)
                    {
                        if (t_image.IsEmpty || t_image.level != m_texLevel)
                        {
                            currMem += LoadImage(t_cmd.xIndex, t_cmd.yIndex);
                        }
                    }
                    else if (t_cmd.cmdType == CmdType.Remove)
                    {
                        if (!t_image.IsEmpty)
                        {
                            ClearImage(t_cmd.xIndex, t_cmd.yIndex);
                        }
                    }

                    if (currMem >= m_maxMemorySize)
                        break;
                }
            }
        }

        /// <summary>
        /// 创建图块
        /// </summary>
        /// <param name="xIndex"></param>
        /// <param name="yIndex"></param>
        /// <returns></returns>
        private Image CreateImage(int xIndex, int yIndex)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Image image = obj.AddComponent<Image>();
            image.Resize(m_tileSize, m_tileSize);
            obj.transform.parent = transform;
            SetTexTransfrom(image, xIndex, yIndex);
            image.name = xIndex + "_" + yIndex;
            return image;
        }

        /// <summary>
        /// 设置地图块坐标
        /// </summary>
        /// <param name="name"></param>
        private void SetTexTransfrom(Image image, int xIndex, int yIndex)
        {
            float x = (xIndex + 0.5f) * m_tileSize - m_mapSize.x * 0.5f;
            float y = (yIndex + 0.5f) * m_tileSize - m_mapSize.y * 0.5f;
            image.transform.localRotation = Quaternion.AngleAxis(90, Vector3.right);
            image.transform.localPosition = new Vector3(x, 0, y);
            image.transform.parent = this.transform;
        }

        /// <summary>
        /// 加载图块
        /// </summary>
        /// <param name="xIndex"></param>
        /// <param name="yIndex"></param>
        /// <returns>返回加载的字节数</returns>
        private long LoadImage(int xIndex, int yIndex)
        {
            //Image t_image = m_viewGrid[xIndex, yIndex];
            //string name = xIndex + "_" + yIndex + "_" + m_texLevel;
            //TextureElement element = PoolFrom(name);

            //if (element == null)
            //{
            //    t_image.Load(Application.streamingAssetsPath + "/" + AssetPath + "/" + m_texLevel + "/" + name + ".JPEG");
            //    t_image.SamplingWidth = Mathf.Max(1, Mathf.RoundToInt(4 / (Mathf.Pow(2, (5 - m_texLevel)))));
            //    t_image.level = m_texLevel;
            //}
            //else
            //{
            //    t_image.Load(element.tex);
            //    t_image.SamplingWidth = Mathf.Max(1, Mathf.RoundToInt(4 / (Mathf.Pow(2, (5 - m_texLevel)))));
            //    t_image.level = m_texLevel;
            //}

            //不用缓存
            Image t_image = m_viewGrid[xIndex, yIndex];
            string name = xIndex + "_" + yIndex + "_" + m_texLevel;
            t_image.Load(Application.streamingAssetsPath + "/" + AssetPath + "/" + m_texLevel + "/" + name + ".JPEG");
            t_image.SamplingWidth = Mathf.Max(1, Mathf.RoundToInt(4 / (Mathf.Pow(2, (5 - m_texLevel)))));
            t_image.level = m_texLevel;
            return t_image.MemorySize;
        }

        /// <summary>
        /// 清除图块
        /// </summary>
        /// <param name="xIndex"></param>
        /// <param name="yIndex"></param>
        private void ClearImage(int xIndex, int yIndex)
        {
            //Image t_image = m_viewGrid[xIndex, yIndex];

            //内存够大就缓存
            //if (m_currCacheMemorySize + t_image.MemorySize < m_maxCacheMemorySize)
            //{
            //    string name = xIndex + "_" + yIndex + "_" + m_texLevel;
            //    TextureElement element = new TextureElement(name, t_image.GetTexture(), 5f, Time.realtimeSinceStartup, t_image.MemorySize);
            //    PoolIn(element);
            //}

            //t_image.Clear();

            //不用缓存
            Image t_image = m_viewGrid[xIndex, yIndex];
            t_image.Clear();
        }

        /// <summary>
        /// 加入缓存池
        /// </summary>
        /// <param name="element"></param>
        public void PoolIn(TextureElement element)
        {
            if (!m_imagePool.ContainsKey(element.name))
            {
                m_imagePool.Add(element.name, element);
                m_currCacheMemorySize += element.memorySize;
            }
        }

        /// <summary>
        /// 移出缓存池
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TextureElement PoolFrom(string id)
        {
            TextureElement element = null;

            if (m_imagePool.TryGetValue(id, out element))
            {
                m_currCacheMemorySize -= element.memorySize;
                m_imagePool.Remove(id);
            }

            return element;
        }

        /// <summary>
        /// 定期销毁超时的元素
        /// </summary>
        public void ExecuteElements()
        {
            string[] keys = new string[m_imagePool.Count];
            m_imagePool.Keys.CopyTo(keys, 0);

            for (int i = 0; i < keys.Length; i++)
            {
                TextureElement element = m_imagePool[keys[i]];

                if (Time.realtimeSinceStartup - element.saveTime >= element.totalTime)
                {
                    m_currCacheMemorySize -= element.memorySize;
                    element.tex = null;
                    m_imagePool.Remove(keys[i]);
                }
            }
        }

        public void Release(bool destroy = false)
        {
            foreach (var item in m_imagePool)
            {
                item.Value.tex = null;
            }

            m_imagePool.Clear();

            for (int i = 0, iLen = m_viewGrid.GetLength(0); i < iLen; i++)
            {
                for (int j = 0, jLen = m_viewGrid.GetLength(1); j < jLen; j++)
                {
                    m_viewGrid[i, j].Dispose();
                    Destroy(m_viewGrid[i, j].gameObject);
                    m_viewGrid[i, j] = null;
                }
            }

            m_viewGrid = null;
            m_commandQueue.Clear();
            m_commandQueue = null;
        }
    }
}