using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class CarControl : MonoBehaviourPunCallbacks
{
    [Tooltip("Maximum steering angle of the wheels")]
    public float maxAngle = 30f;

    [Tooltip("Maximum torque applied to the driving wheels")]
    private float maxTorque = 3000f;

    [Tooltip("Maximum brake torque applied to the driving wheels")]
    public float brakeTorque = 30000f;


    [Tooltip("The vehicle's speed when the physics engine can use different amount of sub-steps (in m/s).")]
    public float criticalSpeed = 5f;


    private WheelCollider[] mWheels;

    public bool canMove;


    private PlayerInfo playerInfo;
    // Start is called before the first frame update
    // ReSharper disable once UnusedMember.Local
    private void Start()
    {
        if (gameObject.GetComponent<PhotonView>())
        {
            if (PhotonNetwork.IsConnected && !photonView.IsMine)
                return;
            else
            {
                gameObject.AddComponent<CameraWork>();
            }
        }

        playerInfo = gameObject.GetComponent<PlayerInfo>();
        canMove = true;
        mWheels = null;


    }

    // Update is called once per frame
    // ReSharper disable once UnusedMember.Local
    private void Update()
    {

        if (gameObject.GetComponent<PhotonView>())
            if (PhotonNetwork.IsConnected && !photonView.IsMine)
                return;
        var angle = maxAngle * Input.GetAxis("Horizontal");
        var vertivalInput = maxTorque * Input.GetAxis("Vertical");
        var torque = vertivalInput;
        if (canMove && Input.GetAxis("Vertical")!=0)
            playerInfo.isMoving = true;
        else
            playerInfo.isMoving = false;
        if (Input.GetKey(KeyCode.S))
        {
            torque= maxTorque/10 * Input.GetAxis("Vertical");
        }
        var handBrake = Input.GetKey(KeyCode.Space) ? brakeTorque : 0;

        var velocity = gameObject.GetComponent<Rigidbody>().velocity.magnitude;


        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.Space))
            handBrake = velocity * 85;
        mWheels = GetComponentsInChildren<WheelCollider>();
        if (mWheels != null)
        {
            foreach (var wheel in mWheels)
            {


                if (wheel.tag == "RotWheel")
                {
                    //if (!Input.GetKey(KeyCode.W)&&!Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.Space))
                    //    handBrake = wheel.rpm * 100;
                    //else
                    //    handBrake = 0;
                    //wheel.brakeTorque = handBrake;
                    if (canMove)
                    {
                        wheel.brakeTorque = handBrake;
                        wheel.steerAngle = angle;
                    }

                    else
                        wheel.steerAngle = 0;
                }

                if (wheel.tag == "PowerWheel")
                {
                    if (canMove)
                    {
                        //if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) && velocity > 20)
                        //{
                        //    torque = 600 * Input.GetAxis("Vertical");
                        //}
                        if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) && wheel.rpm > 300)
                        {
                            torque = 0;
                            handBrake = 10000;
                        }
                        
                        wheel.brakeTorque = handBrake;
                        wheel.motorTorque = torque;
                    }
                    else
                    {
                        wheel.brakeTorque = 50000;
                        wheel.motorTorque = 0;
                    }

                }




                wheel.GetWorldPose(out var p, out var q);
                var shapeTransform = wheel.transform;
                //shapeTransform.localPosition = p;
                if (shapeTransform.parent.childCount > 0)
                {
                    shapeTransform.rotation = q * Quaternion.Euler(0, 90, 0);
                    if (canMove)
                        shapeTransform.parent.GetChild(1).rotation = q * Quaternion.Euler(0, 0, 90);
                    else
                        shapeTransform.parent.GetChild(1).localRotation = Quaternion.Euler(0, -90, 90);

                }

            }
        }

        if ((Input.GetKeyDown(KeyCode.R) && !playerInfo.isReloadingMG) || (Input.GetMouseButtonDown(0) && playerInfo.currentMGBullet == 0))
        {
            ReloadMGBullet();
        }
        if (Input.GetKeyDown(KeyCode.F) && playerInfo.life < playerInfo.totallife && playerInfo.n_Aid > 0 && !playerInfo.isUsingAid && !playerInfo.isReloadingMG && !playerInfo.isRebuilding)
        {
            playerInfo.isUsingAid = true;
            StartCoroutine(UsingAid());
        }
        if (Input.GetKeyDown(KeyCode.Q)  && !playerInfo.isUsingAid && !playerInfo.isReloadingMG && !playerInfo.isRebuilding) 
        {
            transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
        }
        if (playerInfo.isRebuilding && gameObject.GetComponent<Rigidbody>().velocity.magnitude > 0.3f)
        {
            playerInfo.isRebuilding = false;
            playerInfo.isUsingAid = false;
        }
    }

    public bool Fire()
    {
        if (playerInfo.currentMGBullet > 0 && !playerInfo.isReloadingMG)
        {
            playerInfo.currentMGBullet--;
            playerInfo.isRebuilding = false;
            return true;
        }
        return false;
    }
    public void ReloadMGBullet()
    {
        if (playerInfo.MGBullet > 0 && playerInfo.currentMGBullet != playerInfo.MGMagazine)
        {
            playerInfo.isReloadingMG = true;
            StartCoroutine(ReloadMGB());
        }

    }
    IEnumerator UsingAid()
    {
        float time = 0;
        while (playerInfo.isUsingAid)
        {
            if (time >= playerInfo.aidCD)
            {
                playerInfo.life = playerInfo.life + 80 > playerInfo.totallife ? playerInfo.totallife : playerInfo.life + 80;
                playerInfo.n_Aid--;
                playerInfo.isUsingAid = false;
            }
            yield return new WaitForSeconds(0.1f);
            time += 0.1f;
        }
        StopCoroutine(UsingAid());
    }
    IEnumerator ReloadMGB()
    {
        float time = 0;
        while (playerInfo.isReloadingMG)
        {
            if (time >= playerInfo.reloadMGCD)
            {
                if (playerInfo.currentMGBullet + playerInfo.MGBullet < playerInfo.MGMagazine)
                {
                    playerInfo.currentMGBullet += playerInfo.MGBullet;
                    playerInfo.MGBullet = 0;
                }
                else
                {
                    playerInfo.MGBullet -= (playerInfo.MGMagazine - playerInfo.currentMGBullet);
                    playerInfo.currentMGBullet = playerInfo.MGMagazine;
                }
                playerInfo.isReloadingMG = false;
            }
            yield return new WaitForSeconds(0.1f);
            time += 0.1f;
        }
        StopCoroutine(ReloadMGB());
    }
}
