using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Player;

public class ClimaxTower : MonoBehaviour
{

    // S_Climax에서 받아오는 변수 2개
    public WaitForSeconds d_time;
    public WaitForSeconds s_time;

    public LayerMask layer;

    bool land = false;

    public GameObject summonEffect;
    public GameObject attackEffect;
    //public GameObject pushEffect;

    Vector3[] vec;
    Vector3[] pos;

    [SerializeField]
    Collider[] colls;

    GameObject[] push;
    EnemyController[] enemy;

    public GameObject explosion;
    Explosion2 ex;

    // 체크할 원의 반지름
    public float checkRadius;
    // 밀려나갈 거리
    public float distance;
    public bool startPush;

    public bool startSkill;

    public AudioSource audioSource;
    public AudioSource exAudio;

    private void Start()
    {
        startPush = false;
        startSkill = false;
        audioSource = GetComponent<AudioSource>();
        explosion = SkillManager.Instance.Explosion;
        ex = explosion.GetComponent<Explosion2>();
        exAudio = ex.GetComponent<AudioSource>();
    }

    private void Update()
    {
        if(startPush && colls.Length > 0)
        {
            int count = 0;
            for(int i = 0; i < push.Length; i++)
            {
                enemy[i].transform.position = Vector3.Lerp(enemy[i].transform.position,
                        pos[i] - vec[i] * distance, Time.deltaTime * 3f);
                push[i].transform.position = Vector3.Lerp(push[i].transform.position,
                       pos[i] - vec[i] * distance, Time.deltaTime * 3f);

                if(Vector3.Distance(push[i].transform.position, (pos[i] - vec[i] * distance)) < 0.1f || enemy[i].isDead)
                {
                    ++count;
                    SkillManager.Instance.InsertList(push[i].gameObject);
                    //enemy[i].NavMove();
                }
                //if(colls[i].tag != "Boss" && colls[i].tag != "Event")
                //{
                //    colls[i].transform.position = Vector3.Lerp(colls[i].transform.position,
                //        pos[i] - vec[i] * distance, Time.deltaTime * 3f);
                //    push[i].transform.position = Vector3.Lerp(push[i].transform.position,
                //        pos[i] - vec[i] * distance, Time.deltaTime * 3f);
                //}
                //else
                //{
                //    ++count;
                //}

                //if(colls[i].tag != "Boss" && colls[i].tag != "Event" &&
                //    Vector3.Distance(push[i].transform.position, (pos[i] - vec[i] * distance)) < 0.1f)
                //{
                //    ++count;
                //    enemy[i].NavMove();
                //    SkillManager.instance.InsertList(push[i].gameObject);
                //}
            }

            if(count == push.Length)
            {
                Debug.Log("밀어내기 종료");
                startSkill = true;
                startPush = false;

                // 스킬 시간 카운트
                StartCoroutine(StartSkillTimer());
                // 공격,버프 시작
                StartCoroutine(BuffAndAttack());
            }
        }

        else if(startPush && colls.Length == 0)
        {
            Debug.Log("충격파에 밀리는 적이 없다!");
            startSkill = true;
            startPush = false;

            // 스킬 시간 카운트
            StartCoroutine(StartSkillTimer());
            // 공격,버프 시작
            StartCoroutine(BuffAndAttack());
        }
    }


    IEnumerator StartSkillTimer()
    {
        yield return s_time;

        startSkill = false;
        land = false;

        yield return null;
    }

