using UnityEngine;

public class pdPlaneTweakableProperties : ScriptableObject
{
    /// <summary>
    /// 最大滚转角速度
    /// </summary>
    public float MaxRollAcceleration = 270;
    /// <summary>
    /// 失速时对转向能力的影响
    /// </summary>
    public float AngularSpeedReduceByStallNormalized = 0.5f;
    /// <summary>
    /// 受损状态下的滚转角速度缩放
    /// </summary>
    public float RollAngularSpeedScaleWhenDamaged = 0.5f;
    /// <summary>
    /// 低速
    /// </summary>
    public float LowSpeed = 30.0f;
    /// <summary>
    /// 低速时对转向能力的影响
    /// </summary>
    public float TurnAngularSpeedFactor_LowSpeed = 0.8f;
    /// <summary>
    /// 巡航速度
    /// </summary>
    public float NormalSpeed = 60.0f;
    /// <summary>
    /// 高速
    /// </summary>
    public float HightSpeed = 120.0f;
    /// <summary>
    /// 高速时对转向能力的影响
    /// </summary>
    public float TurnAngularSpeedFactor_HighSpeed = 0.8f;
    /// <summary>
    /// 低空飞行高度
    /// </summary>
    public float LowHeight = 1000.0f;
    /// <summary>
    /// 低空飞行时对转向能力的影响
    /// </summary>
    public float TurnAngularSpeedFactor_LowHeight = 0.8f;
    /// <summary>
    /// 正常高度
    /// </summary>
    public float NormalHeight = 2500.0f;
    /// <summary>
    /// 高空飞行高度
    /// </summary>
    public float HightHeight = 4000.0f;
    /// <summary>
    /// 高空飞行时对转向能力的影响
    /// </summary>
    public float TurnAngularSpeedFactor_HighestHeight = 0.8f;
    /// <summary>
    /// 飞机水平最大转向角速度
    /// </summary>
    public float MaxTurnAngularSpeed = 45.0f;
    /// <summary>
	/// 受损状态下的转向角速度缩放
	/// </summary>
	public float TurnAngularSpeedScaleWhenDamaged = 0.5f;
    /// <summary>
    /// 高G转弯时，杆量X轴乘以这个值
    /// </summary>
    public float HighGTurnAxisXMultiplyValue = 1.6f;
    /// <summary>
    /// 转向时飞机受到的减速度的计算系数，这个数值越大，飞机受到的减速度就越大。
    /// </summary>
    public float DecelerationCausedByTurningCoefficient = 0.1f;
    /// <summary>
    /// 高G转弯导致的减速
    /// </summary>
    public float DecelerationCausedByHighGTurn = 2.0f;
    /// <summary>
    /// 高G转弯的最大持续时间
    /// </summary>
    public float HighGTurnDuration = 10.0f;
    /// <summary>
    /// 高G转弯的CD时间
    /// </summary>
    public float HighGTurnCD = 1.0f;
    /// <summary>
    /// 转向时飞机受到的减速度的最大值
    /// </summary>
    public float MaxDecelerationCausedByTurning = 200.0f;
    /// <summary>
    /// Roll轴旋转角度恢复到0的强度
    /// </summary>
    public float RollToZeroStrength = 1.2f;
}