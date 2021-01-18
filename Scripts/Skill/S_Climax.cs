using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using System.Reflection;
using UnityEditor;

public class S_Climax : PlayableSkill
{


    Material material;
    GameObject range;

    ClimaxTower climaxTower;
    AudioSource audioSource;
    GameObject Tower;

    // 매 시간마다 공격
    WaitForSeconds d_time;

    public float attackTime;
    // 거리
    public float distance;
    // 높이
    public float height;


    Vector3 towerPos;
    Transform dummy;

    private void Start()
    {
        distance = 5f;
        height = 10f;
        attackTime = 1f;

        enable = true;
        c_time = new WaitForSeconds(cooltime);
        s_time = new WaitForSeconds(skilltime);
        d_time = new WaitForSeconds(attackTime);

        state = GetComponent<Player_Movement>();
        anim = GetComponent<Animator>();

        range = SkillManager.Instance.Projector;
        range.GetComponent<Projector>().orthographicSize = 1f;
        material = range.GetComponent<Projector>().material;

        Tower = SkillManager.Instance.Installation;
        dummy = new GameObject("dummyRange").transform;
    }

    float roll;
    bool skillActivate = false;

    bool flag = true;

    // 범위 제한
    bool limit = false;

    IEnumerator CheckSkillTime()
    {
        yield return s_time;

        StartCoroutine(WaitSkillCooltime());
    }

    private void Update()
    {
        if(!enable && range.activeSelf && !skillActivate)
        {
            roll = anim.GetFloat("Roll");
            roll = Mathf.Clamp(roll, -30, -10);
            dummy.transform.position = state.transform.position + state.transform.forward * (distance + roll * 0.1f);

            if (!limit)
            {
                range.transform.position = dummy.transform.position;
            }

            Collider[] colls = Physics.OverlapSphere(
                new Vector3(dummy.transform.position.x,
                dummy.transform.position.y,
                dummy.transform.position.z), 1f,
                LayerMask.GetMask("Wall"));

            //if(colls.Length > 0)
            //{
            //}

            if (colls.Length == 0)
            {
                limit = false;
                material.color = Color.green;

                if (Input.GetMouseButtonDown(0))
                {
                    SoundManager.instance.PlaySFX(state.CharacterAudio, "UI_SCI-FI_Zoom_01_stereo");
                    skillActivate = true;
                    towerPos = range.transform.position;
                    range.SetActive(false);
                    range.transform.SetParent(SkillManager.Instance.transform);
                    StartCoroutine(ActivatingSkill());
                }
            }

            if (Input.GetMouseButtonDown(1) && !limit)
            {
                state.p_Astate = AIM_STATE.NONE;
                enable = true;
                range.SetActive(false);
                range.transform.SetParent(SkillManager.Instance.transform);
            }
        }

        else if(climaxTower != null && climaxTower.startSkill && flag)
        {
            Debug.Log("일로 들어감");
            flag = false;
            StartCoroutine(CheckSkillTime());
        }

        else
        {
            limit = true;
        }
    }

    bool skillenable = true;

    [ContextMenu("[스킬 발동]")]
    public override void Shot()
    {
        //base.Shot();

        if (enable)
        {
            flag = true;
            state.p_Astate = AIM_STATE.SKILL_READY;
            enable = false;
            range.transform.SetParent(null);
            range.transform.position = state.transform.position + state.transform.forward * (distance + roll * 0.1f);
            range.SetActive(true);
        }

        else if (!enable && range.activeSelf)
        {
            flag = false;
            state.p_Astate = AIM_STATE.NONE;
            enable = true;
            range.SetActive(false);
            range.transform.SetParent(SkillManager.Instance.transform);
        }
    }


    protected override IEnumerator ActivatingSkill()
    {
        yield return new WaitForSeconds(0.1f);
        state.p_Astate = AIM_STATE.NONE;
  
        if(climaxTower == null)
        {
            climaxTower = Tower.GetComponent<ClimaxTower>();
            climaxTower.d_time = d_time;
            climaxTower.s_time = s_time;
            audioSource = climaxTower.GetComponent<AudioSource>();
        }
        Tower.transform.position = new Vector3(towerPos.x, height, towerPos.z);
        Tower.transform.SetParent(null);
        Tower.SetActive(true);

        skillActivate = false;

        SoundManager.instance.PlaySFX(audioSource, "FLYBY_Sci-Fi_03_mono");
        //yield return s_time;
        //StartCoroutine(CheckSkillTime());
        yield return null;
    }


    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawWireSphere(new Vector3(rangeobj.transform.position.x, rangeobj.transform.position.y, rangeobj.transform.position.z), 1f);
    //}

}

