using Player;
using System.Collections;
using UnityEngine;

public class S_ExplosionDash : PlayableSkill
{
    // 폭발 시간 (매 d_time마다 폭발)
    WaitForSeconds d_time;

    public float dropExplsionTime;
    public float chargingSpeed;
    bool running;
    Explosion ex;

    AudioSource exAudio;

    private void Start()
    {
        enable = true;
        running = false;
        state = GetComponent<Player_Movement>();
        anim = GetComponent<Animator>();

        dropExplsionTime = 0.3f;
        chargingSpeed = 4f;

        c_time = new WaitForSeconds(cooltime);
        s_time = new WaitForSeconds(skilltime);
        d_time = new WaitForSeconds(dropExplsionTime);

        cameraFollow = Camera.main.transform.parent.GetComponent<CameraFollow>();
        attributes = GameManager.instance.player.GetComponent<CharacterAttributes>();
    }

    void Update()
    {
        if(running)
        {
            if(!state.CharacterAudio.isPlaying)
            {
                SoundManager.instance.PlaySFX(state.CharacterAudio, "ROBOTIC_Short_Burst_37_Rapid_Shut_Down_Crop_Deep_stereo");
                SoundManager.instance.PlaySFX(state.CharacterAudio, "MAGIC_SPELL_Flame_02_mono");
            }
            state.body.Move(transform.forward * chargingSpeed * Time.deltaTime);
        }
    }


    [ContextMenu("[스킬 발동]")]
    public override void Shot()
    {
        base.Shot();

        if (enable && state.isGrounded)
        {
            SoundManager.instance.PlaySFX(state.CharacterAudio, "ROBOTIC_Short_Burst_12_Digital_Air_Lock_stereo");
            cameraFollow.locked = true;
            enable = false;
            anim.SetTrigger("Skill_1");
            anim.SetBool("Skill_1ing", true);

            StartCoroutine(ActivatingSkill());
            state.p_state = STATE.SKILL;
        }
    }

    protected override IEnumerator ActivatingSkill()
    {
        while(!anim.GetCurrentAnimatorStateInfo(0).IsTag("Skill_End"))
        {
            yield return null;
        }

        StartCoroutine(DropExplosion());
        StartCoroutine(CastingSkillTime());

        yield return null;
    }

    IEnumerator CastingSkillTime()
    {
        running = true;
        yield return s_time;

        running = false;
        anim.SetBool("Skill_1ing", false);

        StartCoroutine(StateChange("Skill_End"));
        StartCoroutine(WaitSkillCooltime());
        cameraFollow.locked = false;

        yield return null;
    }

    IEnumerator DropExplosion()
    {
        while(state.p_state == STATE.SKILL)
        {
            yield return d_time;

            if(state.p_state != STATE.SKILL)
            {
                break;
            }
            GameObject explosion = SkillManager.Instance.GetObject();
            ex = explosion.GetComponent<Explosion>();
            exAudio = ex.GetComponent<AudioSource>();
            explosion.transform.position = state.transform.position - state.transform.forward;
            StartCoroutine(ex.switchOnOff());
            ex.start = new Vector3(transform.position.x, transform.position.y + ex.BoxSize.y * 0.5f, transform.position.z);
            SoundManager.instance.PlaySFX(exAudio, "EXPLOSION_Medium_Blast_Scatter_Debris_stereo");
        }
    }

}
