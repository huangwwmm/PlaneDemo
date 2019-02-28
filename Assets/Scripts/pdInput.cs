using UnityEngine;

public class pdInput : MonoBehaviour
{
    private static pdInput ms_Instance;

    public pdInputActions InputActions;

    public static pdInput GetInstance()
    {
        return ms_Instance;
    }

    protected void Awake()
    {
        ms_Instance = this;

        InputActions.Plane.Disable();
        InputActions.UIPanelGeneral.Disable();
        InputActions.UIPanelEsc.Disable();
    }

    protected void OnDestroy()
    {
        ms_Instance = null;
    }
}