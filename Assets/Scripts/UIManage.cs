using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

// ReSharper disable once InconsistentNaming
public class UIManage : MonoBehaviourPun
{

    public List<Toggle> partToggle = new List<Toggle>();
    public List<GameObject> partGroup = new List<GameObject>();
    public List<Toggle> partList = new List<Toggle>();
    public GameObject editorManage;
    public GameObject editorUi;
    public GameObject playerUI;
    public GameObject _MGUI;
    public GameObject cam;
    public GameObject[] selName;

    [HideInInspector]
    public PlayerInfo playerInfo;

    public Text text_Cube2;
    public Text text_MG;
    public Text text_RotWheel;
    public Text text_PowerWheel;

    public GameObject core;
    private PlayerInfo coreInfo;
    public Text MGCurrentAmmo;
    public Text MGPreAmmo;
    public GameObject MGReloadPro;

    public Button testBtn;
    public Button deleteBtn;
    public Button editBtn;
    // ReSharper disable once UnusedMember.Local
    private void Start()
    {
        if (PhotonNetwork.IsConnected)
            playerInfo = GameObject.Find(PhotonNetwork.NickName).GetComponent<PlayerInfo>();
        coreInfo = core.GetComponent<PlayerInfo>();
        for (var a = 0; a < partGroup[0].transform.childCount - 2; a++)
        {
            partList.Add(partGroup[0].transform.GetChild(a).GetComponent<Toggle>());
        }
    }

    // ReSharper disable once UnusedMember.Local
    private void Update()
    {
        foreach (var t in partList.Where(t => t.isOn))
        {
            deleteBtn.interactable = true;
            editorManage.GetComponent<EditorManage>().isEditing = true;
            editorManage.GetComponent<EditorManage>().isDeleting = false;
            editorManage.GetComponent<EditorManage>().selectBlockName = t.gameObject.name;
            foreach (var i in selName)
            {
                i.GetComponent<Text>().text = t.transform.GetChild(0).name;
            }
        }
        if (!partList.Any(t => t.isOn))
        {
          
            foreach (var i in selName)
            {
                i.GetComponent<Text>().text = "";
            }

        }

        MGCurrentAmmo.text = "" + coreInfo.currentMGBullet;
        MGPreAmmo.text = "" + coreInfo.MGBullet;

        if (coreInfo.isReloadingMG && !editorManage.GetComponent<EditorManage>().isEditing)
        {
            MGReloadPro.SetActive(true);
            MGReloadPro.transform.GetChild(0).GetComponent<Image>().fillAmount -= Time.deltaTime / coreInfo.reloadMGCD;
        }
        else
        {
            MGReloadPro.transform.GetChild(0).GetComponent<Image>().fillAmount = 1;
            MGReloadPro.SetActive(false);
        }

        text_Cube2.text = playerInfo.n_Cube2.ToString();
        text_MG.text = playerInfo.n_MachineGun.ToString();
        text_RotWheel.text = playerInfo.n_Wheel.ToString();
        text_PowerWheel.text = playerInfo.n_Wheel.ToString();
    }


    public void UpdateToggle()
    {
        partList.Clear();
        for (var i = 0; i < partToggle.Count; i++)
        {
            if (partToggle[i].isOn)
            {
                partGroup[i].SetActive(true);

                for (var a = 0; a < partGroup[i].transform.childCount - 2; a++)
                {
                    partList.Add(partGroup[i].transform.GetChild(a).GetComponent<Toggle>());
                }
            }
            else
                partGroup[i].SetActive(false);
        }

    }


    public void StartTestBtn()
    {
        testBtn.interactable = false;
        deleteBtn.interactable = false;
        editBtn.interactable = true;
        coreInfo.Start();
        editorManage.GetComponent<EditorManage>().isEditing = false;
        editorManage.GetComponent<EditorManage>().isDeleting = false;
        cam.GetComponent<EditorCamControl>().reset = true;
        editorUi.SetActive(false);
        playerUI.SetActive(true);
        if (core.GetComponentInChildren<MachineGun>())
            _MGUI.SetActive(true);
        else
            _MGUI.SetActive(false);
        foreach (var t in partList.Where(t => t.isOn))
        {
            t.isOn = false;
        }
    }
    public void ResetPositionBtn()
    {
        testBtn.interactable = true;
        deleteBtn.interactable = true;
        editBtn.interactable = false;
        if (coreInfo.isReloadingMG)
        {
            coreInfo.isReloadingMG = false;
        }

        editorManage.GetComponent<EditorManage>().isEditing = true;
        editorManage.GetComponent<EditorManage>().isDeleting = false;
        cam.GetComponent<EditorCamControl>().reset = true;
        editorUi.SetActive(true);
        playerUI.SetActive(false);
    }
    public void DeleteBtn()
    {
        testBtn.interactable = true;
        deleteBtn.interactable = false;
        editBtn.interactable = true;
        editorManage.GetComponent<EditorManage>().isEditing = true;
        editorManage.GetComponent<EditorManage>().isDeleting = true;
        foreach (var t in partList.Where(t => t.isOn))
        {
            t.isOn = false;
        }
    }
}

