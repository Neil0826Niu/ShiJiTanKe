using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

public class EditorManage : MonoBehaviourPun
{
    public PlayerInfo playerInfo;
    public Camera editorCamera;
    public GameObject newBlock;
    public GameObject newCreateBlock;
    public GameObject editObject;
    public GameObject core;
    public string selectBlockName;
    private string editingBlockName;
    public Material transparentMaterial;
    public Material transparentRed;
    private Vector3 newBlockSize;
    private bool canCreateBlock;
    public bool isEditing;
    public bool isDeleting;
    private readonly List<Vector3> editBlockPos = new List<Vector3>();
    private readonly List<Quaternion> editBlockRot = new List<Quaternion>();
    // Start is called before the first frame update
    // ReSharper disable once UnusedMember.Local
    private void Start()
    {
        if (PhotonNetwork.IsConnected)
            playerInfo = GameObject.Find(PhotonNetwork.NickName).GetComponent<PlayerInfo>();
        GetTransparentBlock();
        canCreateBlock = false;
        isEditing = true;
        isDeleting = false;
        editBlockPos.Add(core.transform.position);
        editBlockRot.Add(core.transform.rotation);
        for (var i = 0; i < core.transform.childCount; i++)
        {
            editBlockPos.Add(core.transform.GetChild(i).position);
            editBlockRot.Add(core.transform.GetChild(i).rotation);
        }

    }
    // Update is called once per frame
    // ReSharper disable once UnusedMember.Local
    public void InitEditor()
    {

    }
    private void Update()
    {
        ChangeSelBlock();
        LockPosition();
        if (!IsTouchUi() && isEditing && !isDeleting)
        {
            PlantNewBlock();
        }
        else if (!IsTouchUi() && isDeleting)
        {
            DeleteBlock();
        }
        else
        {
            newBlock.gameObject.SetActive(false);
        }
    }
    //检测鼠标是否在UI上
    private static bool IsTouchUi()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
    //锁定位置
    private void LockPosition()
    {

        var coreRig = core.GetComponent<Rigidbody>();
        if (isEditing)
        {
            core.GetComponent<Crosshairs>().enabled = false;
            WheelCollider[] mWheels;
            mWheels = core.GetComponentsInChildren<WheelCollider>();
            foreach (var w in mWheels)
            {
                w.enabled = false;
                // w.transform.rotation = Quaternion.identity;
            }
            if (core.GetComponent<CarControl>() != null)
                core.GetComponent<CarControl>().canMove = false;



            coreRig.constraints = RigidbodyConstraints.FreezeAll;
            coreRig.transform.position = editBlockPos[0];
            coreRig.transform.rotation = editBlockRot[0];
        }
        else
        {
            core.GetComponent<Crosshairs>().enabled = true;
            WheelCollider[] mWheels;
            mWheels = core.GetComponentsInChildren<WheelCollider>();
            foreach (var w in mWheels)
            {
                w.enabled = true;
            }
            if (core.GetComponent<CarControl>() != null)
                core.GetComponent<CarControl>().canMove = true;

            coreRig.constraints = 0;
        }

        for (var i = 0; i < core.transform.childCount; i++)
        {
            var component = core.transform.GetChild(i).transform;
            if (isEditing)
            {
                if (core.transform.GetChild(i).GetComponent<MachineGun>())
                {
                    core.transform.GetChild(i).GetComponent<MachineGun>().enabled = false;
                }
                component.position = editBlockPos[i + 1];
                component.rotation = editBlockRot[i + 1];
            }
            else
            {
                if (core.transform.GetChild(i).GetComponent<MachineGun>())
                {
                    core.transform.GetChild(i).GetComponent<MachineGun>().enabled = true;
                }
            }
        }
    }
    //更换选择方块
    private void ChangeSelBlock()
    {
        if (selectBlockName == editingBlockName) return;
        editingBlockName = selectBlockName;
        newCreateBlock = (GameObject)Resources.Load("Prefabs/" + editingBlockName);
        if (newBlock != null)
            Destroy(newBlock);
        GetTransparentBlock();
    }
    //声明透明方块
    private void GetTransparentBlock()
    {

        newBlock = Instantiate(newCreateBlock, new Vector3(-0, -100, -0), Quaternion.Euler(0, 0, 0));
       
        newBlock.GetComponent<Collider>().enabled = false;
        newBlock.GetComponent<Renderer>().material = transparentMaterial;
        newBlock.layer = 10;
        newBlock.transform.parent = editObject.transform.parent;
        newBlockSize = newBlock.transform.localScale;
        newBlock.tag = "TransparentBlock";
        if (newBlock.GetComponent<MachineGun>())
        {
            newBlock.GetComponent<MachineGun>().enabled = false;
        }
    }
    //删除块
    private void DeleteBlock()
    {
        var ray = editorCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit)) return;
        if (hit.collider.transform.tag != "Block" && hit.collider.transform.tag != "PowerWheel"
            && hit.collider.transform.tag != "RotWheel" && hit.collider.transform.tag != "Weapon"
            || !Input.GetMouseButtonDown(0))
            return;
        var num = hit.collider.transform.GetSiblingIndex();
        editBlockPos.RemoveAt(num + 1);
        editBlockRot.RemoveAt(num + 1);
        if (hit.collider.transform.name.Contains("Cube2"))
            playerInfo.n_Cube2++;
        else if (hit.collider.transform.name.Contains("MachineGun"))
            playerInfo.n_MachineGun++;
        else if (hit.collider.transform.name.Contains("PowerWheel") || hit.collider.transform.name.Contains("RotWheel"))
            playerInfo.n_Wheel++;
        Destroy(hit.collider.transform.gameObject);
    }
    //摆放方块
    private void PlantNewBlock()
    {
        var ray = editorCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
            //射线检测已经生成的cube
            if (hit.collider.transform.tag == "Block" || hit.collider.transform.tag == "Core" ||
                hit.collider.transform.tag == "RotWheel" || hit.collider.transform.tag == "PowerWheel")
            {
                NewBlockCheck(hit);
                if (hit.transform.tag == "Core")
                    ClickToCreate(true);
                else
                    ClickToCreate(false);
            }
            else if (hit.transform.tag == "TransparentBlock")
            {
                //NewBlockCheck(hit);
            }
            else
            {
                newBlock.gameObject.SetActive(false);
                newBlock.transform.position = new Vector3(0, -100, 0);
                canCreateBlock = false;
            }
        }
        else
        {
            newBlock.gameObject.SetActive(false);
            canCreateBlock = false;
        }
    }
    //左键创建新的块
    private void ClickToCreate(bool isCore)
    {
        if (!Input.GetMouseButtonDown(0) || !canCreateBlock) return;
        if (selectBlockName == "Cube2" && playerInfo.n_Cube2 == 0 ||
            selectBlockName == "MachineGun" && playerInfo.n_MachineGun == 0 ||
            (selectBlockName == "PowerWheel" || selectBlockName == "RotWheel") && playerInfo.n_Wheel == 0)
            return;

        canCreateBlock = false;
        var aNewBlock = Instantiate(newCreateBlock, newBlock.transform.position, newBlock.transform.rotation);
        aNewBlock.transform.parent = core.transform;
        aNewBlock.layer = 10;
        editBlockPos.Add(aNewBlock.transform.position);
        editBlockRot.Add(aNewBlock.transform.rotation);
        if (selectBlockName == "Cube2")
            playerInfo.n_Cube2--;
        else if (selectBlockName == "MachineGun")
            playerInfo.n_MachineGun--;
        else if (selectBlockName == "PowerWheel" || selectBlockName == "RotWheel")
            playerInfo.n_Wheel--;
        //if (isCore)
        //    AddJoint(aNewBlock, true);
        //else
        //    AddJoint(aNewBlock, false);
    }
    //添加固定
    private void AddJoint(GameObject aNewBlock, bool isCore)
    {
        if (isCore)
        {
            aNewBlock.AddComponent<FixedJoint>().connectedBody = core.GetComponent<Rigidbody>();
        }
        for (var i = 0; i < core.transform.childCount; i++)
        {
            var block = core.transform.GetChild(i);
            var direction = aNewBlock.transform.position - block.position;
            double sumX = (newBlockSize.x + block.localScale.x) / 2;
            double sumY = (newBlockSize.y + block.localScale.y) / 2;
            double sumZ = (newBlockSize.z + block.localScale.z) / 2;
            if (Math.Abs(direction.x) < 1e-6 && Math.Abs(direction.y) < 1e-6 && Math.Abs(Mathf.Abs(direction.z) - sumZ) < 1e-6 ||
                Math.Abs(direction.y) < 1e-6 && Math.Abs(direction.z) < 1e-6 && Math.Abs(Mathf.Abs(direction.x) - sumX) < 1e-6 ||
                Math.Abs(direction.z) < 1e-6 && Math.Abs(direction.x) < 1e-6 && Math.Abs(Mathf.Abs(direction.y) - sumY) < 1e-6)
            {
                aNewBlock.AddComponent<FixedJoint>().connectedBody = block.GetComponent<Rigidbody>();
            }
        }

    }

    //透明预创建方块位置检测
    private void NewBlockCheck(RaycastHit hit)
    {
        Vector3[] blockPoss;
        bool isBlock = hit.collider.transform.GetComponent<Block>();
        //获取目标快点位
        if (isBlock)
            blockPoss = hit.collider.transform.GetComponent<Block>().pos;
        else
            blockPoss = hit.collider.transform.GetComponent<Wheel>().pos;
        //计算hit点和目标块中心向量
        var direction = hit.point - hit.collider.transform.position;
        var dis = (hit.point - blockPoss[0]).magnitude;

        //寻找最近点
        var newBlockPosition = blockPoss[0];
        foreach (var t in blockPoss)
        {
            if (dis > (hit.point - t).magnitude)
            {
                dis = (hit.point - t).magnitude;
                newBlockPosition = t;
            }
        }

        //创建透明块
        if (isBlock)
            CreateBlock(direction, newBlockPosition);
        else
            CreateWheel(direction, newBlockPosition);
        foreach (Transform block in core.transform.GetComponentsInChildren<Transform>())
        {
            if (block.tag == "Ignore")
                continue;
            Vector3 a = GetSize(block);
            Vector3 b = newBlock.transform.localScale;
            if (newBlock.tag == "Weapon")
                b = new Vector3(1, 1,1);
            if (newBlock.tag == "RotWheel" || newBlock.tag == "PowerWheel")
                b = new Vector3(0.6f, 1.5f, 1.5f);
            Vector3 adis = newBlock.transform.position - block.position;
            Vector3 asize = (a + b) / 2;
            
            if (Mathf.Abs(adis.x) < Mathf.Abs(asize.x) &&
                Mathf.Abs(adis.y) < Mathf.Abs(asize.y) &&
                Mathf.Abs(adis.z) < Mathf.Abs(asize.z) ||
                selectBlockName == "Cube2" && playerInfo.n_Cube2 == 0 ||
                selectBlockName == "MachineGun" && playerInfo.n_MachineGun == 0 ||
                (selectBlockName == "PowerWheel" || selectBlockName == "RotWheel") && playerInfo.n_Wheel == 0)
            {
                newBlock.GetComponent<Renderer>().material = transparentRed;
                if (newBlock.transform.childCount > 0)
                {
                    foreach (Renderer child in newBlock.GetComponentsInChildren<Renderer>())
                    {
                        if (child.GetComponent<Renderer>())
                            child.GetComponent<Renderer>().material = transparentRed;
                    }
                }
                canCreateBlock = false;
                break;
            }
            else
            {
                newBlock.GetComponent<Renderer>().material = transparentMaterial;
                if (newBlock.transform.childCount > 0)
                {
                    foreach (Renderer child in newBlock.GetComponentsInChildren<Renderer>())
                    {
                        if (child.GetComponent<Renderer>())
                            child.GetComponent<Renderer>().material = transparentMaterial;
                    }
                }
                canCreateBlock = true;
            }
        }

    }
    private Vector3 GetSize(Transform transform)
    {
        if (transform.tag == "Weapon")
            return new Vector3(1, 1, 1);
        if (transform.tag == "RotWheel" || transform.tag == "PowerWheel")
            return new Vector3(0.6f, 1.5f, 1.5f);
        Vector3 a;
        if (transform.gameObject.GetComponent<MeshFilter>())
            a = transform.gameObject.GetComponent<MeshFilter>().mesh.bounds.size;
        else
            a = Vector3.one;
        Vector3 b = transform.localScale;
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    //创建透明方块
    private void CreateBlock(Vector3 direction, Vector3 newBlockPosition)
    {

        newBlock.gameObject.SetActive(true);
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y) && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            if (direction.x > 0)
                newBlock.transform.position = newBlockPosition + newBlock.transform.right * newBlockSize.x / 2;
            else
                newBlock.transform.position = newBlockPosition - newBlock.transform.right * newBlockSize.x / 2;
        }
        else if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x) && Mathf.Abs(direction.y) > Mathf.Abs(direction.z))
        {
            if (direction.y > 0)
                newBlock.transform.position = newBlockPosition + newBlock.transform.up * newBlockSize.y / 2;
            else
                newBlock.transform.position = newBlockPosition - newBlock.transform.up * newBlockSize.y / 2;
        }
        else if (Mathf.Abs(direction.z) > Mathf.Abs(direction.x) && Mathf.Abs(direction.z) > Mathf.Abs(direction.y))
        {
            if (direction.z > 0)
                newBlock.transform.position = newBlockPosition + newBlock.transform.forward * newBlockSize.z / 2;
            else
                newBlock.transform.position = newBlockPosition - newBlock.transform.forward * newBlockSize.z / 2;
        }
    }

    private void CreateWheel(Vector3 direction, Vector3 newBlockPosition)
    {

        newBlock.gameObject.SetActive(true);
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y) && Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            if (direction.x > 0)
                newBlock.transform.position = newBlockPosition + newBlock.transform.right * newBlockSize.x / 2;
            else
                newBlock.transform.position = newBlockPosition - newBlock.transform.right * newBlockSize.x / 2;
        }
        else
        {
            newBlock.gameObject.SetActive(false);
        }

    }

}
