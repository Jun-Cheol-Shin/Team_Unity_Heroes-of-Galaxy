using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StringObject : SerializableDictionary<string, GameObject> { }

public class SkillManager : MonoBehaviour
{
    public StringObject dic;
    public PlayableSkill skill;

    //public GameObject[] SkillSourcePrefabs;
    private static SkillManager instance;
    private Queue<GameObject> m_List = new Queue<GameObject>();

    int skillPid;

    // 참조 할 것들
    public GameObject player;
    public GameObject Projector;
    public GameObject Installation;
    public GameObject Explosion;

    private void Start()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            //만약 씬 이동이 되었는데 그 씬에도 Hierarchy에 GameMgr이 존재할 수도 있다.
            //그럴 경우엔 이전 씬에서 사용하던 인스턴스를 계속 사용해주는 경우가 많은 것 같다.
            //그래서 이미 전역변수인 instance에 인스턴스가 존재한다면 자신(새로운 씬의 GameMgr)을 삭제해준다.
            Destroy(this.gameObject);
        }

        player = GameManager.instance.player;
        SetDictionary();

        Initialized(SettingManager.instance.skillCode[0]);
    }

    public static SkillManager Instance
    {
        get
        {
            if(null == instance)
            {
                return null;
            }
            return instance;
        }
    }



    public void SetSkillDamage(Collider coll, int damage)
    {
        if(coll.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            switch(coll.tag)
            {
                case "DBoss":
                    GameManager.instance.Attack_Object(coll.gameObject, GameManager.E_ObjectType.DBoss,
                    GameManager.AttackType.None, damage, player);
                    break;

                case "Boss":
                GameManager.instance.Attack_Object(coll.gameObject, GameManager.E_ObjectType.Boss,
                    GameManager.AttackType.None, damage, player);
                    break;

                case "Event":
                GameManager.instance.Attack_Object(coll.gameObject, GameManager.E_ObjectType.Gimmick,
                    GameManager.AttackType.None, damage, player);
                    break;

                default:
                GameManager.instance.Attack_Object(coll.gameObject, GameManager.E_ObjectType.Monster,
                    GameManager.AttackType.None, damage, player);
                    break;
            }
        }
    }

    public void SetSkillDamage(GameObject coll, int damage)
    {
        if (coll.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            switch (coll.tag)
            {
                case "Boss":
                    GameManager.instance.Attack_Object(coll, GameManager.E_ObjectType.Boss,
                        GameManager.AttackType.None, damage, player);
                    break;

                case "Event":
                    GameManager.instance.Attack_Object(coll, GameManager.E_ObjectType.Gimmick,
                        GameManager.AttackType.None, damage, player);
                    break;

                default:
                    GameManager.instance.Attack_Object(coll, GameManager.E_ObjectType.Monster,
                        GameManager.AttackType.None, damage, player);
                    break;
            }
        }
    }

    void SetDictionary()
    {
        dic.Add("ExplosionDash", Resources.Load("Character/Skill/Explosion") as GameObject);
        dic.Add("Climax_Push", Resources.Load("Character/Skill/Climax_Push") as GameObject);
        dic.Add("Climax_Tower", Resources.Load("Character/Skill/Climax_Tower") as GameObject);

        dic.Add("Hero_of_High_Noon", Resources.Load("Character/Skill/HighNoob") as GameObject);
        dic.Add("PsykickStorm", Resources.Load("Character/Skill/PsykickStorm") as GameObject);
        dic.Add("Psychokinetic_Force", Resources.Load("Character/Skill/PsykickStorm") as GameObject);
        dic.Add("Specter_Bullet", Resources.Load("Character/Skill/SpecterBullet") as GameObject);
        dic.Add("Saturation", null);
        dic.Add("Wind_Hole", null);
        dic.Add("Bomb_Spraying", Resources.Load("Character/Skill/RealBomb") as GameObject);
        dic.Add("Projector", Resources.Load("Character/Skill/ProjectorObject") as GameObject);
        dic.Add("Trajector", Resources.Load("Character/Skill/TrajectoryObject") as GameObject);

        dic.Add("StormExplosion", Resources.Load("Character/Skill/StormExplosion") as GameObject);
        dic.Add("TowerExplosion", Resources.Load("Character/Skill/TowerExplosion") as GameObject);
    }

    void SkillChange(int pid)
    {
        if(skill != null)
        {
            Destroy(skill);

            for(int i = 0; i < m_List.Count; i++)
            {
                GameObject temp = m_List.Dequeue();
                Destroy(temp);
            }

            m_List.Clear();

            if(Projector != null)
            {
                Destroy(Projector.gameObject);
                Projector = null;
            }

            if(Installation != null)
            {
                Destroy(Installation.gameObject);
                Installation = null;
            }

            if(Explosion != null)
            {
                Destroy(Explosion.gameObject);
                Explosion = null;
            }

            for(int i=0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        switch(skillPid)
        {
            case 2000:
            skill = player.AddComponent<S_ExplosionDash>();
            break;
            case 2001:
            skill = player.AddComponent<S_Climax>();
            break;
            case 2002:
            skill = player.AddComponent<S_HeroOfHighNoon>();
            break;
            case 2003:
            skill = player.AddComponent<S_PsykickStorm>();
            break;
            case 2004:
            skill = player.AddComponent<S_PsychokineticForce>();
            break;
            case 2005:
            skill = player.AddComponent<S_SpecterBullet>();
            break;
            case 2006:
            skill = player.AddComponent<S_saturation>();
            break;
            case 2007:
            skill = player.AddComponent<S_WindHole>();
            break;
            case 2008:
            skill = player.AddComponent<S_BombSpraying>();
            break;
        }
    }


    public void Initialized(int pid)
    {
        skillPid = pid;
        SkillChange(pid);

        switch(pid)
        {
            // 폭발대쉬
            case 2000:
            skill.cooltime = 10f;
            skill.skilltime = 3f;

            for(int i = 0; i < 10; i++)
            {
                GameObject explosion = Instantiate(dic["ExplosionDash"]);
                //explosion.GetComponent<Explosion>().audioSource = GetComponent<AudioSource>();
                InsertList(explosion);
            }
            break;

            // 클라이맥스
            case 2001:
            skill.cooltime = 20f;
            skill.skilltime = 20f;

            Installation = Instantiate(dic["Climax_Tower"]);
            //Installation.GetComponent<ClimaxTower>().audioSource = GetComponent<AudioSource>();
            Installation.SetActive(false);
            Installation.transform.SetParent(this.transform);

            for(int i = 0; i < 10; i++)
            {
                GameObject push = Instantiate(dic["Climax_Push"]);
                InsertList(push);
            }

            Projector = Instantiate(dic["Projector"]);
            Projector.SetActive(false);
            Projector.transform.SetParent(this.transform);

            Explosion = Instantiate(dic["TowerExplosion"]);
            //Explosion.GetComponent<Explosion2>().audioSource = GetComponent<AudioSource>();
            Explosion.SetActive(false);
            Explosion.transform.SetParent(this.transform);

            break;

            // 하이눈
            case 2002:
            skill.cooltime = 30f;
            skill.skilltime = 60f;

            GameObject effect = Instantiate(dic["Hero_of_High_Noon"]);
            InsertList(effect);
            break;

            // 사이킥 스톰
            case 2003:
            skill.cooltime = 10f;
            skill.skilltime = 5f;

            GameObject t = Instantiate(dic["PsykickStorm"]);
            //t.GetComponent<PsykickStorm>().audioSource = GetComponent<AudioSource>();
            InsertList(t);

            Explosion = Instantiate(dic["StormExplosion"]);
            //Explosion.GetComponent<Explosion2>().audioSource = GetComponent<AudioSource>();
            Explosion.SetActive(false);
            Explosion.transform.SetParent(this.transform);
            break;

            // 염동력
            case 2004:
            skill.cooltime = 10f;
            skill.skilltime = 3f;

            Projector = Instantiate(dic["Projector"]);
            Projector.SetActive(false);
            Projector.transform.SetParent(this.transform);
            break;

            // 망령 탄환
            case 2005:
            skill.cooltime = 5f;
            skill.skilltime = 5f;
            for(int i = 0; i < 20; i++)
            {
                GameObject push = Instantiate(dic["Specter_Bullet"]);
                InsertList(push);
            }
            break;

            // 포화
            case 2006:
            skill.cooltime = 10f;
            skill.skilltime = 0.25f;
            break;

            // 스나
            case 2007:
            skill.cooltime = 10f;
            break;

            // 폭탄 뿌리기
            case 2008:
            skill.skilltime = 2f;
            skill.cooltime = 10f;
            skill.GetComponent<S_BombSpraying>().bombCount = 8;
            Projector = Instantiate(dic["Trajector"]);
            Projector.SetActive(false);
            Projector.transform.SetParent(this.transform);

            Installation = Instantiate(dic["Bomb_Spraying"]);
            //Installation.GetComponent<SprayingBomb>().audioSource = GetComponent<AudioSource>();
            Installation.SetActive(false);
            Installation.transform.SetParent(this.transform);

            // 효과음 내기용
            Explosion = Instantiate(dic["ExplosionDash"]);
            InsertList(Explosion);
            for(int i = 0; i < skill.GetComponent<S_BombSpraying>().bombCount; i++)
            {
                GameObject explosion = Instantiate(dic["ExplosionDash"]);
                //explosion.GetComponent<Explosion>().audioSource = GetComponent<AudioSource>();
                InsertList(explosion);
            }
            break;
        }
    }

    public void InsertList(GameObject p_object)
    {
        m_List.Enqueue(p_object);
        p_object.transform.SetParent(this.transform);
        p_object.SetActive(false);
    }

    public GameObject GetObject()
    {
        if(m_List.Count == 0)
        {
            switch(skillPid)
            {
                case 2000:
                GameObject explosion = Instantiate(dic["ExplosionDash"]);
                InsertList(explosion);
                break;

                case 2001:
                GameObject push = Instantiate(dic["Climax_Push"]);
                InsertList(push);
                break;

                case 2002:

                break;

                case 2003:

                break;

                case 2004:

                break;

                case 2005:
                GameObject bullet = Instantiate(dic["Specter_Bullet"]);
                InsertList(bullet);
                break;

                case 2006:

                break;

                case 2007:

                break;

                case 2008:

                break;
            }
        }

        GameObject t_obj = m_List.Dequeue();
        t_obj.SetActive(true);

        t_obj.transform.SetParent(null);

        return t_obj;
    }

}
