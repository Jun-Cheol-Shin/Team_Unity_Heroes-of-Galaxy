using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Generics.Dynamics;
using Player;
using UnityEngine.Animations.Rigging;

public class GameManager : MonoBehaviour
{
    public enum E_ObjectType
    {
        None = 0,
        Character,
        Ally,
        Boss,
        DBoss,
        Monster,
        Gimmick
    }

    public enum AttackType
    {
        None = 0,
        MainWeapon,
        SubWeapon,
        SpecialWeapon,
        Grenade,
        Buff,
        Melee
    }

    public static GameManager instance;
    public bool isTimeStopped = false;  // 시간정지 체크 변수
    public int nowStageNum = 0;
    public float reviveTime = 5f;       // 캐릭터 부활에 필요한 시간값

    public List<GameObject> stageObjects = new List<GameObject>();
    public List<StageManager> stageManagers = new List<StageManager>(); // 스테이지 매니저 변수들
    public List<AttackCommand> attackCommands = new List<AttackCommand>(); // 공격명령 대기열

    public GameObject player; // 플레이어 캐릭터 참조변수
    Player_Movement playerMovement;

    float deltaTime = 0.0f; // FPS 체크용 변수
    public Text fpsText;    // FPS 출력 텍스트

    // 매니저 오브젝트들
    public CameraTopView cameraTopView;
    public CameraManager cameraManager;
    public KeyManager keyManager;
    public UIManager uiManager;

    GameObject hpbars;
    public bool endflag = false;

    // 상수 정의
    public const int HidePos = 19;
    public const int Top_Ui = 16;

    private void OnDestroy()
    {
        instance = null;
    }

    private void Awake()
    {
        instance = this;

        reviveTime = 5f;
        Application.targetFrameRate = 60;

        JsonManager.instance.Init();

        if (SettingManager.instance != null)
        {
            JsonManager.instance.characterInfo.w_mainWeapon = SettingManager.instance.weaponCode[0];
            //JsonManager.instance.characterInfo.w_mainWeapon = "011";

            for (int i = 1; i <= 3; i++)
            {
                if (SettingManager.instance.weaponCode[i] == null)
                {
                    SettingManager.instance.weaponCode[i] = "001";
                }
            }
        }
    }

    private void Start()
    {
        CutSeen_camera.instance.Start_GAME();

        player = CreateCharacter(new Vector3(0, 20, 0));

        Sub_Mission_Manager.instance.stageName = SettingManager.instance.stageName;

        if (SettingManager.instance != null)
        {
            switch (SettingManager.instance.skillCode[0])
            {
                case 2002:
                    player.GetComponent<WeaponManager>().m_SpecialWeapon.Add("017");
                    player.GetComponent<WeaponManager>().CreateWeapon("017");
                    break;

                case 2006:
                    player.GetComponent<WeaponManager>().m_SpecialWeapon.Add("018");
                    player.GetComponent<WeaponManager>().CreateWeapon("018");
                    break;

                case 2007:
                    player.GetComponent<WeaponManager>().m_SpecialWeapon.Add("019");
                    player.GetComponent<WeaponManager>().CreateWeapon("019");
                    break;
            }

            GameObject maya = GameObject.Find("Maya");
            GameObject cyclopse = GameObject.Find("Cyclopse");
            GameObject rex = GameObject.Find("Rex");

            maya.GetComponent<WeaponManager>().m_MainWeapon = SettingManager.instance.weaponCode[1];
            //maya.GetComponent<WeaponManager>().m_MainWeapon = "004";
            cyclopse.GetComponent<WeaponManager>().m_MainWeapon = SettingManager.instance.weaponCode[2];
            rex.GetComponent<WeaponManager>().m_MainWeapon = SettingManager.instance.weaponCode[3];

            maya.GetComponent<AllyController>().skill_Id = SettingManager.instance.skillCode[1] - 2010;
            cyclopse.GetComponent<AllyController>().skill_Id = SettingManager.instance.skillCode[2] - 2020;
            rex.GetComponent<AllyController>().skill_Id = SettingManager.instance.skillCode[3] - 2030;
        }

        hpbars = GameObject.Find("HpBarUI");

        switch (SettingManager.instance.stageName)
        {
            case "Varba":
                GameObject.Find("cameraParent").GetComponent<CameraFollow>().cameraRotation(155f, -0.1f);
                GameObject.Find("PlayerManager").GetComponent<Player_Manager>().isFlag = false;
                break;

            case "Dungeon":
                GameObject.Find("cameraParent").GetComponent<CameraFollow>().cameraRotation(165f, -0.1f);
                GameObject.Find("PlayerManager").GetComponent<Player_Manager>().isFlag = false;
                break;
        }

        CutSeen_camera.instance.Start_GAME2();
    }

