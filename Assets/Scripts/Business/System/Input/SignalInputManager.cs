using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

/// <summary>
/// 信号输入管理器，数值监控，尺必须先加载完成才能初始化
/// </summary>
public class SignalInputManager :MonoBehaviour,IUpdate, IRelease
{
    #region 单例
    private static SignalInputManager instance;
    public static SignalInputManager Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    /// <summary>
    /// 输入信号枚举
    /// </summary>
    public enum SignalInputEnum
    {
        /// <summary>
        /// 方向
        /// </summary>
        SteeringWheelAngle,
        /// <summary>
        /// 油门
        /// </summary>
        Accelerator,
        /// <summary>
        /// 离合
        /// </summary>
        Clutch,
        /// <summary>
        /// 刹车
        /// </summary>
        Brake,
        /// <summary>
        /// 手刹
        /// </summary>
        HandBrake,
        /// <summary>
        /// 档位
        /// </summary>
        Gear,
        /// <summary>
        /// 喇叭
        /// 替代原来的变量 Speaker
        /// </summary>
        Horn,
        /// <summary>
        /// 钥匙
        /// </summary>
        CarKey,
        /// <summary>
        /// 安全带
        /// </summary>
        SafeBelt,
        /// <summary>
        /// 雨刮
        /// </summary>
        Wiper,
        /// <summary>
        /// 喷水
        /// </summary>
        WaterInjection,
        /// <summary>
        /// 车门
        /// </summary>
        Door,
        /// <summary>
        /// 双闪（危险警示灯）
        /// </summary>
        HazardWarning,
        /// <summary>
        /// 近光灯
        /// </summary>
        LowBeam,
        /// <summary>
        /// 远光灯
        /// </summary>
        HighBeam,
        /// <summary>
        /// 雾灯
        /// </summary>
        FogLamp,
        /// <summary>
        /// 左转灯
        /// </summary>
        TurnLeftBeam,
        /// <summary>
        /// 右转灯
        /// </summary>
        TurnRightBeam,
        /// <summary>
        /// 行车灯（仪表灯）
        /// </summary>
        DrivingLamp,
        /// <summary>
        /// 后视镜选择
        /// </summary>
        RearviewMirrorIndex,
        /// <summary>
        /// 后视镜调整
        /// </summary>
        RearviewMirrorAdjust,
        /// <summary>
        /// 油箱盖
        /// </summary>
        FuelTankCap,
        /// <summary>
        /// 确认
        /// </summary>
        Confirm,
        /// <summary>
        /// 取消
        /// </summary>
        Cancel,
        /// <summary>
        /// 上选
        /// </summary>
        Up,
        /// <summary>
        /// 下选
        /// </summary>
        Down,
        /// <summary>
        /// 右选
        /// </summary>
        Right,
        /// <summary>
        /// 左选
        /// </summary>
        Left,
        /// <summary>
        /// 摄像机
        /// </summary>
        Camera,
        /// <summary>
        /// UI左选LMJ
        /// </summary>
        UiLeft
    }

    /// <summary>
    /// 信号委托
    /// </summary>
    /// <param name="_val"></param>
    public delegate void SignalDelegate(int _signal);

    private Dictionary<SignalInputEnum, SignalDelegate> m_dicSignal = new Dictionary<SignalInputEnum, SignalDelegate>();

    /// <summary>
    ///档位 0 空挡 12345前进 -1 倒档  CarSound使用以模拟车辆动力及声音
    /// </summary>
    public int iGear = 0;
    public int iHorn = 0;
    /// <summary>
    /// 钥匙状态 0 断电 1 整车加电 2 点火  CarSound使用以模拟车辆声音  CarControl使用以确定车辆引擎状态
    /// </summary>
    public int iCarKey = 0;
    public int iSafeBelt = 0;
    public int iWiper = 0;
    public int iWaterInjection = 0;
    public int iDoor = 0;
    public int iHazardWarning = 0;
    public int iLowBeam = 0;
    public int iHighBeam = 0;
    public int iFogLamp = 0;
    public int iTurnLeftBeam = 0;
    public int iTurnRightBeam = 0;
    public int iDrivingLamp = 0;
    public int iRearviewMirrorIndex = 0;
    public int iRearviewMirrorAdjust = 0;
    public int iFuelTankCap = 0;
    public int iConfirm = 0;
    public int iCancel = 0;
    public int iUp = 0;
    public int iDown = 0;
    public int iRight = 0;
    public int iLeft = 0;
    public int iCamera = 0;

