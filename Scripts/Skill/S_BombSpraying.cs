using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using System.Diagnostics;

public class S_BombSpraying : PlayableSkill
{
    float radius = 3f;
    float distance = 10f;

    GameObject rangeobj;
    Projector skillrange;
    Trejectory traject;

    float roll;
    bool skillActivate;

    public int bombCount = 8;

    GameObject bomb;

    GameObject[] spawnBomb;
    SprayingBomb[] bombMethod;


    AudioSource audioSource;

    private void Start()
    {
        rangeobj = SkillManager.Instance.Projector;
        skillrange = rangeobj.GetComponent<Projector>();
        traject = rangeobj.GetComponent<Trejectory>();

        enable = true;
        anim = GetComponent<Animator>();
        state = GetComponent<Player_Movement>();
        weapon = GetComponent<Player_WeaponAnimation>();

        skillrange.orthographicSize = radius;

        skillActivate = false;

        s_time = new WaitForSeconds(skilltime);
        c_time = new WaitForSeconds(cooltime);

        spawnBomb = new GameObject[bombCount];
        bombMethod = new SprayingBomb[bombCount];

        bomb = SkillManager.Instance.Installation;

        for(int i=0; i<bombCount; i++)
        {
            spawnBomb[i] = Instantiate(bomb);
            bombMethod[i] = spawnBomb[i].GetComponent<SprayingBomb>();
            bombMethod[i].stime = s_time;
            spawnBomb[i].SetActive(false);
            spawnBomb[i].transform.SetParent(SkillManager.Instance.transform);
        }

        audioSource = SkillManager.Instance.Explosion.GetComponent<Explosion>().GetComponent<AudioSource>();
    }

    private void Update()
    {
        if(!enable && rangeobj.activeSelf && !skillActivate)
        {
            roll = anim.GetFloat("Roll") * 0.1f;
            rangeobj.transform.position = state.transform.position + state.transform.forward * (distance + roll);
            traject.start = new Vector3(state.transform.position.x, state.transform.position.y + 1.2f, state.transform.position.z);
            traject.target = rangeobj.transform.position;
            traject.TrejectMethod();

            if(anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.9f &&
                anim.GetCurrentAnimatorStateInfo(0).IsTag("Skill_Ready"))
            {
                if(Input.GetMouseButtonDown(0))
                {
                    skillActivate = true;
                    StartCoroutine(ActivatingSkill());
                }

                else if(Input.GetMouseButtonDown(1))
                {
                    enable = true;
                    rangeobj.SetActive(false);
                    rangeobj.transform.SetParent(SkillManager.Instance.transform);

                    weapon.DestroyGrenade();
                    anim.SetTrigger("Cancel");
                    StartCoroutine(StateChange("Swap"));
                    SoundManager.instance.PlaySFX(state.CharacterAudio, "RELOAD_Pump_Dark_stereo");
                }
            }
        }
    }

    bool click = false;

    [ContextMenu("[스킬 발동]")]
    public override void Shot()
    {
        base.Shot();

        if(click)
            return;

        click = true;

        if(!enable && rangeobj.activeSelf && traject != null && state.isGrounded)
        {
            enable = true;
            rangeobj.SetActive(false);
            rangeobj.transform.SetParent(SkillManager.Instance.transform);

            weapon.DestroyGrenade();
            anim.SetTrigger("Cancel");
            StartCoroutine(StateChange("Swap"));
            StartCoroutine(PreState());
            SoundManager.instance.PlaySFX(state.CharacterAudio, "RELOAD_Pump_Dark_stereo");
        }

        else if(enable)
        {
            state.p_state = STATE.SKILL;
            state.p_Astate = AIM_STATE.SKILL_READY;


            enable = false;
            rangeobj.transform.SetParent(null);
            rangeobj.SetActive(true);
            rangeobj.transform.position = state.transform.position + state.transform.forward * (distance + roll);


            traject.start = new Vector3(state.transform.position.x, state.transform.position.y + 1.2f, state.transform.position.z);
            traject.target = rangeobj.transform.position;

            traject.TrejectMethod();

            anim.SetTrigger("SkillSwap");
            StartCoroutine(NextState());
            SoundManager.instance.PlaySFX(state.CharacterAudio, "RELOAD_Pump_Dark_stereo");
        }
    }


    IEnumerator PreState()
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
        click = false;
        yield return null;
    }
    IEnumerator NextState()
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

        anim.SetTrigger("Skill_4");
        click = false;
    }

    protected override IEnumerator ActivatingSkill()
    {

        state.p_state = STATE.SKILL;
        anim.SetBool("Skill_1ing", true);

        rangeobj.SetActive(false);
        rangeobj.transform.SetParent(SkillManager.Instance.transform);

        skillActivate = false;

        while(!anim.GetCurrentAnimatorStateInfo(0).IsTag("Skill"))
        {
            yield return null;
        }

        // 수류탄 랜덤 위치에 생성
        for(int i = 0; i < bombCount; i++)
        {
            bombMethod[i].start = traject.start;
            spawnBomb[i].transform.position = traject.start;
            spawnBomb[i].transform.SetParent(null);
            spawnBomb[i].SetActive(true);
            if(i == 0)
            {
                bombMethod[i].destination = RandomSphereInPoint(traject.target);
            }
            else
            {
                while(i > 0)
                {
                    int count = 0;
                    bombMethod[i].destination = RandomSphereInPoint(traject.target);
                    for(int j = 0; j < i; j++)
                    {
                        if(Vector3.Distance(bombMethod[j].destination, bombMethod[i].destination) > 1f)
                        {
                            ++count;
                        }
                    }

                    if(count == i)
                    {
                        break;
                    }
                }
            }
            bombMethod[i].setCenter();
            StartCoroutine(bombMethod[i].BombExplosion());
        }

        StartCoroutine(ExplosionBomb());
        SoundManager.instance.PlaySFX(state.CharacterAudio, "WHOOSH_Air_Slow_RR2_mono");

        while(anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1f)
        {
            if(!anim.GetCurrentAnimatorStateInfo(0).IsTag("Skill"))
            {
                break;
            }
            yield return null;
        }

        anim.SetBool("Skill_1ing", false);
        StartCoroutine(StateChange("Swap"));


        yield return null;
    }

    IEnumerator ExplosionBomb()
    {
        yield return s_time;

        SoundManager.instance.PlaySFX(audioSource,
            "EXPLOSION_Long_Pitched_Impact_with_Smooth_Tail_stereo");

        StartCoroutine(WaitSkillCooltime());

        yield return null;
    }


    public Vector3 RandomSphereInPoint(Vector3 pos)
    {
        Vector3 getPoint = Random.onUnitSphere;
        getPoint.y = 0f;

        float r = Random.Range(0.0f, radius);

        return (getPoint * r) + pos;
    }
}
