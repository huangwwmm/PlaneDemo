using UnityEngine;

public class pdBaseController : MonoBehaviour
{
    protected Vector2 m_Axis;
    protected float m_Roll = 0;
    protected pdPlane.ThrottleState m_Throttle = pdPlane.ThrottleState.Normal;

    public Vector2 GetAxis()
    {
        return m_Axis;
    }

    public pdPlane.ThrottleState GetThrottle()
    {
        return m_Throttle;
    }

    public float GetRoll()
    {
        return m_Roll;
    }
}