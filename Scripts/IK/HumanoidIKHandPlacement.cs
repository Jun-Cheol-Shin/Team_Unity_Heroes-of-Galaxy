using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HumanoidIKHandPlacement : MonoBehaviour
{
    public WeaponManager weaponManager;
    public GameObject First_Weapon;
    public GameObject Second_Weapon;
    public GameObject Third_Weapon;
    public Transform targetLook;

    protected Animator anim;

    protected Transform l_Hand_Target;
    protected Transform l_Hand_Target_2;
    protected Transform r_Hand_Target;
    protected Transform r_Hand_Target_2;
    protected Transform l_Hand_Target_3;
    protected Transform r_Hand_Target_3;

    public float smoothDamp;

    protected Transform l_Hand;
    protected Transform r_Hand;

    public float rh_Weight;
    public float lh_Weight;
    public float look_Weight;

    public bool rh_switch;
    public bool lh_switch;

    protected Quaternion lh_rot;

    protected Transform shoulder;
    protected Transform aimPivot;

    protected int FirstWeapon_Num;
    protected int SecondWeapon_Num;
    protected int ThirdWeapon_Num;


    protected void Main_Weapon_Setting() 
    {
        if(First_Weapon.transform.childCount > 0)
        {
            l_Hand_Target = First_Weapon.transform.GetChild(0).GetChild(1);             // 무기의 l_target 넣기
            r_Hand_Target = First_Weapon.transform.GetChild(0).GetChild(2);
        }
    }
    protected void Sub_Weapon_Setting() 
    {
        if(Second_Weapon.transform.childCount > 0)
        {
            l_Hand_Target_2 = Second_Weapon.transform.GetChild(0).GetChild(1);
            r_Hand_Target_2 = Second_Weapon.transform.GetChild(0).GetChild(2);
        }
    }
    protected void Third_Weapon_Setting() 
    {
        if(Third_Weapon.transform.childCount > 0)
        {
            l_Hand_Target_3 = Third_Weapon.transform.GetChild(0).GetChild(1);
            r_Hand_Target_3 = Third_Weapon.transform.GetChild(0).GetChild(2);
        }
    }
    protected virtual void Weight_setting() 
    {
        if(!rh_switch)
        {
            rh_Weight -= smoothDamp * Time.deltaTime;
        }
        else
        {
            rh_Weight += smoothDamp * Time.deltaTime;
        }

        if(!lh_switch)
        {
            lh_Weight -= smoothDamp * Time.deltaTime;
        }
        else
        {
            lh_Weight += smoothDamp * Time.deltaTime;
        }

        rh_Weight = Mathf.Clamp(rh_Weight, 0, 1);
        lh_Weight = Mathf.Clamp(lh_Weight, 0, 1);
    }
    protected virtual void Right_hand_setting() { }
    public virtual void l_handUpdate() { }
}