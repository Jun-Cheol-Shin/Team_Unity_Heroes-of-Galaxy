using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class S_PsykickStorm : PlayableSkill
{
    public float speed = 5f;
    public float waitTime = 2f;
    PsykickStorm Psykickstorm;
    CameraFollow cameraFollow;

    private void Start()
    {
        enable = true;
        anim = GetComponent<Animator>();
        state = GetComponent<Player_Movement>();

        c_time = new WaitForSeconds(cooltime);
        s_time = new WaitForSeconds(skilltime);

        cameraFollow = Camera.main.transform.parent.GetComponent<CameraFollow>();
    }   


    [ContextMenu("[스킬 발동]")]
    public override void Shot()
    {
        if (enable && state.isGrounded)
        {
            SoundManager.instance.PlaySFX(state.CharacterAudio, "A_PK_HD_BoosterStart");
            cameraFollow.locked = true;
            enable = false;
            anim.SetTrigger("Skill_2");
            anim.SetBool("Skill_1ing", true);

            StartCoroutine(ActivatingSkill());
            state.p_state = STATE.SKILL;
        }
    }

    protected override IEnumerator ActivatingSkill()
    {
        GameObject Storm = SkillManager.Instance.GetObject();

        if(Psykickstorm == null)
        {
            Psykickstorm = Storm.GetComponent<PsykickStorm>();
            // 폭풍 오브젝트에게 코루틴과 waitForSeconds를 건네주자.
            Psykickstorm.s_time = s_time;
            Psykickstorm.w_time = new WaitForSeconds(waitTime);
        }

        Psykickstorm.pos = state.transform.forward;
        Vector3 stormPos = new Vector3(state.transform.position.x, state.transform.position.y + Psykickstorm.BoxSize.y * 0.5f, state.transform.position.z);
        Storm.transform.position = stormPos + state.transform.forward;
        if(Psykickstorm.audioSource == null)
        {
            Psykickstorm.audioSource = Psykickstorm.GetComponent<AudioSource>();
        }
        yield return new WaitForSeconds(skilltime + waitTime);


        anim.SetBool("Skill_1ing", false);

        cameraFollow.locked = false;
        StartCoroutine(StateChange("Skill_End"));
        StartCoroutine(WaitSkillCooltime());

        yield return null;
    }

}
