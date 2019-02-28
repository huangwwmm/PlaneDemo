// GENERATED AUTOMATICALLY FROM 'Assets/Test/InputActions.inputactions'

using System;
using UnityEngine;
using UnityEngine.Experimental.Input;


[Serializable]
public class pdInputActions : InputActionAssetReference
{
    public pdInputActions()
    {
    }
    public pdInputActions(InputActionAsset asset)
        : base(asset)
    {
    }
    private bool m_Initialized;
    private void Initialize()
    {
        // Plane
        m_Plane = asset.GetActionMap("Plane");
        m_Plane_MoveVector2 = m_Plane.GetAction("MoveVector2");
        m_Plane_ViewVector2 = m_Plane.GetAction("ViewVector2");
        m_Plane_RollAxis = m_Plane.GetAction("RollAxis");
        m_Plane_ThrottleAxis = m_Plane.GetAction("ThrottleAxis");
        m_Plane_OpenPausePanel = m_Plane.GetAction("OpenPausePanel");
        // UIPanelGeneral
        m_UIPanelGeneral = asset.GetActionMap("UIPanelGeneral");
        m_UIPanelGeneral_Back = m_UIPanelGeneral.GetAction("Back");
        // UIPanelEsc
        m_UIPanelEsc = asset.GetActionMap("UIPanelEsc");
        m_Initialized = true;
    }
    private void Uninitialize()
    {
        m_Plane = null;
        m_Plane_MoveVector2 = null;
        m_Plane_ViewVector2 = null;
        m_Plane_RollAxis = null;
        m_Plane_ThrottleAxis = null;
        m_Plane_OpenPausePanel = null;
        m_UIPanelGeneral = null;
        m_UIPanelGeneral_Back = null;
        m_UIPanelEsc = null;
        m_Initialized = false;
    }
    public void SetAsset(InputActionAsset newAsset)
    {
        if (newAsset == asset) return;
        if (m_Initialized) Uninitialize();
        asset = newAsset;
    }
    public override void MakePrivateCopyOfActions()
    {
        SetAsset(ScriptableObject.Instantiate(asset));
    }
    // Plane
    private InputActionMap m_Plane;
    private InputAction m_Plane_MoveVector2;
    private InputAction m_Plane_ViewVector2;
    private InputAction m_Plane_RollAxis;
    private InputAction m_Plane_ThrottleAxis;
    private InputAction m_Plane_OpenPausePanel;
    public struct PlaneActions
    {
        private pdInputActions m_Wrapper;
        public PlaneActions(pdInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @MoveVector2 { get { return m_Wrapper.m_Plane_MoveVector2; } }
        public InputAction @ViewVector2 { get { return m_Wrapper.m_Plane_ViewVector2; } }
        public InputAction @RollAxis { get { return m_Wrapper.m_Plane_RollAxis; } }
        public InputAction @ThrottleAxis { get { return m_Wrapper.m_Plane_ThrottleAxis; } }
        public InputAction @OpenPausePanel { get { return m_Wrapper.m_Plane_OpenPausePanel; } }
        public InputActionMap Get() { return m_Wrapper.m_Plane; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled { get { return Get().enabled; } }
        public InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator InputActionMap(PlaneActions set) { return set.Get(); }
    }
    public PlaneActions @Plane
    {
        get
        {
            if (!m_Initialized) Initialize();
            return new PlaneActions(this);
        }
    }
    // UIPanelGeneral
    private InputActionMap m_UIPanelGeneral;
    private InputAction m_UIPanelGeneral_Back;
    public struct UIPanelGeneralActions
    {
        private pdInputActions m_Wrapper;
        public UIPanelGeneralActions(pdInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @Back { get { return m_Wrapper.m_UIPanelGeneral_Back; } }
        public InputActionMap Get() { return m_Wrapper.m_UIPanelGeneral; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled { get { return Get().enabled; } }
        public InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator InputActionMap(UIPanelGeneralActions set) { return set.Get(); }
    }
    public UIPanelGeneralActions @UIPanelGeneral
    {
        get
        {
            if (!m_Initialized) Initialize();
            return new UIPanelGeneralActions(this);
        }
    }
    // UIPanelEsc
    private InputActionMap m_UIPanelEsc;
    public struct UIPanelEscActions
    {
        private pdInputActions m_Wrapper;
        public UIPanelEscActions(pdInputActions wrapper) { m_Wrapper = wrapper; }
        public InputActionMap Get() { return m_Wrapper.m_UIPanelEsc; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled { get { return Get().enabled; } }
        public InputActionMap Clone() { return Get().Clone(); }
        public static implicit operator InputActionMap(UIPanelEscActions set) { return set.Get(); }
    }
    public UIPanelEscActions @UIPanelEsc
    {
        get
        {
            if (!m_Initialized) Initialize();
            return new UIPanelEscActions(this);
        }
    }
    private int m_KeyboardMouseSchemeIndex = -1;
    public InputControlScheme KeyboardMouseScheme
    {
        get

        {
            if (m_KeyboardMouseSchemeIndex == -1) m_KeyboardMouseSchemeIndex = asset.GetControlSchemeIndex("Keyboard & Mouse");
            return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
        }
    }
}
