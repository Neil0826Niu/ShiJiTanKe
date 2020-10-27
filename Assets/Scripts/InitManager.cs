using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class InitManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public Transform equip;
    public Transform equitPoints;
    public Transform points;
    public string[] playerPoints;
    public bool isReady=false;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(playerPoints);
            stream.SendNext(isReady);
        }
        else
        {
            playerPoints = (string[])stream.ReceiveNext();
            isReady = (bool)stream.ReceiveNext();
        }
    }

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        playerPoints = new string[points.childCount];
        List<string> playerPointsList = new List<string>();
        List<int> initedPoints = new List<int>();
        int count;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            while (true)
            {
                count = Random.Range(0, points.childCount);
                if (!initedPoints.Contains(count))
                {
                    initedPoints.Add(count);
                    playerPointsList.Add(player.NickName + ":" + count);
                    break;
                }
            }

        }
        foreach(Transform point in equitPoints)
        {
            int i = Random.Range(0, 101);
            
            if (i > 60)
                continue;
            if (i <= 20)
                PhotonNetwork.InstantiateSceneObject("eCube2", point.position, Quaternion.identity).transform.SetParent(equip);
            else if(i<=30)
                PhotonNetwork.InstantiateSceneObject("eWheel", point.position, Quaternion.identity).transform.SetParent(equip);
            else if (i <= 37.5f)
                PhotonNetwork.InstantiateSceneObject("eMachineGun", point.position, Quaternion.identity).transform.SetParent(equip);
            else if (i <= 45)
                PhotonNetwork.InstantiateSceneObject("eAid", point.position, Quaternion.identity).transform.SetParent(equip);
            else if (i <= 60)
                PhotonNetwork.InstantiateSceneObject("eMGBullet", point.position, Quaternion.identity).transform.SetParent(equip);

        }
        playerPoints = playerPointsList.ToArray();
        isReady = true;
    }
}
