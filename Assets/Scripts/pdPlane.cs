using UnityEngine;

public class pdPlane : MonoBehaviour
{
    /// <summary>
    /// 试出来的效果理想的值
    /// </summary>
    private const float MAX_ANGULAR_ACCELERATION_MULTIPLIER = 1.2f;

    private pdBaseController m_Controller;

    /// <summary>
    /// 当前飞机的真实速度
    /// </summary>
    private Vector3 m_Velocity;
    /// <summary>
    /// 上一帧飞机的真实速度
    /// </summary>
    private Vector3 m_LastVelocity;
    /// <summary>
    /// 加速度
    /// </summary>
    private Vector3 m_Acceleration;

    /// <summary>
    /// 角速度
    /// </summary>
    private Vector3 m_AngularVelocity;
    /// <summary>
    /// 上一帧的角速度
    /// </summary>
    private Vector3 m_LastAngularVelocity;
    /// <summary>
    /// 角加速度
    /// </summary>
    private Vector3 m_AngularAcceleration;

    /// <summary>
    /// equal <see cref="m_Velocity"/> magnitude
    /// </summary>
    private float m_Speed;
    /// <summary>
    /// 推进(forward方向)速度
    /// </summary>
    private float m_PropulsiveSpeed;
    /// <summary>
    /// Roll轴最大角加速度
    /// </summary>
    private float m_MaxRollAngularAcceleration = 0;
    /// <summary>
    /// 大于0时，飞机失速
    /// </summary>
    private float m_StallAmount = 0;
    /// <summary>
    /// 最大转向角速度
    /// </summary>
	private float m_MaxAngularVelocity = 0;
    /// <summary>
    /// 最大转向加速度
    /// </summary>
    private float m_MaxAngularAcceleration = 0;
    /// <summary>
    /// 是否是高G转弯
    /// <see cref="http://acecombat.wikia.com/wiki/High-G_Turn"/>
    /// </summary>
    private bool m_IsHighGTurn = false;
    /// <summary>
    /// 高G转弯的CD
    /// </summary>
    private float m_HighGTurnCD = 0;
    /// <summary>
    /// 高G转弯的持续时间
    /// </summary>
    private float m_HighGTrunDuration = 0;
    /// <summary>
    /// 相机上方向
    /// UNDONE 啥啥啥???
    /// </summary>
    private Vector3 m_TargetCameraUp = Vector3.up;
    /// <summary>
    /// Roll轴的角加速度
    /// </summary>
    private float m_RollAcceleration = 0;

    /// <summary>
    /// 从Controller获取到的输入，向量的模小于1
    /// +X => yaw ; +Y => pitch
    /// </summary>
    private Vector2 m_Axis;
    private ThrottleState m_Throttle;

    private Transform m_Transform;
    /// <summary>
    /// UNDONE Load when initialize
    /// </summary>
    public pdPlaneTweakableProperties m_TweakableProerties;

    /// <summary>
    /// 是否有引擎受损
    /// UNDONE 还没做引擎
    /// </summary>
    public bool AnyEngineDamaged()
    {
        return false;
    }

    protected void Awake()
    {
        m_Transform = transform;
        m_Controller = gameObject.GetComponent<pdBaseController>();
    }

