using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 变速箱
/// </summary>
public class GearBoxSystem : ScriptBase, IGearBox,IRelease
{
    /// <summary>
    /// 档位
    /// </summary>
    public class Gear
    {
        /// <summary>
        /// 传动比
        /// </summary>
        public float speedRatio = 0f;

        /// <summary>
        /// 转动惯量
        /// </summary>
        public float momentofInertia = 0f;

        /// <summary>
        /// 传动效率
        /// </summary>
        public float efficiency = 0f;

        public Gear(float speedRatio,float momentofInertia,float efficiency)
        {
            this.speedRatio = speedRatio;
            this.momentofInertia = momentofInertia;
            this.efficiency = efficiency;
        }
    }

    /// <summary>
    /// 齿轮速比
    /// </summary>
    [SerializeField]
    private Gear[] gears;

    /// <summary>
    /// 输入
    /// </summary>
    [SerializeField]
    private int input = 0;

    /// <summary>
    /// 输出
    /// </summary>
    [SerializeField]
    private int output = 0;

    /// <summary>
    /// 机械效率，热能，粘滞阻力带来的损失
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
            return mechanicalEfficiency*(input / output) * (1- m_clutch) * m_torqueInput * gears[m_gearIndex].speedRatio* gears[m_gearIndex].efficiency;
        }
    }

    /// <summary>
    /// 转动惯量，惯量会带啦扭矩损失
    /// </summary>
    //public float MomentofInertia { get { return 0.5f * mass; } }

    public override void Init()
    {
        //读取配置文件获得参数

        gears = new Gear[]
        {
            new Gear(7.31f,1f,0.966f),
            new Gear(4.31f,1f,0.967f),
            new Gear(2.45f,1f,0.972f),
            new Gear(1.54f,1f,0.973f),
            new Gear(1f,1f,0.975f),
            new Gear(7.66f,1f,0.95f)
        };

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
