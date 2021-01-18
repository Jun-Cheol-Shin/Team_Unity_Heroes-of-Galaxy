using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Player
{
    public enum PLAYER_CLASS
    {
        STRIKER = 1,
        COMMANDER,
        SHERIFF,
        EVIL_PSYKER,
    }

    public enum STATE
    {
        IDLE =1,                                // 스탠딩
        WALK,                                   // 천천히 걷기
        JOG,                                    // 보통 걷기
        RUN,                                    // 뛰기
        ROLL,                                   // 구르기
        SKILL,
        DEAD,
        STUN,
        REVIVE,
        REVIVING,
    }

    public enum STAND_STATE
    {
        STAND = 1,
        CROUCH = 2,
    }


    public enum WEAPON_STATE
    {

        MAIN_WEAPON = 1,
        SUB_WEAPON,
        SPECIAL_WEAPON,
    }

    public enum AIM_STATE
    {
        NONE = 1,           // 무조준 상태
        AIMING = 2,         // 견착 상태
        ZOOM = 3,           // 스나이퍼 1인칭 줌 상태
        RELOAD,             // 장전
        SWAP,               // 바꾸기
        MELEE,              // 근접공격
        GREANADE,           // 수류탄
        SKILL_READY,        // 스킬 준비 => 범위 표시
    }

    public struct BodyValues
    {
        public float value0;
        public float value1;
        public float value2;
        public float value3;
    }


    public class Player_Movement : MonoBehaviour
    {

        public JCharacter test = new JCharacter();

        BodyValues animBodyvalues;
        float speed;


        public float smooth;

        [Header("레이저 활성화")]
        public bool SeeRay;

        [SerializeField]
        [Header("내 캐릭터 현재 상태")]
        public STATE p_state;

        [SerializeField]
        [Header("내 캐릭터 무기 상태")]
        public WEAPON_STATE p_Wstate;

        [SerializeField]
        [Header("내 현재 조준 상태")]
        public AIM_STATE p_Astate;
        
        [Header("좌우 시점전환")]
        public bool mirror = false;

        [SerializeField]
        [Header("서있는 상태")]
        public STAND_STATE p_Sstate;

        private Vector3 moveVec;

        [SerializeField]
        [Header("캐릭터의 공중, 지상 상태")]
        public bool isGrounded;

        private float gravityY;
        private float gravityMultiplexer;
        float yVelocity;

        [HideInInspector]
        public bool isJump = false;

        //[Header("캐릭터 죽음 판별")]
        //public bool isDead = false;
        public bool onStun = false;

        public bool onGravityField = false;
        [HideInInspector]
        public bool imposibleShot = false;


        public GameObject SpineBody;
        public float rollDistance;              // 구르기 거리
        public float rollReduction;             // 구르기 가속감소
        public float JumpPower;                 // 점프 높이
        public float SwapDelay;                 // 스왑 딜레이

        public float walkSpeed;                 // 걷기 이동속도
        public float jogSpeed;                  // 조그 이동속도
        public float runSpeed;                  // 달리기 이동속도

        public float MeleeDistance;             // 근접공격 유효거리
        public LayerMask meleelayerMask;        // 근접공격 마스크


        public float RotateturnSpeed;           // 캐릭터 X축 회전 속도
        public Transform fallDirection;         // 떨어질 각도를 재기위함. 매니저를 넣는다.
        public Transform headPivot;             // 머리

        // 캐릭터 컴포넌트 관련
        [HideInInspector]
        public Animator anim;
        [HideInInspector]
        public CharacterController body;

        public Player_IK characterIK;

        // 총알 장전용 왼손
        public GameObject LeftHand;
        public GameObject RightHand;

        string special_w;
        string main_w;
        
       
        // 경사각 관련
        Vector3 collisionPoint;             // 현재 캐릭터가 붙어있는 바닥위치.
        private Ray slopeRay;
        private Ray groundRay;
        private float groundslopeAngle;

        [HideInInspector]
        public float slopeAngle;



        public AudioSource CharacterAudio;
        bool walk = false;

        bool SlowTime = false;
        public float Second;


        float rollCooltime = 0.5f;
        WaitForSeconds cooltime;
        Coroutine RollCoroutine;

        void Awake()
        {
            Second = Time.deltaTime;
        }

        public void SetChangeTime(float time)
        {
            if(time == 1f)
            {
                SlowTime = false;
            }

            else
            {
                SlowTime = true;
            }

            Time.timeScale = time;
            Time.fixedDeltaTime = 0.02f * time;
        }

        private void Update()
        {
            if(SlowTime && Second != Time.unscaledDeltaTime)
            {
                Second = Time.unscaledDeltaTime;
            }

            else if(!SlowTime && Second != Time.deltaTime)
            {
                Second = Time.deltaTime;
            }
        }

        public void MyCharacterStunSwitch(bool value)
        {
            onStun = value;
            anim.SetBool("isStuned", onStun);

            switch(value)
            {
                case true:
                p_state = STATE.STUN;
                break;

                case false:
                p_state = STATE.IDLE;
                break;
            }
        }

        public void MyCharacterDeadSwitch(bool value)
        {
            // true는 죽음 false는 살아나기
            switch(value)
            {
                case true:
                SoundManager.instance.PlaySFX(CharacterAudio, "GROAN_Male_Hurt_Long_Pain_mono");
                anim.SetTrigger("isDead");
                p_state = STATE.DEAD;
                break;

                case false:
                if(p_state != STATE.DEAD)
                {
                    return;
                }

                anim.SetTrigger("isRevive");
                p_state = STATE.REVIVE;
                StartCoroutine(CheckAnimationState("Revive"));
                break;
            }
        }


        // Start is called before the first frame update
        void Start()
        {
            Second = Time.deltaTime;
            p_state = STATE.IDLE;
            anim = GetComponent<Animator>();
            body = GetComponent<CharacterController>();
            gravityY = Physics.gravity.y;


            p_Wstate = WEAPON_STATE.MAIN_WEAPON;
            p_Astate = AIM_STATE.NONE;
            p_Sstate = STAND_STATE.STAND;

            meleelayerMask = LayerMask.GetMask("Map");
            InitBodyValue();

            gravityMultiplexer = 1000f;
            yVelocity = gravityY * gravityMultiplexer * Second;

            force = rollDistance;

            characterIK = GetComponent<Player_IK>();
            CharacterAudio = GetComponent<AudioSource>();
            decisionObjects = new List<GameObject>();

            main_w = JsonManager.instance.characterInfo.w_mainWeapon;
            if(JsonManager.instance.characterInfo.w_specialWeapon != null)
            {
                special_w = JsonManager.instance.characterInfo.w_specialWeapon[0];
            }

            weaponManager = GetComponent<WeaponManager>();

            cooltime = new WaitForSeconds(rollCooltime);


            slopeAngle = 0f;
            groundslopeAngle = 0f;
            fallflag = false;
            isGrounded = true;
        }

        public IEnumerator CheckAnimationState(string Animname)
        {

            switch(Animname)
            {
                case "Melee":
                case "Throw":
                case "Reload":
                case "Swap":

                while(!anim.GetCurrentAnimatorStateInfo(1).IsTag(Animname))
                {
                    yield return null;
                }


                while(anim.GetCurrentAnimatorStateInfo(1).normalizedTime <= 1f)
                {
                    if(!anim.GetCurrentAnimatorStateInfo(1).IsTag(Animname))
                    {
                        break;
                    }
                    yield return null;
                }
                break;

                case "Revive":
                case "Roll":
                case "Skill":
                case "Jump":

                while(!anim.GetCurrentAnimatorStateInfo(0).IsTag(Animname))
                {
                    yield return null;
                }


                while(anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1f)
                {
                    if(!anim.GetCurrentAnimatorStateInfo(0).IsTag(Animname))
                    {
                        break;
                    }
                    yield return null;
                }

                break;
            }

            switch(Animname)
            {

                // 근접 공격 상태
                case "Melee":                       // 1번
                p_Astate = AIM_STATE.NONE;
                break;

                // Grenade 상태
                case "Throw":                       // 1번
                p_Astate = AIM_STATE.NONE;
                break;

                case "Skill":
                p_state = STATE.IDLE;
                break;

                // 장전 상태
                case "Reload":                      // 1번
                p_Astate = AIM_STATE.NONE;
                break;

                // 구르기 상태
                case "Roll":
                p_state = STATE.IDLE;
                force = rollDistance;
                RollCoroutine = StartCoroutine(RollCooltime());
                break;

                // 무기 바꾸기 상태
                case "Swap":                        // 1번
                p_Astate = AIM_STATE.NONE;
                break;

                case "Revive":
                p_state = STATE.IDLE;
                break;

                case "Jump":
                fallflag = false;
                break;
            }

            p_Astate = AIM_STATE.NONE;

            yield return null;
        }


        public bool GetGravityField()
        {
            return onGravityField;
        }

        public void SetGravityField(bool ret)
        {
            onGravityField = ret;
        }

        public void roll_lateUpdate()
        {
            float h2, v2;
            h2 = anim.GetFloat("MovementX");
            v2 = anim.GetFloat("MovementZ");

            Vector3 test = new Vector3(h2, 0, v2);

            if((h2 != 0 && v2 != 0) && 
                (p_state == STATE.RUN || p_state == STATE.ROLL))
            {
                Vector3 ViewAngle = Quaternion.LookRotation(test, Vector3.up).eulerAngles;

                if(v2 > 0)
                {
                    if(ViewAngle.y < 180f)
                    {
                        ViewAngle.y *= 0.5f;
                    }

                    else
                    {
                        float temp = (360f - ViewAngle.y) * 0.3f;
                        ViewAngle.y += temp;
                    }
                    SpineBody.transform.rotation = Quaternion.Euler(SpineBody.transform.rotation.eulerAngles.x,
                                SpineBody.transform.rotation.eulerAngles.y + ViewAngle.y,
                                SpineBody.transform.rotation.eulerAngles.z);
                }
                else
                {

                    if(ViewAngle.y > 180f)
                    {
                        ViewAngle.y *= 0.9f;
                    }

                    SpineBody.transform.rotation = Quaternion.Euler(SpineBody.transform.rotation.eulerAngles.x,
                                SpineBody.transform.rotation.eulerAngles.y + ViewAngle.y + 180f,
                                SpineBody.transform.rotation.eulerAngles.z);
                }
            }
        }


        void InitBodyValue()
        {
            animBodyvalues = new BodyValues();
            animBodyvalues.value0 = 1f;
            animBodyvalues.value1 = 0f;
            animBodyvalues.value2 = 0f;
            animBodyvalues.value3 = 0f;

            anim.SetFloat("BodyValue0", animBodyvalues.value0);
            anim.SetFloat("BodyValue1", animBodyvalues.value1);
            anim.SetFloat("BodyValue2", animBodyvalues.value2);
            anim.SetFloat("BodyValue3", animBodyvalues.value3);
        }

        public void SetMovementSpeed()
        {
            if (anim == null)
            {
                return;
            }

            switch (p_state)
            {
                case STATE.IDLE:
                if(speed < 0.1f)
                {
                    speed = 0f;
                }
                else
                {
                    speed = Mathf.Lerp(speed, 0, Second * smooth);
                }
                break;

                case STATE.WALK:
                speed = Mathf.Lerp(speed, 0.5f, Second * smooth);
                break;

                case STATE.JOG:
                speed = Mathf.Lerp(speed, 1f, Second * smooth);
                break;

                case STATE.RUN:
                speed = Mathf.Lerp(speed, 2f, Second * smooth);
                break;
            }

            speed = Mathf.Round(speed * 100) * 0.01f;
            anim.SetFloat("MovementSpeed", speed);
        }

        void ChangeState()
        {
            if(v == 0 && h == 0 && 
                p_state != STATE.IDLE && isGrounded 
                && p_state != STATE.SKILL)
            {
                p_state = STATE.IDLE;
            }

            if(p_state == STATE.RUN &&
               (h != 0f && v <= 0.9f))
            {
                p_state = STATE.JOG;
            }

            else if(p_state == STATE.IDLE && 
                (v != 0 || h != 0))
            {
                if(walk)
                {
                    p_state = STATE.WALK;
                }
                else
                {
                    p_state = STATE.JOG;
                }
            }

        }

        public void SetPlayerState()
        {
            if (anim == null)
            {
                return;
            }

            anim.SetBool("isGrounded", isGrounded);
            anim.SetFloat("yVelocity", yVelocity);

            if(p_state != STATE.ROLL || p_state != STATE.SKILL)
            {
                ChangeState();
            }

            switch(p_Wstate)
            {
                default:
                case WEAPON_STATE.MAIN_WEAPON:
                anim.SetInteger("WeaponState", 1);
                switch(p_Sstate)
                {
                    case STAND_STATE.STAND:
                    if(anim.GetBool("isCrouched"))
                    {
                        anim.SetBool("isCrouched", false);
                    }

                    LerpingState("BodyValue0", 1f, ref animBodyvalues.value0);
                    LerpingState("BodyValue1", 0f, ref animBodyvalues.value1);
                    LerpingState("BodyValue2", 0f, ref animBodyvalues.value2);
                    LerpingState("BodyValue3", 0f, ref animBodyvalues.value3);
                    break;

                    case STAND_STATE.CROUCH:
                    if(!anim.GetBool("isCrouched"))
                    {
                        anim.SetBool("isCrouched", true);
                    }
                    LerpingState("BodyValue0", 0f, ref animBodyvalues.value0);
                    LerpingState("BodyValue1", 1f, ref animBodyvalues.value1);
                    LerpingState("BodyValue2", 0f, ref animBodyvalues.value2);
                    LerpingState("BodyValue3", 0f, ref animBodyvalues.value3);
                    break;

                }
                break;

                case WEAPON_STATE.SUB_WEAPON:
                anim.SetInteger("WeaponState", 2);
                switch(p_Sstate)
                {
                    case STAND_STATE.STAND:
                    if(anim.GetBool("isCrouched"))
                    {
                        anim.SetBool("isCrouched", false);
                    }
                    LerpingState("BodyValue0", 0f, ref animBodyvalues.value0);
                    LerpingState("BodyValue1", 0f, ref animBodyvalues.value1);
                    LerpingState("BodyValue2", 1f, ref animBodyvalues.value2);
                    LerpingState("BodyValue3", 0f, ref animBodyvalues.value3);
                    break;

                    case STAND_STATE.CROUCH:
                    if(!anim.GetBool("isCrouched"))
                    {
                        anim.SetBool("isCrouched", true);
                    }
                    LerpingState("BodyValue0", 0f, ref animBodyvalues.value0);
                    LerpingState("BodyValue1", 0f, ref animBodyvalues.value1);
                    LerpingState("BodyValue2", 0f, ref animBodyvalues.value2);
                    LerpingState("BodyValue3", 1f, ref animBodyvalues.value3);
                    break;

                }
                break;
            }
            //anim.SetFloat("C_State", (float)p_state);
        }

        void LerpingState(string prameter, float Goalvalue, ref float value)
        {

            if(Goalvalue == value)
                return;

            value = Mathf.Lerp(value, Goalvalue, Second * smooth);
            value = Mathf.Round(value * 100) * 0.01f;

            anim.SetFloat(prameter, value);
        }


        Ray[] initRay()
        {
            Ray[] modify_groundRay = new Ray[5];

            // 시작점은 캐릭터랑 충돌된 땅의 점에서 약간 위로 올린 지점.
            modify_groundRay[0].origin = transform.position + (Vector3.up * 0.05f);
            modify_groundRay[1].origin = transform.position + (transform.forward * (body.radius + 0.02f)) + (Vector3.up * 0.05f);
            modify_groundRay[2].origin = transform.position - (transform.forward * (body.radius + 0.02f)) + (Vector3.up * 0.05f);
            modify_groundRay[3].origin = transform.position + (transform.right * (body.radius + 0.02f)) + (Vector3.up * 0.05f);
            modify_groundRay[4].origin = transform.position - (transform.right * (body.radius + 0.02f)) + (Vector3.up * 0.05f);

            for(int i=0; i<5; ++i)
            {
                modify_groundRay[i].direction = Vector3.down;
            }

            return modify_groundRay;
        }

        public void CheckGround()
        {
            if (body == null)
            {
                return;
            }

            Ray[] modify = initRay();

            RaycastHit hit;
            int hitcount = 0;

            for (int i = 0; i < 5; ++i)
            {
                Debug.DrawRay(modify[i].origin, modify[i].direction * 0.3f, Color.red);
                if (Physics.Raycast(modify[i], out hit, 0.3f))
                {
                    ++hitcount;

                    if (i == 0)
                    {
                        // 캐릭터 중심으로 레이를 쏴 현재 밟는 땅의 경사 각도를 알아낸다.
                        groundslopeAngle = Vector3.Angle(transform.up, hit.normal);
                        // 캐릭터 컨트롤러의 경사 제한보다 크다면...
                        if(groundslopeAngle >= body.slopeLimit)
                        {
                            // 외적을 이용하여 법선 벡터를 구한다. (캐릭터의 x축을 지나는 벡터가 구해진다.)
                            Vector3 groundCross = Vector3.Cross(hit.normal, Vector3.up);
                            // 그 법선벡터 (x축)와 충돌한 땅의 노멀벡터와 수직인 외적 법선 벡터를 구해서 떨어져야 할 각도를 구해낸다. 
                            // 캐릭터의 윗방향 수직인 벡터와 외적으로 구해낸 경사각까지의 각도를 fallDirection에 대입.
                            fallDirection.rotation = Quaternion.FromToRotation(transform.up, Vector3.Cross(groundCross, hit.normal));

                            Debug.DrawRay(transform.position, -fallDirection.up, Color.yellow);
                            return;
                        }

                        else
                        {
                            fallDirection.eulerAngles = Vector3.zero;
                        }
                    }

                    if (groundslopeAngle <= body.slopeLimit && body.isGrounded && hitcount > 0)
                    {
                        isGrounded = true;
                        yVelocity = gravityY * gravityMultiplexer * Second;
                    }
                }
            }


            if (hitcount == 0)
            {
                isGrounded = false;
            }
        }

        public void CheckSlope()
        {
            if (isJump)
                return;

            RaycastHit hit;

            Debug.DrawRay(
                transform.position,
                transform.forward * 2f, Color.cyan);

            if(Physics.Raycast(transform.position,
                               transform.forward, out hit, body.radius * 2f))
            {
                Debug.DrawRay(hit.point, hit.normal * 2f, Color.green);
                slopeAngle = 90f - (Mathf.Acos(Vector3.Dot(transform.forward, hit.normal)) * Mathf.Rad2Deg);

                float slope = Mathf.Cos(Mathf.PI * 0.5f - Mathf.Acos(Vector3.Dot(-transform.forward, hit.normal)));
                //SlopeSliding();
            }
        }

        //void SlopeSliding()
        //{
        //    if (body == null)
        //    {
        //        return;
        //    }

        //    slopeRay.origin = collisionPoint + (Vector3.up * 0.05f);
        //    slopeRay.direction = Vector3.down;
    
        //    RaycastHit slopeHit;

        //    if(Physics.Raycast(slopeRay, out slopeHit, 0.55f))
        //    {
        //        slideAngle = Vector3.Angle(transform.up, slopeHit.normal);
        //        if (slideAngle >= body.slopeLimit)
        //        {
        //            Vector3 groundCross = Vector3.Cross(slopeHit.normal, Vector3.up);
        //            fallDirection.rotation = Quaternion.FromToRotation(transform.up, Vector3.Cross(groundCross, slopeHit.normal));
        //        }
        //    }

        //    //if (Physics.Raycast(slopeRay, out slopeHit, 0.55f))
        //    //{
        //    //    slopeAngle = Vector3.Angle(transform.up, slopeHit.normal);
        //    //    if (slopeAngle < body.slopeLimit)
        //    //    {
        //    //        // 속도 감소
        //    //        //SpeedMult = Mathf.Cos(Mathf.PI * 0.5f - Mathf.Acos(Vector3.Dot(-transform.forward, SlopeHit.normal)));
        //    //        fallDirection.eulerAngles = Vector3.zero;
        //    //    }
        //    //    // 요거 수정필요
        //    //    else
        //    //    {
        //    //        // 떨어지기 시작하는 위치 조정
        //    //        float groundDistance = Vector3.Distance(slopeRay.origin, slopeHit.point);
        //    //        if (groundDistance <= 0.1f)
        //    //        {
        //    //            //SpeedMult = 1 / Mathf.Cos((90 - SlopeAngle) * Mathf.Deg2Rad);

        //    //            Vector3 groundCross = Vector3.Cross(slopeHit.normal, Vector3.up);
        //    //            fallDirection.rotation = Quaternion.FromToRotation(transform.up, Vector3.Cross(groundCross, slopeHit.normal));

        //    //            //
        //    //        }
        //    //    }
        //    //}

        //    //else
        //    //{
        //    //    fallDirection.eulerAngles = Vector3.zero;
        //    //}
        //}

        void normalizeDiagonal(ref Vector3 movement, float power)
        {
            if(movement.magnitude > power)
            {
                float ratio = power / movement.magnitude;
                movement.x *= ratio;
                movement.z *= ratio;
            }
            //Debug.LogFormat("{0}", movement.magnitude);
        }

        float h, v, r_h, r_v;

        // 캐릭터 컨트롤러 move 함수를 이용하여 움직이는 함수. 캐릭터의 벡터를 정의
        public void Player_Move()
        {
            r_h = Input.GetAxisRaw("Horizontal");
            r_v = Input.GetAxisRaw("Vertical");

            if(p_state != STATE.ROLL &&
                p_state != STATE.REVIVE)
            {
                h = Input.GetAxis("Horizontal");
                v = Input.GetAxis("Vertical");

                moveVec = new Vector3(h, 0, v);

                anim.SetFloat("MovementX", moveVec.x);
                anim.SetFloat("MovementZ", moveVec.z);

                anim.SetFloat("RollX", r_h);
                anim.SetFloat("RollZ", r_v);
                normalizeDiagonal(ref moveVec, 1f);
            }

            //Debug.Log(yVelocity);

            switch(p_Sstate)
            {
                case STAND_STATE.CROUCH:
                moveMethod(0.5f);
                break;

                default:
                moveMethod(1f);
                break;
            }

            //Debug.Log(moveVec.y);
        }


        bool fallflag = false;
        bool landflag = false;


        void FallUpdate()
        {
            if(!isGrounded)
            {
                if(!fallflag)
                {
                    if(!isJump)
                    {
                        yVelocity = 0f;
                    }

                    fallflag = true;
                    landflag = true;

                    if(p_state == STATE.ROLL)
                    {
                        force = rollDistance;
                        p_state = STATE.JOG;
                        anim.SetTrigger("JumpCancel");
                    }
                }

                if(!onGravityField && yVelocity > -40f)
                {
                    yVelocity += gravityY * Second;
                }

                if(isJump && yVelocity < 1f)
                {
                    isJump = false;
                }
            }

            else if (landflag)
            {
                SoundManager.instance.PlaySFX(CharacterAudio, "THUD_Dark_01_mono");
                anim.SetTrigger("Landing");
                p_state = STATE.IDLE;
                yVelocity = gravityY * gravityMultiplexer * Time.fixedDeltaTime;
                landflag = false;
                anim.SetBool("isJumped", isJump);
                StartCoroutine(CheckAnimationState("Jump"));
            }
        }

        void RollUpdate(float f)
        {
            if(anim.GetFloat("RollZ") > 0f)
            {
                moveVec = transform.forward * f;
            }
            else if(anim.GetFloat("RollZ") < 0f)
            {
                moveVec = -transform.forward * f;
            }

            if(anim.GetFloat("RollX") < 0f)
            {
                if(anim.GetFloat("RollZ") > 0f)
                {
                    moveVec = transform.forward * f  - transform.right * f;
                }
                else if(anim.GetFloat("RollZ") < 0f)
                {
                    moveVec = -transform.forward * f - transform.right * f;
                }
                else
                {
                    moveVec = -transform.right * f;
                }
            }

            else if(anim.GetFloat("RollX") > 0f)
            {
                if(anim.GetFloat("RollZ") > 0f)
                    moveVec = transform.forward * f + transform.right * f;

                else if(anim.GetFloat("RollZ") < 0f)
                    moveVec = -transform.forward * f + transform.right * f;
                else
                {
                    moveVec = transform.right * f;
                }
            }

            if(anim.GetFloat("RollX") == 0f && anim.GetFloat("RollZ") == 0f)    
            {
                anim.SetFloat("RollZ", 1f);
                moveVec = transform.forward * f;
            }

            normalizeDiagonal(ref moveVec, f);
        }

        float force = 0f;

        public void moveMethod(float mult)
        {
            FallUpdate();

            switch(p_state)
            {
                case STATE.IDLE:
                if(groundslopeAngle >= body.slopeLimit)
                {
                    moveVec += fallDirection.up * yVelocity;
                }
                else
                {
                    moveVec.y = yVelocity;
                }
                body.Move(moveVec * Second);
                break;

                case STATE.WALK:
                moveVec = (transform.forward * moveVec.z + transform.right * moveVec.x) * (walkSpeed * mult);
                if(groundslopeAngle >= body.slopeLimit)
                {
                    moveVec += fallDirection.up * yVelocity;
                }
                else
                {
                    moveVec.y = yVelocity;
                }
                body.Move(moveVec * Second);
                break;

                case STATE.JOG:
                moveVec = (transform.forward * moveVec.z + transform.right * moveVec.x) * (jogSpeed * mult);
                if(groundslopeAngle >= body.slopeLimit)
                {
                    moveVec += fallDirection.up * yVelocity;
                }
                else
                {
                    moveVec.y = yVelocity;
                }
                body.Move(moveVec * Second);
                break;

                case STATE.RUN:
                //if(slopeAngle <= body.slopeLimit)
                //{
                //    moveVec = (transform.forward * moveVec.z + transform.right * moveVec.x) * (runSpeed * mult);
                //}
                moveVec = (transform.forward * moveVec.z + transform.right * moveVec.x) * (runSpeed * mult);
                if(groundslopeAngle >= body.slopeLimit)
                {
                    moveVec += fallDirection.up * yVelocity;
                }
                else
                {
                    moveVec.y = yVelocity;
                }
                body.Move(moveVec * Second);
                break;

                case STATE.ROLL:
                //if(slopeAngle <= body.slopeLimit)
                //{
                //    force = Mathf.Lerp(force, 0, Second * rollReduction);
                //    RollUpdate(force);
                //}
                force = Mathf.Lerp(force, 0, Second * rollReduction);
                RollUpdate(force);
                //Debug.Log(force);
                if(groundslopeAngle >= body.slopeLimit)
                {
                    moveVec += fallDirection.up * yVelocity;
                }
                else
                {
                    moveVec.y = yVelocity;
                }
                body.Move(moveVec * Second);
                break;
            }

        }


        public void Walk()
        {
            if(p_state != STATE.ROLL)
            {
                if(!walk)
                {
                    p_state = STATE.WALK;
                    walk = true;
                }

                else if(walk)
                {
                    walk = false;
                    if(moveVec.x != 0 || moveVec.z != 0)
                        p_state = STATE.JOG;
                    else
                        p_state = STATE.IDLE;
                }
                //anim.SetFloat("C_State", (float)p_state);
            }
        }

        public void Run()
        {
            if (p_Sstate == STAND_STATE.STAND && v > 0f && p_state != STATE.ROLL
                && p_Astate != AIM_STATE.MELEE && p_Astate != AIM_STATE.GREANADE && isGrounded
                && p_Astate != AIM_STATE.ZOOM && p_Astate != AIM_STATE.SWAP && p_Astate != AIM_STATE.RELOAD &&
                p_state != STATE.SKILL)
            {
                if(p_state != STATE.RUN)
                {
                    if(p_state == STATE.WALK)
                    {
                        return;
                    }

                    else
                    {
                        p_state = STATE.RUN;
                        if (p_Astate != AIM_STATE.SKILL_READY)
                        {
                            p_Astate = AIM_STATE.NONE;
                        }
                        SoundManager.instance.PlaySFX(CharacterAudio, "MAGIC_SPELL_Flame_02_mono");
                    }
                }

                else
                {
                    if(moveVec.x != 0 || moveVec.z != 0)
                    {
                        p_state = STATE.JOG;
                    }
                    else
                        p_state = STATE.IDLE;
                }
               // anim.SetFloat("C_State", (float)p_state);
            }
        }

        public void Aiming()
        {
            if(p_state != STATE.ROLL &&
                p_Astate != AIM_STATE.RELOAD &&
                p_state != STATE.SKILL &&
                p_Astate != AIM_STATE.SKILL_READY &&
                p_Astate != AIM_STATE.SWAP &&
                p_Astate != AIM_STATE.ZOOM)
            {
                if(p_Astate != AIM_STATE.AIMING)
                {
                    if(p_state != STATE.WALK)
                    {
                        p_state = STATE.JOG;
                    }
                    p_Astate = AIM_STATE.AIMING;
                    characterIK.rh_switch = true;
                    characterIK.lh_switch = true;
                }
                else
                {
                    p_Astate = AIM_STATE.NONE;
                }
            }
        }

        public void Zoom()
        {
            if(p_state != STATE.ROLL &&
                p_Astate != AIM_STATE.RELOAD &&
                p_Astate != AIM_STATE.SKILL_READY 
                && p_Astate != AIM_STATE.ZOOM)
            {
                Walk();
                p_Astate = AIM_STATE.ZOOM;
            }

        }

        public void Crouch()
        {
            if(p_state != STATE.ROLL)
            {
                if(p_Sstate != STAND_STATE.CROUCH)
                {
                    p_Sstate = STAND_STATE.CROUCH;
                    p_state = STATE.JOG;
                    //cameraFollowFunc.PivotCrouchChange();
                    body.center = new Vector3(body.center.x, body.center.y - 0.3f, body.center.z);
                    body.height -= 0.6f;
                }
                else
                {
                    p_Sstate = STAND_STATE.STAND;
                    //cameraFollowFunc.PivotStandChange();  
                    body.center = new Vector3(body.center.x, body.center.y + 0.3f, body.center.z);
                    body.height += 0.6f;
                }
            }
        }

        public void Jump()
        {
            if(!fallflag && isGrounded
                && p_state != STATE.ROLL && groundslopeAngle < body.slopeLimit)   
            {
                switch(p_Sstate)
                {
                    case STAND_STATE.CROUCH:
                    //Crouch();
                    p_Sstate = STAND_STATE.STAND;
                    //cameraFollowFunc.PivotStandChange();
                    body.center = new Vector3(body.center.x, body.center.y + 0.3f, body.center.z);
                    body.height += 0.6f;
                    break;

                    case STAND_STATE.STAND:
                    if(p_state == STATE.RUN)
                    {
                        p_state = STATE.JOG;
                    }
                    yVelocity = JumpPower;
                    anim.SetTrigger("Jumping");
                    SoundManager.instance.PlaySFX(CharacterAudio, "VOICE_Martial_Art_Male_B_Shout_01_mono");
                    isGrounded = false;
                    isJump = true;
                    anim.SetBool("isJumped", isJump);
                    //cameraFollowFunc.PivotStandChange();

                    break;
                }
            }
        }



        public void Reload(string reloadtype)
        {
            if(p_state != STATE.SKILL && p_state != STATE.ROLL)
            {
                switch(p_Astate)
                {
                    case AIM_STATE.AIMING:
                    case AIM_STATE.NONE:
                    case AIM_STATE.ZOOM:

                    p_Astate = AIM_STATE.RELOAD;
                    p_state = STATE.JOG;

                    if (main_w == "005")
                    {
                        anim.Play("Pistol_Reload.Stand");
                    }
                    else
                    {
                        anim.SetTrigger(reloadtype);
                    }
                    StartCoroutine(CheckAnimationState("Reload"));

                    break;

                    default:
                    break;
                }
            }
        }

        GameObject bullet;
        WeaponManager weaponManager;
        public void Shoot()
        {
            if(p_state != STATE.SKILL && p_state != STATE.ROLL)
            {
                if(!walk)
                {
                    p_state = STATE.JOG;
                }

                switch(p_Sstate) // 플레이어 Fire 애니메이션
                {
                    case STAND_STATE.CROUCH:
                    switch(p_Wstate)
                    {
                        case WEAPON_STATE.MAIN_WEAPON:
                        anim.Play("Rifle_Shot.Crouch_Continuous");
                        break;

                        case WEAPON_STATE.SUB_WEAPON:
                        anim.Play("Pistol_Shot.Crouch_Continuous");
                        break;

                        case WEAPON_STATE.SPECIAL_WEAPON:
                        if(special_w == "17")
                        {
                            anim.Play("Pistol_Shot.Crouch_Continuous");
                        }
                        else
                        {
                            anim.Play("Rifle_Shot.Crouch_Continuous");
                        }
                        break;
                    }
                    break;

                    case STAND_STATE.STAND:
                    switch(p_Wstate)
                    {
                        case WEAPON_STATE.MAIN_WEAPON:
                        anim.Play("Rifle_Shot.Stand_Continuous");
                        break;

                        case WEAPON_STATE.SUB_WEAPON:
                        anim.Play("Pistol_Shot.Stand_Continuous");
                        break;

                        case WEAPON_STATE.SPECIAL_WEAPON:
                        if(special_w == "17")
                        {
                            anim.Play("Pistol_Shot.Stand_Continuous");
                        }
                        else
                        {
                            anim.Play("Rifle_Shot.Stand_Continuous");
                        }
                        break;
                    }
                    break;
                }


                switch(p_Wstate)
                {
                    case WEAPON_STATE.MAIN_WEAPON:
                    switch(weaponManager.m_MainWeapon)
                    {
                        case "001":
                        bullet = MagazinePool.Instance.Pop("shotgun_Ammo");
                        break;
                        case "002":
                        for (int i = 0; i < 2; ++i)
                        {
                            bullet = MagazinePool.Instance.Pop("shotgun_Ammo");
                            // 총알 위치
                            bullet.transform.position = RandomSphereInPoint(RightHand.transform.position);
                            bullet.transform.rotation = Quaternion.LookRotation(this.transform.forward);
                        }
                        break;

                        case "003":
                        case "004":
                        case "007":
                        case "009":
                        case "010":
                        bullet = MagazinePool.Instance.Pop("rifle_Ammo");
                        break;

                        case "005":
                        bullet = MagazinePool.Instance.Pop("pistol_Ammo");
                        break;
                    }
                    break;

                    case WEAPON_STATE.SUB_WEAPON:
                    bullet = MagazinePool.Instance.Pop("pistol_Ammo");
                    break;
                }

                if(bullet != null && weaponManager.m_MainWeapon != "002")
                {
                    // 총알 위치
                    bullet.transform.position = RandomSphereInPoint(RightHand.transform.position);
                    bullet.transform.rotation = Quaternion.LookRotation(this.transform.forward);
                }
            }
        }

        public Vector3 RandomSphereInPoint(Vector3 pos)
        {
            Vector3 getPoint = Random.onUnitSphere;
            getPoint.y = 0f;

            float r = Random.Range(0f, 0.5f);

            return (getPoint * r) + pos;
        }

        public void Melee()
        {
            if(p_state != STATE.SKILL && p_state != STATE.ROLL)
            {
                switch(p_Astate)
                {
                    case AIM_STATE.NONE:
                    case AIM_STATE.AIMING:
                    case AIM_STATE.ZOOM:
                    if(p_state == STATE.RUN)
                    {
                        p_state = STATE.JOG;
                    }

                    p_Astate = AIM_STATE.MELEE;
                    decisionObjects.Clear();
                    anim.SetTrigger("Melee");

                    StartCoroutine(MeleeScanCoroutine("Melee"));
                    StartCoroutine(CheckAnimationState("Melee"));
                    break;

                    default:
                    return; 
                }
            }
        }

        public void Grenade()
        {
            if(p_state != STATE.ROLL && p_state != STATE.SKILL)
            {
                switch(p_Astate)
                {
                    case AIM_STATE.AIMING:
                    case AIM_STATE.NONE:
                    case AIM_STATE.ZOOM:
                    if(p_state == STATE.RUN)
                    {
                        p_state = STATE.JOG;
                    }
                    p_Astate = AIM_STATE.GREANADE;

                    anim.SetTrigger("Grenade");
                    StartCoroutine(CheckAnimationState("Throw"));
                    break;

                    default:
                    return;
                }
            }
        }


        bool rollflag = true;
        
        IEnumerator RollCooltime()
        {
            yield return cooltime;
            rollflag = true;
        }

        public void Roll()
        {
            if(!rollflag)
            {
                return;
            }

            if (p_state != STATE.ROLL
                && isGrounded
                && p_state != STATE.SKILL
                && p_Astate != AIM_STATE.SKILL_READY && !fallflag)
            {
                if(p_Astate == AIM_STATE.RELOAD)
                {
                    anim.SetTrigger("Cancel");
                    p_Astate = AIM_STATE.NONE;

                    switch(p_Wstate)
                    {
                        case WEAPON_STATE.MAIN_WEAPON:
                            if (weaponManager.m_MainWeapon != "002")
                            {
                                weaponManager.m_MainWeaponObject.GetComponent<Weapon>().CancelReload();
                            }
                            break;
                        case WEAPON_STATE.SUB_WEAPON:
                            weaponManager.m_SubWeaponObject.GetComponent<Weapon>().CancelReload();
                            break;
                    }

                }


                if(p_Astate == AIM_STATE.SWAP)
                {
                    anim.SetTrigger("Cancel");
                    p_Astate = AIM_STATE.NONE;

                    switch (p_Wstate)
                    {
                        case WEAPON_STATE.MAIN_WEAPON:
                            transform.GetComponent<Player_WeaponAnimation>().SetMain();
                            transform.GetComponent<Player_WeaponAnimation>().ResetSub();
                            break;
                        case WEAPON_STATE.SUB_WEAPON:
                            transform.GetComponent<Player_WeaponAnimation>().SetSub();
                            transform.GetComponent<Player_WeaponAnimation>().ResetMain();
                            break;
                    }
                }

                rollflag = false;
                p_Astate = AIM_STATE.NONE;
                p_state = STATE.ROLL;
                if(v == 0 && h == 0 &&
                    anim.GetFloat("MovementX") == 0 &&
                    anim.GetFloat("MovementZ") == 0)
                {
                    v = 0.1f;
                    anim.SetFloat("MovementZ", 0.1f);
                }

                anim.SetTrigger("Rolling");
                StartCoroutine(CheckAnimationState("Roll"));
                //SoundManager.instance.PlaySFX(CharacterAudio, "THUD_Movement_Before_After_mono");
                SoundManager.instance.PlaySFX(CharacterAudio, "THUD_Dark_01_mono");
                //StartCoroutine(PlayRollSound());
            }
        }

        // 존재하지 않는 사운드
        //IEnumerator PlayRollSound()
        //{
        //    yield return new WaitForSeconds(0.3f);
        //    SoundManager.instance.PlaySFX(CharacterAudio, "THUD_Movement_Before_After_mono", false);

        //    yield return null;
        //}


        public void ChangeAiming()
        {
            if(p_Astate == AIM_STATE.AIMING)
            {
                mirror = !mirror;
            }
        }


        IEnumerator MeleeScanCoroutine(string Animname)
        {
            Vector3 boxCenter = headPivot.transform.position;               // 피벗 몸통 앞
            Vector3 boxHalfSize = new Vector3(1f, 0.5f, 1f);

            while(!anim.GetCurrentAnimatorStateInfo(1).IsTag(Animname))
            {
                yield return null;
            }

            while(anim.GetCurrentAnimatorStateInfo(1).normalizedTime < 0.5f &&
                anim.GetCurrentAnimatorStateInfo(1).normalizedTime > 0.25f)
            {
                RaycastHit[] hits = Physics.BoxCastAll(boxCenter, boxHalfSize, Vector3.up, Quaternion.identity, 3f, (1 << LayerMask.NameToLayer("EnemyHitBox")));
                yield return null;

                bool flag = false;

                foreach(var hit in hits)
                {
                    if(decisionObjects.Count == 0)
                    {
                        decisionObjects.Add(hit.collider.gameObject.GetComponent<ParentInfomation>().Parent);
                    }

                    else if(decisionObjects.Count > 0)
                    {
                        for(int i = 0; i < decisionObjects.Count; i++)
                        {
                            if(decisionObjects[i].gameObject == hit.collider.gameObject.GetComponent<ParentInfomation>().Parent)
                            {
                                flag = true;
                                break;
                            }
                        }

                        if(!flag)
                        {
                            decisionObjects.Add(hit.collider.gameObject.GetComponent<ParentInfomation>().Parent);
                        }

                        flag = false;
                    }
                }
            }

            SoundManager.instance.PlaySFX(CharacterAudio, "MARTIAL_ARTS_Kick_Punch_RR3_mono");
        }

        [SerializeField]
        [Header("근접공격 충돌한 오브젝트들")]
        List<GameObject> decisionObjects;
        RaycastHit hit;
        Vector3 shot;

        public void MeleeAttack()
        {
            for (int i = 0; i < decisionObjects.Count; i++)
            {
                GameManager.instance.Attack_Object(decisionObjects[i], GameManager.E_ObjectType.Monster, GameManager.AttackType.MainWeapon, 25, this.gameObject);
            }
        }

        //public void MeleeHitScan()
        //{
        //    Vector3 boxCenter = headPivot.transform.position;               // 피벗 몸통 앞
        //    Vector3 boxHalfSize = new Vector3(1f, 0.5f, 1f);

        //    decisionObjects.Clear();

        //    RaycastHit[] hits = Physics.BoxCastAll(boxCenter, boxHalfSize, Vector3.up, Quaternion.identity, 3f, (1 << LayerMask.NameToLayer("EnemyHitBox")));

        //    bool flag = false;

        //    foreach (var hit in hits)
        //    {
        //        if (decisionObjects.Count == 0)
        //        {
        //            decisionObjects.Add(hit.collider.gameObject.GetComponent<ParentInfomation>().Parent);
        //        }

        //        else if (decisionObjects.Count > 0)
        //        {
        //            for (int i = 0; i < decisionObjects.Count; i++)
        //            {
        //                if (decisionObjects[i].gameObject == hit.collider.gameObject.GetComponent<ParentInfomation>().Parent)
        //                {
        //                    flag = true;
        //                    break;
        //                }
        //            }

        //            if (!flag)
        //            {
        //                decisionObjects.Add(hit.collider.gameObject.GetComponent<ParentInfomation>().Parent);
        //            }

        //            flag = false;
        //        }
        //    }
        //}

        public void checkGunforward()
        {
            if (anim == null)
            {
                return;
            }


            //Debug.DrawRay(headPivot.position, headPivot.forward * 0.8f, Color.red);

            if(Physics.Raycast(headPivot.position, headPivot.forward, 0.8f, meleelayerMask))
            {
                //Debug.Log("사격불가");
                imposibleShot = true;
                characterIK.rh_switch = false;
            }

            else
            {
                if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Move") && imposibleShot)
                {
                    characterIK.rh_switch = true;
                }
                imposibleShot = false;
            }
        }

        //IEnumerator SwapWeapon()
        //{
        //    yield return new WaitForSeconds(SwapDelay);
        //    p_Astate = AIM_STATE.NONE;
        //}

        public void SubToMain()
        {
            if(p_Astate != AIM_STATE.SWAP && 
                p_Wstate != WEAPON_STATE.MAIN_WEAPON &&
                 !anim.GetBool("Skill_1ing") && 
                 p_state != STATE.SKILL &&
                 p_Astate != AIM_STATE.SKILL_READY)
            {
                p_state = STATE.JOG;
                anim.SetTrigger("MainWeaponSwap");
                p_Astate = AIM_STATE.SWAP;
                //StartCoroutine("SwapWeapon");
                StartCoroutine(CheckAnimationState("Swap"));
            }

        }
        public void MainToSub()
        {
            if(p_Astate != AIM_STATE.SWAP && 
                p_Wstate != WEAPON_STATE.SUB_WEAPON &&
                !anim.GetBool("Skill_1ing") &&
                 p_state != STATE.SKILL &&
                 p_Astate != AIM_STATE.SKILL_READY)
            {
                p_state = STATE.JOG;
                anim.SetTrigger("SubWeaponSwap");
                p_Astate = AIM_STATE.SWAP;
                //StartCoroutine("SwapWeapon");
                StartCoroutine(CheckAnimationState("Swap"));
            }
        }

        // boxcast 그리기
        //private void OnDrawGizmos()
        //{
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawRay(shot, cam.transform.forward * MeleeDistance);
        //    Gizmos.DrawWireCube(shot + cam.transform.forward * hit.distance, new Vector3(0.3f, 0.3f, 0.3f));
        //}


        public void drawRay()
        {
            if(SeeRay)
                Debug.DrawRay(headPivot.position, headPivot.transform.forward * 100f, Color.yellow);
            //Debug.DrawRay(Mozzle.position, Mozzle.forward * 100f, Color.cyan);
        }

        // 캐릭터가 땅에 닿은 곳

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            collisionPoint = hit.point;
        }



    }

}