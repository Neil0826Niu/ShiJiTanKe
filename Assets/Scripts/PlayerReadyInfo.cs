using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
public class PlayerReadyInfo : MonoBehaviourPun
{
    public Transform gridLayout;
    private void Start()
    {
        gridLayout = GameObject.Find("Content").transform;
        transform.SetParent(gridLayout);
    }
    // Update is called once per frame
    void Update()
    {

        if (photonView.IsMine)
        {
            transform.GetChild(0).GetComponent<Text>().text = PhotonNetwork.NickName;
            transform.GetChild(1).GetComponent<Text>().text = "1";
        }
        else
            
        {
            transform.GetChild(1).GetComponent<Text>().text = " 2";
            transform.GetChild(0).GetComponent<Text>().text = photonView.Owner.NickName;
        }

        bool isReady = photonView.gameObject.GetComponent<PlayerReady>().isReady;
        if (isReady)
            transform.GetChild(1).GetComponent<Text>().text = "已准备";
        else
            transform.GetChild(1).GetComponent<Text>().text = "未准备";



    }
}
