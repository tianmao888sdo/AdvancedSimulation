﻿using UnityEngine;
using System.Collections;

/// <summary>
/// 汽车属性
/// </summary>
public class CarAttributes : MonoBehaviour
{
    public float steeringWheelAngle = 0.0f;//方向
    public float accelerator = 0.0f;//油门
    public float brake = 0.0f;//刹车
    public float handbrake = 0.0f;//手刹
    public float clutch = 0.0f;//离合   CarControl使用以确定换挡流程

    public void Init()
    {
    }

}