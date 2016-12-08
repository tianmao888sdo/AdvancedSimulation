using UnityEngine;
using System.Collections;

public class ImpactBounds : MonoBehaviour {

    [SerializeField]
    private Camera rotateCamera;
    [SerializeField]
    private float angle;
    [SerializeField]
    private Vector3 move;

    [SerializeField]
    private float mapWidth = 5;
    [SerializeField]
    private float mapHeight = 5;

    Vector3 topLeft = new Vector3(-1, 1,0);
    Vector3 topRight = new Vector3(1, 1,0);
    Vector3 bottomLeft = new Vector3(-1, -1,0);
    Vector3 bottomRight = new Vector3(1, -1,0);

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
        GetComponent<SpriteRenderer>().sprite = s;
    }
	
	// Update is called once per frame
	void Update () {
        m.SetTRS(move, Quaternion.AngleAxis(angle, Vector3.forward), Vector3.one);

        Vector3[] temp = new Vector3[] { m.MultiplyPoint3x4(topLeft), m.MultiplyPoint3x4(topRight), m.MultiplyPoint3x4(bottomLeft), m.MultiplyPoint3x4(bottomRight) };
        float top=temp[0].x, left= temp[0].x, right= temp[0].y, bottom= temp[0].y;

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

        if (left < -mapWidth / 2 || right > mapWidth / 2)
        {
            if (top <= mapHeight / 2 && bottom >= -mapHeight / 2)
            {
                rotateCamera.transform.position = new Vector3(rotateCamera.transform.position.x,move.y,-10f);
                rotateCamera.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
                return;
            }
        }
        else
        {
            

            if (top <= mapHeight / 2 && bottom >= -mapHeight / 2)
            {
                rotateCamera.transform.position = new Vector3(rotateCamera.transform.position.x, move.y, -10f);
                rotateCamera.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
                return;
            }
            else
            {
                rotateCamera.transform.position = new Vector3(move.x, rotateCamera.transform.position.y, -10f);
                return;
            }

        }

    }

 
}
