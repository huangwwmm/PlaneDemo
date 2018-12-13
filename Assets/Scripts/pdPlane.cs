using UnityEngine;

public class pdPlane : MonoBehaviour
{
    /// <summary>
    /// equal 1 / 78
    /// 78：这个值越大，转向加速度越小
    /// 试出来的效果理想的值
    /// </summary>
    private const float MAX_TURN_ANGULAR_ACCELERATION_MULTIPLIER = 0.01282f;

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
    private float m_MaxRollAcceleration = 0;
    /// <summary>
    /// 大于0时，飞机失速
    /// </summary>
    private float m_StallAmount = 0;
    /// <summary>
    /// 最大转向速度
    /// </summary>
	private float m_MaxTurnAngularSpeed = 0;
    /// <summary>
    /// 最大转向加速度
    /// </summary>
    private float m_MaxTurnAngularAcceleration = 0;
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
        // 飞机挂载对飞机转向能力的影响
        // 这个数值越大，飞机转向能力越差。为0时，不影响飞机滚转
        // UNDONE 还没做飞机挂载
        float m_RotateReductionRatioByPayload = 0;

        m_MaxRollAcceleration = m_TweakableProerties.MaxRollAcceleration
            // 失速时转向能力降低
            * ((1.0f - Mathf.Clamp01(m_StallAmount * 2.0f)) * m_TweakableProerties.AngularSpeedReduceByStallNormalized)
            // 挂载组对转向的影响
            * (1.0f - m_RotateReductionRatioByPayload)
            // 引擎受损时转向的影响 UNDONE 还没做飞机引擎
            * (false ? m_TweakableProerties.RollAngularSpeedScaleWhenDamaged : 1);

        // 速度对转向能力的影响
        Keyframe[] angularSpeedFactorsAffectByPropulsiveSpeed = new Keyframe[3]; // Keyframe is struct, not have GC
        angularSpeedFactorsAffectByPropulsiveSpeed[0].time = m_TweakableProerties.LowSpeed;
        angularSpeedFactorsAffectByPropulsiveSpeed[0].value = m_TweakableProerties.TurnAngularSpeedFactor_LowSpeed;
        angularSpeedFactorsAffectByPropulsiveSpeed[1].time = m_TweakableProerties.NormalSpeed;
        angularSpeedFactorsAffectByPropulsiveSpeed[1].value = 1.0f;
        angularSpeedFactorsAffectByPropulsiveSpeed[2].time = m_TweakableProerties.HightSpeed;
        angularSpeedFactorsAffectByPropulsiveSpeed[2].value = m_TweakableProerties.TurnAngularSpeedFactor_HighSpeed;
        // 速度对转向能力的影响 TODO 如果效果不好，再考虑特殊处理
        float angularSpeedFactorAffectByPropulsiveSpeed = hwmUtility.Evaluate(m_PropulsiveSpeed, angularSpeedFactorsAffectByPropulsiveSpeed);

        // 高度对旋转能力的影响
        Keyframe[] angularSpeedFactorsAffectByHeight = new Keyframe[3]; // Keyframe is struct, not have GC
        angularSpeedFactorsAffectByHeight[0].time = m_TweakableProerties.LowHeight;
        angularSpeedFactorsAffectByHeight[0].value = m_TweakableProerties.TurnAngularSpeedFactor_LowHeight;
        angularSpeedFactorsAffectByHeight[1].time = m_TweakableProerties.NormalHeight;
        angularSpeedFactorsAffectByHeight[1].value = 1.0f;
        angularSpeedFactorsAffectByHeight[2].time = m_TweakableProerties.HightHeight;
        angularSpeedFactorsAffectByHeight[2].value = m_TweakableProerties.TurnAngularSpeedFactor_HighestHeight;
        // 飞行高度对转向能力的影响
        float angularSpeedFactorAffectByHeight = hwmUtility.Evaluate(Mathf.Max(0, m_Transform.localPosition.y), angularSpeedFactorsAffectByHeight);

        m_MaxTurnAngularSpeed = m_TweakableProerties.MaxTurnAngularSpeed
            // 推进速度
            * angularSpeedFactorAffectByPropulsiveSpeed
            // 飞行高度
            * angularSpeedFactorAffectByHeight
            // 失速
            * (1.0f - Mathf.Clamp01(m_StallAmount * 4.0f)
                * m_TweakableProerties.AngularSpeedReduceByStallNormalized)
            // 机翼受伤 UNDONE 还没做机翼
            * (false ? m_TweakableProerties.TurnAngularSpeedScaleWhenDamaged : 1)
            // 挂载组
            * (1.0f - m_RotateReductionRatioByPayload);

