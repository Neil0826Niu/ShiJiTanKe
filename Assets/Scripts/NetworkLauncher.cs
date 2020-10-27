using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Linq;

public class NetworkLauncher : MonoBehaviourPunCallbacks
{
    public GameObject roomListManager;
    public GameObject loginUI;
    public GameObject mainUI;
    public GameObject roomJoinUI;
    public GameObject roomCreateUI;
    public GameObject connectText;
    public GameObject startText;
    public GameObject roomUI;
    public GameObject alert;
    public GameObject alert2;
    public InputField playerName;
    public Text roomName;
    private string selectRoomName;
    public Text selectRoom;

    private GameObject self;

    public GameObject readyButton;
    public GameObject readyCancelButton;

    public GameObject playerInfoPrefab;
    public Transform gridLayout;

    public float startTime = 10f;
    private float time;
    private bool isStart;
    // Start is called before the first frame update
    void Start()
    {
        isStart = false;
        time = startTime;
        if (PhotonNetwork.IsConnected)
            return;
        PhotonNetwork.ConnectUsingSettings();
        connectText.SetActive(true);
    }
    private void Update()
    {
        if (PhotonNetwork.PlayerList.Length < 2)
        {
            alert2.SetActive(true);
        }
        if (PhotonNetwork.PlayerList.Length >= 2)
        {

            bool allReady = true;
            for (int i = 0; i < gridLayout.childCount; i++)
            {
                if (!gridLayout.GetChild(i).GetComponent<PlayerReady>().isReady)
                    allReady = false;
            }
            if (allReady)
            {
                alert2.SetActive(false);
                startText.SetActive(true);
                startText.transform.GetChild(0).GetComponent<Text>().text = $"所有玩家均已准备\n游戏将在{(int)time}s后开始";
                if (time > 0)
                {
                    time -= Time.deltaTime;
                }
                else
                {
                    if (!isStart)
                    {
                        isStart = true;
                        PhotonNetwork.LoadLevel(1);
                        PhotonNetwork.CurrentRoom.IsVisible = false;
                    }

                }
            }
            else
            {
                alert2.SetActive(true);
                startText.SetActive(false);
                time = startTime;
            }

        }

    }


    public override void OnConnectedToMaster()
    {
        if (PhotonNetwork.NickName != "")
        {
            connectText.SetActive(true);
            PhotonNetwork.JoinLobby();
            return;
        }
        connectText.SetActive(false);
        loginUI.SetActive(true);
    }

    public void LoginBtn()
    {
        if (playerName.text.Length < 2)
            return;
        PhotonNetwork.NickName = playerName.text;
        loginUI.SetActive(false);
        connectText.SetActive(true);
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        connectText.SetActive(false);
        mainUI.SetActive(true);

    }
    public void JoinLobbyButton()
    {
        mainUI.SetActive(false);
        roomJoinUI.SetActive(true);
    }
    public void EnterEditorButton()
    {
        mainUI.SetActive(false);
        PhotonNetwork.LoadLevel(1);
    }
    public void GotoCreateRoom()
    {
        roomJoinUI.SetActive(false);
        if (PhotonNetwork.InLobby)
            roomCreateUI.SetActive(true);
    }

    public void SelectRoom(string name)
    {
        selectRoomName = name;
        selectRoom.text = "选择的房间名：" + name;
    }

    public void JoinRoom()
    {
        if (selectRoom.text == "")
            return;

        roomJoinUI.SetActive(false);
        connectText.SetActive(true);
        PhotonNetwork.JoinRoom(selectRoomName);
    }
    public void CreateRoom()
    {
        var roomList = roomListManager.GetComponent<RoomListManager>().myRoomList;
        if (roomName.text.Length < 2 || roomList.Contains(roomName.text))
        {
            alert.SetActive(true);
            return;
        }
        alert.SetActive(false);
        roomCreateUI.SetActive(false);
        connectText.SetActive(true);
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 5 };
        PhotonNetwork.CreateRoom(roomName.text, roomOptions, default);
    }

    public void BacktoMainButton()
    {
        roomJoinUI.SetActive(false);
        mainUI.SetActive(true);
    }
    public void Back()
    {
        alert.SetActive(false);
        roomCreateUI.SetActive(false);
        roomJoinUI.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        alert2.SetActive(false);
        startText.SetActive(false);
        int count = 0;
        while (true)
        {
            if (PhotonNetwork.PlayerListOthers.Any(p => p.NickName == PhotonNetwork.NickName))
            {
                count++;
                int index = PhotonNetwork.NickName.LastIndexOf('(');
                if (index > 1)
                    PhotonNetwork.NickName = PhotonNetwork.NickName.Substring(0, index) + $"({count})";
                else
                    PhotonNetwork.NickName += $"({count})";
            }
            else
                break;
        }

        for (int i = 0; i < gridLayout.childCount; i++)
        {
            Destroy(gridLayout.GetChild(i).gameObject);
        }
        connectText.SetActive(false);
        roomUI.SetActive(true);

        self = PhotonNetwork.Instantiate("PlayerInfo", gridLayout.position, Quaternion.identity);
    }

    public void ExitRoom()
    {
        roomUI.SetActive(false);
        connectText.SetActive(true);
        PhotonNetwork.LeaveRoom();

    }


    public void ReadyButton()
    {
        self.GetComponent<PlayerReady>().Ready();
        readyButton.SetActive(false);
        readyCancelButton.SetActive(true);
    }
    public void ReadyCancelButton()
    {
        self.GetComponent<PlayerReady>().ReadyCancel();
        readyButton.SetActive(true);
        readyCancelButton.SetActive(false);
    }
    public void ExitBtn()
    {
        PhotonNetwork.Disconnect();
        Application.Quit();
    }
}
