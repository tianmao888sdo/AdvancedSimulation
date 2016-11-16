using System.Collections;
using System;
/// <summary>
/// 追踪数值的变化量,当变化量超过限值时返回True
/// </summary>
public class ChangeTrace
{

    float lastVal = 0;
    float changeVal = 0;
    float changeValSq = 0;
    /// <summary>
    /// 追踪数值的变化量
    /// </summary>
    /// <param name="_val">变化量的初始值</param>
    /// <param name="_changeVal">追踪的变化幅度</param>
    public ChangeTrace(float _val, float _changeVal)
    {
        lastVal = _val;
        changeVal = _changeVal;
        changeValSq = changeVal * changeVal;
    }
    /// <summary>
    /// 参数值是否发生变化
    /// </summary>
    /// <param name="_val"></param>
    /// <returns></returns>
    public bool IsChange(float _val)
    {
        if ((_val - lastVal) * (_val - lastVal) >= changeValSq)
        {
            lastVal = _val;
            return true;
        }
        else
        {
            return false;
        }
    }
    /// <summary>
    /// 修改变化幅度
    /// </summary>
    /// <param name="_changeVal"></param>
    public void SetChangeVal(float _changeVal)
    {
        changeVal = _changeVal;
        changeValSq = changeVal * changeVal;
    }

}
