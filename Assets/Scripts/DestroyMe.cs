using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DestroyMe : MonoBehaviourPun
{
    public void DestroyRPC()
    {
        photonView.RPC("DestroyMyself", RpcTarget.All);
    }

    [PunRPC]
    public void DestroyMyself()
    {
        Destroy(gameObject);
    }
}
