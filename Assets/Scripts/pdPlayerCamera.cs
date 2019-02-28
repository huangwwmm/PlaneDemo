using UnityEngine;

public class pdPlayerCamera : MonoBehaviour
{
    /// <summary>
    /// 玩家的飞机
    /// </summary>
    private pdPlane m_MyPlane;
    private pdBaseController m_MyController;
    /// <summary>
    /// 玩家飞机的Transform，Cache出来是为了性能
    /// </summary>
    private Transform m_MyPlaneTransform;

    /// <summary>
    /// 相机偏移, 可能导致相机偏移的因素包含但不限于:
    ///     加速度 
    /// </summary>
    private Vector3 m_TPSCameraOffset = Vector3.zero;
    /// <summary>
    /// 用户输入的杆量导致的相机旋转角度(欧拉角)
    /// </summary>
    private Vector2 m_TPSCameraAngularVelocityByViewAxis = Vector2.zero;
    /// <summary>
    /// 用户不输入ViewAxis的持续时间
    /// </summary>
    private float m_NoViewAxisInputDuration = 0;

    private Transform m_Transform;
    private Camera m_Camera;
    /// <summary>
    /// UNDONE Load when initialize
    /// </summary>
    public pdCameraProperties m_Properties;

    public void SetMyPlane(pdPlane plane, pdBaseController controller)
    {
        m_MyPlane = plane;
        m_MyPlaneTransform = m_MyPlane != null
            ? m_MyPlane.transform
            : null;

        m_MyController = controller;
    }

    protected void Awake()
    {
        m_Transform = transform;
        m_Camera = gameObject.GetComponent<Camera>();
    }

    protected void OnDestroy()
    {
        m_Camera = null;
    }

    protected void LateUpdate()
    {
        Do_Update(Time.deltaTime);
    }

