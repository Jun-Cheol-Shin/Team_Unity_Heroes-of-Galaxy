using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Generics.Dynamics;


namespace Player
{
    public class Player_Manager : MonoBehaviour
    {
        public bool isFlag = true;
        public GameObject Character;
        public Player_Movement playermovement;
        HumanoidVerticalLegPlacement footPlacement;

        public CameraFollow camerafollow;
        public CameraCollision collisionCamera;

        public Player_IK characterIK;
        public Player_WeaponAnimation weaponManager;

        private void Start()
        {
            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;

            playermovement = Character.GetComponent<Player_Movement>();
            footPlacement = Character.GetComponent<HumanoidVerticalLegPlacement>();

            characterIK = Character.GetComponent<Player_IK>();

            weaponManager = Character.GetComponent<Player_WeaponAnimation>();
        }


        private void FixedUpdate()
        {
            if(!GameManager.instance.isTimeStopped)
            {

                if(playermovement.p_state != STATE.DEAD 
                    && playermovement.p_state != STATE.STUN
                    && playermovement.p_state != STATE.REVIVE)
                {

                    playermovement.CheckGround();
                    playermovement.CheckSlope();

                    // 총 앞 장애물 체크
                    playermovement.checkGunforward();

                    // 애니메이션 스테이트 체크
                    playermovement.SetPlayerState();

                    // 애니메이션 파라미터 MOVESPEED 체크
                    playermovement.SetMovementSpeed();

                }
            } 
        }

        private void Update()
        {
            if (isFlag)
            {
                return;
            }

            if(!GameManager.instance.isTimeStopped)
            {
                playermovement.Player_Move();
                if(playermovement.p_state != STATE.STUN)
                {
                    camerafollow.cameraRotation();
                }
                if (weaponManager != null)
                {
                    weaponManager.LeftHandUpdate();
                }
            }
        }

        private void LateUpdate()
        {
            //playermovement.drawRay();
            characterIK.l_handUpdate();
            if(!GameManager.instance.isTimeStopped)
            {
                //if(playermovement.p_state != STATE.STUN)
                //{
                //    camerafollow.cameraRotation();
                //}
                collisionCamera.CollisionCheck();

                if(playermovement.slopeAngle < playermovement.body.slopeLimit
                    && playermovement.isGrounded)
                {
                    footPlacement.ProcessLegs();
                    footPlacement.ProcessHips();
                    footPlacement.Solve();
                }

                playermovement.roll_lateUpdate();
            }
        }
    }
}