        m_MaxTurnAngularAcceleration = m_MaxTurnAngularSpeed * m_MaxRollAcceleration * MAX_TURN_ANGULAR_ACCELERATION_MULTIPLIER;
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
        // 持续高G转弯，进入CD
        if (m_HighGTrunDuration > m_TweakableProerties.HighGTurnDuration)
        {
            m_HighGTurnCD = m_TweakableProerties.HighGTurnCD;
        }

        // 判断高G转弯
        m_IsHighGTurn = (m_Throttle == ThrottleState.Brake || m_Throttle == ThrottleState.BrakeII)
            // 确保飞机在高速中做不出高G转弯(例如俯冲)
            && m_PropulsiveSpeed < (m_TweakableProerties.HightSpeed + m_TweakableProerties.NormalSpeed) * 0.5f
            && m_HighGTurnCD < 0
            && CanHighGTrunForAxis(m_Axis);

        // 更新角速度
        Vector2 angularVelocity = AxisToAngularVelocity(m_Axis);
        m_AngularVelocity = Vector2.MoveTowards(m_AngularVelocity, angularVelocity, delta * m_MaxTurnAngularAcceleration);

        // UNDONE 角度计算这里没太看懂，记得重新算一遍
        Quaternion qOldWorld = m_Transform.localRotation;
        Vector2 eulerDeltaRotation = m_AngularVelocity * delta;
        Quaternion qDeltaRotation = Quaternion.Euler(eulerDeltaRotation);
        float degreeDeltaRotation = Quaternion.Angle(qDeltaRotation, Quaternion.identity);
        float fTurnSpeed = degreeDeltaRotation * invertDelta;
        // 转向导致的速度损失
        float decelerationCausedByTurning = (fTurnSpeed * 0.01111f) // 0.01111f equal 1 / 90.0f
            * m_PropulsiveSpeed
            * m_TweakableProerties.DecelerationCausedByTurningCoefficient;

        m_HighGTurnCD -= delta;
        if (m_IsHighGTurn)
        {
            // 高G转弯导致减速
            decelerationCausedByTurning *= m_TweakableProerties.DecelerationCausedByHighGTurn;
            m_HighGTrunDuration += delta;
        }
        else
        {
            m_HighGTrunDuration = 0;
        }

        decelerationCausedByTurning = Mathf.Clamp(decelerationCausedByTurning, 0, m_TweakableProerties.MaxDecelerationCausedByTurning);
        // 减速
        m_Velocity -= m_Transform.forward
            * Mathf.Clamp(decelerationCausedByTurning, 0, m_TweakableProerties.MaxDecelerationCausedByTurning)
            * delta;

        Quaternion worldRotation = qOldWorld * qDeltaRotation;
        // 保证CameraUp仅在m_Transform.forward轴上有旋转
        Vector3 forward = worldRotation * Vector3.forward;

