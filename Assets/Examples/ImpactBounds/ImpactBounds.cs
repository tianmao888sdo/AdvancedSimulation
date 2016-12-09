using UnityEngine;
using System.Collections;

public class ImpactBounds : MonoBehaviour {

    [SerializeField]
    private Camera rotateCamera;
    [SerializeField]
    private float angle;
    /// <summary>
    /// 摄像机相对于自己的位移量，原始位置为世界00点
    /// </summary>
    [SerializeField]
    private Vector3 move=Vector3.zero;
    [SerializeField]
    private float mapWidth = 5;
    [SerializeField]
    private float mapHeight = 5;

    Vector3 topLeft;
    Vector3 topRight;
    Vector3 bottomLeft;
    Vector3 bottomRight;

    [SerializeField]
    private Vector3[] temp=new  Vector3[4];
    [SerializeField]
    private Vector3 cameraPos;

    //Position 沿y轴增加5个单位，绕y轴旋转45度，缩放2倍
    //m1.SetTRS(Vector3.up* 5, Quaternion.Euler(Vector3.up* 45.0f), Vector3.one* 2.0f);
    // 也可以使用如下静态方法设置m1变换
    //m1 = Matrix4x4.TRS(Vector3.up * 5, Quaternion.Euler(Vector3.up * 45.0f), Vector3.one * 2.0f);
    //v2 = m1.MultiplyPoint3x4(v1);

    private Matrix4x4 m=new Matrix4x4();



    // Use this for initialization
    void Start () {
        Texture2D t = Resources.Load("TextureScale/1_1") as Texture2D;
        Sprite s = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
    }

    /// <summary>
    /// 根据相机自身旋转和相对自身的位移计算实际世界位移
    /// </summary>
    /// <param name="localMove">相对相机自身坐标系位移</param>
    /// <param name="localAngle">相机坐标系相对世界y方向的z轴旋转角度</param>
    /// <returns>世界坐标系位移</returns>
    private Vector3 GetWorldMove(Vector3 localMove,float localAngle)
    {
        float moveX = localMove.y * Mathf.Sin(localAngle * Mathf.Deg2Rad)+ localMove.x * Mathf.Cos(localAngle * Mathf.Deg2Rad);
        float moveY = localMove.y * Mathf.Sin((localAngle + 90) * Mathf.Deg2Rad) + localMove.x * Mathf.Cos((localAngle + 90) * Mathf.Deg2Rad);
        return new Vector3(moveX, moveY, 0);
    }

    public Vector3 preMousePos=Vector3.zero;
    public Vector3 preMouseMove = Vector3.zero;
    public Vector3 mouseMoved= Vector3.zero;
    public bool isMouseDown = false;


    private Vector3 preCameraMoved;
    public Vector3 cameraMoved = Vector3.zero;
    public Vector3 cacheMouseMoved = Vector3.zero;

    void FixedUpdate()
    {
        if(!isMouseDown)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isMouseDown = true;
                preMousePos = Input.mousePosition;
            }
        }
        else
        {
            if(Input.GetMouseButtonUp(0))
            {
                preMousePos = Vector3.zero;
                isMouseDown = false;
                mouseMoved = Vector3.zero;
                return;
            }

            if(Input.mousePosition!=preMousePos)
            {
                mouseMoved = Input.mousePosition - preMousePos;
                cacheMouseMoved += mouseMoved * 0.1f;
                cameraMoved += GetWorldMove(mouseMoved*0.1f, angle);
                preMousePos = Input.mousePosition;
            }
            else
            {
                mouseMoved = Vector3.zero;
            }
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        topLeft = new Vector3(-mapWidth * 0.5f, mapHeight * 0.5f, 0)- cacheMouseMoved;
        topRight=new Vector3(mapWidth * 0.5f, mapHeight * 0.5f, 0) - cacheMouseMoved;
        bottomLeft = new Vector3(-mapWidth * 0.5f, -mapHeight * 0.5f, 0) - cacheMouseMoved;
        bottomRight = new Vector3(mapWidth * 0.5f, -mapHeight * 0.5f, 0) - cacheMouseMoved;

        m.SetTRS(Vector3.zero, Quaternion.AngleAxis(-angle, -Vector3.forward), Vector3.one);

        temp[0] = m.MultiplyPoint3x4(topLeft);
        temp[1] = m.MultiplyPoint3x4(topRight);
        temp[2] = m.MultiplyPoint3x4(bottomLeft);
        temp[3] = m.MultiplyPoint3x4(bottomRight);
        float top =temp[0].y, left= temp[0].x, right= temp[0].x, bottom= temp[0].y;

        for(int i=1;i<4;i++)
        {
            if (temp[i].x >= right)
            {
                right = temp[i].x;
            }

            if (temp[i].y >= top)
            {
                top = temp[i].y;
            }

            if (temp[i].x <= left)
            {
                left = temp[i].x;
            }

            if (temp[i].y <= bottom)
            {
                bottom = temp[i].y;
            }
        }

        if (cacheMouseMoved.x - rotateCamera.orthographicSize < left || cacheMouseMoved.x+rotateCamera.orthographicSize > right)
        {
            cameraMoved = preCameraMoved;
            cacheMouseMoved = preMouseMove;
            return;
        }

        if (cacheMouseMoved.y - rotateCamera.orthographicSize < bottom || cacheMouseMoved.y + rotateCamera.orthographicSize > top)
        {
            cameraMoved = preCameraMoved;
            cacheMouseMoved = preMouseMove;
            return;
        }

        preCameraMoved = cameraMoved;
        preMouseMove = cacheMouseMoved;
        rotateCamera.transform.position =new Vector3(cameraMoved.x, cameraMoved.y,-10);
        rotateCamera.transform.localRotation = Quaternion.AngleAxis(angle, -Vector3.forward);
    }

 
}
