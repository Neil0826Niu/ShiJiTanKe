using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class MachineGun : MonoBehaviourPun, IPunObservable
{
    private GameObject player;
    private CarControl carControl;
    public GameObject bulletPrefab;
    public Transform bulletPos;
    public float CD = 2f;
    public float timer;
    public float life = 30;
    private LayerMask mask;
    public bool isfire=false;
    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.GetComponent<PhotonView>())
            if (PhotonNetwork.IsConnected && !photonView.IsMine)
            return;
        mask = ~(1 << 8);
        player = transform.parent.gameObject;
        carControl = player.GetComponent<CarControl>();
        timer = CD;
    }

    // Update is called once per frame
    void Update()
    {
        if (isfire)
        {
            gameObject.GetComponent<AudioSource>().Play();
        }
        if (life <= 0)
            gameObject.GetComponent<DestroyMe>().DestroyRPC();
        if (gameObject.GetComponent<PhotonView>())
            if (PhotonNetwork.IsConnected && !photonView.IsMine)
                return;
        timer += Time.deltaTime;
        Ray ray;
        if (Camera.main)
            ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        else
            ray = GameObject.Find("Camera").GetComponent<Camera>().ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        
        if (Physics.Raycast(ray, out var hit, 2000, mask))
        {
            //transform.GetChild(0).rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(hit.point - transform.position), 15 * Time.deltaTime);
            Vector3 rot = Quaternion.LookRotation(hit.point - transform.position).eulerAngles;
            Quaternion vrot = Quaternion.Euler(0, rot.y, 0);
            Quaternion hrot = Quaternion.Euler(0, rot.y+90, rot.x);
            transform.rotation = Quaternion.Slerp(transform.rotation, vrot, 20 * Time.deltaTime);
            transform.localRotation = Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0);
            transform.GetChild(0).GetChild(0).rotation = Quaternion.Slerp(transform.GetChild(0).GetChild(0).rotation, hrot, 1 * Time.deltaTime);
            transform.GetChild(0).GetChild(0).localRotation = Quaternion.Euler(0, 0, transform.GetChild(0).GetChild(0).localRotation.eulerAngles.z);
            //Debug.DrawLine(Camera.main.transform.position, hit.point, Color.red, 100);
            //左键开火
            if (timer >= CD && Input.GetMouseButton(0) && carControl.Fire())
            {
                timer = 0;
                isfire = true;
                if (gameObject.GetComponent<PhotonView>())
                {
                    GameObject bullet = PhotonNetwork.Instantiate("MGBullet", bulletPos.position, bulletPos.rotation);
                    bullet.GetComponent<MGBullet>().playerInfo = player.GetComponent<PlayerInfo>();
                    bullet.GetComponent<MGBullet>().bulletOwnerName = player.GetComponent<PlayerInfo>().playerName;
                }
                else
                {
                    GameObject bullet = Instantiate(bulletPrefab, bulletPos.position, bulletPos.rotation);
                    bullet.layer = 10;
                    bullet.transform.SetParent(player.transform.parent.parent);

                }

            }
            else
                isfire = false;
        }


    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(life);
            stream.SendNext(isfire);
        }
        else
        {
            life = (float)stream.ReceiveNext();
            isfire = (bool)stream.ReceiveNext();
        }
    }
}
