using UnityEngine;
using System.Collections;

namespace Ljf
{
    /// <summary>
    /// 类主要功能：地图旋转方案，要求能旋转，移动相机，地图要围绕相机中心点旋转显示
    ///             相机围绕鼠标位置缩放，跟随模式下，自动跟随小车，以小车在屏幕坐标系的位置为原点缩放，可以重置，但不会重置跟随状态
    ///             初始化时要自适应地图
    /// 注意事项：应该还需要增加一个相机初始旋转矩阵，适应各种方向的地图
    /// 创建日期：2016/12/20
    /// 修改日期：2016/12/26
    /// 修改人：lijunfeng
    /// </summary>
    public sealed class ImpactBounds : MonoBehaviour
    {
        public enum FollowType
        {
            Mouse,//鼠标操控
            AutoFollow //自动跟随 小车
        }

        /// <summary>
        /// 速度倍率，用于调整放大后的各种速度
        /// </summary>
        public float Rate = 100f;

        /// <summary>
        /// 跟随方式
        /// </summary>
        [SerializeField]
        private FollowType followType = FollowType.Mouse;

        /// <summary>
        /// 主摄像机
        /// </summary>
        [SerializeField]
        private Camera mainCamera;

        /// <summary>
        /// 相机坐标系在世界坐标系中累计移动的向量
        /// </summary>
        [SerializeField]
        private Vector3 worldMove;

        /// <summary>
        /// 相机相对世界的累计旋转角度
        /// </summary>
        [SerializeField]
        private float totalAngle;

        /// <summary>
        /// 小车坐标
        /// </summary>
        [SerializeField]
        private Vector3 carPos;

        /// <summary>
        /// 跟随时的速度
        /// </summary>
        [SerializeField]
        private float followSpeed = 1f;

        /// <summary>
        /// reset时的移动速度(米）
        /// </summary>
        [SerializeField]
        private float resetSpd = 10f;

        /// <summary>
        /// reset时的旋转速度(度)
        /// </summary>
        [SerializeField]
        private float rotateSpd = 1f;

        /// <summary>
        /// reset时的缩放速度(度)
        /// </summary>
        [SerializeField]
        private float scaleSpd = 1f;

        //地图宽高（米)
        [SerializeField]
        private float mapWidth = 0;
        [SerializeField]
        private float mapHeight = 0;

        /// <summary>
        /// 相机深度
        /// </summary>
        [SerializeField]
        private float cameraFar = 10f;

        /// <summary>
        /// 是否自动自适应地图
        /// </summary>
        [SerializeField]
        private bool autoSelfAdapt = false;

        /// <summary>
        /// 地图4至点坐标
        /// </summary>
        [SerializeField]
        private Vector3 topLeft;
        [SerializeField]
        private Vector3 topRgiht;
        [SerializeField]
        private Vector3 bottomLeft;
        [SerializeField]
        private Vector3 bottomRight;

        /// <summary>
        /// 保存变换后的地图4至点坐标
        /// </summary>
        [SerializeField]
        private Vector3[] sides = new Vector3[4];

        /// <summary>
        /// 保存变换后的相机边界4至
        /// </summary>
        private Vector3[] cameraSize = new Vector3[4];

        /// <summary>
        /// 记录可移动范围，相机坐标系
        /// </summary>
        [SerializeField]
        private float top;
        [SerializeField]
        private float bottom;
        [SerializeField]
        private float left;
        [SerializeField]
        private float right;

        /// <summary>
        /// 保存初始值，用于reset
        /// </summary>
        private float m_cachedTotalAngle;

        /// <summary>
        /// 记录初始缩放
        /// </summary>
        private float m_cachedScale;

        /// <summary>
        /// 相机原始偏移矩阵
        /// </summary>
        private Matrix4x4 cameraM = Matrix4x4.zero;

        /// <summary>
        /// 累计旋转矩阵
        /// </summary>
        [SerializeField]
        private Matrix4x4 totalRotationM = Matrix4x4.zero;

        /// <summary>
        /// 地图相对于相机坐标系的总的变换，包含旋转和位移变换
        /// </summary>
        [SerializeField]
        private Matrix4x4 totalM = Matrix4x4.zero;

        /// <summary>
        /// 相对相机坐标系的位移矩阵，把相机看成与世界坐标系一致的
        /// </summary>
        private Matrix4x4 moveM = Matrix4x4.zero;