    private void Update()
    {
        if (!endflag && player != null)
        {
            if (player.tag == "Dead")
            {
                if (AllyManager.Instance.Ally[0].tag == "Dead" &&
                    AllyManager.Instance.Ally[1].tag == "Dead" &&
                    AllyManager.Instance.Ally[2].tag == "Dead")
                {
                    endflag = true;
                    Invoke("GameOver", 3f);
                }
            }
        }

        if(attackCommands.Count != 0)
        {
            foreach(AttackCommand ac in attackCommands)
            {
                ac.m_Attack_Function(ac, ac.m_Type);
            }

            attackCommands.Clear();
        }

        //FpsCheck(fpsText); //FPS 체크
    }

    public void GameOver()
    {
        UIManager.Instance.isEndGame = true;
        KeyManager.instance.isMenu = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.SetCursor(TopCommand.instance.Nomal_cursor, Vector2.zero, CursorMode.Auto);

        SoundManager.instance.FadeOut();
        EnemyManager.Instance.gameObject.SetActive(false);
    }

    public void hpbarOff()
    {
        NpcHpBar[] npcHpBars = hpbars.GetComponentsInChildren<NpcHpBar>();

        for (int i = 0; i < npcHpBars.Length; i++)
        {
            npcHpBars[i].hpMount = 1f;
            npcHpBars[i].spMount = 1f;
            npcHpBars[i].activeCount = 0f;
            npcHpBars[i].active = false;
            npcHpBars[i].gameObject.SetActive(false);
        }
    }

