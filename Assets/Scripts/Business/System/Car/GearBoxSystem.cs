using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 变速箱
/// </summary>
public class GearBoxSystem : MonoBase, IGearBox,IRelease
{
    /// <summary>
    /// 档位
    /// </summary>
    public struct Gear
    {
        /// <summary>
        /// 质量
        /// </summary>
        public float mass;

        /// <summary>
        /// 半径
        /// </summary>
        public float radius;

        /// <summary>
        /// 传动比
        /// </summary>
        public float speedRatio;

        /// <summary>
        /// 传动效率
        /// </summary>
        public float efficiency;

        public Gear(float speedRatio,float efficiency, float mass=0, float radius=0)
        {
            this.speedRatio = speedRatio;
            this.efficiency = efficiency;
            this.mass = mass;
            this.radius = radius;
        }
    }

    /// <summary>
    /// 齿轮速比
    /// </summary>
    private Gear[] m_gears;

    /// <summary>
    /// 输入
    /// </summary>
    [SerializeField]
    private int m_input = 0;

    /// <summary>
    /// 输出
    /// </summary>
    [SerializeField]
    private int m_output = 0;

    /// <summary>
    /// 机械效率，热能，粘滞阻力带来的损失
    /// </summary>
    [SerializeField][Range(0,1f)]
    private float m_mechanicalEfficiency = 1f;

    /// <summary>
    /// 质量
    /// </summary>
    [SerializeField]
    private float m_mass=5f;

    /// <summary>
    /// 当前档位
    /// </summary>
    private int m_gearIndex = 0;

    /// <summary>
    /// 离合
    /// </summary>
    private float m_clutch=0f;

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
			float t_torque = m_mechanicalEfficiency * (m_input / m_output) * (1 - m_clutch) * m_torqueInput * m_gears [m_gearIndex].speedRatio * m_gears [m_gearIndex].efficiency;
			t_torque = Mathf.Max (0.000000001f, t_torque);
			return t_torque;
        }
    }

    /// <summary>
    /// 当前转动惯量，惯量会带啦扭矩损失
    /// </summary>
    public float MomentofInertia { get { return m_gears[m_gearIndex].mass* m_gears[m_gearIndex].radius* m_gears[m_gearIndex].radius; } }

    /// <summary>
    /// 角加速度
    /// </summary>
    public float ShaftAngleAcc { get { return 0f; } }

    public override void Init()
    {
        //读取配置文件获得参数

        m_gears = new Gear[]
        {
            new Gear(7.31f,0.966f),
            new Gear(4.31f,0.967f),
            new Gear(2.45f,0.972f),
            new Gear(1.54f,0.973f),
            new Gear(1f,0.975f),
            new Gear(7.66f,0.95f)
        };

        m_input = 145;
        m_output = 145;
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