    private SignalDelegate DelegateGear;
    private SignalDelegate DelegateHorn;
    private SignalDelegate DelegateCarKey;
    private SignalDelegate DelegateSafeBelt;
    private SignalDelegate DelegateWiper;
    private SignalDelegate DelegateWaterInjection;
    private SignalDelegate DelegateDoor;
    private SignalDelegate DelegateHazardWarning;
    private SignalDelegate DelegateLowBeam;
    private SignalDelegate DelegateHighBeam;
    private SignalDelegate DelegateFogLamp;
    private SignalDelegate DelegateTurnLeftBeam;
    private SignalDelegate DelegateTurnRightBeam;
    private SignalDelegate DelegateDrivingLamp;
    private SignalDelegate DelegateRearviewMirrorIndex;
    private SignalDelegate DelegateRearviewMirrorAdjust;
    private SignalDelegate DelegateFuelTankCap;
    private SignalDelegate DelegateConfirm;
    private SignalDelegate DelegateCancel;
    private SignalDelegate DelegateUp;
    private SignalDelegate DelegateDown;
    private SignalDelegate DelegateRight;
    private SignalDelegate DelegateLeft;
    private SignalDelegate DelegateBrake;
    private SignalDelegate DelegateCamera;

    private ChangeTrace ctGear;
    private ChangeTrace ctHorn;
    private ChangeTrace ctCarKey;
    private ChangeTrace ctSafeBelt;
    private ChangeTrace ctWiper;
    private ChangeTrace ctWaterInjection;
    private ChangeTrace ctDoor;
    private ChangeTrace ctHazardWarning;
    private ChangeTrace ctLowBeam;
    private ChangeTrace ctHighBeam;
    private ChangeTrace ctFogLamp;
    private ChangeTrace ctTurnLeftBeam;
    private ChangeTrace ctTurnRightBeam;
    private ChangeTrace ctDrivingLamp;
    private ChangeTrace ctRearviewMirrorIndex;
    private ChangeTrace ctRearviewMirrorAdjust;
    private ChangeTrace ctFuelTankCap;
    private ChangeTrace ctConfirm;
    private ChangeTrace ctCancel;
    private ChangeTrace ctUp;
    private ChangeTrace ctDown;
    private ChangeTrace ctRight;
    private ChangeTrace ctLeft;
    private ChangeTrace ctBrake;
    private ChangeTrace ctCamera;

    public CarAttributes carAttributes;

    private void nullDelegate(int _val) { }

