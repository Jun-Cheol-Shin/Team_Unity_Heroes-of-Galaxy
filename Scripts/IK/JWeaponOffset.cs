using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JWeaponOffset
{
    public enum SPECIES
    {
        HUMAN = 1,
        DWARF,
        LIZARD,
        ORC_WARRIOR,
        ORC_MADNESS,
        ORC_SMALLER,
    }

    public Vector3 W_pos;
    public Vector3 W_rot;
    public Vector3 LH_pos;
    public Vector3 LH_rot;
    public Vector3 RH_pos;
    public Vector3 RH_rot;
    public float W_scale;
    public SPECIES My_Species;
    public string Weapon_ID;


    public void Initialized(GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

        Weapon weapon = obj.GetComponent<Weapon>();

        Weapon_ID = weapon.id;
        W_pos = weapon.transform.localPosition;
        W_rot = weapon.transform.localRotation.eulerAngles;

        LH_pos = weapon.transform.GetChild(1).localPosition;
        LH_rot = weapon.transform.GetChild(1).localRotation.eulerAngles;

        RH_pos = weapon.transform.GetChild(2).localPosition;
        RH_rot = weapon.transform.GetChild(2).localRotation.eulerAngles;
    }
}
