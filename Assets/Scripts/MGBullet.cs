using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class MGBullet : MonoBehaviourPunCallbacks, IPunObservable
{
    public PlayerInfo playerInfo;
    public string bulletOwnerName;
    public float speed = 100f;
    public float damage = 10f;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody>().velocity = transform.forward * speed;
        Destroy(gameObject, 3f);
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.GetComponent<PlayerInfo>())
            if (collision.gameObject.GetComponent<PlayerInfo>().playerName == bulletOwnerName)
            {
                Destroy(gameObject);
                return;
            }
        Destroy(gameObject);

        if (collision.gameObject.GetComponent<PlayerInfo>())
        {
            collision.gameObject.GetComponent<PlayerInfo>().isRebuilding = false;
        }
        //if (photonView.IsMine&& (collision.gameObject.CompareTag("Core") || collision.collider.CompareTag("Core") || collision.collider.CompareTag("Weapon") || collision.collider.CompareTag("RotWheel") || collision.collider.CompareTag("PowerWheel")))

        if (collision.collider.CompareTag("Core"))
        {
            collision.gameObject.GetComponent<PlayerInfo>().life -= damage;
            if (photonView.IsMine)
                playerInfo.gameUI.hit = true;
        }
        if (collision.collider.CompareTag("Weapon"))
        {
            if (photonView.IsMine)
                playerInfo.gameUI.hit = true;
            if (collision.collider.GetComponent<MachineGun>())
            {
                collision.collider.GetComponent<MachineGun>().life -= damage;
            }
        }
        if (collision.collider.CompareTag("RotWheel") || collision.collider.CompareTag("PowerWheel"))
        {
            if (photonView.IsMine)
                playerInfo.gameUI.hit = true;
            if (collision.collider.GetComponent<Wheel>())
            {
                collision.collider.GetComponent<Wheel>().life -= damage;
            }
        }
        if (collision.gameObject.CompareTag("Core"))
        {
            if (photonView.IsMine)
                playerInfo.gameUI.hit = true;
            collision.gameObject.GetComponent<PlayerInfo>().life -= damage;
            collision.gameObject.GetComponent<PlayerInfo>().killerName = bulletOwnerName;
        }
        if (collision.collider.CompareTag("Block"))
        {
            if (photonView.IsMine)
                playerInfo.gameUI.hit = true;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(bulletOwnerName);
        }
        else
        {
            bulletOwnerName = (string)stream.ReceiveNext();
        }
    }
}
