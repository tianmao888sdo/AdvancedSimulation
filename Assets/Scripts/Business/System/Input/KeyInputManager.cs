using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 键盘事件管理器
/// </summary>
public class KeyInputManager : ScriptBase
{
    #region 单例
    private static KeyInputManager instance;
    public static KeyInputManager Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    /// <summary>
    /// 轴委托
    /// </summary>
    /// <param name="_val"></param>
    public delegate void AxisDelegate(float number);

    /// <summary>
    /// 键盘输入模式
    /// </summary>
    public enum InputMode
    {
        Global,//默认，全局模式，该模式不论是什么场景，都有效
        CarControl,//车辆考试场景
        UI_1,//界面
        UI_2,//界面
        UI_3,//界面
    }

    private Dictionary<InputMode, Dictionary<KeyCode, Action>> m_dicKeyboard = new Dictionary<InputMode, Dictionary<KeyCode, Action>>();
    private Dictionary<InputMode, Dictionary<string, AxisDelegate>> m_dicAxis = new Dictionary<InputMode, Dictionary<string, AxisDelegate>>();

    private InputMode m_currInputMode = InputMode.Global;

	// Use this for initialization
	void Awake () {
        instance = this;
	}

    public void Init()
    {
        m_dicKeyboard.Add(InputMode.Global, new Dictionary<KeyCode, Action>());
        m_dicAxis.Add(InputMode.Global, new Dictionary<string, AxisDelegate>());
    }
	
	// Update is called once per frame
	protected override void BUpdate () {
        UpdateKeyboardDelegates(InputMode.Global);
        UpdateKeyboardDelegates(m_currInputMode);
        UpdateAxisDelegates(InputMode.Global);
        UpdateAxisDelegates(m_currInputMode);
	}

    /// <summary>
    /// 选择键盘输入模式，不能选择全局模式，子模式不会覆盖全局模式
    /// </summary>
    /// <param name="inputMode"></param>
    public void SetInputMode(InputMode inputMode)
    {
        if (inputMode == InputMode.Global)
            throw new Exception("不能选择全局模式");

        m_currInputMode = inputMode;
    }

    /// <summary>
    /// 注册键盘事件
    /// </summary>
    /// <param name="inputMode">没有的类型可自行添加</param>
    /// <param name="keyCode"></param>
    /// <param name="action"></param>
    public void AddKeyboardDelegate(InputMode inputMode, KeyCode keyCode, Action action)
    {
        if (!m_dicKeyboard.ContainsKey(inputMode))
            m_dicKeyboard.Add(inputMode, new Dictionary<KeyCode, Action>());

        m_dicKeyboard[inputMode].Add(keyCode, action);
    }

    /// <summary>
    /// 注销键盘事件
    /// </summary>
    /// <param name="inputMode"></param>
    /// <param name="keyCode"></param>
    public void DeleteKeyboardDelegate(InputMode inputMode, KeyCode keyCode)
    {
        if (!m_dicKeyboard.ContainsKey(inputMode))
            return;

        if (m_dicKeyboard[inputMode].ContainsKey(keyCode))
            m_dicKeyboard[inputMode].Remove(keyCode);
    }

    /// <summary>
    /// 注册轴事件
    /// </summary>
    /// <param name="inputMode">没有的类型可自行添加</param>
    /// <param name="axis"></param>
    /// <param name="action"></param>
    public void AddAxisDelegate(InputMode inputMode, string axis, AxisDelegate action)
    {
        if (!m_dicAxis.ContainsKey(inputMode))
            m_dicAxis.Add(inputMode, new Dictionary<string, AxisDelegate>());

        m_dicAxis[inputMode].Add(axis, action);
    }

    /// <summary>
    /// 注销轴事件
    /// </summary>
    /// <param name="inputMode"></param>
    /// <param name="axis"></param>
    public void DeleteAxisDelegate(InputMode inputMode, string axis)
    {
        if (!m_dicAxis.ContainsKey(inputMode))
            return;

        if (m_dicAxis[inputMode].ContainsKey(axis))
            m_dicAxis[inputMode].Remove(axis);
    }

    /// <summary>
    /// 执行键盘事件处理
    /// </summary>
    /// <param name="inputMode"></param>
    private void UpdateKeyboardDelegates(InputMode inputMode)
    {
        if (m_dicKeyboard.ContainsKey(inputMode))
        {
            foreach (var item in m_dicKeyboard[inputMode])
            {
                if (Input.GetKey(item.Key))
                    item.Value();

                if (Input.GetKeyDown(item.Key))
                    item.Value();

                if (Input.GetKeyUp(item.Key))
                    item.Value();
            }
        }
    }

    private void UpdateAxisDelegates(InputMode inputMode)
    {
        if (m_dicAxis.ContainsKey(inputMode))
        {
            foreach (var item in m_dicAxis[inputMode])
            {
                item.Value(Input.GetAxis(item.Key));
            }
        }
    }
}
