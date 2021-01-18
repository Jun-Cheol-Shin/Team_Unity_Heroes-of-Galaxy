using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIK : HumanoidIKHandPlacement
{
    EnemyController enemy;
    public Transform oldtargetLook;


    protected override void Right_hand_setting()
    {
        Quaternion rotRight;

        r_Hand.localPosition = r_Hand_Target.localPosition;
        rotRight = Quaternion.Euler(r_Hand_Target.localRotation.eulerAngles);
        r_Hand.localRotation = rotRight;
    }
    public override void l_handUpdate()
    {
        if(enemy.enemy_wState == EnemyController.Enemy_WeaponState.SWAP || enemy.enemy_wState == EnemyController.Enemy_WeaponState.RELOAD ||
           enemy.enemy_wState == EnemyController.Enemy_WeaponState.STUN)
        {
            rh_switch = false;
            lh_switch = false;
        }

        else if(enemy.enemy_wState == EnemyController.Enemy_WeaponState.GRENADE)
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

        lh_rot = l_Hand_Target.rotation;
        l_Hand.position = l_Hand_Target.position;
    }

    public void ChangePivot(Transform pivot)
    {
        targetLook = pivot;
    }
    public void ChangePivot()
    {
        targetLook = oldtargetLook;
    }

    void OnAnimatorIK()
    {
        if(enemy != null && !enemy.isDead)
        {
            aimPivot.position = shoulder.position;
            anim.SetLookAtWeight(1, 0.2f, 1);

            if(targetLook != null)
            {
                aimPivot.LookAt(targetLook.position + new Vector3(0, 1f, 0));
                anim.SetLookAtPosition(targetLook.position + new Vector3(0, 1f, 0));
            }

            else
            {
                aimPivot.LookAt(oldtargetLook.position + new Vector3(0, 1f, 0));
                anim.SetLookAtPosition(oldtargetLook.position + new Vector3(0, 1f, 0));
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


    private void Start()
    {
        anim = GetComponent<Animator>();
        shoulder = anim.GetBoneTransform(HumanBodyBones.RightShoulder).transform;

        aimPivot = new GameObject().transform;
        aimPivot.name = "aim_pivot";
        aimPivot.transform.parent = transform;

        r_Hand = new GameObject().transform;
        r_Hand.name = "right_hand";
        r_Hand.transform.parent = aimPivot;

        l_Hand = new GameObject().transform;
        l_Hand.name = "left_hand";
        l_Hand.transform.parent = aimPivot;

        enemy = GetComponent<EnemyController>();

        Main_Weapon_Setting();
        Sub_Weapon_Setting();
        Third_Weapon_Setting();
        Right_hand_setting();
    }
}
