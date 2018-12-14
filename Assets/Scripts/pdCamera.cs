using UnityEngine;

public class pdCamera : MonoBehaviour
{
    /// <summary>
    /// 第三人称视角下，相机与飞机的极坐标半径
    /// </summary>
    public float TPSCameraPlaneDistance = 23.0f;
    /// <summary>
    /// 第三人称视角下，相机与飞机的极坐标角度
    /// </summary>
    public float TPSCameraToPlaneAngle = 13.79f;
    /// <summary>
    /// 相机的LookAt点位于Plane前方的距离
    /// </summary>
    public float TPSCameraLookAtToPlaneForwardDistance = 200;

    /// <summary>
    /// 加速度乘上这个值，所得为相机偏移(m)
    /// 三个值分别对应飞机本地空间的三个方向：x左右，y垂直，z前后
    /// 一版垂直要比左右低，因为:
    ///     屏幕左右的空间比上下大。上下位移太大会导致飞机以到屏幕中间，遮挡准星
    /// </summary>
    public Vector3 TPSCameraOffsetMultiplyByGForce = new Vector3(6.0f / 67.4f, 2.0f / 67.4f, 0.1f);
    /// <summary>
    /// 加速度导致相机偏移的最大值
    /// </summary>
    public Vector3 TPSCameraOffsetMaxByGForce = new Vector3(6.0f, 2.0f, 2.0f);
    /// <summary>
    /// 相机偏移<see cref="m_TPSCameraOffset"/>的Lerp速度
    /// </summary>
    public float TPSCameraOffsetLerpSpeed = 2.0f;

    /// <summary>
    /// 玩家的飞机
    /// </summary>
    private pdPlane m_MyPlane;
    /// <summary>
    /// 玩家飞机的Transform，Cache出来是为了性能
    /// </summary>
    private Transform m_MyPlaneTransform;

    /// <summary>
    /// 相机偏移, 可能导致相机偏移的因素包含但不限于:
    ///     加速度 
    /// </summary>
    private Vector3 m_TPSCameraOffset = Vector3.zero;

    private Transform m_Transform;
    private Camera m_Camera;

    public void SetMyPlane(pdPlane plane)
    {
        m_MyPlane = plane;
        m_MyPlaneTransform = m_MyPlane != null
            ? m_MyPlane.transform
            : null;
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
        float cameraToPlaneForwardOffset = TPSCameraPlaneDistance * Mathf.Cos(TPSCameraToPlaneAngle * Mathf.Deg2Rad);
        // 相机延飞机上方向的偏移 UNDONE 值不会经常变动，Cache下来不要每帧算
        float cameraToPlaneUpOffset = TPSCameraPlaneDistance * Mathf.Sin(TPSCameraToPlaneAngle * Mathf.Deg2Rad);

        #region TPS
        CameraProperties tpsCameraProperties_WorldSpace;
        {
            // 注视点
            tpsCameraProperties_WorldSpace.LookAt = m_MyPlaneTransform.position + m_MyPlaneTransform.forward * TPSCameraLookAtToPlaneForwardDistance;
            // 根据注视点推算出的相机坐标
            tpsCameraProperties_WorldSpace.Position = tpsCameraProperties_WorldSpace.LookAt
                    + m_MyPlaneTransform.TransformDirection(new Vector3(0
                , cameraToPlaneUpOffset
                , -cameraToPlaneForwardOffset - TPSCameraLookAtToPlaneForwardDistance));
            tpsCameraProperties_WorldSpace.UpDirection = Vector3.up;

            // 根据飞机的G力偏移相机, 给玩家传达飞行员承受的加速度
            Vector3 planeAcceleration_LocalSpace = m_MyPlane.GetAcceleration_LocalSpace();

            Vector3 offsetByGForce = m_MyPlaneTransform.TransformDirection(new Vector3(hwmUtility.ClampAbs(planeAcceleration_LocalSpace.x * TPSCameraOffsetMultiplyByGForce.x, TPSCameraOffsetMaxByGForce.x)
                , hwmUtility.ClampAbs(planeAcceleration_LocalSpace.y * TPSCameraOffsetMultiplyByGForce.y, TPSCameraOffsetMaxByGForce.y)
                , hwmUtility.ClampAbs(-planeAcceleration_LocalSpace.z * TPSCameraOffsetMultiplyByGForce.z, TPSCameraOffsetMaxByGForce.z)));

            // TODO 根据负载程度抖动相机? 电影和动画中经常这么做

            // 计算Offset
            m_TPSCameraOffset = Vector3.Lerp(m_TPSCameraOffset, offsetByGForce, Mathf.Min(deltaTime * TPSCameraOffsetLerpSpeed, 1.0f));
            tpsCameraProperties_WorldSpace.Position += m_TPSCameraOffset;
        }
        #endregion

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
}