        /// <summary>
        /// 相对相机坐标系的旋转矩阵，把相机看成与世界坐标系一致的
        /// </summary>
        private Matrix4x4 rotationM = Matrix4x4.zero;

        /// <summary>
        /// 越界颜色，越出为红色，否则为绿色
        /// </summary>
        private Color warningColor = Color.green;

        private bool isMouseDown = false;
        private Vector3 preMousePos;

        /// <summary>
        /// 是否跟随中,只读
        /// </summary>
        private bool isFollowing = false;

        /// <summary>
        /// 是否正在重置
        /// </summary>
        private bool isReseting = false;

        /// <summary>
        /// 小车在地图上的坐标
        /// </summary>
        public Vector3 CarPos { get { return carPos; } }

        /// <summary>
        /// 保存变换后的地图4至点坐标
        /// </summary>
        public Vector3[] Sides { get { return sides; } }

        /// <summary>
        /// 相机4至点在世界上的坐标
        /// </summary>
        public Vector3[] CameraSides { get { return cameraSize; } }

        /// <summary>
        /// 相机视野的一半
        /// </summary>
        public float CameraSize { get { return mainCamera == null ? 0 : mainCamera.orthographicSize; } }

        // Use this for initialization
        void Start()
        {
            cameraM = totalM = totalRotationM = moveM = rotationM = Matrix4x4.identity;
            cameraM = getXRotationMatrix(cameraM, 90f);
            topLeft = new Vector3(-mapWidth * 0.5f, mapHeight * 0.5f, 0);
            topRgiht = new Vector3(mapWidth * 0.5f, mapHeight * 0.5f, 0);
            bottomLeft = new Vector3(-mapWidth * 0.5f, -mapHeight * 0.5f, 0);
            bottomRight = new Vector3(mapWidth * 0.5f, -mapHeight * 0.5f, 0);
            m_cachedTotalAngle = totalAngle;
            m_cachedScale = mainCamera.orthographicSize;
            Reset();
        }

        /// <summary>
        /// 地图自适应
        /// </summary>
        private void MapSelfAdapt()
        {
            SetRotation(totalAngle);
            ApplyTotalMatrix(totalM);
            m_cachedScale = mainCamera.orthographicSize = GetMapHeight() * 0.5f;
        }

        /// <summary>
        /// 重置,跟随模式下，不重置跟随状态
        /// </summary>
        private void Reset()
        {
            totalAngle = m_cachedTotalAngle;
            cameraM = totalM = Matrix4x4.identity;
            cameraM = getXRotationMatrix(cameraM, 90f);

            sides[0] = topLeft;
            sides[1] = topRgiht;
            sides[2] = bottomLeft;
            sides[3] = bottomRight;

            worldMove = Vector3.zero;

            if (autoSelfAdapt)
                MapSelfAdapt();
        }

        /// <summary>
        /// 鼠标移动单位转化为世界地图移动单位
        /// </summary>
        /// <returns></returns>
        private Vector3 ScreenToWorld(Vector3 deltaMove)
        {
            deltaMove.y /= (Screen.height * 0.5f / mainCamera.orthographicSize);
            deltaMove.x /= (Screen.height * mainCamera.aspect * 0.5f / mainCamera.orthographicSize);
            return deltaMove;
        }

        /// <summary>
        /// 世界地图移动单位转化为鼠标移动单位
        /// </summary>
        /// <returns></returns>
        private Vector3 WorldToScreen(Vector3 deltaMove)
        {
            deltaMove.y *= (Screen.height * 0.5f / mainCamera.orthographicSize);
            deltaMove.x *= (Screen.height * mainCamera.aspect * 0.5f / mainCamera.orthographicSize);
            return deltaMove;
        }

        /// <summary>
        /// 设置累计移动值
        /// </summary>
        /// <param name="deltaMove"></param>
        private void SetDeltaMove(Vector3 deltaMove)
        {
            worldMove += totalRotationM.inverse.MultiplyPoint(deltaMove);
            //地图相对相机坐标系的移动矩阵，与鼠标拖拽方向相反
            moveM = getXYMoveMatrix(moveM, -deltaMove);
            totalM = moveM * totalM;//一定要右乘
        }

        /// <summary>
        /// 直接设置绝对坐标
        /// </summary>
        private void SetMove(Vector3 move)
        {
            worldMove = move;
        }