    // 게임 시간 진행
    public void ContinueTime()
    {
        isTimeStopped = false;
        UnityEngine.Time.timeScale = 1f;
        player.GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    // 게임 시간 정지
    public void StopTime()
    {
        isTimeStopped = true;
        UnityEngine.Time.timeScale = 0.000000000001f;
        player.GetComponent<Animator>().updateMode = AnimatorUpdateMode.Normal;
    }

    public bool Attack_Object(GameObject target, E_ObjectType targetType, AttackType attackType, int Damage, GameObject attacker)
    {
        AttackCommand command = new AttackCommand();
        command.m_Attack_Function = new AttackCommand.AttackDelegate(GiveDamage);
        command.m_Attacker = attacker;
        command.m_Target = target;
        command.attackType = attackType;
        command.m_Type = targetType;
        command.m_Damage = Damage;

        attackCommands.Add(command);
        return true;
    }

    public void GiveDamage(AttackCommand command, E_ObjectType targetType)
    {
        switch(targetType)
        {
            case E_ObjectType.Character:
                {
                    if(playerMovement && playerMovement.p_state == STATE.ROLL)
                    {
                        return;
                    }

                    command.m_Target.GetComponent<CharacterAttributes>().TakeDamage(command.m_Damage);

                    if (!SkillManager.Instance.player.GetComponent<Player_Movement>().CharacterAudio.isPlaying)
                    {
                        SoundManager.instance.PlaySFX(SkillManager.Instance.player.GetComponent<Player_Movement>().CharacterAudio,
                            "VOICE_Martial_Art_Shout_02_mono");
                    }
                }
                break;

            case E_ObjectType.Ally:
                command.m_Target.GetComponent<AllyController>().GetDamage(command.m_Damage, command.m_Attacker);
                break;

            case E_ObjectType.Boss:
                command.m_Target.GetComponent<BossController>().GetDamage(command.m_Damage, command.m_Attacker);
                break;
            case E_ObjectType.DBoss:
                command.m_Target.GetComponent<DBossController>().GetDamage(command.m_Damage, command.m_Attacker);
                break;

            case E_ObjectType.Monster:
                {
                    // 플레이어 또는 아군이 데미지를 줬을 경우 퍽에 따른 버프 적용
                    
                    

                    Buff buffTemp = null;
                    int addDamage = 0;

                    // 플레이어
                    if (command.m_Attacker.GetComponent<CharacterAttributes>() != null)
                    {
                        CharacterAttributes characterAttributes = command.m_Attacker.GetComponent<CharacterAttributes>();

                        if (command.attackType == AttackType.MainWeapon || command.attackType == AttackType.SubWeapon)
                        {
                            if (command.attackType == AttackType.MainWeapon && characterAttributes.weaponManager.m_MainWeapon == "009" /*&& characterAttributes.SearchBuff("Party Mood Maker Cooltime") == null*/) // 파티 분위기 메이커 (스턴)
                            {
                                int stunChance = Random.Range(1, 10);

                                if (stunChance == 1)
                                {
                                    command.m_Target.GetComponent<AIAttributte>().AddBuff("Stun", command.m_Attacker);

                                    /*characterAttributes.AddBuff("Party Mood Maker Cooltime", characterAttributes.gameObject);*/
                                }
                            }

                            if (characterAttributes.SearchPerk(2001) != null) // 방사능 디버프
                            {
                                command.m_Target.GetComponent<AIAttributte>().AddBuff("Radiation", command.m_Attacker);
                            }

                            if (characterAttributes.SearchPerk(2005) != null) // 대구경탄(스턴)
                            {
                                command.m_Target.GetComponent<AIAttributte>().AddBuff("Stun", command.m_Attacker);
                            }

                            if (characterAttributes.SearchPerk(2006) != null) // 소이탄(발화)
                            {
                                command.m_Target.GetComponent<AIAttributte>().AddBuff("Burn", command.m_Attacker);
                            }

                            // 스위치탄
                            buffTemp = characterAttributes.SearchBuff("Switch ON");
                            if (buffTemp != null) 
                            {
                                addDamage += buffTemp.value[buffTemp.currentStack - 1];
                                buffTemp = null;
                            }

                            // 위협적인 광기
                            buffTemp = characterAttributes.SearchBuff("Maniacing Madness");
                            if (buffTemp != null)
                            {
                                addDamage += buffTemp.value[buffTemp.currentStack - 1];
                                buffTemp = null;
                            }

                            // 키클롭스 데미지 2배 장판버프
                            buffTemp = characterAttributes.SearchBuff("Damage Buff");
                            if (buffTemp != null)
                            {
                                addDamage += command.m_Damage;
                                buffTemp = null;
                            }

                            if (addDamage != 0)
                            {
                                if (characterAttributes.SearchPerk(2003) != null) // 쉴드 드레인
                                {
                                    characterAttributes.DrainShield(command.m_Damage + addDamage);
                                }

                                if (characterAttributes.isDrainHealth) // HP 드레인
                                {
                                    characterAttributes.isDrainHealth = false;
                                    characterAttributes.DrainHealth(command.m_Damage + addDamage);
                                    characterAttributes.AddBuff("Drain Health Cooltime", characterAttributes.gameObject);
                                }

                                command.m_Target.GetComponent<EnemyController>().GetDamage(command.m_Damage + addDamage, command.m_Attacker, command.attackType);
                                break;
                            }
                            else
                            {
                                if (characterAttributes.SearchPerk(2003) != null) // 쉴드 드레인
                                {
                                    characterAttributes.DrainShield(command.m_Damage);
                                }

                                if (characterAttributes.isDrainHealth) // HP 드레인
                                {
                                    characterAttributes.isDrainHealth = false;
                                    characterAttributes.DrainHealth(command.m_Damage);
                                    characterAttributes.AddBuff("Drain Health Cooltime", characterAttributes.gameObject);
                                }
                            }
                        }
                    }

                    // 아군 동료
                    else if (command.m_Attacker.GetComponent<AIAttributte>() != null)
                    {
                        AIAttributte aIAttributte = command.m_Attacker.GetComponent<AIAttributte>();

                        if (command.attackType == AttackType.MainWeapon || command.attackType == AttackType.SubWeapon)
                        {
                            if (command.attackType == AttackType.MainWeapon && aIAttributte.GetComponent<WeaponManager>().m_MainWeapon == "009" /*&& aIAttributte.SearchBuff("Party Mood Maker Cooltime") == null*/) // 파티 분위기 메이커 (스턴)
                            {
                                int stunChance = Random.Range(1, 10);

                                if (stunChance == 1)
                                {
                                    command.m_Target.GetComponent<AIAttributte>().AddBuff("Stun", command.m_Attacker);

                                    /*aIAttributte.AddBuff("Party Mood Maker Cooltime", aIAttributte.gameObject);*/
                                }
                            }

                            if (aIAttributte.SearchPerk(2001) != null) // 방사능 디버프
                            {
                                command.m_Target.GetComponent<AIAttributte>().AddBuff("Radiation", command.m_Attacker);
                            }

                            if (aIAttributte.SearchPerk(2005) != null) // 대구경탄(스턴)
                            {
                                command.m_Target.GetComponent<AIAttributte>().AddBuff("Stun", command.m_Attacker);
                            }

                            if (aIAttributte.SearchPerk(2006) != null) // 소이탄(발화)
                            {
                                command.m_Target.GetComponent<AIAttributte>().AddBuff("Burn", command.m_Attacker);
                            }

                            // 스위치탄
                            buffTemp = aIAttributte.SearchBuff("Switch ON");
                            if (buffTemp != null)
                            {
                                addDamage += buffTemp.value[buffTemp.currentStack - 1];
                                buffTemp = null;
                            }

                            // 키클롭스 데미지 2배 장판버프
                            buffTemp = aIAttributte.SearchBuff("Damage Buff");
                            if (buffTemp != null)
                            {
                                addDamage += command.m_Damage;
                                buffTemp = null;
                            }

                            if (addDamage != 0)
                            {
                                if (aIAttributte.SearchPerk(2003) != null) // 쉴드 드레인
                                {
                                    aIAttributte.DrainShield(command.m_Damage + addDamage);
                                }

                                if (aIAttributte.isDrainHealth) // HP 드레인
                                {
                                    aIAttributte.isDrainHealth = false;
                                    aIAttributte.DrainHealth(command.m_Damage + addDamage);
                                    aIAttributte.AddBuff("Drain Health Cooltime", aIAttributte.gameObject);
                                }

                                command.m_Target.GetComponent<EnemyController>().GetDamage(command.m_Damage + addDamage, command.m_Attacker, command.attackType);
                                break;
                            }
                            else
                            {
                                if (aIAttributte.SearchPerk(2003) != null) // 쉴드 드레인
                                {
                                    aIAttributte.DrainShield(command.m_Damage);
                                }

                                if (aIAttributte.isDrainHealth) // HP 드레인
                                {
                                    aIAttributte.isDrainHealth = false;
                                    aIAttributte.DrainHealth(command.m_Damage);
                                    aIAttributte.AddBuff("Drain Health Cooltime", aIAttributte.gameObject);
                                }
                            }
                        }
                    }

                    command.m_Target.GetComponent<EnemyController>().GetDamage(command.m_Damage, command.m_Attacker, command.attackType);
                }
                break;

            case E_ObjectType.Gimmick:
                command.m_Target.GetComponent<GimmickObject>().GetDamage(command.m_Damage, command.m_Attacker);
                break;
        }
    }

    public void FpsCheck(Text fpsText)
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        fpsText.text = text;
    }