    private void Do_Update(float deltaTime)
    {
        if (m_MyPlane == null)
        {
            return;
        }
        // 相机延飞机前方向的偏移 UNDONE 值不会经常变动，Cache下来不要每帧算
        float cameraToPlaneForwardOffset = m_Properties.TPSCameraPlaneDistance * Mathf.Cos(m_Properties.TPSCameraToPlaneAngle * Mathf.Deg2Rad);
        // 相机延飞机上方向的偏移 UNDONE 值不会经常变动，Cache下来不要每帧算
        float cameraToPlaneUpOffset = m_Properties.TPSCameraPlaneDistance * Mathf.Sin(m_Properties.TPSCameraToPlaneAngle * Mathf.Deg2Rad);

        #region TPS
        CameraProperties tpsCameraProperties_WorldSpace;
        {    
            // 用户主动控制视角
            Quaternion rotationByInput;
            { 
                Vector2 viewVector2 = m_MyController.GetViewVector2();
                if (viewVector2.sqrMagnitude > 0)
                {
                    m_NoViewAxisInputDuration = 0;
                    
                    // 用户有输入
                    Vector2 angularAcceleration = new Vector2(-viewVector2.y * m_Properties.TPSCameraAngularAccelerationByInput
                        , viewVector2.x * m_Properties.TPSCameraAngularAccelerationByInput);
                    m_TPSCameraAngularVelocityByViewAxis = m_TPSCameraAngularVelocityByViewAxis + angularAcceleration * deltaTime;
                }
                else
                {
                    m_NoViewAxisInputDuration += deltaTime;

                    if (m_NoViewAxisInputDuration > m_Properties.TPSCameraAngularByInputToZeroWaitTime)
                    {
                        // 用户无输入
                        m_TPSCameraAngularVelocityByViewAxis = Vector2.MoveTowards(m_TPSCameraAngularVelocityByViewAxis
                            , Vector2.zero, m_Properties.TPSCameraAngularByInputToZeroAcceleration * deltaTime);
                    }
                }

                m_TPSCameraAngularVelocityByViewAxis.y = m_TPSCameraAngularVelocityByViewAxis.y > 180.0f
                    ? m_TPSCameraAngularVelocityByViewAxis.y - 360.0f
                    : m_TPSCameraAngularVelocityByViewAxis.y < -180.0f
                        ? m_TPSCameraAngularVelocityByViewAxis.y + 360.0f
                        : m_TPSCameraAngularVelocityByViewAxis.y;

                // UNDONE 防止pitch越位

                rotationByInput = Quaternion.Euler(m_TPSCameraAngularVelocityByViewAxis);
            }

            // 注视点
            tpsCameraProperties_WorldSpace.LookAt = m_MyPlaneTransform.position + rotationByInput * m_MyPlaneTransform.forward * m_Properties.TPSCameraLookAtToPlaneForwardDistance;

            // 根据注视点推算出的相机坐标
            tpsCameraProperties_WorldSpace.Position = tpsCameraProperties_WorldSpace.LookAt
                + rotationByInput * m_MyPlaneTransform.TransformDirection(new Vector3(0
                    , cameraToPlaneUpOffset
                    , -cameraToPlaneForwardOffset - m_Properties.TPSCameraLookAtToPlaneForwardDistance));
            tpsCameraProperties_WorldSpace.UpDirection = Vector3.up;

            // 根据飞机的G力偏移相机, 给玩家传达飞行员承受的加速度
            Vector3 planeAcceleration_LocalSpace = m_MyPlane.GetAcceleration_LocalSpace();

            Vector3 offsetByGForce = m_MyPlaneTransform.TransformDirection(new Vector3(hwmMath.ClampAbs(planeAcceleration_LocalSpace.x * m_Properties.TPSCameraOffsetMultiplyByGForce.x, m_Properties.TPSCameraOffsetMaxByGForce.x)
                , hwmMath.ClampAbs(planeAcceleration_LocalSpace.y * m_Properties.TPSCameraOffsetMultiplyByGForce.y, m_Properties.TPSCameraOffsetMaxByGForce.y)
                , hwmMath.ClampAbs(-planeAcceleration_LocalSpace.z * m_Properties.TPSCameraOffsetMultiplyByGForce.z, m_Properties.TPSCameraOffsetMaxByGForce.z)));

            // TODO 根据负载程度抖动相机? 电影和动画中经常这么做

            // 计算Offset
            m_TPSCameraOffset = Vector3.Lerp(m_TPSCameraOffset
                , offsetByGForce
                , Mathf.Min(deltaTime * m_Properties.TPSCameraOffsetLerpSpeed, 1.0f));
            tpsCameraProperties_WorldSpace.Position += m_TPSCameraOffset;
        }
        #endregion

        // UNDONE lerp to CameraProperties
        m_Transform.localPosition = tpsCameraProperties_WorldSpace.Position;
        m_Transform.localRotation = Quaternion.LookRotation(tpsCameraProperties_WorldSpace.LookAt - tpsCameraProperties_WorldSpace.Position
            , tpsCameraProperties_WorldSpace.UpDirection);
    }

    protected struct CameraProperties
    {
        public Vector3 Position;
        public Vector3 LookAt;
        public Vector3 UpDirection;

        public CameraProperties Transform(Transform transform)
        {
            CameraProperties cameraProperties;
            cameraProperties.Position = transform.TransformPoint(Position);
            cameraProperties.LookAt = transform.TransformPoint(LookAt);
            cameraProperties.UpDirection = transform.TransformDirection(UpDirection);
            return cameraProperties;
        }

        public CameraProperties InverseTransform(Transform transform)
        {
            CameraProperties cameraProperties;
            cameraProperties.Position = transform.InverseTransformPoint(Position);
            cameraProperties.LookAt = transform.InverseTransformPoint(LookAt);
            cameraProperties.UpDirection = transform.InverseTransformDirection(UpDirection);
            return cameraProperties;
        }
    }

    private void OnGUI()
    {
        GUILayout.Box(m_TPSCameraAngularVelocityByViewAxis.ToString());
    }
}