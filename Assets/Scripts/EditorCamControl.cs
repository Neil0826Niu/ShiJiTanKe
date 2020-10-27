using UnityEngine;


public class EditorCamControl : MonoBehaviour
{
    //观察目标
    public Transform target;

    public GameObject targetGameObject;

    //移动速度
    private const float MoveSpeed = 5;

    //旋转速度
    private const float RotationSpeedX = 240;

    private const float RotationSpeedY = 120;

    //角度限制
    private const float RotationMinLimitY = -60;

    private const float RotationMaxLimitY = 40;

    //旋转角度
    private float moveH;
    private float moveV;
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

    private const float Roll = 30f * Mathf.PI * 2 / 360;

    private static float _rot;
    private static float _rot2;
    private const float RotSpeed = 0.05f;

    public bool reset = false;
    public bool ishighest = false;
    public float offHigh;

    private static void Rotate()
    {
        var w = Input.GetAxis("Mouse X") * RotSpeed;
        _rot += w;
        var h = Input.GetAxis("Mouse Y") * RotSpeed;
        //_rot2 += h;
    }

    // ReSharper disable once UnusedMember.Local
    private void Start()
    {
        ResetCamPos(45, 45);
    }
    private void ResetCamPos(int x, int y)
    {
        mY = y;
        mX = x;
        //初始化旋转角度
        mRotation = Quaternion.Euler(mY, mX, 0);
        transform.rotation = mRotation;
        target.rotation = Quaternion.Euler(0, mX, 0);
        transform.position = mRotation * new Vector3(0.0F, 0.0F, -distance) + target.position;
    }

    // ReSharper disable once UnusedMember.Local
    private void Update()
    {
        var isEditing = GameObject.Find("EditorManage").GetComponent<EditorManage>().isEditing;
        var isDeleting = GameObject.Find("EditorManage").GetComponent<EditorManage>().isDeleting;
        if (isDeleting == false && isEditing == false)
        {
            float targetHight = targetGameObject.transform.position.y;
            if (!ishighest)
            {
                offHigh = 0;
                foreach(Transform child in targetGameObject.transform)
                {
                    if ((child.position.y - targetHight) > offHigh)
                        offHigh = child.position.y - targetHight;
                }
                ishighest = true;
            }
            var d = distance * Mathf.Cos(Roll);
            if (reset)
            {
                ResetCamPos(0, 20);
                reset = false;
            }
            //获取鼠标输入
            mX += Input.GetAxis("Mouse X") * RotationSpeedX * 0.02F;
            mY -= Input.GetAxis("Mouse Y") * RotationSpeedY * 0.02F;
            //范围限制
            mY = ClampAngle(mY, RotationMinLimitY, RotationMaxLimitY);
            //计算旋转
            mRotation = Quaternion.Euler(mY, mX, 0);
            transform.rotation = mRotation;

            //鼠标滚轮缩放
            distance -= Input.GetAxis("Mouse ScrollWheel") * ZoomSpeed;
            distance = Mathf.Clamp(distance, MinDistance, MaxDistance);

            var tarPos = targetGameObject.transform.position + new Vector3(0, offHigh + 2, 0);
            //重新计算位置
            var mPosition = mRotation * new Vector3(0.0F, 0.0F, -distance) + tarPos;
            //设置相机的角度和位置
            transform.position = mPosition;
        }
        else
        {
            if (reset)
            {
                ResetCamPos(45, 45);
                reset = false;
            }
            //观察对象移动
            moveH = Input.GetAxis("Horizontal") * MoveSpeed;
            moveV = Input.GetAxis("Vertical") * MoveSpeed;
            target.transform.position += target.transform.forward * moveV * Time.deltaTime;
            target.transform.position += target.transform.right * moveH * Time.deltaTime;

            //鼠标右键旋转
            if (target != null && Input.GetMouseButton(1))
            {
                //获取鼠标输入
                mX += Input.GetAxis("Mouse X") * RotationSpeedX * 0.02F;
                mY -= Input.GetAxis("Mouse Y") * RotationSpeedY * 0.02F;
                //范围限制
                mY = ClampAngle(mY, -90, 90);
                //计算旋转
                target.rotation = Quaternion.Euler(0, mX, 0); //观察对象转动
                mRotation = Quaternion.Euler(mY, mX, 0);
                transform.rotation = mRotation;
            }

            //鼠标中键
            if (target != null && Input.GetMouseButton(2))
            {
                moveH = Input.GetAxis("Mouse X") * MoveSpeed * 0.08f;
                moveV = Input.GetAxis("Mouse Y") * MoveSpeed * 0.08f;
                target.transform.position -= target.transform.up * moveV;
                target.transform.position -= target.transform.right * moveH;
            }

            //鼠标滚轮缩放
            distance -= Input.GetAxis("Mouse ScrollWheel") * ZoomSpeed;
            distance = Mathf.Clamp(distance, MinDistance, MaxDistance);

            //重新计算位置
            var mPosition = mRotation * new Vector3(0.0F, 0.0F, -distance) + target.position;
            //设置相机的角度和位置
            transform.position = mPosition;
        }
    }


    //角度限制
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
