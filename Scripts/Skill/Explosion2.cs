using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion2 : Explosion
{


    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        sec = 0.7f;
        s_time = new WaitForSeconds(sec);
        StartCoroutine(switchOnOff());
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        Check();
    }

    protected override void Check()
    {
        if(boxswitch && this.gameObject.activeSelf)
        {
            //Vector3 up = new Vector3(transform.position.x, transform.position.y + BoxSize.y, transform.position.z);
            //Vector3 down = new Vector3(transform.position.x, transform.position.y - BoxSize.y * 0.5f, transform.position.z);
            //Collider[] colls = Physics.OverlapCapsule(up, down, BoxSize.x * 0.5f, LayerMask.GetMask("Enemy"));

            Collider[] colls = Physics.OverlapBox(start, BoxSize, Quaternion.Euler(Vector3.zero), LayerMask.GetMask("Enemy"));

            if(colls.Length > 0 && !damage)
            {
                damage = true;
                for(int i = 0; i < colls.Length; i++)
                {
                    SkillManager.Instance.SetSkillDamage(colls[i], 30);
                    Debug.LogFormat("{0}이 {1}에 마지막 폭발을 맞았다", colls[i].gameObject.name, this.gameObject.name);
                }
            }
        }

        if(ps)
        {
            if(!ps.IsAlive())
            {
                this.gameObject.SetActive(false);
                this.transform.SetParent(SkillManager.Instance.transform);
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