    void Awake()
    {
        instance = this;
    }
    public void Init()
    {
        //添加空方法，防止空对象错误
        DelegateGear = nullDelegate;
        DelegateHorn = nullDelegate;
        DelegateCarKey = nullDelegate;
        DelegateSafeBelt = nullDelegate;
        DelegateWiper = nullDelegate;
        DelegateWaterInjection = nullDelegate;
        DelegateDoor = nullDelegate;
        DelegateHazardWarning = nullDelegate;
        DelegateLowBeam = nullDelegate;
        DelegateHighBeam = nullDelegate;
        DelegateFogLamp = nullDelegate;
        DelegateTurnLeftBeam = nullDelegate;
        DelegateTurnRightBeam = nullDelegate;
        DelegateDrivingLamp = nullDelegate;
        DelegateRearviewMirrorIndex = nullDelegate;
        DelegateRearviewMirrorAdjust = nullDelegate;
        DelegateFuelTankCap = nullDelegate;
        DelegateConfirm = nullDelegate;
        DelegateCancel = nullDelegate;
        DelegateUp = nullDelegate;
        DelegateDown = nullDelegate;
        DelegateRight = nullDelegate;
        DelegateLeft = nullDelegate;
        DelegateBrake = nullDelegate;
        DelegateCamera = nullDelegate;

        //委托添加到委托字典中
        m_dicSignal.Add(SignalInputEnum.Gear, DelegateGear);
        m_dicSignal.Add(SignalInputEnum.Horn, DelegateHorn);
        m_dicSignal.Add(SignalInputEnum.CarKey, DelegateCarKey);
        m_dicSignal.Add(SignalInputEnum.SafeBelt, DelegateSafeBelt);
        m_dicSignal.Add(SignalInputEnum.Wiper, DelegateWiper);
        m_dicSignal.Add(SignalInputEnum.WaterInjection, DelegateWaterInjection);
        m_dicSignal.Add(SignalInputEnum.Door, DelegateDoor);
        m_dicSignal.Add(SignalInputEnum.HazardWarning, DelegateHazardWarning);
        m_dicSignal.Add(SignalInputEnum.LowBeam, DelegateLowBeam);
        m_dicSignal.Add(SignalInputEnum.HighBeam, DelegateHighBeam);
        m_dicSignal.Add(SignalInputEnum.FogLamp, DelegateFogLamp);
        m_dicSignal.Add(SignalInputEnum.TurnLeftBeam, DelegateTurnLeftBeam);
        m_dicSignal.Add(SignalInputEnum.TurnRightBeam, DelegateTurnRightBeam);
        m_dicSignal.Add(SignalInputEnum.DrivingLamp, DelegateDrivingLamp);
        m_dicSignal.Add(SignalInputEnum.RearviewMirrorIndex, DelegateRearviewMirrorIndex);
        m_dicSignal.Add(SignalInputEnum.RearviewMirrorAdjust, DelegateRearviewMirrorAdjust);
        m_dicSignal.Add(SignalInputEnum.FuelTankCap, DelegateFuelTankCap);
        m_dicSignal.Add(SignalInputEnum.Confirm, DelegateConfirm);
        m_dicSignal.Add(SignalInputEnum.Cancel, DelegateCancel);
        m_dicSignal.Add(SignalInputEnum.Up, DelegateUp);
        m_dicSignal.Add(SignalInputEnum.Down, DelegateDown);
        m_dicSignal.Add(SignalInputEnum.Right, DelegateRight);
        m_dicSignal.Add(SignalInputEnum.Left, DelegateLeft);
        m_dicSignal.Add(SignalInputEnum.Brake, DelegateBrake);
        m_dicSignal.Add(SignalInputEnum.Camera, DelegateCamera);

        //输入事件依赖的变量追踪初始化
        ctGear = new ChangeTrace(iGear, 1f);
        ctHorn = new ChangeTrace(iHorn, 1f);
        ctCarKey = new ChangeTrace(iCarKey, 1f);
        ctSafeBelt = new ChangeTrace(iSafeBelt, 1f);
        ctWiper = new ChangeTrace(iWiper, 1f);
        ctWaterInjection = new ChangeTrace(iWaterInjection, 1f);
        ctDoor = new ChangeTrace(iDoor, 1f);
        ctHazardWarning = new ChangeTrace(iHazardWarning, 1f);
        ctLowBeam = new ChangeTrace(iLowBeam, 1f);
        ctHighBeam = new ChangeTrace(iHighBeam, 1f);
        ctFogLamp = new ChangeTrace(iFogLamp, 1f);
        ctTurnLeftBeam = new ChangeTrace(iTurnLeftBeam, 1f);
        ctTurnRightBeam = new ChangeTrace(iTurnRightBeam, 1f);
        ctDrivingLamp = new ChangeTrace(iDrivingLamp, 1f);
        ctRearviewMirrorIndex = new ChangeTrace(iRearviewMirrorIndex, 1f);
        ctRearviewMirrorAdjust = new ChangeTrace(iRearviewMirrorAdjust, 1f);
        ctFuelTankCap = new ChangeTrace(iFuelTankCap, 1f);
        ctConfirm = new ChangeTrace(iConfirm, 1f);
        ctCancel = new ChangeTrace(iCancel, 1f);
        ctUp = new ChangeTrace(iUp, 1f);
        ctDown = new ChangeTrace(iDown, 1f);
        ctRight = new ChangeTrace(iRight, 1f);
        ctLeft = new ChangeTrace(iLeft, 1f);

        try
        {
            ctBrake = new ChangeTrace(carAttributes.brake, 0.1f);
            ctCamera = new ChangeTrace(iCamera, 1f);
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void Update()
    {
        ValTrace();
    }

    public void Play()
    {
        GameManager.Instance.AddUpdater(this);
    }

    public void Stop()
    {
        GameManager.Instance.RemoveUpdater(this);
    }

    /// <summary>
    /// 值域追踪的公用代码段
    /// </summary>
    private void ValTrace()
    {
        if (ctGear.IsChange(iGear)) { m_dicSignal[SignalInputEnum.Gear](iGear); }
        if (ctHorn.IsChange(iHorn)) { m_dicSignal[SignalInputEnum.Horn](iHorn); }
        if (ctCarKey.IsChange(iCarKey)) { m_dicSignal[SignalInputEnum.CarKey](iCarKey); }
        if (ctSafeBelt.IsChange(iSafeBelt)) { m_dicSignal[SignalInputEnum.SafeBelt](iSafeBelt); }
        if (ctWiper.IsChange(iWiper)) { m_dicSignal[SignalInputEnum.Wiper](iWiper); }
        if (ctWaterInjection.IsChange(iWaterInjection)) { m_dicSignal[SignalInputEnum.WaterInjection](iWaterInjection); }
        if (ctDoor.IsChange(iDoor)) { m_dicSignal[SignalInputEnum.Door](iDoor); }
        if (ctHazardWarning.IsChange(iHazardWarning)) { m_dicSignal[SignalInputEnum.HazardWarning](iHazardWarning); }
        if (ctLowBeam.IsChange(iLowBeam)) { m_dicSignal[SignalInputEnum.LowBeam](iLowBeam); }
        if (ctHighBeam.IsChange(iHighBeam)) { m_dicSignal[SignalInputEnum.HighBeam](iHighBeam); }
        if (ctFogLamp.IsChange(iFogLamp)) { m_dicSignal[SignalInputEnum.FogLamp](iFogLamp); }
        if (ctTurnLeftBeam.IsChange(iTurnLeftBeam)) { m_dicSignal[SignalInputEnum.TurnLeftBeam](iTurnLeftBeam); }
        if (ctTurnRightBeam.IsChange(iTurnRightBeam)) { m_dicSignal[SignalInputEnum.TurnRightBeam](iTurnRightBeam); }
        if (ctDrivingLamp.IsChange(iDrivingLamp)) { m_dicSignal[SignalInputEnum.DrivingLamp](iDrivingLamp); }
        if (ctRearviewMirrorIndex.IsChange(iRearviewMirrorIndex)) { m_dicSignal[SignalInputEnum.RearviewMirrorIndex](iRearviewMirrorIndex); }
        if (ctRearviewMirrorAdjust.IsChange(iRearviewMirrorAdjust)) { m_dicSignal[SignalInputEnum.RearviewMirrorAdjust](iRearviewMirrorAdjust); }
        if (ctFuelTankCap.IsChange(iFuelTankCap)) { m_dicSignal[SignalInputEnum.FuelTankCap](iFuelTankCap); }
        if (ctConfirm.IsChange(iConfirm)) { m_dicSignal[SignalInputEnum.Confirm](iConfirm); }
        if (ctCancel.IsChange(iCancel)) { m_dicSignal[SignalInputEnum.Cancel](iCancel); }
        if (ctUp.IsChange(iUp)) { m_dicSignal[SignalInputEnum.Up](iUp); }
        if (ctDown.IsChange(iDown)) { m_dicSignal[SignalInputEnum.Down](iDown); }
        if (ctRight.IsChange(iRight)) { m_dicSignal[SignalInputEnum.Right](iRight); }
        if (ctLeft.IsChange(iLeft)) { m_dicSignal[SignalInputEnum.Left](iLeft); }
        if (ctBrake.IsChange(carAttributes.brake)) { m_dicSignal[SignalInputEnum.Brake]((int)(carAttributes.brake * 100)); }
        if (ctCamera.IsChange(iCamera)){m_dicSignal[SignalInputEnum.Camera](iCamera); }
    }

    public void Release(bool destroy = false)
    {
        throw new NotImplementedException();
    }
}
