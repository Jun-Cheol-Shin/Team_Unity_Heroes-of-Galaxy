using UnityEngine;

public class AllyIK : HumanoidIKHandPlacement
{

    AllyController ally;
    public WeaponType AI_IKweaponState;
    public Transform oldtargetLook;


    protected override void Right_hand_setting()
    {
        Quaternion rotRight;

        switch(ally.weaponType)
        {
            case WeaponType.MainWeapon:
                r_Hand.localPosition = r_Hand_Target.localPosition;
                rotRight = Quaternion.Euler(r_Hand_Target.localRotation.eulerAngles);
                r_Hand.localRotation = rotRight;
            break;

            case WeaponType.SubWeapon:
                r_Hand.localPosition = r_Hand_Target_2.localPosition;
                rotRight = Quaternion.Euler(r_Hand_Target_2.localRotation.eulerAngles);
                r_Hand.localRotation = rotRight;
            break;

            case WeaponType.SpecialWeapon:
                r_Hand.localPosition = r_Hand_Target_3.localPosition;
                rotRight = Quaternion.Euler(r_Hand_Target_3.localRotation.eulerAngles);
                r_Hand.localRotation = rotRight;
                break;
        }
    }
    public override void l_handUpdate()
    {
        if (ally.ally_wState == AllyController.Ally_WeaponState.SWAP || ally.ally_wState == AllyController.Ally_WeaponState.RELOAD ||
            ally.ally_wState == AllyController.Ally_WeaponState.SKILL2HAND || ally.ally_wState == AllyController.Ally_WeaponState.STUN ||
            ally.ally_wState == AllyController.Ally_WeaponState.REVIVE || ally.ally_wState == AllyController.Ally_WeaponState.PUMP) 
        {
            rh_switch = false;
            lh_switch = false;
        }

        else if(ally.ally_wState == AllyController.Ally_WeaponState.GRENADE || ally.ally_wState == AllyController.Ally_WeaponState.SKILL1HAND)
        {
            lh_switch = false;
            rh_switch = true;
        }

        else
        {
            rh_switch = true;
            lh_switch = true;
        }

        Weight_setting();

        switch(ally.weaponType)
        {
            case WeaponType.MainWeapon:
                lh_rot = l_Hand_Target.rotation;
                l_Hand.position = l_Hand_Target.position;
                break;

            case WeaponType.SubWeapon:
                lh_rot = l_Hand_Target_2.rotation;
                l_Hand.position = l_Hand_Target_2.position;
                break;

            case WeaponType.SpecialWeapon:
                lh_rot = l_Hand_Target_3.rotation;
                l_Hand.position = l_Hand_Target_3.position;
                break;
        }
    }

    public void ChangePivot(Transform pivot)
    {
        targetLook = pivot;
    }
    public void ChangePivot()
    {
        targetLook = oldtargetLook;
    }


    private void Start()
    {
        ally = GetComponent<AllyController>();
        anim = GetComponent<Animator>();
        shoulder = anim.GetBoneTransform(HumanBodyBones.RightUpperArm).transform;

        aimPivot = new GameObject().transform;
        aimPivot.name = "aim_pivot";
        aimPivot.transform.parent = transform;

        r_Hand = new GameObject().transform;
        r_Hand.name = "right_hand";
        r_Hand.transform.parent = aimPivot;

        l_Hand = new GameObject().transform;
        l_Hand.name = "left_hand";
        l_Hand.transform.parent = aimPivot;

        Main_Weapon_Setting();
        Sub_Weapon_Setting();
        Third_Weapon_Setting();
        Right_hand_setting();

        AI_IKweaponState = ally.weaponType;
    }

    void OnAnimatorIK()
    {
        if(ally != null && AI_IKweaponState != ally.weaponType)
        {
            switch(ally.weaponType)
            {
                case WeaponType.MainWeapon:
                    Main_Weapon_Setting();
                    break;

                case WeaponType.SubWeapon:
                    Sub_Weapon_Setting();
                    break;

                case WeaponType.SpecialWeapon:
                    Third_Weapon_Setting();
                    break;
            }

            AI_IKweaponState = ally.weaponType;
            Right_hand_setting();
        }

        else if(ally != null && !ally.isDead)
        {
            aimPivot.position = shoulder.position;
            anim.SetLookAtWeight(1, 0.2f, 1);

            if(targetLook != null)
            {
                aimPivot.LookAt(targetLook.position);
                anim.SetLookAtPosition(targetLook.position);
            }

            else
            {
                aimPivot.LookAt(oldtargetLook.position);
                anim.SetLookAtPosition(oldtargetLook.position);
            }

            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, lh_Weight);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, lh_Weight);
            anim.SetIKPosition(AvatarIKGoal.LeftHand, l_Hand.position);
            anim.SetIKRotation(AvatarIKGoal.LeftHand, lh_rot);

            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, rh_Weight);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, rh_Weight);
            anim.SetIKPosition(AvatarIKGoal.RightHand, r_Hand.position);
            anim.SetIKRotation(AvatarIKGoal.RightHand, r_Hand.rotation);
        }
    }

}
