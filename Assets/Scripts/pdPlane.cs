using UnityEngine;

public class pdPlane : MonoBehaviour
{
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
    /// 本地空间的加速度
    /// </summary>
    private Vector3 m_Acceleration_LocalSpace;
    /// <summary>
    /// equal <see cref="m_Velocity"/> magnitude
    /// </summary>
    private float m_Speed;
    /// <summary>
    /// 推进(forward方向)速度
    /// </summary>
    private float m_PropulsiveSpeed;

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
    /// 最大转向加速度
    /// </summary>
    private float m_MaxAngularAcceleration = 0;

    /// <summary>
    /// Roll轴的角加速度
    /// </summary>
    private float m_RollAcceleration = 0;
    /// <summary>
    /// Roll轴最大角加速度
    /// </summary>
    private float m_MaxRollAcceleration = 0;
    /// <summary>
    /// 用于模拟Coordinate Turn的飞机Roll旋转
    /// </summary>
    private float m_FakeCoordinateRollVelocity = 0;

    /// <summary>
    /// 范围0 ~ 1
    /// 大于0时，飞机失速
    /// <see cref="m_StallState"/>
    /// </summary>
    private float m_StallAmount = 0;
    /// <summary>
    /// 失速状态
    /// <see cref="m_StallAmount"/>
    /// </summary>
    private StallState m_StallState = StallState.None;

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
    /// 节流阀状态
    /// </summary>
    private ThrottleState m_Throttle;

    /// <summary>
    /// 从Controller获取到的输入，向量的模小于1
    /// +X => yaw ; +Y => pitch
    /// </summary>
    private Vector2 m_MoveAxis;

    /// <summary>
    /// pdPlane节点
    /// 用于更新位置位置和yaw\pitch旋转
    /// </summary>
    private Transform m_Transform;
    /// <summary>
    /// pdPlane的子节点
    /// 用于个更新roll旋转
    /// </summary>
    private Transform m_PlaneRootTransform;
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

    public Vector3 GetAcceleration_LocalSpace()
    {
        return m_Acceleration_LocalSpace;
    }

    protected void Awake()
    {
        m_Transform = transform;
        m_PlaneRootTransform = transform.Find("PlaneRoot");
        m_Controller = gameObject.GetComponent<pdBaseController>();
        m_Controller.SetControllerPlane(this);
    }

    protected void OnDestroy()
    {
        m_Controller.UnControllerPlane();
        m_Controller = null;

        m_PlaneRootTransform = null;
        m_Transform = null;
    }

