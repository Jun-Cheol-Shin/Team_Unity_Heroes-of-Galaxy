using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using UnityEngine;



namespace Player
{
    public class Player_IK : HumanoidIKHandPlacement
    {
        Player_Movement state;
        public WEAPON_STATE IKweaponState;


        private void Start()
        {
            anim = GetComponent<Animator>();
            state = GetComponent<Player_Movement>();

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
        }

        protected override void Right_hand_setting()
        {
            Quaternion rotRight;

            switch(state.p_Wstate)
            {
                case WEAPON_STATE.MAIN_WEAPON:
                r_Hand.localPosition = r_Hand_Target.localPosition;
                rotRight = Quaternion.Euler(r_Hand_Target.localRotation.eulerAngles);
                r_Hand.localRotation = rotRight;
                break;

                case WEAPON_STATE.SUB_WEAPON:
                r_Hand.localPosition = r_Hand_Target_2.localPosition;
                rotRight = Quaternion.Euler(r_Hand_Target_2.localRotation.eulerAngles);
                r_Hand.localRotation = rotRight;
                break;

                case WEAPON_STATE.SPECIAL_WEAPON:
                r_Hand.localPosition = r_Hand_Target_3.localPosition;
                rotRight = Quaternion.Euler(r_Hand_Target_3.localRotation.eulerAngles);
                r_Hand.localRotation = rotRight;
                break;
            }
        }
        public override void l_handUpdate()
        {

            if(state.p_state == STATE.RUN ||
                state.p_state == STATE.STUN ||
                !state.isGrounded) 
                //|| state.p_Astate == AIM_STATE.RELOAD
                //|| state.p_Astate == AIM_STATE.MELEE)
            {

                rh_switch = false;
                if(state.p_Astate == AIM_STATE.RELOAD)
                {
                    lh_switch = false;
                }

                else
                {
                    lh_switch = true;
                }

                //if (state.p_Astate == AIM_STATE.MELEE || state.p_Astate == AIM_STATE.RELOAD)
                //{
                //    lh_switch = false;
                //}
            }

            else if(state.p_state == STATE.SKILL || 
                state.p_Astate == AIM_STATE.MELEE || 
                state.p_Astate == AIM_STATE.RELOAD || 
                state.p_state == STATE.DEAD ||
                state.p_Astate == AIM_STATE.SWAP || 
                state.p_state == STATE.REVIVE)
            {
                rh_switch = false;
                lh_switch = false;
            }

            else if(state.p_Astate == AIM_STATE.GREANADE || state.p_state == STATE.REVIVING)
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

            switch(state.p_Wstate)
            {
                case WEAPON_STATE.MAIN_WEAPON:
                lh_rot = l_Hand_Target.rotation;
                l_Hand.position = l_Hand_Target.position;
                break;

                case WEAPON_STATE.SUB_WEAPON:
                lh_rot = l_Hand_Target_2.rotation;
                l_Hand.position = l_Hand_Target_2.position;
                break;

                case WEAPON_STATE.SPECIAL_WEAPON:
                lh_rot = l_Hand_Target_3.rotation;
                l_Hand.position = l_Hand_Target_3.position;
                break;
            }
        }

        protected override void Weight_setting()
        {
            if(!rh_switch)
            {
                rh_Weight -= smoothDamp * state.Second;
            }
            else
            {
                rh_Weight += smoothDamp * state.Second;
            }

            if(!lh_switch)
            {
                lh_Weight -= smoothDamp * state.Second;
            }
            else
            {
                lh_Weight += smoothDamp * state.Second;
            }

            rh_Weight = Mathf.Clamp(rh_Weight, 0, 1);
            lh_Weight = Mathf.Clamp(lh_Weight, 0, 1);
        }


        void OnAnimatorIK()
        {
            if(state != null && IKweaponState != state.p_Wstate)
            {
                switch(state.p_Wstate)
                {
                    case WEAPON_STATE.MAIN_WEAPON:
                    Main_Weapon_Setting();
                    IKweaponState = WEAPON_STATE.MAIN_WEAPON;
                    break;

                    case WEAPON_STATE.SUB_WEAPON:
                    Sub_Weapon_Setting();
                    IKweaponState = WEAPON_STATE.SUB_WEAPON;
                    break;

                    case WEAPON_STATE.SPECIAL_WEAPON:
                    Third_Weapon_Setting();
                    IKweaponState = WEAPON_STATE.SPECIAL_WEAPON;
                    break;
                }
                Right_hand_setting();
            }

            else if(state != null)
            {
                if(state.p_state != STATE.DEAD && state.p_state != STATE.STUN && state.p_state != STATE.ROLL)
                {
                    // HumanBones.RightArm을 shoulder 변수로 만듬.
                    aimPivot.position = shoulder.position;
                    anim.SetLookAtWeight(1, 0.2f, 1);

                    aimPivot.LookAt(targetLook.position);
                    anim.SetLookAtPosition(targetLook.position);

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
    }
}