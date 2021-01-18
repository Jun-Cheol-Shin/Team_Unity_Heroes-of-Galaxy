using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class CameraCollision : MonoBehaviour
    {
        [System.Serializable]
        public class Offset
        {
            [Header("기본 자세")]
            public Vector3 idle;
            [Header("스나이퍼 조준")]
            public Vector3 Zoom;
            [Header("3인칭 견착")]
            public Vector3 Aiming;

            [HideInInspector]
            public Vector3 mirrorAiming;
        }

        [Header("카메라 피벗")]
        public Transform Target;                            // 피벗
        [Header("카메라 더미 (충돌검사 카메라)")]
        public Transform CameraTransform;                   // 카메라 더미
        public LayerMask collisionLayer;                    // 충돌을 걸러낼 마스크

        [Header("카메라 스무딩 값")]
        public float smooth;

        Vector3 modify;
        Camera thisCamera;                                  // 메인 카메라 실제로 위치를 옮길 오브젝트
        RaycastHit hit;


        [SerializeField]
        [Header("내 캐릭터")]
        public GameObject Character;                       // 내 캐릭터
        Player_Movement player;
        public Offset offset;                               // 카메라 옵셋 구조체           


        [Header("카메라 레이저")]
        public bool SeeRay;


        LayerMask layer;

        private void Start()
        { 
            thisCamera = Camera.main;
            //CameraClipPlanePoint(thisCamera.nearClipPlane);
            player = Character.GetComponent<Player_Movement>();

            transform.localPosition = offset.idle;
            CameraTransform.localPosition = transform.localPosition;
            offset.mirrorAiming = new Vector3(-offset.Aiming.x, offset.Aiming.y, offset.Aiming.z);
            collisionLayer = LayerMask.GetMask("Wall") + LayerMask.GetMask("Map");
        }

        // 연산된 vector3 배열을 이용해 레이캐스트를 이용한 충돌 연산을 해준다.
        public void CollisionCheck()
        {
            if(SeeRay)
                drawLine();

            
            // Target은 화면의 정중앙, CameraTransform은 더미
            Ray ray = new Ray(Target.position, CameraTransform.transform.position - Target.position);
            float distance = Vector3.Distance(CameraTransform.transform.position, Target.position);
            if(Physics.Raycast(ray, out hit, distance + 0.5f, collisionLayer) && player.p_Astate != AIM_STATE.ZOOM)
            {
                transform.position = hit.point;
                // modify는 캐릭터를 기준 (0,0,0) 으로 카메라까지의 방향
                // 카메라는 캐릭터의 뒤로 땡겨져있다. 그 반대는 캐릭터의 앞으로...
                transform.Translate(-modify * 0.5f);
            }
            else
            {
                cameraPositionSetting();
            }
        }


        void drawLine()
        {
            // 수정된 라인
            Debug.DrawRay(Target.position, thisCamera.transform.position - Target.position, Color.cyan);
        }

        void cameraPositionSetting()
        {
            if(player.p_state != STATE.ROLL)
            {
                switch(player.p_Astate)
                {
                    // 기본 스탠딩
                    case Player.AIM_STATE.SWAP:
                    case Player.AIM_STATE.RELOAD:
                    case Player.AIM_STATE.NONE:
                    if(thisCamera.fieldOfView != 60f)
                    {
                        thisCamera.fieldOfView = 60f;
                        thisCamera.nearClipPlane = 0.3f;
                        thisCamera.cullingMask = layer;
                    }
                    transform.localPosition = Vector3.Lerp(transform.localPosition, offset.idle, smooth * player.Second);
                    CameraTransform.localPosition = Vector3.Lerp(CameraTransform.localPosition, offset.idle, smooth * player.Second);

                    modify = new Vector3(0f, offset.idle.y, offset.idle.z).normalized;
                    break;

                    //
                    case Player.AIM_STATE.AIMING:
                    if(thisCamera.fieldOfView != 60f)
                    {
                        thisCamera.cullingMask = layer;
                        thisCamera.fieldOfView = 60f;
                        thisCamera.nearClipPlane = 0.3f;

                    }
                    if(!player.mirror)
                    {
                        transform.localPosition = Vector3.Lerp(transform.localPosition, offset.Aiming, smooth * player.Second);
                        CameraTransform.localPosition = Vector3.Lerp(CameraTransform.localPosition, offset.Aiming, smooth * player.Second);
                        modify = new Vector3(0f, offset.idle.y, offset.idle.z).normalized;
                        }
                    else
                    {
                        transform.localPosition = Vector3.Lerp(transform.localPosition, offset.mirrorAiming, smooth * player.Second);
                        CameraTransform.localPosition = Vector3.Lerp(CameraTransform.localPosition, offset.mirrorAiming, smooth * player.Second);
                        modify = new Vector3(0f, offset.mirrorAiming.y, offset.mirrorAiming.z).normalized;
                    }
                    break;

                    case Player.AIM_STATE.ZOOM:
                    if(thisCamera.fieldOfView != 30f)
                    {
                        layer = thisCamera.cullingMask;
                        thisCamera.fieldOfView = 30f;
                        thisCamera.cullingMask += ~(1 << LayerMask.NameToLayer("Player"));
                        thisCamera.cullingMask += (1 << LayerMask.NameToLayer("Default"));
                    }
                    transform.localPosition = Vector3.Lerp(transform.localPosition, offset.Zoom, smooth * player.Second);
                    CameraTransform.localPosition = Vector3.Lerp(CameraTransform.localPosition, offset.Zoom, smooth * player.Second);
                    break;
                }
            }

            else
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x,
                        transform.localPosition.y, offset.idle.z - 1.5f), Time.deltaTime * 2f);
            }
        }

    }
}
