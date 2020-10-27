using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class GameUI : MonoBehaviourPun
{
    [HideInInspector]
    public GameObject player;

    public GameManager gameManager;
    public bool isDead;
    public bool isWin;
    public GameObject playerUI;
    public GameObject deathUI;
    public GameObject winUI;
    public GameObject EditUI;
    public GameObject escUI;
    public GameObject editor;
    public GameObject mainCamera;
    public GameObject inCircle;
    public GameObject _MGUI;

    public Text n_Cube2;
    public Text n_Wheel;
    public Text n_MG;
    public Text n_Aid;

    private PlayerInfo playerInfo;
    public Text MGCurrentAmmo;
    public Text MGPreAmmo;
    public GameObject MGReloadPro;
    public Image lifeSlider;
    public Image zhuangjiaSlider;
    public GameObject RebuildPro;
    public Transform killList;
    public GameObject killInfoPrefab;

    public Text lastNumber;
    public Text killNumber;
    public Text deathInfo;
    public Text deathTimeTxt;
    public Text winTimeTxt;
    public Text winInfo;
    public Terrain terrain;
    public RectTransform smallMap;
    public RectTransform smallMapPos;
    public RectTransform bigMap;
    public RectTransform bigMapPos;
    public Transform currentCircle1;
    public Transform preCircle1;
    public Transform currentCircle2;
    public Transform preCircle2;
    public Text circleInfo;
    private int kill;
    private int rank;
    private bool isEditing;
    private bool isOpenBigMap;
    private bool isOpenEsc;

    private int count = 2;
    public float timer;
    private bool isleave = false;
    [HideInInspector]
    public bool isInCircle;
    public bool hit;
    public GameObject hitUI;
    private float hitTimer;
    // Start is called before the first frame update
    void Start()
    {
        hit = false;
        timer = 10;
        isWin = false;
        isOpenEsc = false;
        isOpenBigMap = false;
        isEditing = false;
        Cursor.visible = false;//隐藏鼠标
        Cursor.lockState = CursorLockMode.Locked;//把鼠标锁定到屏幕中间
        player = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.current_Status == 4)
            circleInfo.text = "决赛圈！";
        else
            circleInfo.text = gameManager._CD - gameManager.current_CD > 0 ? (int)(gameManager._CD - gameManager.current_CD) + "秒后缩圈" : "正在缩圈";

      
        if (player != null && !isEditing)
        {
            DisplayPlayerUI();
        }
        if (isEditing && Input.GetKeyDown(KeyCode.Escape))
        {
            BackGameBtn();
            count = 1;
        }
        if (hit)
        {

            if (hitTimer < 0.3F)
            {
                hitTimer += Time.deltaTime;
                hitUI.SetActive(true);
            }
            else
            {
                hit = false;
                hitTimer = 0;
                hitUI.SetActive(false);
            }
        }

        if ( isWin)
        {
            timer -= Time.deltaTime;
            if (timer <= 0 && !isleave)
            {
                isleave = true;
                
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.LoadLevel(0);
            }
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            player = null;
            playerUI.SetActive(false);
            circleInfo.gameObject.SetActive(false);
            winUI.SetActive(true);
            escUI.SetActive(false);
            winInfo.text = $"您赢得了比赛！您是第1名\n\n击败了{kill}名玩家";
            winTimeTxt.text = (int)timer + "秒后退出战局";
        }
        if (isInCircle)
            inCircle.SetActive(true);
        else
            inCircle.SetActive(false);
        if (isDead)
        {
            timer -= Time.deltaTime;
            if (timer <= 0 && !isleave)
            {
                isleave = true;
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.LoadLevel(0);
            }
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            player = null;
            playerUI.SetActive(false);
            circleInfo.gameObject.SetActive(false);
            deathUI.SetActive(true);
            escUI.SetActive(false);
            deathInfo.text = $"真遗憾！您是第{rank}名\n\n击败了{kill}名玩家";
            deathTimeTxt.text = (int)timer + "秒后退出战局";
        }
        if (Input.GetKeyDown(KeyCode.H) && !playerInfo.isUsingAid && !playerInfo.isRebuilding && !playerInfo.isReloadingMG && !isEditing
            && !isOpenBigMap && !isOpenEsc && player.GetComponent<Rigidbody>().velocity.magnitude < 1)
        {
            isEditing = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            player.GetComponent<CarControl>().canMove = false;
            player.GetComponent<CameraWork>().enabled = false;
            player.GetComponent<Crosshairs>().enabled = false;
            foreach (var mg in player.GetComponentsInChildren<MachineGun>())
            {
                mg.enabled = false;
            }
            mainCamera.SetActive(false);
            editor.SetActive(true);
            playerUI.SetActive(false);
            EditUI.SetActive(true);
            count = 0;
        }
        if (player == null)
            return;

        if (Input.GetKeyDown(KeyCode.Escape) && !isEditing)
        {
            if (count == 2)
                if (!isOpenEsc)
                {
                    isOpenEsc = true;
                    escUI.SetActive(true);
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    player.GetComponent<CarControl>().canMove = false;
                    player.GetComponent<Crosshairs>().enabled = false;
                    player.GetComponent<CameraWork>().canWork = false;
                    foreach (var mg in player.GetComponentsInChildren<MachineGun>())
                    {
                        mg.enabled = false;
                    }
                }
                else
                {
                    isOpenEsc = false;
                    escUI.SetActive(false);
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    player.GetComponent<CarControl>().canMove = true;
                    player.GetComponent<Crosshairs>().enabled = true;
                    player.GetComponent<CameraWork>().canWork = true;
                    foreach (var mg in player.GetComponentsInChildren<MachineGun>())
                    {
                        mg.enabled = true;
                    }
                    count = 2;
                }
            else
                count++;
        }
    }

    public void QuitBtn()
    {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(0);
    }

    public void ExitBtn()
    {
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
        if (PhotonNetwork.InLobby)
            PhotonNetwork.LeaveLobby();
        Application.Quit();
    }

    public void NewKillInfo(string killer, string deadPlayer)
    {
        GameObject killInfo = Instantiate(killInfoPrefab, killList.position, Quaternion.identity);
        killInfo.transform.SetParent(killList);
        killInfo.GetComponent<Text>().text = killer + " 击败 " + deadPlayer;
        Destroy(killInfo, 3f);

    }

    public void DisplayPlayerUI()
    {

        playerInfo = player.GetComponent<PlayerInfo>();

        playerUI.SetActive(true);
        if (player.GetComponentInChildren<MachineGun>())
        {
            _MGUI.SetActive(true);

            MGCurrentAmmo.text = "" + playerInfo.currentMGBullet;
            MGPreAmmo.text = "" + playerInfo.MGBullet;

            if (playerInfo.isReloadingMG)
            {
                MGReloadPro.SetActive(true);
                MGReloadPro.transform.GetChild(0).GetComponent<Image>().fillAmount -= Time.deltaTime / playerInfo.reloadMGCD;
            }
            else
            {
                MGReloadPro.transform.GetChild(0).GetComponent<Image>().fillAmount = 1;
                MGReloadPro.SetActive(false);
            }
            if (playerInfo.isRebuilding || playerInfo.isUsingAid)
            {
                float cd = playerInfo.isRebuilding ? playerInfo.rebuildCD : playerInfo.aidCD;
                RebuildPro.SetActive(true);
                RebuildPro.transform.GetChild(0).GetComponent<Image>().fillAmount -= Time.deltaTime / cd;
            }
            else
            {
                RebuildPro.transform.GetChild(0).GetComponent<Image>().fillAmount = 1;
                RebuildPro.SetActive(false);
            }

        }
        else
            _MGUI.SetActive(false);
        lastNumber.text = "" + gameManager.livePlayer.Length;
        rank = gameManager.livePlayer.Length;
        killNumber.text = "" + playerInfo.killNumber;
        kill = playerInfo.killNumber;
        float life = playerInfo.life > 100 ? 100f : playerInfo.life;
        lifeSlider.fillAmount = life / 100f;
        float zhuangjia = playerInfo.life > 100 ? playerInfo.life - 100 : 0;
        zhuangjiaSlider.fillAmount = zhuangjia / 100f;

        n_Cube2.text = playerInfo.n_Cube2.ToString();
        n_Wheel.text = playerInfo.n_Wheel.ToString();
        n_MG.text = playerInfo.n_MachineGun.ToString();
        n_Aid.text = playerInfo.n_Aid.ToString();

        float smallPosX = player.transform.position.x / terrain.terrainData.size.x * smallMap.rect.width;
        float smallPosZ = player.transform.position.z / terrain.terrainData.size.z * smallMap.rect.height;
        Vector3 smallPosition = new Vector3(-smallPosX, -smallPosZ, 0);
        smallMap.localPosition = smallPosition;
        smallMapPos.rotation = Quaternion.Euler(0, 0, -player.transform.eulerAngles.y);

        float currentCircleX = gameManager.circle.transform.position.x / terrain.terrainData.size.x * bigMap.rect.width;
        float currentCircleZ = gameManager.circle.transform.position.z / terrain.terrainData.size.z * bigMap.rect.height;
        float currentSizeX = gameManager.circle.GetComponent<MeshFilter>().mesh.bounds.size.x * gameManager.circle.localScale.x
            / terrain.terrainData.size.x * bigMap.rect.width;
        currentCircle2.localPosition = new Vector3(currentCircleX, currentCircleZ, 0);
        currentCircle2.GetComponent<RectTransform>().sizeDelta = new Vector2(currentSizeX, currentSizeX);
        float preCircleX = gameManager.circlePos.x / terrain.terrainData.size.x * bigMap.rect.width;
        float preCircleZ = gameManager.circlePos.z / terrain.terrainData.size.z * bigMap.rect.height;
        float preSizeX = gameManager.preCirclesize / terrain.terrainData.size.x * bigMap.rect.width;
        if (preSizeX <= 0)
            preCircle2.gameObject.SetActive(false);
        preCircle2.localPosition = new Vector3(preCircleX, preCircleZ, 0);
        preCircle2.GetComponent<RectTransform>().sizeDelta = new Vector2(preSizeX, preSizeX);

        if (isOpenBigMap)
        {
            float bigPosX = player.transform.position.x / terrain.terrainData.size.x * bigMap.rect.width;
            float bigPosZ = player.transform.position.z / terrain.terrainData.size.z * bigMap.rect.height;
            Vector3 bigPosition = new Vector3(bigPosX, bigPosZ, 0);
            bigMapPos.localPosition = bigPosition;
            bigMapPos.rotation = Quaternion.Euler(0, 0, -player.transform.eulerAngles.y);


            currentCircle1.localPosition = new Vector3(currentCircleX, currentCircleZ, 0);
            currentCircle1.GetComponent<RectTransform>().sizeDelta = new Vector2(currentSizeX, currentSizeX);
            preCircle1.localPosition = new Vector3(preCircleX, preCircleZ, 0);
            if (preSizeX <= 0)
                preCircle1.gameObject.SetActive(false);
            preCircle1.GetComponent<RectTransform>().sizeDelta = new Vector2(preSizeX, preSizeX);
        }
        if (Input.GetKeyDown(KeyCode.Escape) && isOpenBigMap)
        {
            bigMap.gameObject.SetActive(false);
            smallMap.parent.gameObject.SetActive(true);
            player.GetComponent<Crosshairs>().enabled = true;
            isOpenBigMap = false;
            count = 1;
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (!isOpenBigMap)
            {
                bigMap.gameObject.SetActive(true);
                smallMap.parent.gameObject.SetActive(false);
                player.GetComponent<Crosshairs>().enabled = false;
                isOpenBigMap = true;
                count = 0;
            }
            else
            {
                bigMap.gameObject.SetActive(false);
                smallMap.parent.gameObject.SetActive(true);
                player.GetComponent<Crosshairs>().enabled = true;
                isOpenBigMap = false;
                count = 2;
            }
        }
    }
    public void RebuildBtn()
    {

        playerInfo.isRebuilding = true;
        isEditing = false;
        gameManager.Rebuild();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        player.GetComponent<CarControl>().canMove = true;
        player.GetComponent<CameraWork>().enabled = true;
        player.GetComponent<Crosshairs>().enabled = true;
        foreach (var mg in player.GetComponentsInChildren<MachineGun>())
        {
            mg.enabled = true;
        }
        mainCamera.SetActive(true);
        editor.SetActive(false);
        playerUI.SetActive(true);
        EditUI.SetActive(false);
    }
    public void BackGameBtn()
    {
        isEditing = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        player.GetComponent<CarControl>().canMove = true;
        player.GetComponent<CameraWork>().enabled = true;
        player.GetComponent<Crosshairs>().enabled = true;
        foreach (var mg in player.GetComponentsInChildren<MachineGun>())
        {
            mg.enabled = true;
        }
        mainCamera.SetActive(true);
        editor.SetActive(false);
        playerUI.SetActive(true);
        EditUI.SetActive(false);
    }
}
