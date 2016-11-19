using UnityEngine;
using System.Collections;

public class ClutchSystem : MonoBehaviour {

    private float m_clutchInput = 0f;
    /// <summary>
    /// 外部离合输入
    /// </summary>
    public void SetClutchInput(float val)
    {
        m_clutchInput = val;
    }
}
