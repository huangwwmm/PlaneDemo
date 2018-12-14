using System;
using UnityEngine;

public class pdBaseController : MonoBehaviour
{
    protected Vector2 m_MoveAxis;
    protected Vector2 m_ViewAxis;
    protected float m_Roll = 0;
    protected pdPlane.ThrottleState m_Throttle = pdPlane.ThrottleState.Normal;
    protected pdPlane m_Plane;

    public Vector2 GetMoveAxis()
    {
        return m_MoveAxis;
    }

    public Vector2 GetViewAxis()
    {
        return m_ViewAxis;
    }

    public pdPlane.ThrottleState GetThrottle()
    {
        return m_Throttle;
    }

    public float GetRoll()
    {
        return m_Roll;
    }

    public void SetControllerPlane(pdPlane snake)
    {
        hwmDebug.Assert(m_Plane == null, "m_Plane == null");

        m_Plane = snake;

        HandleSetControllerPlane();
    }

    public void UnControllerPlane()
    {
        hwmDebug.Assert(m_Plane != null, "m_Plane != null");

        HandleUnControllerPlane();

        m_Plane = null;
    }

    protected virtual void HandleSetControllerPlane()
    {
    }

    protected virtual void HandleUnControllerPlane()
    {
    }
}