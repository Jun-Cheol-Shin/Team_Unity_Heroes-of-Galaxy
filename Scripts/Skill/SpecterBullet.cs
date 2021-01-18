using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecterBullet : MonoBehaviour
{

    bool hit;
    public float PosA;
    public float PosB;
    public float BulletSpeed;
    public float CheckRadius;

    public GameObject character;
    public GameObject Destination;

    Vector3 tempDes = Vector3.zero;

    ParticleSystem particle;

    Vector3[] point;

    AudioSource audioSource;

    [Range(0, 1)]
    float t = 0;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Initialized()
    {
        hit = false;
        t = 0f;

        if(particle == null)
        {
            particle = transform.GetChild(0).GetComponent<ParticleSystem>();
        }

        if(point == null)
        {
            point = new Vector3[4];
        }

        Vector3 temp = new Vector3(character.transform.position.x,
            character.transform.position.y - 0.1f, character.transform.position.z) + character.transform.forward;
        point[0] = temp;
        point[1] = PointSetting(temp);

        if(Destination != null)
        {
            Destination.transform.position = new Vector3(Destination.transform.position.x, Destination.transform.position.y + 1f, Destination.transform.position.z);
            point[2] = PointSetting(Destination.transform.position);
            point[3] = Destination.transform.position;
        }

        else
        {
            tempDes = new Vector3(character.transform.position.x, character.transform.position.y, character.transform.position.z) + character.transform.forward * 20f;
            point[2] = PointSetting(tempDes);
            point[3] = tempDes;
        }
    }

    Vector3 PointSetting(Vector3 origin)
    {
        float x, z;

        if(Mathf.Abs(character.transform.forward.z) > Mathf.Abs(character.transform.forward.x))
        {
            z = PosB * Mathf.Cos(UnityEngine.Random.Range(0, 360) * Mathf.Deg2Rad) + origin.z;
            x = PosA * Mathf.Sin(UnityEngine.Random.Range(0, 360) * Mathf.Deg2Rad) * 10f + origin.x;

        }

        else
        {
            x = PosB * Mathf.Cos(UnityEngine.Random.Range(0, 360) * Mathf.Deg2Rad) + origin.x;
            z = PosA * Mathf.Sin(UnityEngine.Random.Range(0, 360) * Mathf.Deg2Rad) * 10f + origin.z;
        }

        return new Vector3(x, origin.y, z);
    }


    void DrawBulletPosition()

    {
        Vector3 vec = new Vector3(
                   PointSetBezier(point[0].x, point[1].x, point[2].x, point[3].x),
                   PointSetBezier(point[0].y, point[1].y, point[2].y, point[3].y),
                   PointSetBezier(point[0].z, point[1].z, point[2].z, point[3].z));

        Vector3 dir = transform.position - vec;

        transform.position = vec;
        transform.rotation = Quaternion.LookRotation(dir.normalized);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + 180f, transform.rotation.eulerAngles.z);
    }


    float PointSetBezier(float p0, float p1, float p2, float p3)
    {
        return Mathf.Pow((1 - t), 3) * p0
            + Mathf.Pow((1 - t), 2) * 3 * t * p1
            + Mathf.Pow(t, 2) * 3 * (1 - t) * p2
            + Mathf.Pow(t, 3) * p3;
    }


    bool checkNull = false;

    void SetDestination()
    {
        if(Destination == null && tempDes == Vector3.zero)
        {
            while(Destination == null)
            {
                Collider[] collider = Physics.OverlapSphere(transform.position, CheckRadius, LayerMask.GetMask("Enemy"));

                if(collider.Length > 0)
                {
                    t = 0f;
                    while(true)
                    {
                        int rand = UnityEngine.Random.Range(0, collider.Length);
                        Destination = collider[rand].gameObject;

                        if(collider[rand].tag != "Boss" && collider[rand].tag != "DBoss" && collider[rand].tag != "Event")
                        {
                            if(!collider[rand].GetComponent<EnemyController>().isDead)
                            {
                                break;
                            }
                        }
                    }

                    Vector3 DestinationPos = new Vector3(Destination.transform.position.x, 
                        Destination.transform.position.y + 1f, Destination.transform.position.z);

                    point[0] = new Vector3(transform.position.x, transform.position.y, transform.position.z);
                    point[1] = PointSetting(new Vector3(transform.position.x, transform.position.y, transform.position.z));
                    point[2] = PointSetting(DestinationPos);
                    point[3] = DestinationPos;
                }

                else
                {
                    // 범위에 적이 없다고 판단
                    checkNull = true;
                    t = 0.95f;
                    break;
                }
            }
        }

    }

    bool flag = false;

    void Update()
    {
        if(!audioSource.isPlaying && !flag)
        {
            flag = true;
            SoundManager.instance.PlaySFX(audioSource, "P_PK_RS_LaserBeam");
        }

        // 중간에 목적지가 null이 되버리는 경우 다시 목적지를 바꾸어 출발지와 목적지를 다시 세팅.
        if(!checkNull)
        {
            SetDestination();
        }

        else
        {
            SkillManager.Instance.InsertList(this.gameObject);
        }

        if (hit || t > 1)
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, 1f, Vector3.up, 0f, LayerMask.GetMask("Enemy"));

            if (hits.Length > 0)
            {
                foreach (RaycastHit h in hits)
                {
                    if (h.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                    {
                        audioSource.pitch = 1.5f;
                        SoundManager.instance.PlaySFX(h.collider.gameObject.GetComponent<AudioSource>(), "P_PK_ML_ElectronicShoot");
                        Debug.LogFormat("{0}", h.collider.gameObject.name);
                        SkillManager.Instance.SetSkillDamage(h.collider.gameObject, 40);
                    }
                }

                particle.Stop();
                SkillManager.Instance.InsertList(this.gameObject);

                ExplosionsEffect(transform.position, Vector3.up, ObjectPool.E_OBJECT_POOL.SpecterExplosion);
            }
            else
            {
                Destination = null;
                tempDes = Vector3.zero;
                SetDestination();
            }
        }

        t += Time.deltaTime * BulletSpeed;
        DrawBulletPosition();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            audioSource.pitch = 1.5f;
            SoundManager.instance.PlaySFX(other.GetComponent<AudioSource>(), "P_PK_ML_ElectronicShoot");
            Debug.LogFormat("{0}", other.gameObject.name);
            SkillManager.Instance.SetSkillDamage(other, 40);
        }
    }

    // 폭발 이펙트를 생성시키는 함수
    public void ExplosionsEffect(Vector3 Hitpos, Vector3 Hitnormal, ObjectPool.E_OBJECT_POOL BulletEffect, float scale = 1f)
    {
        GameObject bulletEffect = ObjectPool.instance.GetObject(BulletEffect);

        if (bulletEffect == null)
        {
            return;
        }

        bulletEffect.transform.position = Hitpos;
        bulletEffect.transform.rotation = Quaternion.FromToRotation(Vector3.up, Hitnormal);
        Vector3 eulerAngle = bulletEffect.transform.rotation.eulerAngles;
        bulletEffect.transform.rotation = Quaternion.Euler(new Vector3(UnityEngine.Random.Range(0, 365), eulerAngle.y, eulerAngle.z));
        bulletEffect.transform.localScale = new Vector3(scale, scale, scale);

        bulletEffect.GetComponent<ParticleSystem>().Play();
    }

    // 디버그용 sphere 기즈모 출력
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}
