using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] public float Sensitivity;
        [SerializeField] public float clampAngle;
        [SerializeField] public Transform CameraPivot;
        [SerializeField] public Transform Character;
        [SerializeField] public Animator anim;

        Player_Movement playerMovement;

        //[Header("앉기 카메라 위치 보정값")]
        //public float CrouchGain;                            // 앉기 카메라 위치 보정값
        // 마우스 좌우 이동값을 담는 변수
        public float horizontal;
        // 마우스 상하 이동값을 담는 변수
        public float vertical;

        public float rotY;
        public float rotX;


        Vector3 originalVec;

        public bool locked;

        void Start()
        {
            transform.position = CameraPivot.localPosition;
            Vector3 rot = transform.localRotation.eulerAngles;
            rotY = rot.y;
            rotX = rot.x;

            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;

            originalVec = CameraPivot.localPosition;

            playerMovement = Character.GetComponent<Player_Movement>();
        }

        public void cameraRotation(float horizontal, float vertical)
        {
            // Slerp로 바꾸자
            Quaternion localRotation = Quaternion.Euler(-vertical, horizontal, 0f);

            // 카메라 회전
            transform.rotation = localRotation;
            SetRotateCharacter();

            transform.position = CameraPivot.position;
        }

        public void cameraRotation()
        {
            if(!locked)
            {
                horizontal = Input.GetAxis("Mouse X");
                vertical = Input.GetAxis("Mouse Y");

                if(playerMovement != null)
                {
                    rotY += horizontal * Sensitivity * playerMovement.Second * 5f;
                    rotX += vertical * Sensitivity * playerMovement.Second * 5f;
                }

                else
                {
                    rotY += horizontal * Sensitivity * Time.deltaTime * 5f;
                    rotX += vertical * Sensitivity * Time.deltaTime * 5f;
                }

                rotX = Mathf.Clamp(rotX, -clampAngle + 5f, clampAngle);

                if(rotX > 180f)
                {
                    rotX -= 360f;
                }
                anim.SetFloat("Roll", (int)rotX);


                // Slerp로 바꾸자
                Quaternion localRotation = Quaternion.Euler(-rotX, rotY, 0f);

                if(rotX != 0 && rotY != 0)
                {
                    // 카메라 회전
                    transform.rotation = localRotation;
                    RotateCharacter();
                }
            }

            else if(locked)
            {
                transform.rotation = Quaternion.Euler(0f,
                    transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                anim.SetFloat("Roll", 0f);
                RotateCharacter();
                rotX = 0f;
            }

            transform.position = CameraPivot.position;
        }

        public void RotateCharacter()
        {
            if (playerMovement == null)
            {
                return;
            }

            if (playerMovement.p_state != STATE.DEAD &&
                     playerMovement.p_state != STATE.ROLL)
            {
                Quaternion a = Quaternion.Euler(
                     Character.rotation.eulerAngles.x,
                     rotY,
                     Character.rotation.eulerAngles.z);

                Character.rotation = Quaternion.Lerp(Character.rotation, a, playerMovement.Second * 15f);
            }
        }

        public void SetRotateCharacter()
        {
            Quaternion a = Quaternion.Euler(
                     Character.rotation.eulerAngles.x,
                     rotY,
                     Character.rotation.eulerAngles.z);

            Character.rotation = Quaternion.Lerp(Character.rotation, a, Time.deltaTime * 15f);
        }

    }
}