        /// <summary>
        /// 设置累计旋转值
        /// </summary>
        private void SetDeltaRotation(float deltaAngle)
        {
            //计算相机相对自身的旋转
            totalAngle += deltaAngle;

            if (totalAngle > 360)
                totalAngle -= 360f;

            if (totalAngle < 0)
                totalAngle += 360;

            totalRotationM = getZRotationMatrix(totalRotationM, totalAngle);
            //地图相对相机的逆旋转矩阵
            rotationM = getZRotationMatrix(rotationM, deltaAngle);
            totalM = rotationM * totalM;//一定要右乘
        }

        /// <summary>
        /// 设置绝对旋转，只在自适应时有用
        /// </summary>
        /// <param name="deltaAngle"></param>
        private void SetRotation(float angle)
        {
            totalAngle = angle;
            totalRotationM = getZRotationMatrix(totalRotationM, totalAngle);
            totalM = totalRotationM;
        }

        /// <summary>
        /// 围绕鼠标位置缩放
        /// </summary>
        /// <param name="deltaScale"></param>
        /// <param name="pos">围绕屏幕上哪个点，从左下角为00点</param>
        private void SetScale(float deltaScale, Vector3 pos)
        {
            float y = 2f * (pos.y - 0.5f * Screen.height) * deltaScale / Screen.height;
            float x = 2f * (pos.x - 0.5f * Screen.width) * mainCamera.aspect * deltaScale / Screen.width;
            mainCamera.orthographicSize += deltaScale;
            SetDeltaMove(-new Vector3(x, y, 0));
        }

        /// <summary>
        /// 执行总矩阵变化的后续处理
        /// </summary>
        /// <param name="m"></param>
        private void ApplyTotalMatrix(Matrix4x4 m)
        {
            sides[0] = m.MultiplyPoint(topLeft);
            sides[1] = m.MultiplyPoint(topRgiht);
            sides[2] = m.MultiplyPoint(bottomLeft);
            sides[3] = m.MultiplyPoint(bottomRight);

            float h = mainCamera.orthographicSize;
            float w = mainCamera.orthographicSize * mainCamera.aspect;

            cameraSize[0] = m.inverse.MultiplyPoint(new Vector3(-w, h, 0));
            cameraSize[1] = m.inverse.MultiplyPoint(new Vector3(w, h, 0));
            cameraSize[2] = m.inverse.MultiplyPoint(new Vector3(-w, -h, 0));
            cameraSize[3] = m.inverse.MultiplyPoint(new Vector3(w, -h, 0));
        }

        /// <summary>
        /// 执行移动相机，并旋转相机到指定角度
        /// </summary>
        /// <param name="totalMove">相机在世界的总移动</param>
        /// <param name="totalAngle">相机绕自身旋转的总角度，延z轴的负轴旋转</param
        private void MoveAndRotatCamera(Vector3 totalMove, float totalAngle)
        {
            //计算相机在世界中实际位移
            mainCamera.transform.localPosition = new Vector3(worldMove.x, cameraFar, worldMove.y);
            //计算相机在世界中实际旋转
            mainCamera.transform.localRotation = Quaternion.Euler(90, 0f, -totalAngle);
        }

        /// <summary>
        /// 跟随地图上某个点
        /// </summary>
        /// <param name="followSpd">跟随速度(米）</param>
        /// <param name="pos">汽车所在地图上的世界坐标点</param>
        private void Follow(Vector3 pos, float followSpd, System.Action callback = null)
        {
            //转换地图坐标到相机坐标系
            Vector3 t_carPos = totalM.MultiplyPoint(pos);

            if (t_carPos == Vector3.zero)
            {
                if (callback != null)
                    callback();

                return;
            }

            //把地图单位转化为屏幕单位
            Vector3 t_carPosScreen = WorldToScreen(t_carPos);

            if (Mathf.Abs(t_carPos.x) < followSpd)
            {
                Vector3 t_deltaMove = ScreenToWorld(new Vector3(t_carPosScreen.x, 0, 0));
                SetDeltaMove(t_deltaMove);
                isFollowing = false;
            }
            else
            {
                Vector3 t_deltaMove2 = ScreenToWorld(new Vector3(Mathf.Sign(t_carPosScreen.x), 0, 0) * followSpd);
                SetDeltaMove(t_deltaMove2);
                isFollowing = true;
            }

            if (Mathf.Abs(t_carPos.y) < followSpd)
            {
                Vector3 t_deltaMove = ScreenToWorld(new Vector3(0, t_carPosScreen.y, 0));
                SetDeltaMove(t_deltaMove);
                isFollowing = false;
            }
            else
            {
                Vector3 t_deltaMove2 = ScreenToWorld(new Vector3(0, Mathf.Sign(t_carPosScreen.y), 0) * followSpd);
                SetDeltaMove(t_deltaMove2);
                isFollowing = true;
            }
        }

