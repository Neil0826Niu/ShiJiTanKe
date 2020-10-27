using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerInfo : MonoBehaviourPunCallbacks, IPunObservable
{
    public string playerName;
    public float totallife = 100;
    public float life = 100;
    public int MGBullet = 20;
    public int UnitMGMagazine = 5;
    public int MGMagazine;
    public int currentMGBullet;
    public float reloadMGCD = 3;
    public float rebuildCD = 10;
    public bool isReloadingMG;
    public bool isRebuilding;
    public bool isUsingAid;
    public float aidCD = 3;
    public int killNumber;
    public GameUI gameUI;
    public GameManager gameManager;
    public string killerName;

    //[HideInInspector]
    public int n_Cube2;
    //[HideInInspector]
    public int n_Wheel;
    //[HideInInspector]
    public int n_MachineGun;
    //[HideInInspector]
    public int n_Aid;

    private float time;
    private bool flag = false;
    public bool isMoving = false;
    // Start is called before the first frame update
    public void Start()
    {
        time = 0;
        if (gameObject.GetComponent<PhotonView>())
            if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
            {
                gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
                gameUI = GameObject.Find("GameUI").GetComponent<GameUI>();
                if (photonView.IsMine)
                {
                    playerName = PhotonNetwork.NickName;
                    gameObject.name = playerName;
                    gameObject.GetComponent<Crosshairs>().enabled = true;
                    gameObject.layer = 8;
                    foreach (Transform transform in transform.GetComponentsInChildren<Transform>())
                        transform.gameObject.layer = 8;
                }
                else
                {
                    playerName = photonView.Owner.NickName;
                    gameObject.name = playerName;
                    gameObject.GetComponent<Crosshairs>().enabled = false;
                    gameObject.layer = 9;
                    foreach (Transform transform in transform.GetComponentsInChildren<Transform>())
                        transform.gameObject.layer = 9;
                }

            }
        if (gameObject.GetComponent<PhotonView>() && !photonView.IsMine)
            return;
        totallife = 100;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).tag == "Block")
            {
                totallife += 5;

            }
        }
        if (totallife > 200)
            totallife = 200;
        n_Cube2 = 0;
        n_Wheel = 0;
        n_MachineGun = 0;
        n_Aid = 0;
        isUsingAid = false;
        isReloadingMG = false;
        isRebuilding = false;
        killNumber = 0;
        int MG = gameObject.GetComponentsInChildren<MachineGun>().Length;
        MGMagazine = UnitMGMagazine * MG;
        life = totallife;
        currentMGBullet = MGMagazine;
    }

    private void Update()
    {

        int MG = gameObject.GetComponentsInChildren<MachineGun>().Length;
        MGMagazine = UnitMGMagazine * MG;
        if (currentMGBullet > MGMagazine)
            currentMGBullet = MGMagazine;
        if (isMoving)
        {
            if (!gameObject.GetComponent<AudioSource>().isPlaying)
                gameObject.GetComponent<AudioSource>().Play();
        }
        else
        {
            if (gameObject.GetComponent<AudioSource>().isPlaying)
                gameObject.GetComponent<AudioSource>().Stop();
        }

        if (life <= 0 && gameManager.livePlayer.Length > 1)
        {
            if (flag)
                return;
            if (!gameObject.GetComponent<PhotonView>())
                return;
            flag = true;
            //if (killerName != "电圈")
            //{
            //    Debug.Log("addkill" );
            //    gameManager.AddKilled(killerName); }
            //Debug.Log(killerName);
            //gameUI.NewKillInfo(killerName, playerName);
            if (photonView.IsMine)
                photonView.RPC("DeathInfo", RpcTarget.All);
            gameManager.LivePLayerRemove(playerName);
            if (photonView.IsMine)
            {
                gameUI.isDead = true;
                gameManager.enabled = false;
                PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
            }
        }
        if (!gameObject.GetComponent<PhotonView>())
            return;

        float x = transform.position.x - gameManager.circle.position.x;
        float z = transform.position.z - gameManager.circle.position.z;
        if (photonView.IsMine && gameManager.livePlayer.Length > 1 && Mathf.Sqrt(x * x + z * z)
                > gameManager.circle.GetComponent<MeshFilter>().mesh.bounds.size.x * gameManager.circle.localScale.x / 2)
        {
            gameUI.isInCircle = true;
            time += Time.deltaTime;
            if (time >= 1)
            {
                time = 0;
                killerName = "电圈";
                switch (gameManager.current_Status)
                {
                    case 0:
                        life -= 1;
                        break;
                    case 1:
                        life -= 2;
                        break;
                    case 2:
                        life -= 3;
                        break;
                    case 3:
                        life -= 4;
                        break;
                    case 4:
                        life -= 8;
                        break;
                    default:
                        break;
                }
            }
        }
        else
            gameUI.isInCircle = false;
    }
    [PunRPC]
    public void DeathInfo()
    {
        gameManager.AddKilled(killerName);
        gameUI.NewKillInfo(killerName, playerName);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(playerName);
            stream.SendNext(killerName);
            stream.SendNext(life);
            stream.SendNext(killNumber);
            stream.SendNext(isMoving);
        }
        else
        {
            playerName = (string)stream.ReceiveNext();
            killerName = (string)stream.ReceiveNext();
            life = (float)stream.ReceiveNext();
            killNumber = (int)stream.ReceiveNext();
            isMoving = (bool)stream.ReceiveNext();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (gameObject.GetComponent<PhotonView>() && !photonView.IsMine)
            return;
        if (other.tag == "Equipment")
        {
            if (other.name.Contains("eCube2"))
                n_Cube2 += 5;
            if (other.name.Contains("eMachineGun"))
                n_MachineGun++;
            if (other.name.Contains("eWheel"))
                n_Wheel += 2;
            if (other.name.Contains("eAid"))
                n_Aid++;
            if (other.name.Contains("eMGBullet"))
                MGBullet += 5;
            other.gameObject.GetComponent<DestroyMe>().DestroyRPC();
        }
    }
}
