using UnityEngine;
using System.Collections;

public class TextureScale : MonoBehaviour
{
    [SerializeField]
    private Camera mapCamera;

    /// <summary>
    /// 地图级别1-5
    /// </summary>
    [SerializeField]
    [Range(1, 5)]
    private float mapLevel = 1;//1-5

    /// <summary>
    /// 上一个地图级别
    /// </summary>
    private int preMapLevel = 1;

    /// <summary>
    /// 相机size倍数
    /// </summary>
    [SerializeField]
    private float cameraSizeRate = 1f;

    /// <summary>
    /// 原始像素密度为100个一单位
    /// </summary>
    [SerializeField]
    private int OraglePixelPerUnit = 100;

    /// <summary>
    /// 缓存不同级别tex
    /// </summary>
    private Sprite[] cacheSprites = new Sprite[5]; 


    public void CacheTextures()
    {
        for (int i = 1; i <= 5; i++)
        {
            Texture2D t = Resources.Load("TextureScale/1_" + i) as Texture2D;
            Sprite s = Sprite.Create(t, new Rect(0, 0,t.width, t.height), new Vector2(0.5f, 0.5f), OraglePixelPerUnit/(Mathf.Pow(2,i-1)));
            cacheSprites[i-1] = s;
        }

    }

    /// <summary>
    /// 改变图片级别
    /// </summary>
    /// <param name="level"></param>
    public void ChangeTexure(int level)
    {
        GetComponent<SpriteRenderer>().sprite = cacheSprites[level-1];
    }

	// Use this for initialization
	void Start () {
        CacheTextures();
        ChangeTexure(1);
        preMapLevel = 1;
    }

    // Update is called once per frame
    void Update () {
        mapCamera.orthographicSize = mapLevel*cameraSizeRate;

        if(Mathf.Abs(preMapLevel- Mathf.RoundToInt(mapLevel)) >=1)
        {
            ChangeTexure(Mathf.RoundToInt(mapLevel));
            preMapLevel = Mathf.RoundToInt(mapLevel);
        }
    }
}
