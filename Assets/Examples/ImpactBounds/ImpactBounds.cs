﻿using UnityEngine;
using System.Collections;

/// <summary>
/// 地图旋转方案，要求能旋转位移，移动相机，地图要围绕相机中心点旋转显示
/// 应该还需要增加一个相机初始旋转矩阵，适应各种方向的地图
/// </summary>
public class ImpactBounds : MonoBehaviour
{
    public Camera mainCamera;

    /// <summary>
    /// 相机坐标系在世界中累计移动的向量
    /// </summary>
    public Vector3 worldMove;

    /// <summary>
    /// 相机相对世界的累计旋转角度
    /// </summary>
    public float totalAngle;

    //地图宽高
    public float mapWidth = 0;
    public float mapHeight = 0;

    /// <summary>
    /// 相机深度
    /// </summary>
    public float cameraFar = 10f;

    /// <summary>
    /// 4至点坐标
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
    /// 保存变换后的4至点坐标
    /// </summary>
    [SerializeField]
    private Vector3[] sides = new Vector3[4];

    /// <summary>
    /// 记录可移动范围，相机坐标系
    /// </summary>
    [SerializeField]
    private float top;
    [SerializeField]
    private float bottom ;
    [SerializeField]
    private float left;
    [SerializeField]
    private float right;

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
    private Color warningColor=Color.green;

    private bool isMouseDown = false;
    private Vector3 preMousePos;

    // Use this for initialization
    void Start()
    {
        cameraM=totalM = totalRotationM = moveM = rotationM = getDefaultMatrix();
        cameraM = getXRotationMatrix(cameraM, 90f);
        topLeft = new Vector3(-mapWidth * 0.5f, mapHeight * 0.5f, 0);
        topRgiht = new Vector3(mapWidth * 0.5f, mapHeight * 0.5f, 0);
        bottomLeft = new Vector3(-mapWidth * 0.5f, -mapHeight * 0.5f, 0);
        bottomRight = new Vector3(mapWidth * 0.5f, -mapHeight * 0.5f, 0);

        sides[0] = topLeft;
        sides[1] = topRgiht;
        sides[2] = bottomLeft;
        sides[3] = bottomRight;
        MapSelfAdapt(totalAngle);
    }

    /// <summary>
    /// 地图自适应
    /// </summary>
    private void MapSelfAdapt(float totalAngle)
    {
        SetRotation(totalAngle);
        MoveAndRotatCamera(Vector3.zero, totalAngle, totalM);
        mainCamera.orthographicSize=GetMapHeight() * 0.5f;
    }

    /// <summary>
    /// 鼠标移动单位转化为世界地图移动单位
    /// </summary>
    /// <returns></returns>
    private Vector3 MouseToWorld(Vector3 deltaMove)
    {
        deltaMove.y /= (Screen.height * 0.5f / mainCamera.orthographicSize);
        deltaMove.x /= (Screen.height * mainCamera.aspect * 0.5f / mainCamera.orthographicSize);
        return deltaMove;
    }

    /// <summary>
    /// 设置移动值
    /// </summary>
    /// <param name="deltaMove"></param>
    private void SetMove(Vector3 deltaMove)
    {
        worldMove += totalRotationM.inverse.MultiplyPoint(deltaMove);
        //地图相对相机坐标系的移动矩阵，与鼠标拖拽方向相反
        moveM = getXYMoveMatrix(moveM, -deltaMove);
        totalM = moveM * totalM;//一定要左乘
    }

    /// <summary>
    /// 设置旋转值
    /// </summary>
    private void SetRotation(float deltaAngle)
    {
        //计算相机相对自身的旋转
        totalAngle += deltaAngle;
        totalRotationM = getZRotationMatrix(totalRotationM, totalAngle);
        //地图相对相机的逆旋转矩阵
        rotationM = getZRotationMatrix(rotationM, deltaAngle);
        totalM = rotationM * totalM;//一定要左乘
    }

    /// <summary>
    /// 围绕鼠标位置缩放
    /// </summary>
    /// <param name="deltaScale"></param>
    private void SetScale(float deltaScale)
    {
        float y = 2f*(Input.mousePosition.y - 0.5f * Screen.height) * deltaScale/Screen.height;
        float x = 2f * (Input.mousePosition.x - 0.5f * Screen.width) * mainCamera.aspect * deltaScale / Screen.width;
        mainCamera.orthographicSize += deltaScale;
        SetMove(-new Vector3(x, y, 0));
    }

