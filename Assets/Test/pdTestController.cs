using System;
using UnityEngine;
using UnityEngine.Experimental.Input;

public class pdTestController : pdBaseController
{
    public pdPlayerCamera MyCamera;

    protected override void HandleSetControllerPlane()
    {
        MyCamera.SetMyPlane(m_Plane, this);

        pdInput.GetInstance().InputActions.Plane.Enable();
        pdInput.GetInstance().InputActions.Plane.MoveVector2.performed += OnInputPerformed_MoveVector2;
        pdInput.GetInstance().InputActions.Plane.MoveVector2.cancelled += OnInputCancelled_MoveVector2;
        pdInput.GetInstance().InputActions.Plane.ViewVector2.performed += OnInputPerformed_ViewVector2;
        pdInput.GetInstance().InputActions.Plane.ViewVector2.cancelled += OnInputCancelled_ViewVector2;
        pdInput.GetInstance().InputActions.Plane.RollAxis.performed += OnInputPerformed_RollAxis;
        pdInput.GetInstance().InputActions.Plane.RollAxis.cancelled += OnInputCancelled_RollAxis;
        pdInput.GetInstance().InputActions.Plane.ThrottleAxis.performed += OnInputPerformed_ThrottleAxis;
        pdInput.GetInstance().InputActions.Plane.ThrottleAxis.cancelled += OnInputCancelled_ThrottleAxis;
        pdInput.GetInstance().InputActions.Plane.OpenPausePanel.performed += OnInputPerformed_OpenPausePanel;
    }

    protected override void HandleUnControllerPlane()
    {
        pdInput.GetInstance().InputActions.Plane.MoveVector2.performed -= OnInputPerformed_MoveVector2;
        pdInput.GetInstance().InputActions.Plane.MoveVector2.cancelled -= OnInputCancelled_MoveVector2;
        pdInput.GetInstance().InputActions.Plane.ViewVector2.performed -= OnInputPerformed_ViewVector2;
        pdInput.GetInstance().InputActions.Plane.ViewVector2.cancelled -= OnInputCancelled_ViewVector2;
        pdInput.GetInstance().InputActions.Plane.RollAxis.performed -= OnInputPerformed_RollAxis;
        pdInput.GetInstance().InputActions.Plane.RollAxis.cancelled -= OnInputCancelled_RollAxis;
        pdInput.GetInstance().InputActions.Plane.ThrottleAxis.performed -= OnInputPerformed_ThrottleAxis;
        pdInput.GetInstance().InputActions.Plane.ThrottleAxis.cancelled -= OnInputCancelled_ThrottleAxis;
        pdInput.GetInstance().InputActions.Plane.OpenPausePanel.performed -= OnInputPerformed_OpenPausePanel;
        pdInput.GetInstance().InputActions.Plane.Disable();

        MyCamera.SetMyPlane(null, null);
    }

    private void OnInputPerformed_MoveVector2(InputAction.CallbackContext context)
    {
        m_MoveVector2 = context.ReadValue<Vector2>();
    }

    private void OnInputCancelled_MoveVector2(InputAction.CallbackContext context)
    {
        m_MoveVector2 = Vector2.zero;
    }

    private void OnInputPerformed_ViewVector2(InputAction.CallbackContext context)
    {
        m_ViewVector2 = context.ReadValue<Vector2>();
    }

    private void OnInputCancelled_ViewVector2(InputAction.CallbackContext context)
    {
        m_ViewVector2 = Vector2.zero;
    }

    private void OnInputPerformed_RollAxis(InputAction.CallbackContext context)
    {
        m_RollAxis = context.ReadValue<float>();
    }

    private void OnInputCancelled_RollAxis(InputAction.CallbackContext context)
    {
        m_RollAxis = 0;
    }

    private void OnInputPerformed_ThrottleAxis(InputAction.CallbackContext context)
    {
        float throttle = context.ReadValue<float>();
        if (Mathf.Sign(throttle) > 0)
        {
            m_ThrottleState = throttle > 0.5f ? pdPlane.ThrottleState.BoostII : pdPlane.ThrottleState.Boost;
        }
        else
        {
            m_ThrottleState = throttle < -0.5f ? pdPlane.ThrottleState.BrakeII : pdPlane.ThrottleState.Brake;
        }
    }

    private void OnInputCancelled_ThrottleAxis(InputAction.CallbackContext context)
    {
        m_ThrottleState = pdPlane.ThrottleState.Normal;
    }

    private void OnInputPerformed_OpenPausePanel(InputAction.CallbackContext context)
    {
        pdUIPanelManager.GetInstance().ShowPanel(pdConstants.UIPanelName.TestEsc);
    }
}