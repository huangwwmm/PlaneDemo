using System.Collections.Generic;
using UnityEngine;

public abstract class pdBaseUIPanel : MonoBehaviour
{
    [SerializeField]
    private pdConstants.UIPanelName m_UIPanelName;

    private bool m_Displayed;

    public pdConstants.UIPanelName GetUIPanelName()
    {
        return m_UIPanelName;
    }

    internal void _Initialize()
    {
        m_Displayed = false;
        gameObject.SetActive(false);
    }

    internal void _Show(object param)
    {
        hwmDebug.Assert(!m_Displayed, "!m_Displayed");
        m_Displayed = true;

        _OnShow(param);
    }
    
    internal void _Hide()
    {
        hwmDebug.Assert(m_Displayed, "m_Displayed");
        m_Displayed = false;

        _OnHide();
    }

    protected abstract void _OnShow(object param);

    protected abstract void _OnHide();

    // UNDONE Add function
    //  1. get InputActionMap of this UIPanel
    //  2. get This UIPanel need disable InputAction[] of UIPanelGeneral(InputActionMap)
}