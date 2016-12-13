using UnityEngine;
using System.Collections;

/// <summary>
/// 地图旋转方案，要求能旋转位移，移动相机，地图要围绕相机中心点旋转显示
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
    /// 4至点坐标
    /// </summary>
    public Vector3 topLeft;
    public Vector3 topRgiht;
    public Vector3 bottomLeft;
    public Vector3 bottomRight;

    /// <summary>
    /// 保存变换后的4至点坐标
    /// </summary>
    public Vector3[] sides = new Vector3[4];

    /// <summary>
    /// 记录可移动范围
    /// </summary>
    public float top;
    public float bottom ;
    public float left;
    public float right;

    /// <summary>
    /// 相机实际旋转矩阵
    /// </summary>
    public Matrix4x4 cameraM = Matrix4x4.zero;

    /// <summary>
    /// 相对相机坐标系的位移矩阵，把相机看成与世界坐标系一致的
    /// </summary>
    public Matrix4x4 moveM = Matrix4x4.zero;

    /// <summary>
    /// 累计的移动矩阵
    /// </summary>
    public Matrix4x4 totalMoveM = Matrix4x4.zero;

    /// <summary>
    /// 相对相机坐标系的旋转矩阵，把相机看成与世界坐标系一致的
    /// </summary>
    public Matrix4x4 rotationM = Matrix4x4.zero;

    /// <summary>
    /// 累计旋转矩阵
    /// </summary>
    public Matrix4x4 totalRotationM = Matrix4x4.zero;

    /// <summary>
    /// 相对相机坐标系的缩放矩阵，把相机看成与世界坐标系一致的
    /// </summary>
    public Matrix4x4 scaleM = Matrix4x4.zero;

    private bool isMouseDown = false;
    private Vector3 preMousePos;

    // Use this for initialization
    void Start()
    {
        totalMoveM=totalRotationM = cameraM = moveM = rotationM = scaleM = getDefaultMatrix();
        topLeft = new Vector3(-mapWidth * 0.5f, mapHeight * 0.5f, 0);
        topRgiht = new Vector3(mapWidth * 0.5f, mapHeight * 0.5f, 0);
        bottomLeft = new Vector3(-mapWidth * 0.5f, -mapHeight * 0.5f, 0);
        bottomRight = new Vector3(mapWidth * 0.5f, -mapHeight * 0.5f, 0);

        sides[0] = topLeft;
        sides[1] = topRgiht;
        sides[2] = bottomLeft;
        sides[3] = bottomRight;
    }

    /// <summary>
    /// 绘制边框
    /// </summary>
    void OnDrawGizmos()
    {
        //范围框永远是相对相机原始中心点旋转的，只跟旋转有关，上下左右对称
        float w = Mathf.Abs(left - right)*0.5f;
        float h = Mathf.Abs(top - bottom)*0.5f;

        Vector2 t_topLeft = new Vector2(-w, h);
        Vector2 t_topRight = new Vector2(w, h);
        Vector2 t_bottomLeft = new Vector2(-w, -h);
        Vector2 t_bottomRight = new Vector2(w, -h);

        //用相机的逆矩阵返回的是世界真正的坐标
        Matrix4x4 M = totalRotationM.inverse;

        t_topLeft = M.MultiplyPoint(t_topLeft);
        t_topRight = M.MultiplyPoint(t_topRight);
        t_bottomLeft = M.MultiplyPoint(t_bottomLeft);
        t_bottomRight = M.MultiplyPoint(t_bottomRight);

        Gizmos.DrawLine(t_topLeft, t_topRight);
        Gizmos.DrawLine(t_topRight, t_bottomRight);
        Gizmos.DrawLine(t_bottomRight, t_bottomLeft);
        Gizmos.DrawLine(t_bottomLeft, t_topLeft);
    }

    void FixedUpdate()
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

        Vector3 t_moveBack = Vector3.zero;

        if (top < mainCamera.orthographicSize)
        {
            t_moveBack.y = top - mainCamera.orthographicSize;
        }

        if(bottom > -mainCamera.orthographicSize)
        {
            t_moveBack.y = bottom + mainCamera.orthographicSize;
        }

        if(right < mainCamera.orthographicSize * mainCamera.aspect)
        {
            t_moveBack.x = right - mainCamera.orthographicSize * mainCamera.aspect;
        }

        if(left > -mainCamera.orthographicSize * mainCamera.aspect)
        {
            t_moveBack.x = left + mainCamera.orthographicSize * mainCamera.aspect;
        }

        if(t_moveBack != Vector3.zero)
        {
            //相机向相反方向移动，地图相当于朝相机方向移动
            moveM = getXYMoveMatrix(moveM, -t_moveBack);
            //每次对4至点做新的位移变换
            totalMoveM *= moveM;

            sides[0] = moveM.MultiplyPoint(sides[0]);
            sides[1] = moveM.MultiplyPoint(sides[1]);
            sides[2] = moveM.MultiplyPoint(sides[2]);
            sides[3] = moveM.MultiplyPoint(sides[3]);

            worldMove += totalRotationM.inverse.MultiplyPoint(t_moveBack);

            Debug.Log(11111111);
        }

        //计算相机在世界中实际位移
        mainCamera.transform.position = new Vector3(worldMove.x, worldMove.y, -10);
        //计算相机在世界中实际旋转
        mainCamera.transform.localRotation = Quaternion.AngleAxis(totalAngle, -Vector3.forward);
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
                Vector3 deltaMove = (Input.mousePosition - preMousePos);
                deltaMove.y/=(Screen.height * 0.5f / mainCamera.orthographicSize);
                deltaMove.x /= (Screen.height* mainCamera.aspect * 0.5f / mainCamera.orthographicSize);
                //因为是从原点到z轴正方向看是顺时针，但实际在z轴正方向看是逆时针，所以-angle为我们所要的正时针旋转矩阵所要角度
                worldMove += totalRotationM.inverse.MultiplyPoint(deltaMove);

                //地图相对相机坐标系的移动矩阵，与鼠标拖拽方向相反
                moveM = getXYMoveMatrix(moveM, -deltaMove);
                totalMoveM *= moveM;

                //每次对4至点做新的位移变换
                sides[0] = moveM.MultiplyPoint(sides[0]);
                sides[1] = moveM.MultiplyPoint(sides[1]);
                sides[2] = moveM.MultiplyPoint(sides[2]);
                sides[3] = moveM.MultiplyPoint(sides[3]);

                preMousePos = Input.mousePosition;

            }

            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                float deltaAngle = Input.GetAxis("Mouse ScrollWheel") * 10;
                totalAngle += deltaAngle;
                //地图相对相机的逆旋转矩阵
                rotationM = getZRotationMatrix(rotationM, deltaAngle);
                totalRotationM *= rotationM;

                sides[0] = rotationM.MultiplyPoint(sides[0]);
                sides[1] = rotationM.MultiplyPoint(sides[1]);
                sides[2] = rotationM.MultiplyPoint(sides[2]);
                sides[3] = rotationM.MultiplyPoint(sides[3]);
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
        m.m00 = Mathf.Cos(angle * Mathf.Deg2Rad);
        m.m10 = Mathf.Sin(angle * Mathf.Deg2Rad);
        m.m01 = -Mathf.Sin(angle * Mathf.Deg2Rad);
        m.m11 = Mathf.Cos(angle * Mathf.Deg2Rad);
        m.m22 = m.m33 = 1;
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