    GameObject CreateCharacter(Vector3 Spawnpos)
    {
        GameObject obj = Instantiate(Resources.Load("Models/Character/Character") as GameObject);

        switch (SettingManager.instance.stageName)
        {
            case "Varba":
                obj.transform.position = new Vector3(80f, -13.4f, 243f);
                //obj.transform.rotation = Quaternion.Euler(new Vector3(0f, 155f, 0f));
                break;

            case "Dungeon":
                obj.transform.position = new Vector3(-1.5f, 1f, 2.3f);
                //obj.transform.rotation = Quaternion.Euler(new Vector3(0f, -180f, 0f));
                break;
        }

        // 1번 오디오 소스
        AudioSource objAudio = obj.AddComponent<AudioSource>();
        objAudio.playOnAwake = false;
        objAudio.spatialBlend = 1f;
        objAudio.dopplerLevel = 1f;
        objAudio.rolloffMode = AudioRolloffMode.Linear;
        objAudio.minDistance = 3f;
        objAudio.maxDistance = 40f;

        // 2번 애니메이터
        Avatar objAvatar = Resources.Load("Character/CharactersAvatar") as Avatar;
        RuntimeAnimatorController objController = Resources.Load("Character/PlayerAnimator_Male") as RuntimeAnimatorController;
        Animator objAnim = obj.AddComponent<Animator>();

        objAnim.avatar = objAvatar;
        obj.GetComponent<Animator>().runtimeAnimatorController = objController;
        objAnim.updateMode = AnimatorUpdateMode.UnscaledTime;
        objAnim.applyRootMotion = true;
        objAnim.cullingMode = AnimatorCullingMode.CullUpdateTransforms;

        // 3번 캐릭터 컨트롤러
        CharacterController objcon = obj.AddComponent<CharacterController>();
        objcon.slopeLimit = JsonManager.instance.characterInfo.cc_slopeLimit;
        objcon.stepOffset = JsonManager.instance.characterInfo.cc_stepOffset;
        objcon.skinWidth = JsonManager.instance.characterInfo.cc_skinWidth;
        objcon.minMoveDistance = JsonManager.instance.characterInfo.cc_minMoveDistance;
        objcon.center = JsonManager.instance.characterInfo.cc_center;
        objcon.radius = JsonManager.instance.characterInfo.cc_radius;
        objcon.height = JsonManager.instance.characterInfo.cc_height;

        // 4번 스크립트 Player_Movement
        Player_Movement objMove = obj.AddComponent<Player_Movement>();
        objMove.smooth = JsonManager.instance.characterInfo.p_smooth;
        objMove.onGravityField = JsonManager.instance.characterInfo.p_onGravityField;
        objMove.rollDistance = JsonManager.instance.characterInfo.p_rollDistance;
        objMove.rollReduction = JsonManager.instance.characterInfo.p_rollReduction;
        objMove.JumpPower = JsonManager.instance.characterInfo.p_jumpPower;
        objMove.SwapDelay = JsonManager.instance.characterInfo.p_swapDelay;
        objMove.walkSpeed = JsonManager.instance.characterInfo.p_walkSpeed;
        objMove.jogSpeed = JsonManager.instance.characterInfo.p_jogSpeed;
        objMove.runSpeed = JsonManager.instance.characterInfo.p_runSpeed;
        objMove.MeleeDistance = JsonManager.instance.characterInfo.p_meleeDistance;
        objMove.RotateturnSpeed = JsonManager.instance.characterInfo.p_rotateTurnSpeed;
        objMove.SpineBody = obj.transform.GetChild(0).GetChild(0).gameObject;
        playerMovement = objMove;

        GameObject objManager = new GameObject("PlayerManager");
        objManager.transform.SetParent(obj.transform);
        objManager.transform.localPosition = Vector3.zero;

        objMove.fallDirection = objManager.transform;
        objMove.headPivot = obj.GetComponent<BodyInfomation>().Head.transform;
        objMove.LeftHand = obj.GetComponent<BodyInfomation>().Hand1.transform.GetChild(0).gameObject;
        objMove.RightHand = obj.GetComponent<BodyInfomation>().Hand2.transform.GetChild(4).gameObject;

        GameObject CameraPivot = new GameObject("CameraPivot");
        CameraPivot.transform.SetParent(obj.transform);
        CameraPivot.transform.localPosition = JsonManager.instance.characterInfo.p_pivot;

        GameObject CameraTarget = new GameObject("CameraTarget");
        CameraTarget.transform.SetParent(CameraPivot.transform);
        CameraTarget.transform.localPosition = JsonManager.instance.characterInfo.p_target;

        GameObject Grenade = new GameObject("Grenade");
        Grenade.transform.SetParent(obj.transform);
        Grenade.transform.localPosition = Vector3.zero;

        GameObject fixedGrenade = new GameObject("FixedAngle");
        fixedGrenade.transform.SetParent(Grenade.transform);
        fixedGrenade.transform.localPosition = Vector3.zero;

        //Player_Manager objPlayerManager = objManager.AddComponent<Player_Manager>();
        //objPlayerManager.Character = obj;

        // 6번 스크립트 WeaponManager
        WeaponManager objWeapon = obj.AddComponent<WeaponManager>();
        objWeapon.m_MainWeapon = JsonManager.instance.characterInfo.w_mainWeapon;
        objWeapon.m_SubWeapon = JsonManager.instance.characterInfo.w_subWeapon;

        objWeapon.m_WeaponObject = obj.GetComponent<BodyInfomation>().Hand2.transform.GetChild(0).gameObject;
        objWeapon.m_AudioSource = objAudio;
        objWeapon.playerMovement = objMove;
        objWeapon.anim = obj.GetComponent<Animator>();
        objWeapon.m_Attacker = obj;
        objWeapon.species = JWeaponOffset.SPECIES.HUMAN;

        // 7번 스크립트 Weapon_Animation
        Player_WeaponAnimation objWanim = obj.AddComponent<Player_WeaponAnimation>();
        objWanim.Pose_MainWeapon = obj.GetComponent<BodyInfomation>().Body.transform.GetChild(0).gameObject;
        objWanim.Pose_SubWeapon = obj.transform.GetChild(0).GetChild(0).GetChild(2).GetChild(0).gameObject;
        objWanim.type1 = obj.GetComponent<BodyInfomation>().Hand2.transform.GetChild(0).GetChild(0).gameObject;
        objWanim.type2 = obj.GetComponent<BodyInfomation>().Hand2.transform.GetChild(0).GetChild(1).gameObject;

        // 8번 스크립트 player_IK
        Player_IK objHand = obj.AddComponent<Player_IK>();
        objHand.smoothDamp = JsonManager.instance.characterInfo.ik_smoothDamp;
        objHand.weaponManager = objWeapon;
        objHand.First_Weapon = objWanim.type1;
        objHand.Second_Weapon = objWanim.type2;
        objHand.Third_Weapon = obj.GetComponent<BodyInfomation>().Hand2.transform.GetChild(0).GetChild(2).gameObject;

        obj.GetComponent<HumanoidVerticalLegPlacement>().CameraPivot = CameraPivot.transform;
        obj.GetComponent<HumanoidVerticalLegPlacement>().LayerMask = LayerMask.GetMask("Map");

        // 10번 스크립트 Granade
        Grenade objG = Grenade.AddComponent<Grenade>();
        objG.m_CurrentClip = JsonManager.instance.characterInfo.g_currentClip;
        objG.m_ThrowPower = JsonManager.instance.characterInfo.g_throwPower;
        objG.m_Explosion_Time = JsonManager.instance.characterInfo.g_explosionTime;
        objG.m_Explosion_Distance = JsonManager.instance.characterInfo.g_explosionDistance;
        objG.m_ThrowRate = JsonManager.instance.characterInfo.g_throwRate;
        objG.m_Damage = JsonManager.instance.characterInfo.g_damage;
        objG.m_Bounce_Count = JsonManager.instance.characterInfo.g_bounceCount;
        objG.isThrow = JsonManager.instance.characterInfo.g_isThrow;
        objG.isSticky = JsonManager.instance.characterInfo.g_isSticky;
        objG.isAir = JsonManager.instance.characterInfo.g_isAir;
        objG.isPenetrate = JsonManager.instance.characterInfo.g_isPenetrate;
        objG.isStun = JsonManager.instance.characterInfo.g_isStun;
        objG.isRadiation = JsonManager.instance.characterInfo.g_isRadiation;
        objG.isSlow = JsonManager.instance.characterInfo.g_isSlow;
        objG.isDouble = JsonManager.instance.characterInfo.g_isDouble;
        objG.isAttraction = JsonManager.instance.characterInfo.g_isAttraction;
        objG.isWeakness = JsonManager.instance.characterInfo.g_isWeakness;
        objG.isAi = JsonManager.instance.characterInfo.g_isAI;
        objG.FixedAngle = fixedGrenade;
        objG.m_Muzzle = obj.GetComponent<BodyInfomation>().Hand1;

        // 11번 GrenadeManager
        GrenadeManager objGM = Grenade.AddComponent<GrenadeManager>();
        objGM.grenade = objG;
        objGM.isEnemy = JsonManager.instance.characterInfo.gm_isEnemy;
        objGM.isFriend = JsonManager.instance.characterInfo.gm_isFriend;

        // 12번 스크립트 CharacterAttribute
        CharacterAttributes objAtt = obj.AddComponent<CharacterAttributes>();
        objAtt.name = JsonManager.instance.characterInfo.c_name;
        objAtt.maxHealth = JsonManager.instance.characterInfo.c_maxHealth;
        objAtt.health = JsonManager.instance.characterInfo.c_health;
        objAtt.maxShield = JsonManager.instance.characterInfo.c_maxShield;
        objAtt.shield = JsonManager.instance.characterInfo.c_shield;
        objAtt.shieldRecoverValue = JsonManager.instance.characterInfo.c_shieldRecoverValue;
        objAtt.shieldRecoverStartTime = JsonManager.instance.characterInfo.c_shieldRecoverStart;
        objAtt.shieldRecoverIntervalTime = JsonManager.instance.characterInfo.c_shieldRecoverInterval;
        objAtt.faceSprite = Resources.Load("Images/CharacterFace/" + JsonManager.instance.characterInfo.c_faceSprite, typeof(Sprite)) as Sprite;
        objAtt.animator = obj.GetComponent<Animator>();
        objAtt.weaponManager = objWeapon;
        objAtt.playerMovement = objMove;
        objAtt.grenade = objG;
        objAtt.characterIK = objHand;

        // 13번 CameraFollow
        GameObject CameraParent = new GameObject("cameraParent");
        CameraFollow camfol = CameraParent.AddComponent<CameraFollow>();
        camfol.Sensitivity = JsonManager.instance.characterInfo.cf_Sensitivity;
        camfol.clampAngle = JsonManager.instance.characterInfo.cf_ClampAngle;
        camfol.CameraPivot = CameraPivot.transform;
        camfol.Character = obj.transform;
        camfol.anim = objAnim;

        GameObject camera = new GameObject("Main Camera");
        camera.transform.SetParent(CameraParent.transform);
        camera.transform.localPosition = Vector3.zero;
        camera.tag = "MainCamera";
        var cameraComponent = camera.AddComponent<Camera>();
        cameraComponent.cullingMask = ~(1 << HidePos);
        camera.AddComponent<AudioListener>();
        camera.AddComponent<VolumetricFogAndMist.VolumetricFog>().density = 0;

        GameObject targetLook = new GameObject("targetLook");
        targetLook.transform.SetParent(camera.transform);
        targetLook.transform.localPosition = new Vector3(0, 0, 30);

        CameraCollision camcol = camera.AddComponent<CameraCollision>();
        camcol.Target = CameraTarget.transform;

        GameObject dummy = new GameObject("Dummy");
        dummy.transform.SetParent(CameraParent.transform);
        dummy.transform.localPosition = Vector3.zero;

        camcol.CameraTransform = dummy.transform;
        camcol.Character = obj;
        camcol.smooth = JsonManager.instance.characterInfo.cc_smooth;
        camcol.offset = new CameraCollision.Offset();
        camcol.offset.idle = JsonManager.instance.characterInfo.cc_idle;
        camcol.offset.Zoom = JsonManager.instance.characterInfo.cc_zoom;
        camcol.offset.Aiming = JsonManager.instance.characterInfo.cc_aiming;

        // 15번 스크립트 Player_Manager
        Player_Manager objM = objManager.AddComponent<Player_Manager>();
        objM.Character = obj;
        objM.camerafollow = camfol;
        objM.collisionCamera = camcol;

        // 카메라 생기고
        objHand.targetLook = camera.transform.GetChild(0);

        GameObject SubPos = new GameObject("SubCameraPosition");
        SubPos.transform.SetParent(obj.transform);
        SubPos.transform.localPosition = JsonManager.instance.characterInfo.sc_pos;

        // 매니저에 넣는작업
        player = obj;
        cameraTopView.TpsCamera = camera;
        cameraTopView.SubCameraposition = SubPos;
        cameraTopView.Character = obj;

        cameraManager.arrCam[0] = camera.GetComponent<Camera>();

        keyManager.weaponManager = objWeapon;
        keyManager.grenadeManager = objGM;
        keyManager.playerMovement = objMove;
        keyManager.characterAttributes = objAtt;

        uiManager.player_Attributes = objAtt;
        uiManager.playerWeapon = obj;

        // 파티클 생성
        GameObject particleObj = new GameObject("Particle");
        particleObj.transform.SetParent(obj.transform);
        particleObj.transform.localPosition = Vector3.zero;

        GameObject bleedingObj = Instantiate(Resources.Load("Prefabs/Particle/" + JsonManager.instance.characterInfo.bleeding) as GameObject, particleObj.transform);
        bleedingObj.transform.localPosition = JsonManager.instance.characterInfo.bleeding_pos;
        objAtt.bleeding = bleedingObj.GetComponent<ParticleSystem>();

        GameObject stunObj = Instantiate(Resources.Load("Prefabs/Particle/" + JsonManager.instance.characterInfo.stun) as GameObject, particleObj.transform);
        stunObj.transform.localPosition = JsonManager.instance.characterInfo.stun_pos;
        objAtt.stun = stunObj.GetComponent<ParticleSystem>();

        GameObject radiationObj = Instantiate(Resources.Load("Prefabs/Particle/" + JsonManager.instance.characterInfo.radiation) as GameObject, particleObj.transform);
        radiationObj.transform.localPosition = JsonManager.instance.characterInfo.radiation_pos;
        objAtt.radiation = radiationObj.GetComponent<ParticleSystem>();

        GameObject burnObj = Instantiate(Resources.Load("Prefabs/Particle/" + JsonManager.instance.characterInfo.burn) as GameObject, particleObj.transform);
        burnObj.transform.localPosition = JsonManager.instance.characterInfo.burn_pos;
        objAtt.burn = burnObj.GetComponent<ParticleSystem>();

        GameObject strenuousObj = Instantiate(Resources.Load("Prefabs/Particle/FX_Strenuous") as GameObject, particleObj.transform);
        strenuousObj.transform.localPosition = new Vector3(0f, 1f, 0f);
        objAtt.strenuous = strenuousObj.GetComponent<ParticleSystem>();

        GameObject drainHealthObj = Instantiate(Resources.Load("Prefabs/Particle/FX_DrainHealth") as GameObject, particleObj.transform);
        drainHealthObj.transform.localPosition = new Vector3(0f, 1f, 0f);
        objAtt.drainHealth = drainHealthObj.GetComponent<ParticleSystem>();

        GameObject healObj = Instantiate(Resources.Load("Prefabs/Particle/FX_Heal_02") as GameObject, particleObj.transform);
        healObj.transform.localPosition = Vector3.zero;
        objAtt.heal = healObj.GetComponent<ParticleSystem>();

        GameObject electricObj = Instantiate(Resources.Load("Prefabs/Particle/FX_Electricity_gun") as GameObject, particleObj.transform);
        electricObj.transform.localPosition = new Vector3(0f, 1f, 0f);
        objAtt.electric = electricObj.GetComponent<ParticleSystem>();

        GameObject poisonObj = Instantiate(Resources.Load("Prefabs/Particle/FX_Poison_Purple") as GameObject, particleObj.transform);
        poisonObj.transform.localPosition = JsonManager.instance.characterInfo.radiation_pos;
        objAtt.poison = poisonObj.GetComponent<ParticleSystem>();

        // Character Addforce
        obj.AddComponent<CharacterAddforce>();

        // WeaponManager 생성 후 생성.
        GameObject a = new GameObject("MagazinePool");
        a.transform.SetParent(this.transform.parent);
        a.AddComponent<MagazinePool>();
        a.transform.localPosition = Vector3.zero;

        return obj;
    }
}

public class AttackCommand
{
    public delegate void AttackDelegate(AttackCommand command, GameManager.E_ObjectType type);
    public AttackDelegate m_Attack_Function;

    public GameObject m_Attacker;
    public GameObject m_Target;
    public GameManager.AttackType attackType = GameManager.AttackType.None;
    public GameManager.E_ObjectType m_Type = GameManager.E_ObjectType.None;
    public int m_Damage = 0;
}