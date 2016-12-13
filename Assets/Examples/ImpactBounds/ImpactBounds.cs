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

    /// <summary>
    /// 还原点
    /// </summary>
    private Vector3 preWorldMove;

    /// <summary>
    /// 还原角度
    /// </summary>
    private float preTotalAngle;


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
    private Vector3[] sides = new Vector3[4];

    /// <summary>
    /// 相机实际旋转矩阵
    /// </summary>
    public Matrix4x4 cameraM = Matrix4x4.zero;
    /// <summary>
    /// 相对相机坐标系的位移矩阵，把相机看成与世界坐标系一致的
    /// </summary>
    public Matrix4x4 moveM = Matrix4x4.zero;
    /// <summary>
    /// 相对相机坐标系的旋转矩阵，把相机看成与世界坐标系一致的
    /// </summary>
    public Matrix4x4 rotationM = Matrix4x4.zero;
    /// <summary>
    /// 相对相机坐标系的缩放矩阵，把相机看成与世界坐标系一致的
    /// </summary>
    public Matrix4x4 scaleM = Matrix4x4.zero;

    private bool isMouseDown = false;
    private Vector3 preMousePos;

    // Use this for initialization
    void Start()
    {
        cameraM = moveM = rotationM = scaleM = getDefaultMatrix(); ;
        topLeft = new Vector3(-mapWidth * 0.5f, mapHeight * 0.5f, 0);
        topRgiht = new Vector3(mapWidth * 0.5f, mapHeight * 0.5f, 0);
        bottomLeft = new Vector3(-mapWidth * 0.5f, -mapHeight * 0.5f, 0);
        bottomRight = new Vector3(mapWidth * 0.5f, -mapHeight * 0.5f, 0);

        sides[0] = topLeft;
        sides[1] = topRgiht;
        sides[2] = bottomLeft;
        sides[3] = bottomRight;
    }

    void FixedUpdate()
    {
        float top = sides[0].y;
        float bottom = sides[0].y;
        float left = sides[0].x;
        float right = sides[0].x;

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

        if (top < mainCamera.orthographicSize || bottom > -mainCamera.orthographicSize || right < mainCamera.orthographicSize || left > -mainCamera.orthographicSize)
        {
            //worldMove = preWorldMove;
            //totalAngle = preTotalAngle;
            //mainCamera.transform.position = new Vector3(worldMove.x, worldMove.y, -10);
            //mainCamera.transform.localRotation = Quaternion.AngleAxis(totalAngle, -Vector3.forward);
            Debug.Log(1111111111111);
            return;
        }

        preWorldMove = worldMove;
        preTotalAngle = totalAngle;
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
                Vector3 deltaMove = (Input.mousePosition - preMousePos) / (Screen.width * 0.5f);
                //因为是从原点到z轴正方向看是顺时针，但实际在z轴正方向看是逆时针，所以-angle为我们所要的正时针旋转矩阵所要角度
                cameraM = getZRotationMatrix(cameraM, -totalAngle);
                worldMove += cameraM.MultiplyPoint(deltaMove);
                //计算相机在世界中实际位移
                mainCamera.transform.position = new Vector3(worldMove.x, worldMove.y, -10);
                //地图相对相机坐标系的移动矩阵，与鼠标拖拽方向相反
                moveM = getXYMoveMatrix(moveM, -deltaMove);

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
                //计算相机在世界中实际旋转
                mainCamera.transform.localRotation = Quaternion.AngleAxis(totalAngle, -Vector3.forward);
                //地图相对相机的逆旋转矩阵
                rotationM = getZRotationMatrix(rotationM, deltaAngle);

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
