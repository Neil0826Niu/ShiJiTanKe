using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerReady : MonoBehaviourPunCallbacks, IPunObservable
{
    public bool isReady;
    
    public void Ready()
    {
            isReady = true;
    }
    public void ReadyCancel()
    {
            isReady = false;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (stream.IsWriting)
        {
            stream.SendNext(isReady);
        }
        else
        {
            isReady = (bool)stream.ReceiveNext();
        }
    }
}
