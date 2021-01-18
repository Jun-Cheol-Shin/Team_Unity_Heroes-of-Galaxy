using UnityEngine;

public class WarIK : HumanoidIKHandPlacement
{

    WarMachineController con;
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
        if (con.wState == WarMachineController.WeaponState.NONE)
        {
            rh_switch = false;
            lh_switch = false;
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

    private void Start()
    {
        con = GetComponent<WarMachineController>();
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

        Main_Weapon_Setting();
        Sub_Weapon_Setting();
        Third_Weapon_Setting();
        Right_hand_setting();
    }

    void OnAnimatorIK()
    {
        if(con != null)
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