        /// <summary>
        /// 跟随地图上某个点,并旋转到指定角度
        /// </summary>
        /// <param name="pos">地图上目标位置</param>
        /// <param name="followSpd">移动速度（米）</param>
        /// <param name="angle">目标旋转角</param>
        /// <param name="rotateSpd">旋转速度（度）</param>
        /// <param name="scale">目标缩放</param>
        /// <param name="scaleSpd">缩放速度</param>
        /// <param name="callback">位移和旋转结束回调</param>
        private void Follow(Vector3 pos, float followSpd, float angle, float rotateSpd, float scale, float scaleSpd, System.Action callback = null)
        {
            //向小夹角方向旋转
            float isRevive = totalAngle - angle > 180f ? -1 : 1;

            //处理旋转
            if (Mathf.Abs(totalAngle - angle) < rotateSpd)
            {
                SetDeltaRotation(isRevive * (angle - totalAngle) * rotateSpd);
            }
            else
            {
                SetDeltaRotation(isRevive * Mathf.Sign(angle - totalAngle) * rotateSpd);
            }

            //处理缩放
            if (Mathf.Abs(mainCamera.orthographicSize - scale) < scaleSpd)
            {
                mainCamera.orthographicSize += (scale - mainCamera.orthographicSize) * scaleSpd;
            }
            else
            {
                mainCamera.orthographicSize += Mathf.Sign(scale - mainCamera.orthographicSize) * scaleSpd;
            }

            //计算初始点相对于相机坐标系的位置
            Vector3 t_carPos = totalM.MultiplyPoint(pos);

            if (totalAngle == angle && t_carPos == Vector3.zero && mainCamera.orthographicSize == scale)
            {
                if (callback != null)
                    callback();

                return;
            }

            //把地图单位转化为屏幕单位
            Vector3 t_carPosScreen = WorldToScreen(t_carPos);

            if (Mathf.Abs(t_carPos.x) < followSpd)
            {
                Vector3 t_deltaMove = ScreenToWorld(new Vector3(t_carPosScreen.x, 0, 0));
                SetDeltaMove(t_deltaMove);
            }
            else
            {
                Vector3 t_deltaMove2 = ScreenToWorld(new Vector3(Mathf.Sign(t_carPosScreen.x), 0, 0) * followSpd);
                SetDeltaMove(t_deltaMove2);
            }

            if (Mathf.Abs(t_carPos.y) < followSpd)
            {
                Vector3 t_deltaMove = ScreenToWorld(new Vector3(0, t_carPosScreen.y, 0));
                SetDeltaMove(t_deltaMove);
            }
            else
            {
                Vector3 t_deltaMove2 = ScreenToWorld(new Vector3(0, Mathf.Sign(t_carPosScreen.y), 0) * followSpd);
                SetDeltaMove(t_deltaMove2);
            }
        }

        /// <summary>
        /// 获取地图高度（米）
        /// </summary>
        /// <returns></returns>
        private float GetMapHeight()
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

            return Mathf.Abs(top - bottom);
        }

        /// <summary>
        /// 判断相机范围是否合法
        /// </summary>
        /// <param name="moveBack">要还原的位移</param>
        /// <returns></returns>
        private bool IsRangeValid(out Vector3 moveBack)
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

            float v = Mathf.Abs(top - bottom);
            float h = Mathf.Abs(left - right);

            if (mainCamera.orthographicSize > v * 0.5f)
                mainCamera.orthographicSize = v * 0.5f;

            moveBack = Vector3.zero;

