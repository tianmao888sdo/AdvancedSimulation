using UnityEngine;

public class Car
{
    public float altitudeOffset;//车在水平状态下，前后天线组成的连线可能存在夹角，上行机会把这个叫当成有仰俯角，所以计算时需要减去
    public float backAerialHeight;//后天线点在平地时相对轮廓面的距离
    public Vector2[] origianlPoints;//车的周围点，水平时相对后天线位置偏移（x,y都表示相对偏移量）
    public Vector2 frontAerial;//前天线点位置，水平时相对后天线位置偏移（x,y都表示相对偏移量）
    //未投影四个轮子相对基准点的坐标偏移
    public Vector2 posWheelFL = Vector2.zero;
    public Vector2 posWheelRL = Vector2.zero;
    public Vector2 posWheelFR = Vector2.zero;
    public Vector2 posWheelRR = Vector2.zero;
    //未投影车外轮廓四个角相对基准点的坐标偏移
    public Vector2 posFL = Vector2.zero;
    public Vector2 posRL = Vector2.zero;
    public Vector2 posFR = Vector2.zero;
    public Vector2 posRR = Vector2.zero;

    public float altitudeAngle;//杨俯角（度）通过计算得到
    public float pathAngle;//航向角，z轴负方向，相对y轴正半轴顺时针
    public Vector2 backAerial;//后天线位置

    public Vector2[] projectedPoints;//投影后相对后天线的位置偏移，（x,y都表示相对世界原点的偏移量）
    public Vector2 projectedFrontAerial;//投影后的天线位置

    /// <summary>
    /// 初始化数据
    /// </summary>
    /// <param name="origianlPoints">车身所有相对基准点的点，水平时相对后天线位置偏移（x,y都表示相对偏移量）</param>
    public void Init(Vector2[] origianlPoints)
    {
        int len = origianlPoints.Length;
        this.origianlPoints = new Vector2[len];

        for (int i = 0; i < len; i++)
        {
            this.origianlPoints[i].x = Mathf.Sin(origianlPoints[i].y) * origianlPoints[i].x;
            this.origianlPoints[i].y = Mathf.Cos(origianlPoints[i].y) * origianlPoints[i].x;
        }
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    /// <param name="origianlPoints">车的周围点，水平时相对后天线位置偏移（x,y都表示相对偏移量）</param>
    /// <param name="frontAerial">前天线点位置，水平时相对后天线位置偏移（x,y都表示相对偏移量）</param>
    /// <param name="backAerialHeight">后天线点在平地时相对轮廓面的距离</param>
    /// <param name="altitudeOffset">水平时杨俯角偏移量，计算是需要减去</param>
    public void Init(Vector2[] origianlPoints,Vector2 frontAerial,float backAerialHeight, float altitudeOffset = 0)
    {
        int len = origianlPoints.Length;
        this.origianlPoints = new Vector2[len];
        this.projectedPoints = new Vector2[len];

        for (int i=0;i< len;i++)
        {
            this.origianlPoints[i].x = Mathf.Sin(origianlPoints[i].y) * origianlPoints[i].x;
            this.origianlPoints[i].y = Mathf.Cos(origianlPoints[i].y) * origianlPoints[i].x;
        }

        this.frontAerial = frontAerial;
        this.backAerialHeight = backAerialHeight;
        this.altitudeOffset = altitudeOffset;
    }
}
