using UnityEngine;

public class pdCameraProperties : ScriptableObject
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
    /// 用户输入的杆量导致的相机旋转的角加速度
    /// </summary>
    public float TPSCameraAngularAccelerationByInput = 45.0f;
    /// <summary>
    /// 用户输入的杆量导致的相机旋转恢复到0的角加速度
    /// </summary>
    public float TPSCameraAngularByInputToZeroAcceleration = 60.0f;
    /// <summary>
    /// 超过这个时间用户没有输入杆量, 相机旋转开始恢复到0度
    /// </summary>
    public float TPSCameraAngularByInputToZeroWaitTime = 0.6f;
}