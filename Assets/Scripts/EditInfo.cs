using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditInfo : MonoBehaviour
{
    public string playerName;
    public float life = 100;
    public int MGBullet = 120;
    private int UnitMGMagazine = 50;
    public int MGMagazine;
    public int currentMGBullet;
    public bool isReloadingMG;
    public int killNumber;
    private GameUI gameUI;
    public GameManager gameManager;
    public string killerName;

    public int n_Cube2;
    public int n_Wheel;
    public int n_MachineGun;
    // Start is called before the first frame update
    public void Start()
    {
        
        n_Cube2 = 0;
        n_Wheel = 0;
        n_MachineGun = 0;
        isReloadingMG = false;
        killNumber = 0;
        int MG = gameObject.GetComponentsInChildren<MachineGun>().Length;
        MGMagazine = UnitMGMagazine * MG;
        life = 100;
        MGBullet = 120;
        currentMGBullet = MGMagazine;
    }
}
