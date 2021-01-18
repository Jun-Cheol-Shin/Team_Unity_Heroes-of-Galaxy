using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class S_PsychokineticForce : PlayableSkill
{
    float radius = 5f;
    float distance = 10f;
    float height = 4f;

    Material material;
    Projector skillrange;
    GameObject rangeobj;

    public AudioSource audioSource;


    EnemyController[] enemy;

    float roll;
    bool skillActivate;
    Transform dummy;

    bool limit = false;

    private void Start()
    {
        rangeobj = SkillManager.Instance.Projector;
        skillrange = rangeobj.GetComponent<Projector>();
        audioSource = rangeobj.GetComponent<AudioSource>();

        enable = true;
        skillActivate = false;

        anim = GetComponent<Animator>();
        state = GetComponent<Player_Movement>();

        skillrange.orthographicSize = radius;


        s_time = new WaitForSeconds(skilltime);
        c_time = new WaitForSeconds(cooltime);
        cameraFollow = Camera.main.transform.parent.GetComponent<CameraFollow>();
        attributes = GameManager.instance.player.GetComponent<CharacterAttributes>();

        material = skillrange.material;
        material.color = new Color(128f, 0f, 128f);
        dummy = new GameObject("dummyRange").transform;
    }

    private void Update()
    {
        if (!skillenable && rangeobj.activeSelf && !skillActivate)
        {

            roll = anim.GetFloat("Roll");
            roll = Mathf.Clamp(roll, -65, -10);
            dummy.transform.position = state.transform.position + state.transform.forward * (distance + roll * 0.1f);

            if (!limit)
            {
                rangeobj.transform.position = dummy.transform.position;
            }

            Collider[] colls = Physics.OverlapSphere(new Vector3(dummy.transform.position.x,
                dummy.transform.position.y, dummy.transform.position.z), radius);

            for (int i = 0; i < colls.Length; i++)
            {
                if (colls[i].gameObject.layer == LayerMask.NameToLayer("Wall"))
                {
                    limit = true;
                    break;
                }
                else
                {
                    limit = false;
                }
            }

            if (Input.GetMouseButtonDown(0) && state.isGrounded)
            {
                SoundManager.instance.PlaySFX(audioSource, "MAGIC_SPELL_Power_mono");
                state.CharacterAudio.pitch = 1.5f;
                SoundManager.instance.PlaySFX(audioSource, "SPACE_WARP_Type_03A_Charge_Up_1_Deep_stereo");
                cameraFollow.locked = true;
                StartCoroutine(ActivatingSkill());
            }

            else if(Input.GetMouseButtonDown(1))
            {
                enable = true;
                rangeobj.SetActive(false);
                rangeobj.transform.SetParent(SkillManager.Instance.transform);
                state.p_Astate = AIM_STATE.NONE;
            }
        }

        // 띄우는 거
        else if(skillActivate)
        {
            if (enemy != null)
            {
                for (int i = 0; i < enemy.Length; ++i)
                {
                    enemy[i].transform.position = Vector3.Lerp(enemy[i].transform.position,
                            new Vector3(pos[i].x, pos[i].y + height, pos[i].z), Time.deltaTime * 2.5f);
                }
            }
        }

        // 떨구는거
        else if(!skillenable && !rangeobj.activeSelf && !skillActivate && colls != null)
        {
            int count = 0;
            int j = 0;
            for(int i = 0; i < colls.Length; ++i)
            {
                if (colls[i].tag != "Boss" && colls[i].tag != "Event") 
                {
                    colls[i].transform.position = Vector3.Lerp(colls[i].transform.position, new Vector3
                        (pos[j].x, pos[j].y, pos[j].z), Time.deltaTime * 5f);

                    if (Vector3.Distance(colls[i].transform.position, pos[j]) < 0.1f || enemy[j].isDead)
                    {
                        SoundManager.instance.PlaySFX(audioSource, "EXPLOSION_Medium_Dirty_Crackle_stereo");
                        ++count;

                        if (!enemy[j].isDead)
                        {
                            enemy[j].agent.enabled = true;
                            enemy[j].NavMove();
                            enemy[j].isStun = false;
                            enemy[j].CheckState();
                            SkillManager.Instance.SetSkillDamage(enemy[j].gameObject, 150);
                        }
                    }
                    ++j;
                }
                else
                {
                    ++count;
                }

            }

            if(count == colls.Length)
            {
                colls = null;
                pos = null;

                if (enemy != null)
                {
                    for (int k = 0; k < enemy.Length; ++k)
                    {
                        if (enemy[k] != null && !enemy[k].isDead)
                        {
                            //SoundManager.instance.PlaySFX(audioSource, "EXPLOSION_Medium_Dirty_Crackle_stereo", false);
                            //SkillManager.Instance.SetSkillDamage(enemy[k].gameObject, 60);
                            //if (!enemy[k].isDead)
                            //{
                            //    enemy[k].agent.enabled = true;
                            //    enemy[k].NavMove();
                            //    enemy[k].isStun = false;
                            //    enemy[k].CheckState();
                            //}
                        }
                    }
                }
                SoundManager.instance.PlaySFX(audioSource, "SPACE_WARP_Type_02_Warp_E_stereo");
                Debug.Log("완료");
            }

        }

    }

    bool skillenable = true;

    [ContextMenu("[스킬 발동]")]
    public override void Shot()
    {
        if (!enable)
            return;
        else
        {
            if (!rangeobj.activeSelf)
            {
                skillenable = true;
            }
            if (!skillenable && rangeobj != null && state.p_state != STATE.SKILL && enable)
            {
                skillenable = true;
                rangeobj.SetActive(false);
                rangeobj.transform.SetParent(SkillManager.Instance.transform);
                state.p_Astate = AIM_STATE.NONE;
            }
            else if (skillenable)
            {
                skillenable = false;
                rangeobj.SetActive(true);
                rangeobj.transform.SetParent(null);
                state.p_Astate = AIM_STATE.SKILL_READY;


            }
        }
    }


    Collider[] colls;
    Vector3[] pos;

    protected override IEnumerator ActivatingSkill()
    {
        enable = false;
        state.p_state = STATE.SKILL;
        anim.SetTrigger("Skill_3");
        anim.SetBool("Skill_1ing", true);

        colls = Physics.OverlapSphere(rangeobj.transform.position, radius, LayerMask.GetMask("Enemy"));
        int count = 0;

        for(int i = 0; i < colls.Length; i++)
        {
            if(colls[i].tag != "Boss" && colls[i].tag != "Event" && !colls[i].GetComponent<EnemyController>().isDead)
            {
                ++count;
            }
        }

        if(count > 0)
        {
            pos = new Vector3[count];
            enemy = new EnemyController[count];
        }

        int j = 0;
        for(int i=0; i<colls.Length; ++i)
        {
            if(colls[i].tag != "Boss" && colls[i].tag != "Event" && !colls[i].GetComponent<EnemyController>().isDead)
            {
                pos[j] = colls[i].transform.position;
                enemy[j] = colls[i].GetComponent<EnemyController>();
                if(enemy[j] != null)
                {
                    enemy[j].isStun = true;
                    enemy[j].CheckState();
                    enemy[j].NavStop();
                    enemy[j].agent.enabled = false;
                }
                ++j;
            }
        }

        skillActivate = true;

        yield return s_time;

        anim.SetBool("Skill_1ing", false);

        state.CharacterAudio.pitch = 1f;
        skillActivate = false;


        rangeobj.SetActive(false);
        rangeobj.transform.SetParent(SkillManager.Instance.transform);

        StartCoroutine(StateChange("Skill_End"));
        cameraFollow.locked = false;
        StartCoroutine(WaitSkillCooltime());
    }


    //private void OnDrawGizmos()
    //{
    //    if(rangeobj != null)
    //        Gizmos.DrawWireSphere(new Vector3(rangeobj.transform.position.x, rangeobj.transform.position.y, rangeobj.transform.position.z), radius);
    //}
}