            if (mainCamera.orthographicSize * mainCamera.aspect > h * 0.5)//相机两边大于范围时，相机x轴固定（仍然可以左右放大)，上下可拖动
            {
                moveBack.x = (right + left) * 0.5f;
            }
            else if (mainCamera.orthographicSize * mainCamera.aspect <= h * 0.5f)//相机两边小于地图范围时，相机任意一边不能超出范围
            {
                if (right < mainCamera.orthographicSize * mainCamera.aspect)
                {
                    moveBack.x = right - mainCamera.orthographicSize * mainCamera.aspect;
                }

                if (left > -mainCamera.orthographicSize * mainCamera.aspect)
                {
                    moveBack.x = left + mainCamera.orthographicSize * mainCamera.aspect;
                }
            }

            if (top < mainCamera.orthographicSize)
            {
                moveBack.y = top - mainCamera.orthographicSize;
            }

            if (bottom > -mainCamera.orthographicSize)
            {
                moveBack.y = bottom + mainCamera.orthographicSize;
            }

            return moveBack == Vector3.zero;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
                Reset();

            if (Input.GetKeyDown(KeyCode.Q))
            {
                isReseting = true;
                followType = FollowType.Mouse;
            }

            if (isReseting)
            {
                Follow(Vector3.zero, resetSpd, m_cachedTotalAngle, rotateSpd * Rate, m_cachedScale, scaleSpd * Rate, () => {
                    isReseting = false;
                });
            }

            if (followType == FollowType.AutoFollow && isReseting == false)
            {
                carPos.x += Input.GetAxis("Horizontal");
                carPos.y += Input.GetAxis("Vertical");
                Follow(carPos, followSpeed * Rate);

                if (Input.GetKey(KeyCode.G))//放大
                {
                    //转换小车坐标到相机坐标系
                    Vector3 t_carPos = totalM.MultiplyPoint(carPos);
                    SetScale(Time.deltaTime * Rate, t_carPos + new Vector3(Screen.width * 0.5f, Screen.width * 0.5f, 0f));
                }
                else if (Input.GetKey(KeyCode.B))//缩小
                {
                    //转换小车坐标到相机坐标系
                    Vector3 t_carPos = totalM.MultiplyPoint(carPos);
                    SetScale(-Time.deltaTime * Rate, t_carPos + new Vector3(Screen.width * 0.5f, Screen.width * 0.5f, 0f));
                }
            }