    protected void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime;
        float invertDelta = 1.0f / delta; // for performance
        DoUpdateMovement(delta, invertDelta);
    }

    private void DoUpdateMovement(float delta, float invertDelta)
    {
        m_Acceleration = (m_Velocity - m_LastVelocity) * invertDelta;
        m_LastVelocity = m_Velocity;

        m_AngularAcceleration = (m_AngularVelocity - m_LastAngularVelocity) * invertDelta;
        m_LastAngularVelocity = m_AngularVelocity;

        m_Speed = m_Velocity.magnitude;
        m_PropulsiveSpeed = Vector3.Dot(m_Velocity, m_Transform.forward);

        #region 计算转向能力
        // 转向能力的影响因素
        float angularFactorsAffect = 1.0f;

        // 飞机挂载对飞机转向能力的影响
        {
            // 这个数值越大，飞机转向能力越差。为0时，不影响飞机滚转
            // UNDONE 还没做飞机挂载
            float rotateReductionRatioByPayload = 0;
            angularFactorsAffect *= 1.0f - rotateReductionRatioByPayload;
        }

        // 失速时转向能力降低
        angularFactorsAffect *= (1.0f - Mathf.Clamp01(m_StallAmount * 2.0f)) * m_TweakableProerties.AngularSpeedReduceByStallNormalized;

        // 引擎受损时转向的影响 UNDONE 还没做飞机引擎
        angularFactorsAffect *= false ? m_TweakableProerties.RollAngularSpeedScaleWhenDamaged : 1;

        // 速度对转向能力的影响
        Keyframe[] angularAffectCache = new Keyframe[3]; // Keyframe is struct, not have GC
        {
            angularAffectCache[0].time = m_TweakableProerties.LowSpeed;
            angularAffectCache[0].value = m_TweakableProerties.TurnAngularSpeedFactor_LowSpeed;
            angularAffectCache[1].time = m_TweakableProerties.NormalSpeed;
            angularAffectCache[1].value = 1.0f;
            angularAffectCache[2].time = m_TweakableProerties.HightSpeed;
            angularAffectCache[2].value = m_TweakableProerties.TurnAngularSpeedFactor_HightSpeed;
            // 速度对转向能力的影响 TODO 如果效果不好，再考虑特殊处理
            angularFactorsAffect *= hwmUtility.Evaluate(m_PropulsiveSpeed, angularAffectCache);
        }

        // 高度对旋转能力的影响
        {
            angularAffectCache[0].time = m_TweakableProerties.LowHeight;
            angularAffectCache[0].value = m_TweakableProerties.TurnAngularSpeedFactor_LowHeight;
            angularAffectCache[1].time = m_TweakableProerties.NormalHeight;
            angularAffectCache[1].value = 1.0f;
            angularAffectCache[2].time = m_TweakableProerties.HightHeight;
            angularAffectCache[2].value = m_TweakableProerties.TurnAngularSpeedFactor_HightHeight;
            // 飞行高度对转向能力的影响
            angularFactorsAffect *= hwmUtility.Evaluate(Mathf.Max(0, m_Transform.localPosition.y), angularAffectCache);
        }

        // 机翼受伤时转向的影响 UNDONE 还没做机翼
        angularFactorsAffect *= false ? m_TweakableProerties.TurnAngularSpeedScaleWhenDamaged : 1;

        m_MaxRollAngularAcceleration = m_TweakableProerties.MaxRollAngularAcceleration * angularFactorsAffect;
        m_MaxAngularVelocity = m_TweakableProerties.MaxAngularVelocity * angularFactorsAffect;
        m_MaxAngularAcceleration = m_MaxAngularVelocity * MAX_ANGULAR_ACCELERATION_MULTIPLIER;
        #endregion

        #region 获取杆量输入
        m_Axis = m_Controller.GetAxis();
        m_Axis.y = -m_Axis.y;
        float axisLength = m_Axis.magnitude;
        m_Axis = axisLength > 1.0f
            ? m_Axis / axisLength
            : m_Axis;

        // 左右翼受伤时，对杆量进行偏移 UNDONE 还没做机翼
        #endregion

        #region 更新节流阀状态
        ThrottleState inputThrottle = m_Controller.GetThrottle();
        switch (inputThrottle)
        {
            case ThrottleState.Normal:
            // 减速没有限制
            case ThrottleState.Brake:
            case ThrottleState.BrakeII:
                m_Throttle = inputThrottle;
                break;
            // 加速
            case ThrottleState.BoostII:
            case ThrottleState.Boost:
                // 发动机受损时，不能加速
                if (AnyEngineDamaged())
                {
                    m_Throttle = ThrottleState.Normal;
                }
                else
                {
                    m_Throttle = inputThrottle;
                }
                break;
        }

        // UNDONE 长时间加速后，引擎过热
        #endregion

        #region 更新飞机旋转
        // 高G转弯
        {
            // 判断高G转弯
            m_IsHighGTurn = (m_Throttle == ThrottleState.Brake || m_Throttle == ThrottleState.BrakeII)
                // 确保飞机在高速中做不出高G转弯(例如俯冲)
                && m_PropulsiveSpeed < (m_TweakableProerties.HightSpeed + m_TweakableProerties.NormalSpeed) * 0.5f
                && m_HighGTurnCD < 0
                && CanHighGTrunForAxis(m_Axis);

            if (m_IsHighGTurn)
            {
                m_HighGTrunDuration += delta;

                // 持续高G转弯，进入CD
                if (m_HighGTrunDuration > m_TweakableProerties.HighGTurnDuration)
                {
                    m_HighGTurnCD = m_TweakableProerties.HighGTurnCD;
                }
            }
            else
            {
                m_HighGTurnCD = Mathf.Max(0, m_HighGTurnCD - delta);
                m_HighGTrunDuration = 0;
            }
        }

        // 更新角速度
        Vector2 angularVelocity = AxisToAngularVelocity(m_Axis);
        m_AngularVelocity = Vector2.MoveTowards(m_AngularVelocity, angularVelocity, delta * m_MaxAngularAcceleration);
        // 角度变化
        Quaternion deltaRotation = Quaternion.Euler(m_AngularVelocity * delta);
        // 计算旋转后的角度(yaw和pitch)
        Quaternion newRotation = m_Transform.localRotation * deltaRotation;
        Vector3 newForward = newRotation * Vector3.forward;

        // 更新Roll轴 UNDONE 这里没看懂
        Quaternion rollDelta;
        {
            float inputRoll = -m_Controller.GetRoll();
            float targetRollAcceleration = 0;
            if (Mathf.Abs(inputRoll) < Mathf.Epsilon)
            {
                Vector3 vLeft = Vector3.Cross(newForward, Vector3.up).normalized;
                m_TargetCameraUp = Vector3.Cross(vLeft, newForward).normalized;

                Vector3 vMyUp = m_Transform.up;
                float fDeltaCos = Vector3.Dot(m_TargetCameraUp, vMyUp);
                fDeltaCos = Mathf.Sign(fDeltaCos) * Mathf.Clamp01(Mathf.Abs(fDeltaCos));
                if (fDeltaCos < 0.999f)
                {
                    Vector3 vRotAxis = Vector3.Cross(m_TargetCameraUp, vMyUp);
                    float fDeltaSin = vRotAxis.magnitude;
                    vRotAxis = fDeltaSin < Mathf.Epsilon
                        ? newForward // 当m_CameraTargetUp = -m_CameraUp时，默认沿着逆时针旋转
                        : vRotAxis / fDeltaSin;
                    float fDeltaDegree = Mathf.Acos(fDeltaCos) * Mathf.Rad2Deg;

                    // 机头接近与地面垂直时，减小自动校正的程度（否则会造成较大的偏航）
                    float fLerpAmountMul = 1.0f - Mathf.Clamp01(Mathf.Abs(Vector3.Dot(newForward, Vector3.up)));
                    // 用户在垂直方向上的输入分量较大（做Loop）时，减弱恢复速度。
                    fLerpAmountMul *= 1.0f - Mathf.Clamp01(Mathf.Abs(m_Axis.y));

                    // fDeltaDegree越大需要的旋转速度越高
                    float fLerpResult = Mathf.Lerp(fDeltaDegree, 0, fLerpAmountMul * m_TweakableProerties.RollToZeroStrength * delta);
                    targetRollAcceleration = (fLerpResult - fDeltaDegree) * invertDelta;
                    targetRollAcceleration = Vector3.Dot(vRotAxis, newForward) > 0
                        ? targetRollAcceleration
                        : -targetRollAcceleration;

                    targetRollAcceleration = hwmUtility.ClampAbs(targetRollAcceleration, m_MaxRollAngularAcceleration);
                }
            }
            else
            {
                targetRollAcceleration = Mathf.Clamp(inputRoll, -1.0f, 1.0f) * m_MaxRollAngularAcceleration;
            }

            targetRollAcceleration *= Mathf.Clamp01(1.0f - m_StallAmount * 10.0f);

            m_RollAcceleration = Mathf.MoveTowards(m_RollAcceleration
                , targetRollAcceleration
                , m_MaxRollAngularAcceleration * delta * 2.0f); // 实际Roll操作时，Roll的角加速度

            rollDelta = Quaternion.AngleAxis(m_RollAcceleration * delta, newForward);
        }

        m_Transform.localRotation = rollDelta * newRotation;
        #endregion

        #region 更新节流阀(速度)
        // 转向导致的减速
        {
            float degreeDeltaRotation = Quaternion.Angle(deltaRotation, Quaternion.identity);
            float turnSpeed = degreeDeltaRotation * invertDelta;

            // 转向导致的速度损失
            float decelerationCausedByTurning = (turnSpeed * 0.01111f) // 0.01111f equal 1 / 90.0f
                * m_PropulsiveSpeed
                * m_TweakableProerties.DecelerationCausedByTurningCoefficient;

            // 高G转弯导致减速
            if (m_IsHighGTurn)
            {
                decelerationCausedByTurning *= m_TweakableProerties.DecelerationCausedByHighGTurn;
            }

            decelerationCausedByTurning = Mathf.Clamp(decelerationCausedByTurning, 0, m_TweakableProerties.MaxDecelerationCausedByTurning);

            // 减速
            m_Velocity -= m_Transform.forward
                * Mathf.Clamp(decelerationCausedByTurning, 0, m_TweakableProerties.MaxDecelerationCausedByTurning)
                * delta;
        }

        // 飞机在径向和轴向上的阻力
        {
            // 本地坐标系下的速度
            Vector3 velocity_LocalSpace = m_Transform.InverseTransformDirection(m_Velocity);
            // 空气阻力
            Vector3 dragForce_LocalSpace = hwmUtility.CalculateDrag(velocity_LocalSpace
                , new Vector3(m_TweakableProerties.VerticalDragCoefficient
                    , m_TweakableProerties.VerticalDragCoefficient
                    , m_Throttle == ThrottleState.Brake
                        ? m_TweakableProerties.PropulsiveDragCoefficient_Brake
                        : m_Throttle == ThrottleState.BrakeII
                            ? m_TweakableProerties.PropulsiveDragCoefficient_BrakeII
                            : m_TweakableProerties.PropulsiveDragCoefficient));
            // a = F / m (加速度 = 阻力 / 自身质量), 为简化计算, 这里假设自身质量为1
            Vector3 dragAcceleration_LocalSpace = dragForce_LocalSpace; 
            // 旋转阻力加速度
            float verticalDragAcceleration = Mathf.Sqrt(dragAcceleration_LocalSpace.x * dragAcceleration_LocalSpace.x
                + dragAcceleration_LocalSpace.y * dragAcceleration_LocalSpace.y);
            if (verticalDragAcceleration > Mathf.Epsilon)
            {
                //限制径向阻力的最大值
                float clampedVerticalDragAcceleration = Mathf.Min(verticalDragAcceleration, m_TweakableProerties.MaxVerticalDragDeceleration);
                float clampVerticalDragScale = clampedVerticalDragAcceleration / verticalDragAcceleration;
                dragAcceleration_LocalSpace.x *= clampVerticalDragScale;
                dragAcceleration_LocalSpace.y *= clampVerticalDragScale;
            }

            // 阻力对速度的影响
            Vector3 velocityChangeCausedByDrag = dragAcceleration_LocalSpace * delta;
            // 阻力不能大于速率
            velocityChangeCausedByDrag.x = Mathf.Sign(velocityChangeCausedByDrag.x)
                * Mathf.Min(Mathf.Abs(velocityChangeCausedByDrag.x), Mathf.Abs(velocity_LocalSpace.x));
            velocityChangeCausedByDrag.y = Mathf.Sign(velocityChangeCausedByDrag.y)
                * Mathf.Min(Mathf.Abs(velocityChangeCausedByDrag.y), Mathf.Abs(velocity_LocalSpace.y));
            velocityChangeCausedByDrag.z = Mathf.Sign(velocityChangeCausedByDrag.z)
                * Mathf.Min(Mathf.Abs(velocityChangeCausedByDrag.z), Mathf.Abs(velocity_LocalSpace.z));
            velocityChangeCausedByDrag = m_Transform.TransformDirection(velocityChangeCausedByDrag);

            m_Velocity += velocityChangeCausedByDrag;
        }

        // 引擎推力
        {
            float thrustPower;
            switch (m_Throttle)
            {
                case ThrottleState.BoostII:
                    thrustPower = m_TweakableProerties.ThrustPower_BoostII;
                    break;
                case ThrottleState.Boost:
                    thrustPower = m_TweakableProerties.ThrustPower_Boost;
                    break;
                case ThrottleState.Normal:
                    thrustPower = m_TweakableProerties.ThrustPower_Normal;
                    break;
                case ThrottleState.Brake:
                    thrustPower = m_TweakableProerties.ThrustPower_Brake;
                    break;
                case ThrottleState.BrakeII:
                    thrustPower = m_TweakableProerties.ThrustPower_BrakeII;
                    break;
                default:
                    thrustPower = 0;
                    hwmDebug.Assert(false, "Invalid Throttle: " + m_Throttle);
                    break;
            }

            // 高度对速度的影响
            {
                float thrustPowerFactorAffectByHeight;
                Keyframe[] thrustAccelerationFactorsAffectByHeight = new Keyframe[3]; // Keyframe is struct, not have GC
                thrustAccelerationFactorsAffectByHeight[0].time = m_TweakableProerties.LowHeight;
                thrustAccelerationFactorsAffectByHeight[0].value = m_TweakableProerties.ThrustPowerFactor_LowHeight;
                thrustAccelerationFactorsAffectByHeight[1].time = m_TweakableProerties.NormalHeight;
                thrustAccelerationFactorsAffectByHeight[1].value = 1.0f;
                thrustAccelerationFactorsAffectByHeight[2].time = m_TweakableProerties.HightHeight;
                thrustAccelerationFactorsAffectByHeight[2].value = m_TweakableProerties.ThrustPowerFactor_HightHeight;
                thrustPowerFactorAffectByHeight = hwmUtility.Evaluate(Mathf.Max(0, m_Transform.localPosition.y), thrustAccelerationFactorsAffectByHeight);

                thrustPower *= thrustPowerFactorAffectByHeight;
            }

            float m_ThrustAcceleration = hwmUtility.PowerToAcceleration(thrustPower, m_PropulsiveSpeed, 1.0f, delta);
            m_ThrustAcceleration = Mathf.Min(m_ThrustAcceleration, m_TweakableProerties.MaxThrustAcceleration);

            // 计算由重力产生的减速度、加速度
            float climbAngle = -Mathf.DeltaAngle(0, m_Transform.eulerAngles.x);
            float climbAmount = Mathf.Clamp(Mathf.Sin(climbAngle * Mathf.Deg2Rad), -1.0f, 1.0f);
            float propulsiveGravityAcceleration = climbAmount > 0
                ? Mathf.Lerp(0, -m_TweakableProerties.GravityDeceleration, climbAmount)
                : Mathf.Lerp(0, m_TweakableProerties.GravityAcceleration, -climbAmount);

            // 计算前向加速度
            float propulsiveAcceleration = m_ThrustAcceleration + propulsiveGravityAcceleration;

            float newPropulsiveSpeed = m_PropulsiveSpeed + propulsiveAcceleration * delta;
            newPropulsiveSpeed = Mathf.Min(newPropulsiveSpeed, m_TweakableProerties.MaxPropulsiveSpeed);

            float deltaSpeed = newPropulsiveSpeed - m_PropulsiveSpeed;
            m_Velocity += (m_Transform.forward * deltaSpeed);
        }
        #endregion

        #region 更新失速
        #endregion
    }

    /// <summary>
	/// 根据杆量算出角速度(本地坐标)
	/// </summary>
	/// <param name="axis">输入的旋转向量，这个向量的+Y代表飞机抬头</param>
	/// <returns></returns>
	public Vector2 AxisToAngularVelocity(Vector2 axis)
    {
        Vector2 angularVelocity = new Vector2(axis.y * m_MaxAngularVelocity
            , axis.x * m_MaxAngularVelocity);

        if (m_IsHighGTurn)
        {
            angularVelocity.y *= m_TweakableProerties.HighGTurnAxisXMultiplyValue;
        }

        return angularVelocity;
    }

    /// <summary>
	/// 杆量是否达到执行高G转弯的条件
	/// </summary>
	private bool CanHighGTrunForAxis(Vector2 axis)
    {
        // 0 => 0度; 1 => 90度; 2 => 180度
        float angularValue = Mathf.Abs(Mathf.Atan2(axis.y, axis.x) * hwmUtility.IVNERT_PI * 2);
        // 使angularValue始终为0到1
        angularValue = angularValue > 1 ? 2 - angularValue : angularValue;

        // 算这个magnitudeScale是因为，我假设椭圆上的点到圆心的半径与角度的关系为linear的。所以根据rotateVector算一个角度，再根据角度算magnitudeScale
        float magnitudeScale = 1 - angularValue;

        float differentAngular = m_MaxAngularVelocity * magnitudeScale - m_AngularVelocity.magnitude;
        // 1.5 就是一个估计值
        return differentAngular < 1.5f;
    }

    /// <summary>
    /// 节流阀状态，<see cref="https://en.wikipedia.org/wiki/Throttle"/>
    /// </summary>
    public enum ThrottleState
    {
        BoostII,
        Boost,
        Normal,
        Brake,
        BrakeII,
    }
}