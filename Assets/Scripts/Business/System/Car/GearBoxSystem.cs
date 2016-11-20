using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 变速箱
/// </summary>
public class GearBoxSystem : ScriptBase, IGearBox,IRelease
{
    /// <summary>
    /// 齿轮速比
    /// </summary>
    [SerializeField]
    private float[] speedRatios;

    /// <summary>
    /// 输入齿轮数
    /// </summary>
    [SerializeField]
    private int input = 0;

    /// <summary>
    /// 输出齿轮数
    /// </summary>
    [SerializeField]
    private int output = 0;

    /// <summary>
    /// 机械效率
    /// </summary>
    [SerializeField]
    private float mechanicalEfficiency = 1f;

    /// <summary>
    /// 质量
    /// </summary>
    [SerializeField]
    private float mass=5f;

    /// <summary>
    /// 离合
    /// </summary>
    private float m_clutch=0f;

    /// <summary>
    /// 当前档位
    /// </summary>
    private int m_gearIndex = 0;

    /// <summary>
    /// 输入扭矩
    /// </summary>
    private float m_torqueInput;

    /// <summary>
    /// 输出扭矩
    /// </summary>
    public float OutputTorque
    {
        get
        {
            return mechanicalEfficiency*(input / output) * (1- m_clutch) * m_torqueInput * speedRatios[m_gearIndex];
        }
    }

    public override void Init()
    {
        //读取配置文件获得参数
        speedRatios = new float[]{7.31f,4.31f,2.45f,1.54f,1.0f,7.66f};
        input = 145;
        output = 145;
    }

    public override void Play()
    {
        throw new NotImplementedException();
    }

    public override void Stop()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 设置挡位
    /// </summary>
    /// <param name="val"></param>
    public void SetGear(int val)
    {
        m_gearIndex = val;
    }

    /// <summary>
    /// 设置离合
    /// </summary>
    /// <param name="val"></param>
    public void SetClutch(float val)
    {
        m_clutch = val;
    }

    /// <summary>
    /// 设置扭矩
    /// </summary>
    /// <param name="val"></param>
    public void SetTorque(float val)
    {
        m_torqueInput = val;
    }

    public void Release(bool destroy = false)
    {
        throw new NotImplementedException();
    }
}
