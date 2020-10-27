using UnityEngine;


public class Block : MonoBehaviour
{
    public Vector3[] pos = new Vector3[6];
    // ReSharper disable once UnusedMember.Local
    private void Start()
    {
        var position = transform.position;
        var size = transform.localScale;
        pos[0] = position + transform.forward * size.x / 2;//前
        pos[1] = position - transform.forward * size.x / 2;//后
        pos[2] = position + transform.up * size.y / 2;    //上
        pos[3] = position - transform.up * size.y / 2;    //下
        pos[4] = position + transform.right * size.z / 2;//右
        pos[5] = position - transform.right * size.z / 2;//左
    }

}

