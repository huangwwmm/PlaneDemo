using UnityEngine;

public class pdPlane : MonoBehaviour
{
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
    private Vector3 m_AngularVelocity ;
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
    /// 飞机挂载对飞机滚转能力的影响
    /// 这个数值越大，飞机滚转能力越差。为0时，不影响飞机滚转
    /// </summary>
    private float m_RotateReductionRatioByPayload;
    /// <summary>
    /// 最大滚转速度
    /// </summary>
    private float m_MaxRollSpeed = 0;
    /// <summary>
    /// 大于0时，飞机失速
    /// </summary>
    private float m_StallAmount = 0;

    private Transform m_Transform;

    /// <summary>
    /// UNDONE Load when initialize
    /// </summary>
    public pdPlaneTweakableProperties m_TweakableProerties;

    protected void Awake()
    {
        m_Transform = transform;
    }

    protected void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime;
        DoUpdateMovement(delta);
    }

    private void DoUpdateMovement(float delta)
    {
        float invertDelta = 1.0f / delta; // for performance

        m_Acceleration = (m_Velocity - m_LastVelocity) * invertDelta;
        m_LastVelocity = m_Velocity;

        m_AngularAcceleration = (m_AngularVelocity - m_LastAngularVelocity) * invertDelta;
        m_LastAngularVelocity = m_AngularVelocity;

        m_Speed = m_Velocity.magnitude;
        m_PropulsiveSpeed = Vector3.Dot(m_Velocity, m_Transform.forward);

        m_RotateReductionRatioByPayload = 0; // UNDONE 还没做飞机挂载

        m_MaxRollSpeed = m_TweakableProerties.MaxRollAngularSpeed
            // 失速时转向能力降低
            * ((1.0f - Mathf.Clamp01(m_StallAmount * 2.0f)) * m_TweakableProerties.AngularSpeedReduceByStallNormalized)
            // 挂载组对转向的影响
            * (1.0f - m_RotateReductionRatioByPayload)
            // 引擎受损时转向的影响 UNDONE 还没做飞机引擎
            * (false ? m_TweakableProerties.RollAngularSpeedScaleWhenDamaged : 1);
    }
}