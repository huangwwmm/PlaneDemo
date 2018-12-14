using UnityEngine;

public class pdPlaneTweakableProperties : ScriptableObject
{
    /// <summary>
    /// 最大滚转角速度
    /// </summary>
    public float MaxRollAngularAcceleration = 270;
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
    public float TurnAngularSpeedFactor_LowSpeed = 0.9f;
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
    public float TurnAngularSpeedFactor_HightSpeed = 0.9f;
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
    public float TurnAngularSpeedFactor_HightHeight = 0.8f;
    /// <summary>
    /// 飞机水平最大转向角加速度
    /// </summary>
    public float MaxAngularAcceleration = 45.0f;
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
    /// <summary>
    /// 飞机在左右和上下方向上的阻尼大小，注意计算时会忽略质量直接用这个值和速度计算出阻力的加速度。
    /// 所以质量越大的飞机，这里填的数值应该越小，这样模拟出的惯性就越大。
    /// 阻力的运算方式是：阻力 = 速度 * 速度 * 阻力系数
    /// </summary>
    public float VerticalDragCoefficient = 12.0f;
    /// <summary>
    /// 飞机在前后方向上的阻力系数
    /// </summary>
    public float PropulsiveDragCoefficient = 0.002f;
    /// <summary>
    /// 减速时，飞机在前后方向上的阻力系数
    /// </summary>
    public float PropulsiveDragCoefficient_Brake = 0.02f;
    /// <summary>
    /// 减速时，飞机在前后方向上的阻力系数
    /// </summary>
    public float PropulsiveDragCoefficient_BrakeII = 0.1f;
    /// <summary>
    /// 允许组尼产生的最大加速度，注意这个值主要用于模拟转过小的圈时向心加速度不足导致机头方向和飞机朝向不同的情况。
    /// 一旦发生这种情况，没能通过向心加速度偏转的动量就会受组尼影响而浪费掉。所以Drag越高的飞机转向时损失的能力也相对越小。
    /// F = m * (v^2 / r)
    /// F = m * w^2 * r
    /// </summary>
    public float MaxVerticalDragDeceleration = 100;
    /// <summary>
    /// 二档加速的引擎推力
    /// </summary>
    public float ThrustPower_BoostII = 25920.0f;
    /// <summary>
    /// 加速的引擎推力
    /// </summary>
    public float ThrustPower_Boost = 17280.0f;
    /// <summary>
    /// 引擎推力
    /// </summary>
    public float ThrustPower_Normal = 2160.0f;
    /// <summary>
    /// 减速引擎推力
    /// </summary>
    public float ThrustPower_Brake = -2160.0f;
    /// <summary>
    /// 二档减速的引擎推力
    /// </summary>
    public float ThrustPower_BrakeII = 4320.0f;
    /// <summary>
    /// 低空飞行时，引擎推力的乘数
    /// </summary>
    public float ThrustPowerFactor_LowHeight = 1.0f;
    /// <summary>
    /// 高空飞行时，引擎推力的乘数
    /// </summary>
    public float ThrustPowerFactor_HightHeight = 1.0f;
    /// <summary>
	/// 引擎可以提供的最大向前加速度（抵消阻力、加速度前）
	/// </summary>
    public float MaxThrustAcceleration = 30;
    /// <summary>
    /// 极限速度
    /// </summary>
    public float MaxPropulsiveSpeed = 150.0f;
    /// <summary>
    /// 飞机垂直俯冲时获得的额外加速度
    /// </summary>
    public float GravityAcceleration = 0;
    /// <summary>
    /// 飞机垂直爬升时获得的额外减速度
    /// </summary>
    public float GravityDeceleration = 0;
    /// <summary>
    /// 用于计算失速程度，速度(<see cref="pdPlane.m_PropulsiveSpeed"/>)到达这个值时, <see cref="pdPlane.m_StallAmount"/>为0
    /// </summary>
    public float BeginStallSpeed = 30.0f;
    /// <summary>
    /// 用于计算失速程度，速度(<see cref="pdPlane.m_PropulsiveSpeed"/>)到达这个值时, <see cref="pdPlane.m_StallAmount"/>为1
    /// </summary>
    public float HeavyStallSpeed = 10.0f;
}