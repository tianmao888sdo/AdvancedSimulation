using UnityEngine;
using System.Collections;

public class BrakeSystem : MonoBehaviour {

    public float maxBrakeTorque = 1000f;

    private float m_brakeInput = 0f;


    /// <summary>
    /// 外部刹车输入
    /// </summary>
    public void SetBrakeInput(float val)
    {
        m_brakeInput = val;
    }

    public float GetBrakeTorque()
    {
        return m_brakeInput * maxBrakeTorque;
    }
}
