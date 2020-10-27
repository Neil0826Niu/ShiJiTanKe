using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public Transform initPoints;
    public Transform editCore;

    [HideInInspector]
    public Transform player;

    public Transform circle;
    [HideInInspector]
    public bool isUpdateCircle;
    public float circle_size = 140F;
    [HideInInspector]
    public Vector3 circlePos;
    [HideInInspector]
    public float preCirclesize;
    [HideInInspector]
    public float distance;

    public float circle_CD0 = 120f;
    public float circle_Time0 = 60f;
    public float circle_CD1 = 100f;
    public float circle_Time1 = 40f;
    public float circle_CD2 = 80f;
    public float circle_Time2 = 30f;
    public float circle_CD3 = 60f;
    public float circle_Time3 = 20f;

    [HideInInspector]
    public float current_CD;
    [HideInInspector]
    public float current_Time;
    [HideInInspector]
    public float current_Status;
    [HideInInspector]
    public float _Time;
    [HideInInspector]
    public float _CD;

    [HideInInspector]
    public int initCount;
    [HideInInspector]
    public bool isInited = false;
    public InitManager initManager;
    public GameUI gameUI;
    [HideInInspector]
    public string[] livePlayer;

    public float readytimer = 2f;
    private List<GameObject> item;
    // Update is called once per frame
    void Start()
    {
        item = new List<GameObject>();
        _Time = 0;
        _CD = 0;
        isUpdateCircle = false;
        current_CD = 0;
        current_Time = 0;
        current_Status = 0;
        livePlayer = new string[PhotonNetwork.PlayerList.Length];
        List<string> livePlayerList = new List<string>();
        foreach (var player in PhotonNetwork.PlayerList)
        {
            livePlayerList.Add(player.NickName);
        }
        livePlayer = livePlayerList.ToArray();
        if (!PhotonNetwork.IsConnected)
            return;
        PhotonNetwork.SendRate = 120;
        PhotonNetwork.SerializationRate = 120;
    }

    private void Update()
    {
        //if (readytimer > 0)
        //{
        //    Debug.Log(readytimer);
        //    readytimer -= Time.deltaTime;
        //    foreach (Player p in PhotonNetwork.PlayerList) {
        //        if (GameObject.Find(p.NickName))
        //        {
        //            item.Add(GameObject.Find(p.NickName));
        //            GameObject.Find(p.NickName).SetActive(false);
        //        }
        //    }
        //}
        //else
        //{
        //    foreach (GameObject p in item)
        //    {
        //        p.SetActive(true);
        //    }
        //}
        if (initManager.isReady && !isInited)
        {
            isInited = true;
            foreach (var point in initManager.playerPoints)
            {
                if (point.Split(':')[0] == PhotonNetwork.NickName)
                {
                    initCount = int.Parse(point.Split(':')[1]);
                    break;
                }
            }
                InitPlayer();
        }
        //刷圈
        CircleManager();

        //胜利
        if (livePlayer.Length == 1 && player.GetComponent<PlayerInfo>().life > 0)
        {
            gameUI.isWin = true;
            player.GetComponent<CarControl>().canMove = false;
            player.GetComponent<Crosshairs>().enabled = false;
            foreach (var mg in player.GetComponentsInChildren<MachineGun>())
            {
                mg.enabled = false;
            }
        }


    }
    public void CircleManager()
    {
        if (!isUpdateCircle)
        {
            isUpdateCircle = true;
            var a = circle.GetComponent<MeshFilter>().mesh.bounds.size.x * circle.localScale.x;
            float x = Random.Range(-a * 0.11f, a * 0.11f);
            float z = Random.Range(-a * 0.11f, a * 0.11f);
            Vector3 offPos = new Vector3(x, 0, z);
            circlePos = circle.position + offPos;
            distance = offPos.magnitude;
            preCirclesize = circle.GetComponent<MeshFilter>().mesh.bounds.size.x * circle.localScale.x - circle_size;

        }
        current_CD += Time.deltaTime;

        switch (current_Status)
        {
            case 0:
                _CD = circle_CD0;
                _Time = circle_Time0;
                break;
            case 1:
                _CD = circle_CD1;
                _Time = circle_Time1;
                break;
            case 2:
                _CD = circle_CD2;
                _Time = circle_Time2;
                break;
            case 3:
                _CD = circle_CD3;
                _Time = circle_Time3;
                break;
        }
        if (current_CD >= _CD && current_Status < 4)
        {
            current_Time += Time.deltaTime;
            circle.position = Vector3.MoveTowards(circle.position, circlePos, distance / _Time * Time.deltaTime);
            float speed = circle_size / (circle.GetComponent<MeshFilter>().mesh.bounds.size.x) / _Time;
            if (circle.localScale.x >= speed * Time.deltaTime)
                circle.localScale -= new Vector3(speed * Time.deltaTime, 0, speed * Time.deltaTime);
            if (current_Time >= _Time)
            {
                current_Time = 0;
                current_CD = 0;
                current_Status++;
                isUpdateCircle = false;
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        List<string> livePlayerList = new List<string>(livePlayer);
        base.OnPlayerLeftRoom(otherPlayer);
        if (livePlayerList.Contains(otherPlayer.NickName))
            livePlayerList.Remove(otherPlayer.NickName);
        livePlayer = livePlayerList.ToArray();
    }

    public void LivePLayerRemove(string deadPlayName)
    {
        List<string> livePlayerList = new List<string>(livePlayer);
        if (livePlayerList.Contains(deadPlayName))
            livePlayerList.Remove(deadPlayName);
        livePlayer = livePlayerList.ToArray();
    }
    public void AddKilled(string killerName)
    {
        if (killerName == "电圈")
            return;

        Debug.Log(killerName);
        GameObject killer = GameObject.Find(killerName);
        killer.GetComponent<PlayerInfo>().killNumber++;
    }

    public void Rebuild()
    {
        StartCoroutine(RebuildPlayer());
    }

    IEnumerator RebuildPlayer()
    {
        PlayerInfo playerInfo = player.GetComponent<PlayerInfo>();
        float time = 0;
        while (playerInfo.isRebuilding)
        {
            if (time >= playerInfo.rebuildCD)
            {
                float oldLowestY = 0;
                foreach (Transform part in player)
                {
                    if (part.localPosition.y < oldLowestY)
                        oldLowestY = part.localPosition.y;
                    part.GetComponent<DestroyMe>().DestroyRPC();
                }
                int number = editCore.childCount;
                float newLowestY = 0;
                for (int i = 0; i < number; i++)
                {
                    string objName = editCore.GetChild(i).name.Split('(')[0].Split(' ')[0];
                    GameObject obj = PhotonNetwork.Instantiate(objName, Vector3.zero, Quaternion.identity);
                    obj.transform.parent = player;
                    obj.transform.localPosition = editCore.GetChild(i).localPosition;
                    if (obj.transform.localPosition.y < newLowestY)
                        newLowestY = obj.transform.localPosition.y;
                    obj.transform.localRotation = editCore.GetChild(i).localRotation;
                    obj.layer = 8;
                }

                playerInfo.totallife = 100;
                for (int i = 0; i < player.transform.childCount; i++)
                {
                    if (player.transform.GetChild(i).tag == "Block")
                        playerInfo.totallife += 5;
                }
                if (playerInfo.totallife > 200)
                    playerInfo.totallife = 200;
                playerInfo.life = playerInfo.totallife;
                player.transform.position += Vector3.up * (oldLowestY - newLowestY);
                player.transform.rotation = new Quaternion(0, player.transform.rotation.y, 0, player.transform.rotation.w);
                foreach (Transform transform in player.GetComponentsInChildren<Transform>())
                    transform.gameObject.layer = 8;
                player.gameObject.SetActive(false);
                player.gameObject.SetActive(true);
                playerInfo.isRebuilding = false;
                player.GetComponent<CameraWork>().ResetHigh();
            }
            yield return new WaitForSeconds(0.1f);
            time += 0.1f;
        }
        StopCoroutine(RebuildPlayer());
    }



    public void InitPlayer()
    {

        Vector3 positon = initPoints.GetChild(initCount).position;
        GameObject mine = PhotonNetwork.Instantiate("Player", positon, Quaternion.identity);
        player = mine.transform;
        gameUI.player = mine;

        string[] name = new string[player.childCount];
        Vector3[] pos = new Vector3[player.childCount];
        Quaternion[] rot = new Quaternion[player.childCount];

        for (int i = 0; i < player.childCount; i++)
        {
            name[i] = player.GetChild(i).name.Split('(')[0].Split(' ')[0];
            pos[i] = player.GetChild(i).localPosition;
            rot[i] = player.GetChild(i).localRotation;
            player.GetChild(i).GetComponent<DestroyMe>().DestroyRPC();
        }
        for (int i = 0; i < name.Length; i++)
        {

            GameObject obj = PhotonNetwork.Instantiate(name[i], Vector3.zero, Quaternion.identity);
            obj.transform.parent = player;
            obj.transform.localPosition = pos[i];
            obj.transform.localRotation = rot[i];
            obj.layer = 8;
        }
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(livePlayer);
            stream.SendNext(circlePos);
            stream.SendNext(current_Time);
            stream.SendNext(current_CD);
            stream.SendNext(current_Status);
        }
        else
        {
            livePlayer = (string[])stream.ReceiveNext();
            circlePos = (Vector3)stream.ReceiveNext();
            current_Time = (float)stream.ReceiveNext();
            current_CD = (float)stream.ReceiveNext();
            current_Status = (float)stream.ReceiveNext();
        }
    }
}