    IEnumerator BuffAndAttack()
    {
        // 공격
        attackEffect.SetActive(true);
        attackEffect.transform.SetParent(this.transform);
        attackEffect.transform.localPosition = new Vector3(0, -7f, 0);
        attackEffect.transform.localRotation = Quaternion.Euler(Vector3.zero);
        attackEffect.transform.localScale = new Vector3(2, 2, 2);

        while(startSkill)
        {
            SoundManager.instance.PlaySFX(audioSource,
           "SPACE_WARP_Type_01_Warp_A_stereo");
            Collider[] colls = Physics.OverlapSphere(transform.position, checkRadius, layer);

            // 적은 어택 아군은 버프
            foreach(Collider obj in colls)
            {
                if(obj.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    SkillManager.Instance.SetSkillDamage(obj, 10);
                    //Debug.LogFormat("{0} 에게 데미지를 준다!", obj.gameObject.name);
                }
                else if(obj.gameObject.layer == LayerMask.NameToLayer("Ally"))
                {
                    AIAttributte ai = obj.GetComponent<AIAttributte>();
                    ai.AddBuff("Speed UP", SkillManager.Instance.player);
                    //ai.AddBuff("", SkillManager.instance.player);
                    //Debug.LogFormat("{0} 에게 버프를 준다!", obj.gameObject.name);
                }
                else if(obj.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    CharacterAttributes my = obj.GetComponent<CharacterAttributes>();  
                    my.AddBuff("Speed UP", SkillManager.Instance.player);
                    //my.AddBuff("", SkillManager.instance.player);
                }
            }

            yield return d_time;
        }


        attackEffect.SetActive(false);
        summonEffect.SetActive(false);

        // 폭발 파티클 추가 필요

        explosion.transform.SetParent(null);
        explosion.transform.position = point;
        explosion.SetActive(true);
        StartCoroutine(ex.switchOnOff());
        ex.start = new Vector3(point.x, point.y + ex.BoxSize.y * 0.5f, point.z);
        SoundManager.instance.PlaySFX(exAudio,
            "EXPLOSION_Medium_Blast_Scatter_Debris_Long_Faint_Tail_stereo");
        //Collider[] explosionColls = Physics.OverlapSphere(transform.position, checkRadius, LayerMask.GetMask("Enemy"));

        //foreach(Collider obj in explosionColls)
        //{
        //    SkillManager.instance.SetSkillDamage(obj, 30);
        //}

        this.transform.SetParent(SkillManager.Instance.transform);
        this.gameObject.SetActive(false);

        //Destroy(this.gameObject);

        yield return null;
    }

    Quaternion rot;
    Vector3 point;

    void OnCollisionEnter(Collision collision)
    {
        if(!land)
        {
            SkillManager.Instance.player.GetComponent<Player_Movement>().CharacterAudio.Stop();
            SoundManager.instance.PlaySFX(audioSource,
            "EXPLOSION_Medium_Dirty_Crackle_stereo");
            // 착지 성공
            land = true;

            // 이펙트 발생!
            ContactPoint contact = collision.contacts[0];
            rot = Quaternion.FromToRotation(-Vector3.forward, contact.normal);
            point = new Vector3(contact.point.x, contact.point.y + 0.1f, contact.point.z);

            //summonEffect.transform.SetParent(this.transform);

            // 이펙트 크기 조절
            summonEffect.transform.position = new Vector3(transform.position.x, point.y, transform.position.z);
            // 먼지
            //summonEffect.transform.GetChild(0).localScale = new Vector3(summonEffect.transform.localScale.x * 2f - 4f, summonEffect.transform.localScale.y * 2f - 4f, summonEffect.transform.localScale.z * 2f - 4f);
            //// 균열
            //summonEffect.transform.GetChild(1).localScale = new Vector3(summonEffect.transform.localScale.x - 1f, summonEffect.transform.localScale.y - 1f, summonEffect.transform.localScale.z - 1f);
            //summonEffect.transform.GetChild(1).GetComponent<ParticleSystem>().transform.position = new Vector3(
            //   summonEffect.transform.GetChild(1).GetComponent<ParticleSystem>().transform.position.x,
            //    summonEffect.transform.GetChild(1).GetComponent<ParticleSystem>().transform.position.y + 0.02f,
            //    summonEffect.transform.GetChild(1).GetComponent<ParticleSystem>().transform.position.z);

            summonEffect.SetActive(true);

            // 적 밀치기 시작
            startPush = true;

            // 범위안에 있는 적 알아내기
            colls = Physics.OverlapSphere(transform.position, checkRadius, LayerMask.GetMask("Enemy"));

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
                // 적이 밀려나갈 방향
                vec = new Vector3[count];
                // 적이 있는 위치
                pos = new Vector3[count];
                // 밀리는 파티클
                push = new GameObject[count];
                // 밀리는 적의 코드 컴포넌트
                enemy = new EnemyController[count];
            }

            int j = 0;

            for(int i = 0; i < colls.Length;)
            {
                if(count == 0)
                    break;

                if(colls[i].tag != "Boss" && colls[i].tag != "Event" && !colls[i].GetComponent<EnemyController>().isDead)
                {
                    enemy[j] = colls[i].GetComponent<EnemyController>();
                    push[j] = SkillManager.Instance.GetObject();
                    pos[j] = colls[i].transform.position;
                    push[j].transform.position = pos[j];
                    vec[j] = transform.position - pos[j];
                    vec[j] = vec[j].normalized;
                    ++j;
                }

                //if(enemy[j - 1] != null)
                //{
                //    enemy[j - 1].NavStop();
                //}
                SkillManager.Instance.SetSkillDamage(colls[i], 25);
                // 오브젝트 자체를 움직여야함.
                ++i;
            }
        }
    }

}