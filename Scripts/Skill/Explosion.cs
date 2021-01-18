using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{

    protected float sec;
    protected WaitForSeconds s_time;

    public Vector3 BoxSize;
    public Vector3 start;
    protected bool boxswitch = true;

    protected bool damage = false;

    protected ParticleSystem ps;


    public AudioSource audioSource;

    public IEnumerator switchOnOff()
    {
        yield return s_time;

        boxswitch = false;

        yield return null;
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        ps = GetComponent<ParticleSystem>();
        sec = 0.7f;
        s_time = new WaitForSeconds(sec);
        StartCoroutine(switchOnOff());
    }

    // Update is called once per frame
    void Update()
    {
        Check();
    }

    protected virtual void Check()
    {
        if(boxswitch && this.gameObject.activeSelf)
        {
            Collider[] colls = Physics.OverlapBox(start, BoxSize, Quaternion.Euler(Vector3.zero), LayerMask.GetMask("Enemy"));

            if(colls.Length > 0 && !damage)
            {
                damage = true;
                for(int i = 0; i < colls.Length; i++)
                {
                    SkillManager.Instance.SetSkillDamage(colls[i], 40);
                    Debug.LogFormat("{0}이 {1}에 맞음!", colls[i].gameObject.name, this.gameObject.name);
                }
            }
        }

        if(ps)
        {
            if(!ps.IsAlive())
            {
                SkillManager.Instance.InsertList(this.gameObject);
                boxswitch = true;
                damage = false;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if(boxswitch && this.gameObject.activeSelf)
        {
            Gizmos.DrawWireCube(start, BoxSize);
        }
    }
}
