using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class S_saturation : PlayableSkill
{

    Weapon MachineGun;

    private void Start()
    {
        enable = true;
        anim = GetComponent<Animator>();
        weaponManager = GetComponent<WeaponManager>();
        state = GetComponent<Player_Movement>();
        MachineGun = weaponManager.m_SpecialWeaponObject.GetComponent<Weapon>();
        s_time = new WaitForSeconds(skilltime);
        c_time = new WaitForSeconds(cooltime);
    }

    protected override IEnumerator ActivatingSkill()
    {
        while (state.characterIK.rh_Weight < 1f)
        {
            yield return null;
        }

        state.CharacterAudio.pitch = 1f;

        while(MachineGun.m_Current_Clip != 0)
        {
            MachineGun.Attack();
            yield return s_time;
        }

        state.Walk();
        state.p_state = STATE.SKILL;
        state.p_Astate = AIM_STATE.SWAP;
        anim.SetTrigger("Cancel");
        StartCoroutine(ReverseCheck());

        MachineGun.m_Current_Clip = MachineGun.m_Reload_Clip_Value;

        yield return null;
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

        // 움직이지 못하게 할때
        //state.p_state = STATE.SKILL;

        // 움직이기 가능할때 ( 런)
        //state.p_state = STATE.IDLE;

        // 움직이기 가능할때 (걷기)
        state.CharacterAudio.pitch = 0.9f;
        SoundManager.instance.PlaySFX(state.CharacterAudio, "RELOAD_Pump_Dark_stereo");
        state.Walk();
        state.p_Astate = AIM_STATE.NONE;

        state.p_Wstate = WEAPON_STATE.SPECIAL_WEAPON;

        StartCoroutine(ActivatingSkill());
    }

    [ContextMenu("지건")]
    public override void Shot()
    {
        if (enable && state.isGrounded)
        {
            SoundManager.instance.PlaySFX(state.CharacterAudio, "VOICE_MALE_HaHa_1_mono");
            enable = false;
            anim.SetTrigger("SkillSwap");

            state.p_state = STATE.SKILL;
            state.p_Astate = AIM_STATE.SWAP;

            StartCoroutine(Check());
        }
    }

}
