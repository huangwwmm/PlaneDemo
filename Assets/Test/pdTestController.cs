using UnityEngine;

public class pdTestController : pdBaseController
{
    public pdCamera m_Camera;

    protected void FixedUpdate()
    {
        m_MoveAxis.x = Input.GetKey(KeyCode.A)
            ? -1
            : Input.GetKey(KeyCode.D)
                ? 1
                : 0;
        m_MoveAxis.y = Input.GetKey(KeyCode.S)
            ? -1
            : Input.GetKey(KeyCode.W)
                ? 1
                : 0;

        m_ViewAxis.x = Input.GetKey(KeyCode.LeftArrow)
           ? -1
           : Input.GetKey(KeyCode.RightArrow)
               ? 1
               : 0;
        m_ViewAxis.y = Input.GetKey(KeyCode.DownArrow)
            ? -1
            : Input.GetKey(KeyCode.UpArrow)
                ? 1
                : 0;
        
        m_Roll = Input.GetKey(KeyCode.Q)
            ? -1
            : Input.GetKey(KeyCode.E)
                ? 1
                : 0;

        m_Throttle = Input.GetKey(KeyCode.LeftControl)
            ? pdPlane.ThrottleState.Brake
            : Input.GetKey(KeyCode.LeftShift)
                ? pdPlane.ThrottleState.Boost
                : pdPlane.ThrottleState.Normal;
    }

    protected override void HandleSetControllerPlane()
    {
        m_Camera.SetMyPlane(m_Plane, this);
    }

    protected override void HandleUnControllerPlane()
    {
        m_Camera.SetMyPlane(null, null);
    }
}