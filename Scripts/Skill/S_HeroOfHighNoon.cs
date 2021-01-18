using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class S_HeroOfHighNoon : PlayableSkill
{
    Weapon Revolver;
    GameObject field;
    WaitForSeconds s_timeFix;

    Coroutine StartSkill;
    ParticleSystem Skilleffect;

    private void Start()
    {
        enable = true;

        anim = GetComponent<Animator>();
        weaponManager = GetComponent<WeaponManager>();
        state = GetComponent<Player_Movement>();

        Revolver = weaponManager.m_SpecialWeaponObject.GetComponent<Weapon>();
        s_timeFix = new WaitForSeconds(skilltime * 0.5f);
        c_time = new WaitForSeconds(cooltime);
    }

    private void Update()
    {
        if(Revolver.m_Current_Clip == 0)
        {
            if(field != null)
            {
                StopCoroutine(StartSkill);

                SkillManager.Instance.InsertList(field);
                field = null;


                state.CharacterAudio.Stop();
                state.SetChangeTime(1f);
                state.p_state = STATE.SKILL;
                state.p_Astate = AIM_STATE.SWAP;
                anim.SetTrigger("NoonCancel");
                StartCoroutine(ReverseCheck());

                Revolver.m_Current_Clip = Revolver.m_Reload_Clip_Value;
            }
        }

        else if(field != null)
        {
            if (!Skilleffect)
            {
                Skilleffect = field.GetComponent<ParticleSystem>();
            }

            if (GameManager.instance.isTimeStopped && !Skilleffect.isPaused)
            {
                Skilleffect.Pause(true);
            }
            else if(!GameManager.instance.isTimeStopped && field.GetComponent<ParticleSystem>().isPaused)
            {
                Skilleffect.Pause(false);
                field.transform.position = Vector3.Lerp(field.transform.position, state.transform.position, Time.deltaTime);
            }
        }
    }

    protected override IEnumerator ActivatingSkill()
    {
        SoundManager.instance.PlaySFX(state.CharacterAudio, "AMBIENCE_Under_Water_Deep_Dark_loop_stereo");
        state.SetChangeTime(0.5f);

        field = SkillManager.Instance.GetObject();

        field.transform.position = state.transform.position;

        yield return s_timeFix;

        state.CharacterAudio.Stop();

        SkillManager.Instance.InsertList(field);
        field = null;

        //SoundManager.instance.StopSFX(state.CharacterAudio);
        state.SetChangeTime(1f);

        state.p_state = STATE.SKILL;
        state.p_Astate = AIM_STATE.SWAP;
        anim.SetTrigger("NoonCancel");
        StartCoroutine(ReverseCheck());
    }

    IEnumerator ReverseCheck()
    {
        while(!anim.GetCurrentAnimatorStateInfo(0).IsTag("Skill_End"))
        {
            //Debug.Log("아직 애니메이션으로 안감");
            yield return null;
        }


        while(anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1f)
        {
            //Debug.Log("애니메이션 실행 중");
            if(!anim.GetCurrentAnimatorStateInfo(0).IsTag("Skill_End"))
            {
                //Debug.Log("이 애니메이션이 아니라서 탈출");
                break;
            }
            yield return null;
        }

        anim.SetBool("Skill_1ing", false);

        while(!anim.GetCurrentAnimatorStateInfo(0).IsTag("Swap"))
        {
            //Debug.Log("아직 애니메이션으로 안감");
            yield return null;
        }


        while(anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1f)
        {
            //Debug.Log("애니메이션 실행 중");
            if(!anim.GetCurrentAnimatorStateInfo(0).IsTag("Swap"))
            {
                //Debug.Log("이 애니메이션이 아니라서 탈출");
                break;
            }
            yield return null;
        }
        state.p_state = STATE.IDLE;
        state.p_Astate = AIM_STATE.NONE;
        StartCoroutine(WaitSkillCooltime());
    }

    IEnumerator Check()
    {
        while(!anim.GetCurrentAnimatorStateInfo(0).IsTag("Swap"))
        {
            //Debug.Log("아직 애니메이션으로 안감");
            yield return null;
        }


        while(anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1f)
        {
            //Debug.Log("애니메이션 실행 중");
            if(!anim.GetCurrentAnimatorStateInfo(0).IsTag("Swap"))
            {
                //Debug.Log("이 애니메이션이 아니라서 탈출");
                break;
            }
            yield return null;
        }

        anim.SetTrigger("Skill_6");
        anim.SetBool("Skill_1ing", true);

        while(!anim.GetCurrentAnimatorStateInfo(0).IsTag("Skill"))
        {
            //Debug.Log("아직 애니메이션으로 안감");
            yield return null;
        }


        while(anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1f)
        {
            //Debug.Log("애니메이션 실행 중");
            if(!anim.GetCurrentAnimatorStateInfo(0).IsTag("Skill"))
            {
                //Debug.Log("이 애니메이션이 아니라서 탈출");
                break;
            }
            yield return null;
        }

        SoundManager.instance.PlaySFX(state.CharacterAudio, "RELOAD_Chamber_03_stereo");
        state.CharacterAudio.rolloffMode = AudioRolloffMode.Logarithmic;
        // 움직이지 못하게 할때
        //state.p_state = STATE.SKILL;

        // 움직이기 가능할때 ( 런)
        state.p_state = STATE.IDLE;

        // 움직이기 가능할때 (걷기)
        //state.Walk();

        state.p_Wstate = WEAPON_STATE.SPECIAL_WEAPON;
        state.p_Astate = AIM_STATE.NONE;
        StartSkill = StartCoroutine(ActivatingSkill());
    }

    [ContextMenu("지건")]
    public override void Shot()
    {
        base.Shot();

        if (enable && state.isGrounded)
        {
            SoundManager.instance.PlaySFX(state.CharacterAudio, "BoxingBell3Time");

            enable = false;
            anim.SetTrigger("SkillSwap");

            state.p_state = STATE.SKILL;
            state.p_Astate = AIM_STATE.SWAP;

            StartCoroutine(Check());
        }
    }

}
