using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
public class RoomListManager : MonoBehaviourPunCallbacks
{
    public GameObject roomNamePrefab;
    public Transform gridLayout;
    public GameObject launcher;
    public List<string> myRoomList;
    private void Start()
    {
        myRoomList = new List<string>();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        for(int roomIndex=0; roomIndex < roomList.Count;roomIndex++)
        {
            for(int i = 0; i < gridLayout.childCount; i++)
            {
                if (gridLayout.GetChild(i).transform.GetChild(0).GetComponent<Text>().text == roomList[roomIndex].Name)
                {
                    Destroy(gridLayout.GetChild(i).gameObject);
                }
            }
            if (roomList[roomIndex].PlayerCount == 0)
            {
                roomList.Remove(roomList[roomIndex]);
                continue;
            }
            GameObject newRoom = Instantiate(roomNamePrefab, gridLayout.position, Quaternion.identity);
            myRoomList.Add(roomList[roomIndex].Name);

            newRoom.transform.GetChild(0).GetComponent<Text>().text = roomList[roomIndex].Name;
            newRoom.transform.GetChild(1).GetComponent<Text>().text = "("+roomList[roomIndex].PlayerCount+ ")";
            newRoom.transform.SetParent(gridLayout);
            newRoom.GetComponent<Button>().onClick.AddListener(delegate () {
                launcher.GetComponent<NetworkLauncher>().SelectRoom(newRoom.transform.GetChild(0).GetComponent<Text>().text);
            });
        }
    }
}