            if (isMouseDown)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    isMouseDown = false;
                    return;
                }

                if (followType == FollowType.Mouse && Input.mousePosition != preMousePos)
                {
                    Vector3 deltaMove = ScreenToWorld(Input.mousePosition - preMousePos);
                    SetDeltaMove(-deltaMove * Rate);
                    preMousePos = Input.mousePosition;
                }

                if (Input.GetAxis("Mouse ScrollWheel") != 0)
                {
                    float deltaAngle = Input.GetAxis("Mouse ScrollWheel") * 10;
                    SetDeltaRotation(deltaAngle);
                }

                if (followType == FollowType.Mouse)
                {
                    if (Input.GetKey(KeyCode.G))//放大
                    {
                        SetScale(Time.deltaTime * Rate, Input.mousePosition);
                    }
                    else if (Input.GetKey(KeyCode.B))//缩小
                    {
                        SetScale(-Time.deltaTime * Rate, Input.mousePosition);
                    }
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    isMouseDown = true;
                    preMousePos = Input.mousePosition;
                }
            }
        }

        void LateUpdate()
        {
            ApplyTotalMatrix(totalM);
            Vector3 t_moveBack;

            if (!IsRangeValid(out t_moveBack))
            {
                SetDeltaMove(t_moveBack);
                ApplyTotalMatrix(totalM);
                warningColor = Color.red;
            }
            else
            {
                warningColor = Color.green;
            }

            MoveAndRotatCamera(worldMove, totalAngle);
        }

        /// <summary>
        /// 绘制边框
        /// </summary>
        void OnDrawGizmos()
        {
            //范围框永远是相对相机原始中心点旋转的，只跟旋转有关，上下左右对称
            float w = Mathf.Abs(left - right) * 0.5f;
            float h = Mathf.Abs(top - bottom) * 0.5f;

            Vector2 t_topLeft = new Vector2(-w, h);
            Vector2 t_topRight = new Vector2(w, h);
            Vector2 t_bottomLeft = new Vector2(-w, -h);
            Vector2 t_bottomRight = new Vector2(w, -h);

            //转化成世界坐标，要乘totalRotationM的逆矩阵
            t_topLeft = totalRotationM.inverse.MultiplyPoint(t_topLeft);
            t_topRight = totalRotationM.inverse.MultiplyPoint(t_topRight);
            t_bottomLeft = totalRotationM.inverse.MultiplyPoint(t_bottomLeft);
            t_bottomRight = totalRotationM.inverse.MultiplyPoint(t_bottomRight);

            Gizmos.color = warningColor;
            //Gizmos.DrawLine(t_topLeft, t_topRight);
            //Gizmos.DrawLine(t_topRight, t_bottomRight);
            //Gizmos.DrawLine(t_bottomRight, t_bottomLeft);
            //Gizmos.DrawLine(t_bottomLeft, t_topLeft);

            //Gizmos.color = Color.blue;
            //Gizmos.DrawLine(sides[0], sides[1]);
            //Gizmos.DrawLine(sides[1], sides[3]);
            //Gizmos.DrawLine(sides[3], sides[2]);
            //Gizmos.DrawLine(sides[2], sides[0]);

            Gizmos.DrawLine(new Vector3(t_topLeft.x, 0, t_topLeft.y), new Vector3(t_topRight.x, 0, t_topRight.y));
            Gizmos.DrawLine(new Vector3(t_topRight.x, 0, t_topRight.y), new Vector3(t_bottomRight.x, 0, t_bottomRight.y));
            Gizmos.DrawLine(new Vector3(t_bottomRight.x, 0, t_bottomRight.y), new Vector3(t_bottomLeft.x, 0, t_bottomLeft.y));
            Gizmos.DrawLine(new Vector3(t_bottomLeft.x, 0, t_bottomLeft.y), new Vector3(t_topLeft.x, 0, t_topLeft.y));

            Gizmos.color = Color.blue;

            Gizmos.DrawLine(new Vector3(sides[0].x, 0, sides[0].y), new Vector3(sides[1].x, 0, sides[1].y));
            Gizmos.DrawLine(new Vector3(sides[1].x, 0, sides[1].y), new Vector3(sides[3].x, 0, sides[3].y));
            Gizmos.DrawLine(new Vector3(sides[3].x, 0, sides[3].y), new Vector3(sides[2].x, 0, sides[2].y));
            Gizmos.DrawLine(new Vector3(sides[2].x, 0, sides[2].y), new Vector3(sides[0].x, 0, sides[0].y));

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(new Vector3(carPos.x, 0, carPos.y), 0.5f);
        }

        /// <summary>
        /// 获取沿z轴逆时针旋转的矩阵
        /// </summary>
        /// <param name="m"></param>
        /// <param name="angle">沿原点到z轴正半轴的顺时针角度</param>
        /// <returns></returns>
        private Matrix4x4 getZRotationMatrix(Matrix4x4 m, float angle)
        {
            m.m10 = Mathf.Sin(angle * Mathf.Deg2Rad);
            m.m01 = -Mathf.Sin(angle * Mathf.Deg2Rad);
            m.m00 = m.m11 = Mathf.Cos(angle * Mathf.Deg2Rad);
            return m;
        }

        /// <summary>
        /// 获取沿x轴旋转的矩阵
        /// </summary>
        /// <param name="m"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        private Matrix4x4 getXRotationMatrix(Matrix4x4 m, float angle)
        {
            m.m12 = -Mathf.Sin(angle * Mathf.Deg2Rad);
            m.m21 = Mathf.Sin(angle * Mathf.Deg2Rad);
            m.m11 = m.m22 = Mathf.Cos(angle * Mathf.Deg2Rad);
            return m;
        }


        /// <summary>
        /// 返回沿x,y轴移动的矩阵
        /// </summary>
        /// <param name="m"></param>
        /// <param name="move"></param>
        /// <returns></returns>
        private Matrix4x4 getXYMoveMatrix(Matrix4x4 m, Vector3 move)
        {
            m.m00 = m.m11 = m.m22 = m.m33 = 1;
            m.m03 = move.x;
            m.m13 = move.y;
            return m;
        }

        /// <summary>
        /// 沿x,y轴统一缩放矩阵
        /// </summary>
        /// <param name="m"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Matrix4x4 getXYScaleMatrix(Matrix4x4 m, float scale)
        {
            m.m00 = m.m11 = scale;
            m.m22 = m.m33 = 1;
            return m;
        }
    }
}

