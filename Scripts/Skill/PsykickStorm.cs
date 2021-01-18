using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Player;

public class PsykickStorm : MonoBehaviour
{
    public WaitForSeconds w_time;
    public WaitForSeconds s_time;
    public Vector3 pos;

    public float speed;
    public Vector3 BoxSize;
    public float DamageSec;
    //Vector3 start;

    bool boxswitch = true;
    bool move = false;

    WaitForSeconds d_time;

    GameObject stormExplosion;
    Explosion2 explosion;

    public Collider[] colls;

    public AudioSource audioSource;


    private void Start()
    {
        d_time = new WaitForSeconds(DamageSec);
        //start = new Vector3(transform.position.x, transform.position.y + BoxSize.y * 0.5f, transform.position.z);
        StartCoroutine(WaitStorm());
        stormExplosion = SkillManager.Instance.Explosion;
        explosion = stormExplosion.GetComponent<Explosion2>();
        audioSource = GetComponent<AudioSource>();
    }

    IEnumerator WaitStorm()
    {
        yield return w_time;
        move = true;
        SoundManager.instance.PlaySFX(audioSource, "A_PK_HD_BoosterMove");
        StartCoroutine(switchOnOff());
        StartCoroutine(DamageMethod());
        yield return null;
    }

    bool flag = false;

    private void Update()
    {
        if(!flag)
        {
            flag = true;
            StartCoroutine(WaitStorm());
        }

        if(this.gameObject.activeSelf && boxswitch)
        {
            if(move)
            {
                transform.Translate(pos * speed * Time.deltaTime);
            }

            Vector3 up = new Vector3(transform.position.x, transform.position.y + BoxSize.y, transform.position.z);
            Vector3 down = new Vector3(transform.position.x, transform.position.y - BoxSize.y * 0.5f, transform.position.z);
            colls = Physics.OverlapCapsule(up, down, BoxSize.x * 0.5f, LayerMask.GetMask("Enemy") | LayerMask.GetMask("Wall"));


            //for(int i=0; i<colls.Length; i++)
            //{
            //    if(colls[i].gameObject.layer == LayerMask.NameToLayer("Enemy"))
            //    {
            //        SkillManager.instance.SetSkillDamage(colls[i], 25);
            //    }
            //}
        }
    }

    IEnumerator switchOnOff()
    {
        yield return s_time;

        boxswitch = false;

        // 폭발 데미지
        stormExplosion.transform.SetParent(null);

        if(explosion == null)
        {
            explosion = stormExplosion.GetComponent<Explosion2>();
        }
        stormExplosion.transform.position = new Vector3(transform.position.x, transform.position.y - BoxSize.y * 0.5f, transform.position.z);
        stormExplosion.SetActive(true);
        StartCoroutine(explosion.switchOnOff());
        explosion.start = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        SoundManager.instance.StopSFX(audioSource);
        SoundManager.instance.PlaySFX(explosion.GetComponent<AudioSource>(), "A_PK_ML_ElectronicExplosion");
        //SoundManager.instance.PlaySFX(audioSource, "A_PK_ML_ElectronicExplosion", false);
        //Vector3 up = new Vector3(transform.position.x, transform.position.y + BoxSize.y, transform.position.z);
        //Vector3 down = new Vector3(transform.position.x, transform.position.y - BoxSize.y * 0.5f, transform.position.z);
        //colls = Physics.OverlapCapsule(up, down, BoxSize.x * 0.5f, LayerMask.GetMask("Enemy") | LayerMask.GetMask("Wall"));
        //colls = Physics.OverlapBox(
        //       new Vector3(transform.position.x, transform.position.y + BoxSize.y * 0.5f, 
        //       transform.position.z), BoxSize, Quaternion.Euler(Vector3.zero), LayerMask.GetMask("Enemy"));

        //foreach(var item in colls)
        //{
        //    SkillManager.instance.SetSkillDamage(item, 30);
        //    //Debug.LogFormat("{0}이 폭발 데미지를 입었다!", item.name);
        //}

        SkillManager.Instance.InsertList(this.gameObject);

        move = false;
        flag = false;
        boxswitch = true;

        yield return null;
    }

    IEnumerator DamageMethod()
    {
        while(boxswitch)
        {
            for(int i = 0; i < colls.Length; i++)
            {
                if(colls[i].gameObject.layer == LayerMask.NameToLayer("Wall"))
                {
                    move = false;
                }
                SkillManager.Instance.SetSkillDamage(colls[i], 25);
                Debug.LogFormat("{0}이 {1}에 맞음!", colls[i].gameObject.name, this.gameObject.name);
            }
            yield return d_time;
        }

        yield return null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(new Vector3(transform.position.x, transform.position.y, transform.position.z), BoxSize);
    }
}
