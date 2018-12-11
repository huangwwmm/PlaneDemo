using UnityEngine;

public class pdPlaneTweakableProperties : ScriptableObject
{
    /// <summary>
    /// 最大滚转角速度
    /// </summary>
    public float MaxRollAngularSpeed = 270;
    /// <summary>
    /// 失速时对转向能力的影响
    /// </summary>
    public float AngularSpeedReduceByStallNormalized = 0.5f;
    /// <summary>
    /// 受损状态下的滚转角速度缩放
    /// </summary>
    public float RollAngularSpeedScaleWhenDamaged = 0.5f;
}