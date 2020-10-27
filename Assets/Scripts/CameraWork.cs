using UnityEngine;


public class CameraWork : MonoBehaviour
{
    //观察目标
    private Transform cameraTransform;


    //旋转速度
    private const float RotationSpeedX = 240;

    private const float RotationSpeedY = 120;

    //角度限制
    private const float RotationMinLimitY = -60;

    private const float RotationMaxLimitY = 40;

    //旋转角度
    private float mX = 45.0F;
    private float mY = 45.0F;

    //观察距离
    private float distance = 10F;
    //鼠标缩放距离最值
    private const float MaxDistance = 30;

    private const float MinDistance = 5F;

    //鼠标缩放速率
    private const float ZoomSpeed = 5F;

    private Quaternion mRotation;

    private float offHigh;

    [HideInInspector]
    public bool canWork;
    // ReSharper disable once UnusedMember.Local
    private void Start()
    {
        canWork = true;
        cameraTransform = Camera.main.transform;
        ResetCamPos(0, 20);
        
        ResetHigh();
    }
    public void ResetHigh()
    {
        float targetHight = transform.position.y;
        offHigh = 0;
        foreach (Transform child in transform)
        {
            if ((child.position.y - targetHight) > offHigh)
                offHigh = child.position.y - targetHight;
        }
    }
    private void ResetCamPos(int x, int y)
    {
        mY = y;
        mX = x;
        //初始化旋转角度
        mRotation = Quaternion.Euler(mY, mX, 0);
        cameraTransform.rotation = mRotation;
    }

    // ReSharper disable once UnusedMember.Local
    private void Update()
    {
        if (canWork)
        {  //获取鼠标输入
            mX += Input.GetAxis("Mouse X") * RotationSpeedX * 0.02F;
            mY -= Input.GetAxis("Mouse Y") * RotationSpeedY * 0.02F;
            //范围限制
            mY = ClampAngle(mY, RotationMinLimitY, RotationMaxLimitY);

            //鼠标滚轮缩放
            distance -= Input.GetAxis("Mouse ScrollWheel") * ZoomSpeed;
            distance = Mathf.Clamp(distance, MinDistance, MaxDistance);
        }

        //计算旋转
        mRotation = Quaternion.Euler(mY, mX, 0);
        cameraTransform.rotation = mRotation;

        var tarPos = transform.position + new Vector3(0, offHigh + 2, 0);
        //重新计算位置
        var mPosition = mRotation * new Vector3(0.0F, 0.0F, -distance) + tarPos;
        //设置相机的角度和位置
        cameraTransform.position = mPosition;

    }


    //角度限制
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
