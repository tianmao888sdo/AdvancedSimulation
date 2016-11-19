using UnityEngine;
using System.Collections;
using System;

public class EngineSystem:MonoBehaviour
{
    /// <summary>
    /// 最大转速，转速/分钟
    /// </summary>
    public float maxRMP = 7000f;

    /// <summary>
    /// 当前转速
    /// </summary>
    public float rmp = 0f;

    /// <summary>
    /// 转速加速性，转速/分钟
    /// 需要通过变速箱传回的力来计算真正的加速
    /// </summary>
    public float rmpAcceleration = 1000f;

    /// <summary>
    /// 转速减速性，转速/分钟
    /// 需要通过变速箱传回的力来计算真正的减速
    /// </summary>
    public float rmpDeceleration=1000f;

    /// <summary>
    /// 扭矩转速
    /// </summary>
    public float rpm = 0f;

    /// <summary>
    /// 功率
    /// </summary>
    public float watts = 0f;

    /// <summary>
    /// 燃油消耗
    /// </summary>
    public float fuelOil=0f;

    /// <summary>
    /// rpm曲线
    /// </summary>
    public AnimationCurve rpmCurve;

    private float m_throttleInput = 0f;

    /// <summary>
    /// 外部油门输入
    /// </summary>
    public void SetThrottleInput(float val)
    {
        m_throttleInput = val;

        if (m_throttleInput > 0)
        {
            rmp += m_throttleInput * rmpAcceleration * Time.deltaTime;
        }
        else
        {
            rmp -= rmpDeceleration * Time.deltaTime;
        }

        rmp = Mathf.Clamp(rmp, 0f, maxRMP);
        rpm = rmp * 0.1f;// rpmCurve.Evaluate(rmp);
    }
}
