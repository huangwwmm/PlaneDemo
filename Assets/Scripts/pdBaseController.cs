using System;
using UnityEngine;

public class pdBaseController : MonoBehaviour
{
    protected Vector2 m_MoveVector2;
    protected Vector2 m_ViewVector2;
    protected float m_RollAxis = 0;
    protected pdPlane.ThrottleState m_ThrottleState = pdPlane.ThrottleState.Normal;
    protected pdPlane m_Plane;

    public Vector2 GetMoveVector2()
    {
        return m_MoveVector2;
    }

    public Vector2 GetViewVector2()
    {
        return m_ViewVector2;
    }

    public pdPlane.ThrottleState GetThrottleState()
    {
        return m_ThrottleState;
    }

    public float GetRollAxis()
    {
        return m_RollAxis;
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