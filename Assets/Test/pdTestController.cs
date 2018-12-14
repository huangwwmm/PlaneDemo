using UnityEngine;

public class pdTestController : pdBaseController
{
    public pdCamera m_Camera;

    protected void FixedUpdate()
    {
        m_Axis.x = Input.GetKey(KeyCode.A)
            ? -1
            : Input.GetKey(KeyCode.D)
                ? 1
                : 0;
        m_Axis.y = Input.GetKey(KeyCode.S)
            ? -1
            : Input.GetKey(KeyCode.W)
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
        m_Camera.SetMyPlane(m_Plane);
    }

    protected override void HandleUnControllerPlane()
    {
        m_Camera.SetMyPlane(null);
    }
}