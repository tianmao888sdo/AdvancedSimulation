using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 变速箱
/// </summary>
public class GearBoxSystem : ScriptBase,IRelease
{
    private int m_gear = 0;
    private float m_rpmInput;

    public float OutputRPM
    {
        get
        {
            if (m_gear == 0)
            {

            }
            else if (m_gear == 1)
            {
                return m_gear * m_rpmInput;
            }
            else if (m_gear == 2)
            {

            }
            else if (m_gear == 3)
            {


            }
            else if (m_gear == 4)
            {


            }
            else if (m_gear == 5)
            {


            }
            else if (m_gear == -1)
            {

            }

            return 0;

        }
    }

    public override void Init()
    {
        throw new NotImplementedException();
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
    /// 
    /// </summary>
    /// <param name="val"></param>
    public void SetGear(int val)
    {
        m_gear = val;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="val"></param>
    public void SetRPM(float val)
    {
        m_rpmInput = val;
    }

    public void Release(bool destroy = false)
    {
        throw new NotImplementedException();
    }
}
