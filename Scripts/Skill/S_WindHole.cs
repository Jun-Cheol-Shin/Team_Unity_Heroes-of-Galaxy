using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class S_WindHole : PlayableSkill
{

    Weapon Sniper;

    private void Start()
    {
        enable = true;
        anim = GetComponent<Animator>();
        weaponManager = GetComponent<WeaponManager>();
        state = GetComponent<Player_Movement>();
        Sniper = weaponManager.m_SpecialWeaponObject.GetComponent<Weapon>();

        c_time = new WaitForSeconds(cooltime);
    }

    private void Update()
    {
        // 스킬 해제
        if(state.p_Astate == AIM_STATE.ZOOM)
        {
            if(Input.GetMouseButtonDown(1) || Sniper.m_Current_Clip == 0)
            {
                state.Walk();
                state.p_state = STATE.SKILL;
                state.p_Astate = AIM_STATE.SWAP;
                anim.SetTrigger("Cancel");  
                StartCoroutine(ReverseCheck());
                SoundManager.instance.PlaySFX(state.CharacterAudio, "UI_SCI-FI_Zoom_02_stereo");

                Sniper.m_Current_Clip = Sniper.m_Reload_Clip_Value;
            }
        }
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

        anim.SetTrigger("Skill_5");
        anim.SetBool("Skill_1ing", true);
        SoundManager.instance.PlaySFX(state.CharacterAudio, "RELOAD_Pump_Dark_stereo");

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


        state.p_Wstate = WEAPON_STATE.SPECIAL_WEAPON;
        state.Zoom();
        SoundManager.instance.PlaySFX(state.CharacterAudio, "UI_SCI-FI_Zoom_01_stereo");
    }

    [ContextMenu("지건")]
    public override void Shot()
    {
        base.Shot();

        if (enable && state.isGrounded)
        {
            enable = false;
            anim.SetTrigger("SkillSwap");

            state.p_state = STATE.SKILL;
            state.p_Astate = AIM_STATE.SWAP;

            StartCoroutine(Check());
        }
    }
}
