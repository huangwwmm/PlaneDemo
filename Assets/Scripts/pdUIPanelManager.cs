using UnityEngine;
using UnityEngine.Experimental.Input;

public class pdUIPanelManager : MonoBehaviour
{
    private static pdUIPanelManager ms_Instance;

    private pdBaseUIPanel[] m_UIPanels;

    public static pdUIPanelManager GetInstance()
    {
        return ms_Instance;
    }

    public void ShowPanel(pdConstants.UIPanelName panelName, object param = null)
    {
        m_UIPanels[(int)panelName]._Show(param);
        // UNDONE Disable InputActionMap of last UIPanel and acitve InputActionMap of current UIPanel InputActionMap
    }

    public void HidePanel(pdConstants.UIPanelName panelName)
    {
        // UNDONE Disable InputActionMap of current UIPanel and acitve InputActionMap of last UIPanel InputActionMap
        m_UIPanels[(int)panelName]._Hide();
    }

    private void Awake()
    {
        ms_Instance = this;

        int panelCount = (int)pdConstants.UIPanelName.Count;
        m_UIPanels = new pdBaseUIPanel[panelCount];
        pdBaseUIPanel[] uiPanels = gameObject.GetComponentsInChildren<pdBaseUIPanel>();
        for (int iPanel = 0; iPanel < uiPanels.Length; iPanel++)
        {
            pdBaseUIPanel iterPanel = uiPanels[iPanel];
            int panelName = (int)iterPanel.GetUIPanelName();
            hwmDebug.Assert(m_UIPanels[panelName] == null, "m_UIPanels[panelName] == null");
            m_UIPanels[panelName] = iterPanel;

            iterPanel._Initialize();
        }

        pdInput.GetInstance().InputActions.UIPanelGeneral.Back.performed += OnInputPerformed_Back;
    }

    private void OnDestroy()
    {
        pdInput.GetInstance().InputActions.UIPanelGeneral.Back.performed -= OnInputPerformed_Back;

        m_UIPanels = null;

        ms_Instance = null;
    }

    private void OnInputPerformed_Back(InputAction.CallbackContext context)
    {
        // UNDONE Input Event apply to current UIPanel
    }
}