        // 更新Roll轴
        float inputRoll = -m_Controller.GetRoll();
        float targetRollAcceleration = 0;
        if (Mathf.Abs(inputRoll) < Mathf.Epsilon)
        {
            Vector3 vLeft = Vector3.Cross(forward, Vector3.up).normalized;
            m_TargetCameraUp = Vector3.Cross(vLeft, forward).normalized;

            Vector3 vMyUp = m_Transform.up;
            float fDeltaCos = Vector3.Dot(m_TargetCameraUp, vMyUp);
            fDeltaCos = Mathf.Sign(fDeltaCos) * Mathf.Clamp01(Mathf.Abs(fDeltaCos));
            if (fDeltaCos < 0.999f)
            {
                Vector3 vRotAxis = Vector3.Cross(m_TargetCameraUp, vMyUp);
                float fDeltaSin = vRotAxis.magnitude;
                vRotAxis = fDeltaSin < Mathf.Epsilon
                    ? forward // 当m_CameraTargetUp = -m_CameraUp时，默认沿着逆时针旋转
                    : vRotAxis / fDeltaSin;
                float fDeltaDegree = Mathf.Acos(fDeltaCos) * Mathf.Rad2Deg;

                // 机头接近与地面垂直时，减小自动校正的程度（否则会造成较大的偏航）
                float fLerpAmountMul = 1.0f - Mathf.Clamp01(Mathf.Abs(Vector3.Dot(forward, Vector3.up)));
                // 用户在垂直方向上的输入分量较大（做Loop）时，减弱恢复速度。
                fLerpAmountMul *= 1.0f - Mathf.Clamp01(Mathf.Abs(m_Axis.y));

                // fDeltaDegree越大需要的旋转速度越高
                float fLerpResult = Mathf.Lerp(fDeltaDegree, 0, fLerpAmountMul * m_TweakableProerties.RollToZeroStrength * delta);
                targetRollAcceleration = (fLerpResult - fDeltaDegree) * invertDelta;
                targetRollAcceleration = Vector3.Dot(vRotAxis, forward) > 0
                    ? targetRollAcceleration
                    : -targetRollAcceleration;

                targetRollAcceleration = hwmUtility.ClampAbs(targetRollAcceleration, m_MaxRollAcceleration);
            }
        }
        else
        {
            targetRollAcceleration = Mathf.Clamp(inputRoll, -1.0f, 1.0f) * m_MaxRollAcceleration;
        }

        targetRollAcceleration *= Mathf.Clamp01(1.0f - m_StallAmount * 10.0f);

        m_RollAcceleration = Mathf.MoveTowards(m_RollAcceleration
            , targetRollAcceleration
            , m_MaxRollAcceleration * delta * 2.0f); // 实际Roll操作时，Roll的角加速度

        Quaternion rollDelta = Quaternion.AngleAxis(m_RollAcceleration * delta, forward);
        m_Transform.localRotation = rollDelta * worldRotation;
        #endregion

        #region 更新节流阀(速度)
        // 飞机在径向和轴向上的阻力
        // 本地坐标系下的速度
		Vector3 velocity_LocalSpace = m_Transform.InverseTransformDirection(m_Velocity);
        // -(V * V)
        Vector3 dragForce_LocalSpace = new Vector3(-velocity_LocalSpace.x * Mathf.Abs(velocity_LocalSpace.x),
            -velocity_LocalSpace.y * Mathf.Abs(velocity_LocalSpace.y),
            -velocity_LocalSpace.z * Mathf.Abs(velocity_LocalSpace.z));
        // -(V * V) * D
        dragForce_LocalSpace.Scale(new Vector3(m_TweakableProerties.VerticalDrag
            , m_TweakableProerties.VerticalDrag
            , m_Throttle == ThrottleState.Brake
                ? m_TweakableProerties.PropulsiveDrag_Brake
                : m_Throttle == ThrottleState.BrakeII
                    ? m_TweakableProerties.PropulsiveDrag_BrakeII
                    : m_TweakableProerties.PropulsiveDrag));
        Vector3 dragAcceleration_LocalSpace = dragForce_LocalSpace; // 加速度 = 阻力 / 自身质量，这里假设自身质量为1
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
        #endregion
    }

    /// <summary>
	/// 根据杆量算出角速度(本地坐标)
	/// </summary>
	/// <param name="axis">输入的旋转向量，这个向量的+Y代表飞机抬头</param>
	/// <returns></returns>
	public Vector2 AxisToAngularVelocity(Vector2 axis)
    {
        Vector2 angularVelocity = new Vector2(axis.y * m_MaxTurnAngularSpeed
            , axis.x * m_MaxTurnAngularSpeed);

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
        // UNDONE 没看懂，但是感觉有问题。 m_MaxTurnAngularSpeed的单位是度/秒，m_AngularVelocity.magnitude是杆量大小，这两个单位不同，怎么相减
        // 0 => 0度; 1 => 90度; 2 => 180度
        float angularValue = Mathf.Abs(Mathf.Atan2(axis.y, axis.x) * hwmUtility.IVNERT_PI * 2);
        // 使angularValue始终为0到1
        angularValue = angularValue > 1 ? 2 - angularValue : angularValue;

        // 算这个magnitudeScale是因为，我假设椭圆上的点到圆心的半径与角度的关系为linear的。所以根据rotateVector算一个角度，再根据角度算magnitudeScale
        float magnitudeScale = 1 - angularValue;

        float differentAngular = m_MaxTurnAngularSpeed * magnitudeScale - m_AngularVelocity.magnitude;
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