    protected void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime;
        float invertDelta = 1.0f / delta; // for performance
        DoUpdateMovement(delta, invertDelta);
    }

    private void DoUpdateMovement(float deltaTime, float invertDeltaTime)
    {
        Vector3 acceleration_WorldSpace = (m_Velocity - m_LastVelocity) * invertDeltaTime;
        m_Acceleration_LocalSpace = m_Transform.InverseTransformDirection(acceleration_WorldSpace);
        m_LastVelocity = m_Velocity;

        m_AngularAcceleration = (m_AngularVelocity - m_LastAngularVelocity) * invertDeltaTime;
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
        //angularFactorsAffect *= false ? m_TweakableProerties.RollAngularSpeedScaleWhenDamaged : 1;

        // 速度对转向能力的影响
        Keyframe[] keyframe3Cache = new Keyframe[3]; // Keyframe is struct, not have GC
        {
            keyframe3Cache[0].time = m_TweakableProerties.LowSpeed;
            keyframe3Cache[0].value = m_TweakableProerties.TurnAngularSpeedFactor_LowSpeed;
            keyframe3Cache[1].time = m_TweakableProerties.NormalSpeed;
            keyframe3Cache[1].value = 1.0f;
            keyframe3Cache[2].time = m_TweakableProerties.HightSpeed;
            keyframe3Cache[2].value = m_TweakableProerties.TurnAngularSpeedFactor_HightSpeed;
            // 速度对转向能力的影响 TODO 如果效果不好，再考虑特殊处理
            angularFactorsAffect *= hwmMath.Evaluate(m_PropulsiveSpeed, keyframe3Cache);
        }

        // 高度对旋转能力的影响
        {
            keyframe3Cache[0].time = m_TweakableProerties.LowHeight;
            keyframe3Cache[0].value = m_TweakableProerties.TurnAngularSpeedFactor_LowHeight;
            keyframe3Cache[1].time = m_TweakableProerties.NormalHeight;
            keyframe3Cache[1].value = 1.0f;
            keyframe3Cache[2].time = m_TweakableProerties.HightHeight;
            keyframe3Cache[2].value = m_TweakableProerties.TurnAngularSpeedFactor_HightHeight;
            // 飞行高度对转向能力的影响
            angularFactorsAffect *= hwmMath.Evaluate(Mathf.Max(0, m_Transform.localPosition.y), keyframe3Cache);
        }

        // 机翼受伤时转向的影响 UNDONE 还没做机翼
        //angularFactorsAffect *= false ? m_TweakableProerties.TurnAngularSpeedScaleWhenDamaged : 1;

        m_MaxRollAcceleration = m_TweakableProerties.MaxRollAcceleration * angularFactorsAffect;
        m_MaxAngularAcceleration = m_TweakableProerties.MaxAngularAcceleration * angularFactorsAffect;
        #endregion

        #region 获取杆量输入
        m_MoveAxis = m_Controller.GetMoveAxis();
        m_MoveAxis.y = -m_MoveAxis.y; // y轴反转
        float axisLength = m_MoveAxis.magnitude;
        m_MoveAxis = axisLength > 1.0f
            ? m_MoveAxis / axisLength
            : m_MoveAxis;

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

        #region 计算飞机旋转
        // 高G转弯
        {
            // 判断高G转弯
            m_IsHighGTurn = (m_Throttle == ThrottleState.Brake || m_Throttle == ThrottleState.BrakeII)
                // 确保飞机在高速中做不出高G转弯(例如俯冲)
                && m_PropulsiveSpeed < (m_TweakableProerties.HightSpeed + m_TweakableProerties.NormalSpeed) * 0.5f
                && m_HighGTurnCD < 0
                && CanHighGTrunForAxis(m_MoveAxis);

            if (m_IsHighGTurn)
            {
                m_HighGTrunDuration += deltaTime;

                // 持续高G转弯，进入CD
                if (m_HighGTrunDuration > m_TweakableProerties.HighGTurnDuration)
                {
                    m_HighGTurnCD = m_TweakableProerties.HighGTurnCD;
                }
            }
            else
            {
                m_HighGTurnCD = Mathf.Max(0, m_HighGTurnCD - deltaTime);
                m_HighGTrunDuration = 0;
            }
        }

        // Yaw轴和Pitch轴
        Vector2 angularVelocity = AxisToAngularVelocity(m_MoveAxis);
        m_AngularVelocity = Vector2.MoveTowards(m_AngularVelocity, angularVelocity, deltaTime * m_MaxAngularAcceleration);
        // 角度变化
        Quaternion deltaYawPitchRotation = Quaternion.Euler(m_AngularVelocity * deltaTime);
        // 绕Yaw轴和Pitch轴旋转后的角度
        Quaternion newRotation = m_Transform.localRotation * deltaYawPitchRotation;

        // Roll轴
        Vector3 newPlaneRootEulerAngles;
        {
            Vector3 currentRollEulerAngles = m_PlaneRootTransform.localEulerAngles;
            float currentRoll = Mathf.DeltaAngle(0, currentRollEulerAngles.z);
            float rollAxis = -m_Controller.GetRoll();
            float targetRoll = Mathf.Abs(rollAxis) < Mathf.Epsilon
                ? Mathf.Clamp(Mathf.Asin(-m_MoveAxis.x) * Mathf.Rad2Deg, -90, 90)
                : currentRoll + rollAxis * m_MaxRollAcceleration;

            // 和fTargetRollAngle距离小于45度时开始减速，越接近目标速度越小，这里的魔法数字是试出来的
            float distance = Mathf.Abs(currentRoll - targetRoll);
            float smooth = Mathf.Clamp01(distance / 60.0f);
            smooth = Mathf.Pow(smooth, 2.5f);
            // 限制最低转速为10度/秒
            float fakeCoordinateSmoothedMaxRollVelocity = Mathf.Min(m_MaxRollAcceleration
                , Mathf.Max(10.0f
                    , m_MaxRollAcceleration * smooth));

            float roll = hwmMath.MoveTowards(currentRoll
                , targetRoll
                , ref m_FakeCoordinateRollVelocity
                , fakeCoordinateSmoothedMaxRollVelocity
                , m_MaxRollAcceleration * 5.0f // 试出来的，这样比较好看
                , deltaTime);

            m_RollAcceleration = roll - currentRollEulerAngles.z;
            if (Mathf.Abs(m_RollAcceleration) >= 350)
            {
                m_RollAcceleration += Mathf.Sign(m_RollAcceleration) * -1 * 360f;
            }
            m_RollAcceleration /= deltaTime;

            newPlaneRootEulerAngles = new Vector3(currentRollEulerAngles.x
                , currentRollEulerAngles.y
                , roll);
        }
        #endregion

        #region 计算节流阀(速度)
        // 转向导致的减速
        {
            float degreeDeltaRotation = Quaternion.Angle(deltaYawPitchRotation, Quaternion.identity);
            float turnSpeed = degreeDeltaRotation * invertDeltaTime;

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
                * deltaTime;
        }

        // 阻力对速度的影响
        Vector3 velocityChangeCausedByDrag;
        {
            // 本地坐标系下的速度
            Vector3 velocity_LocalSpace = m_Transform.InverseTransformDirection(m_Velocity);
            // 空气阻力
            Vector3 dragForce_LocalSpace = hwmMath.CalculateDrag(velocity_LocalSpace
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

            velocityChangeCausedByDrag = dragAcceleration_LocalSpace * deltaTime;
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
                keyframe3Cache[0].time = m_TweakableProerties.LowHeight;
                keyframe3Cache[0].value = m_TweakableProerties.ThrustPowerFactor_LowHeight;
                keyframe3Cache[1].time = m_TweakableProerties.NormalHeight;
                keyframe3Cache[1].value = 1.0f;
                keyframe3Cache[2].time = m_TweakableProerties.HightHeight;
                keyframe3Cache[2].value = m_TweakableProerties.ThrustPowerFactor_HightHeight;

                thrustPower *= hwmMath.Evaluate(Mathf.Max(0, m_Transform.localPosition.y), keyframe3Cache);
            }

            float thrustAcceleration = hwmMath.PowerToAcceleration(thrustPower, m_PropulsiveSpeed, 1.0f, deltaTime);
            thrustAcceleration = Mathf.Min(thrustAcceleration, m_TweakableProerties.MaxThrustAcceleration);

            // 计算由重力产生的减速度、加速度
            float climbAngle = -Mathf.DeltaAngle(0, m_Transform.eulerAngles.x);
            float climbAmount = Mathf.Clamp(Mathf.Sin(climbAngle * Mathf.Deg2Rad), -1.0f, 1.0f);
            float propulsiveGravityAcceleration = climbAmount > 0
                ? Mathf.Lerp(0, -m_TweakableProerties.GravityDeceleration, climbAmount)
                : Mathf.Lerp(0, m_TweakableProerties.GravityAcceleration, -climbAmount);

            // 计算前向加速度
            float propulsiveAcceleration = thrustAcceleration + propulsiveGravityAcceleration;

            float newPropulsiveSpeed = m_PropulsiveSpeed + propulsiveAcceleration * deltaTime;
            newPropulsiveSpeed = Mathf.Min(newPropulsiveSpeed, m_TweakableProerties.MaxPropulsiveSpeed);

            float deltaSpeed = newPropulsiveSpeed - m_PropulsiveSpeed;
            m_Velocity += (m_Transform.forward * deltaSpeed);
        }
        #endregion

        #region 更新失速状态
        m_StallAmount = Mathf.MoveTowards(m_StallAmount
            , m_PropulsiveSpeed < m_TweakableProerties.BeginStallSpeed
                ? Mathf.Clamp01((m_PropulsiveSpeed - m_TweakableProerties.BeginStallSpeed) / (m_TweakableProerties.HeavyStallSpeed - m_TweakableProerties.BeginStallSpeed))
                : 0
            , deltaTime);

        m_StallState = m_StallAmount > 0
            ? m_StallState = StallState.Stall
            : m_PropulsiveSpeed <
                // 0.1: 决定NearStal的触发速度, 随便定的值
                (m_TweakableProerties.BeginStallSpeed + (m_TweakableProerties.NormalSpeed - m_TweakableProerties.BeginStallSpeed) * 0.1f)
                    ? m_StallState = StallState.NearStall
                    : m_StallState = StallState.None;

        if (m_StallState == StallState.Stall)
        {
            // 升力不足所产生的下落加速度
            m_Velocity.y += Mathf.Min(0
                , -30.0f * deltaTime - velocityChangeCausedByDrag.y) // 这里需要抵消阻力
                    * m_StallAmount;

            // 压机头
            Vector3 targetHeadDirection = Vector3.Normalize(m_Velocity);
            Vector3 rotAxis = Vector3.Cross(m_Transform.forward, targetHeadDirection);
            float maxAngle = Vector3.Angle(m_Transform.forward, targetHeadDirection);
            newRotation = Quaternion.AngleAxis(Mathf.Min(maxAngle, deltaTime * m_StallAmount * 60), rotAxis)
                * newRotation;

            // 失速导致高G转弯的CoolDown
            m_HighGTurnCD = m_TweakableProerties.HighGTurnCD;
        }
        #endregion

        // 更新旋转
        m_Transform.localRotation = newRotation;
        m_PlaneRootTransform.localEulerAngles = newPlaneRootEulerAngles;

        // 更新位置
        m_Transform.localPosition = m_Velocity * deltaTime + m_Transform.localPosition;
    }

    /// <summary>
	/// 根据杆量算出角速度(本地坐标)
	/// </summary>
	/// <param name="axis">输入的旋转向量，这个向量的+Y代表飞机抬头</param>
	/// <returns></returns>
	public Vector2 AxisToAngularVelocity(Vector2 axis)
    {
        Vector2 angularVelocity = new Vector2(axis.y * m_MaxAngularAcceleration
            , axis.x * m_MaxAngularAcceleration);

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
        float angularValue = Mathf.Abs(Mathf.Atan2(axis.y, axis.x) * hwmMath.IVNERT_PI * 2);
        // 使angularValue始终为0到1
        angularValue = angularValue > 1 ? 2 - angularValue : angularValue;

        // 算这个magnitudeScale是因为，我假设椭圆上的点到圆心的半径与角度的关系为linear的。所以根据rotateVector算一个角度，再根据角度算magnitudeScale
        float magnitudeScale = 1 - angularValue;

        float differentAngular = m_MaxAngularAcceleration * magnitudeScale - m_AngularVelocity.magnitude;
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

    /// <summary>
    /// 失速状态
    /// </summary>
    public enum StallState
    {
        /// <summary>
        /// 未失速
        /// </summary>
        None,
        /// <summary>
        /// 接近失速(未失速)
        /// </summary>
        NearStall,
        /// <summary>
        /// 失速
        /// </summary>
        Stall,
    }
}