    /// <summary>
    /// 执行移动相机，并旋转相机到指定角度
    /// </summary>
    /// <param name="totalMove">相机在世界的总移动</param>
    /// <param name="totalAngle">相机绕自身旋转的总角度，延z轴的负轴旋转</param
    /// <param name="m">相机相对世界变换的逆变换</param>
    private void MoveAndRotatCamera(Vector3 totalMove,float totalAngle,Matrix4x4 m)
    {
        //计算相机在世界中实际位移
        mainCamera.transform.localPosition = new Vector3(worldMove.x, cameraFar, worldMove.y);
        //计算相机在世界中实际旋转
        mainCamera.transform.localRotation = Quaternion.Euler(90,0f,-totalAngle);

        sides[0] = m.MultiplyPoint(topLeft);
        sides[1] = m.MultiplyPoint(topRgiht);
        sides[2] = m.MultiplyPoint(bottomLeft);
        sides[3] = m.MultiplyPoint(bottomRight);
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
        if (isMouseDown)
        {
            if (Input.GetMouseButtonUp(0))
            {
                isMouseDown = false;
                return;
            }

            if (Input.mousePosition != preMousePos)
            {
                Vector3 deltaMove = MouseToWorld(Input.mousePosition - preMousePos);
                SetMove(-deltaMove);
                preMousePos = Input.mousePosition;
            }

            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                float deltaAngle = Input.GetAxis("Mouse ScrollWheel") * 10;
                SetRotation(deltaAngle);
            }

            Vector3 scale = Vector3.zero;

            if (Input.GetKey(KeyCode.A))//放大
            {
                SetScale(Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.B))//缩小
            {
                SetScale(-Time.deltaTime);
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
        //四至点转化成相机坐标系
        MoveAndRotatCamera(worldMove, totalAngle, totalM);

        Vector3 t_moveBack;

        if (!IsRangeValid(out t_moveBack))
        {
            SetMove(t_moveBack);
            MoveAndRotatCamera(worldMove, totalAngle, totalM);
            warningColor = Color.red;
        }
        else
        {
            warningColor = Color.green;
        }
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

        Gizmos.DrawLine(new Vector3(t_topLeft.x,0, t_topLeft.y),new Vector3(t_topRight.x,0, t_topRight.y));
        Gizmos.DrawLine(new Vector3(t_topRight.x, 0, t_topRight.y), new Vector3(t_bottomRight.x, 0, t_bottomRight.y));
        Gizmos.DrawLine(new Vector3(t_bottomRight.x, 0, t_bottomRight.y), new Vector3(t_bottomLeft.x, 0, t_bottomLeft.y));
        Gizmos.DrawLine(new Vector3(t_bottomLeft.x, 0, t_bottomLeft.y), new Vector3(t_topLeft.x, 0, t_topLeft.y));

        Gizmos.color = Color.blue;

        Gizmos.DrawLine(new Vector3(sides[0].x, 0, sides[0].y), new Vector3(sides[1].x, 0, sides[1].y));
        Gizmos.DrawLine(new Vector3(sides[1].x, 0, sides[1].y), new Vector3(sides[3].x, 0, sides[3].y));
        Gizmos.DrawLine(new Vector3(sides[3].x, 0, sides[3].y), new Vector3(sides[2].x, 0, sides[2].y));
        Gizmos.DrawLine(new Vector3(sides[2].x, 0, sides[2].y), new Vector3(sides[0].x, 0, sides[0].y));
    }

    /// <summary>
    /// 获取对角单位矩阵
    /// </summary>
    /// <returns></returns>
    private Matrix4x4 getDefaultMatrix()
    {
        Matrix4x4 m = Matrix4x4.zero;
        m.m00 = m.m11 = m.m22 = m.m33 = 1;
        return m;
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
        m.m00 =m.m11 = Mathf.Cos(angle * Mathf.Deg2Rad);
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
        m.m12= -Mathf.Sin(angle * Mathf.Deg2Rad);
        m.m21= Mathf.Sin(angle * Mathf.Deg2Rad);
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