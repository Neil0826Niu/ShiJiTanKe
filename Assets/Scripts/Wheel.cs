using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Wheel : MonoBehaviourPun,IPunObservable
{
    public Vector3[] pos = new Vector3[2];
    public float life = 40;
    // ReSharper disable once UnusedMember.Local
    private void Start()
    {
        var position = transform.position;
        var size = transform.localScale;
        pos[0] = position + transform.right * size.x / 2;
        pos[1] = position - transform.right * size.x / 2;
    }
    private void Update()
    {
        if (life <= 0 )
            gameObject.GetComponent<DestroyMe>().DestroyRPC();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(life);
        }
        else
        {
            life = (float)stream.ReceiveNext();
        }
    }
}
