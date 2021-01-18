using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;


public class MagazinePool : MonoBehaviour
{

    public StringObject dic;
    public WeaponManager weaponManager;

    public Dictionary<string, Queue<GameObject>> m_list;

    private static MagazinePool instance;

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

        dic = new StringObject();
        SetDictionary();
        ObjectInitialized();
    }

    public static MagazinePool Instance
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

    void SetDictionary()
    {
        weaponManager = GameManager.instance.player.GetComponent<WeaponManager>();

        weaponManager.m_MainWeaponObject.SetActive(true);
        weaponManager.m_SubWeaponObject.SetActive(true);

        dic.Add("rifle_Ammo", Resources.Load("Character/Bullet/rifleBullet") as GameObject);
        dic.Add("pistol_Ammo", Resources.Load("Character/Bullet/pistolBullet") as GameObject);
        dic.Add("shotgun_Ammo", Resources.Load("Character/Bullet/shotgunBullet") as GameObject);

        //dic.Add("bomb_Ammo", Resources.Load("Character/Bullet/bombBullet") as GameObject);

        int count;

        switch(weaponManager.m_MainWeapon)
        {
            case "001":
            case "002":
            dic.Add("rifle_Mag", Resources.Load("Character/Bullet/shotgunBullet") as GameObject);
            break;

            case "011":
            case "012":
            dic.Add("rifle_Mag", Resources.Load("Character/Bullet/bombBullet") as GameObject);
            break;

            case "006":
            case "008":
            count = weaponManager.m_MainWeaponObject.transform.childCount - 2;
            dic.Add("rifle_Mag", weaponManager.m_MainWeaponObject.transform.GetChild(count).gameObject);
            break;

            default:
            count = weaponManager.m_MainWeaponObject.transform.childCount - 1;
            dic.Add("rifle_Mag", weaponManager.m_MainWeaponObject.transform.GetChild(count).gameObject);
            break;
        }

        count = weaponManager.m_SubWeaponObject.transform.childCount - 1;
        dic.Add("pistol_Mag", weaponManager.m_SubWeaponObject.transform.GetChild(count).gameObject);

        //Debug.LogFormat("{0} {1}", weaponManager.m_MainWeaponObject.transform.childCount, weaponManager.m_SubWeaponObject.transform.childCount);

        m_list = new Dictionary<string, Queue<GameObject>>();
        m_list["shotgun_Ammo"] = new Queue<GameObject>();
        m_list["bomb_Ammo"] = new Queue<GameObject>();

        m_list["pistol_Ammo"] = new Queue<GameObject>();
        m_list["rifle_Ammo"] = new Queue<GameObject>();
        m_list["pistol_Mag"] = new Queue<GameObject>();
        m_list["rifle_Mag"] = new Queue<GameObject>();
    }

    void ObjectInitialized()
    {

        switch(weaponManager.m_MainWeapon)
        {
            // 샷건
            case "001":
            case "002":
            for(int i = 0; i < 20; ++i)
            {
                GameObject ammo = Instantiate(dic["shotgun_Ammo"]);
                AutoBulletDestroying a = ammo.AddComponent<AutoBulletDestroying>();
                a.time = 2f;
                a.destroyingWaitTime = new WaitForSeconds(a.time);
                a.name = "shotgun_Ammo";

                Push("shotgun_Ammo", ammo);
            }
            break;

            // 라이플 반물질 발사기 제외
            case "003":
            case "004":
            case "009":
            case "010":
            case "007":
            for(int i = 0; i < 100; ++i)
            {
                GameObject ammo = Instantiate(dic["rifle_Ammo"]);
                AutoBulletDestroying a = ammo.AddComponent<AutoBulletDestroying>();
                a.time = 2f;
                a.destroyingWaitTime = new WaitForSeconds(a.time);
                a.name = "rifle_Ammo";

                Push("rifle_Ammo", ammo);
            }
            break;

            // 기관단총 킬링머신 제외
            case "005":
            for(int i = 0; i < 100; ++i)
            {
                GameObject ammo = Instantiate(dic["pistol_Ammo"]);
                AutoBulletDestroying a = ammo.AddComponent<AutoBulletDestroying>();
                a.time = 2f;
                a.destroyingWaitTime = new WaitForSeconds(a.time);
                a.name = "pistol_Ammo";

                Push("pistol_Ammo", ammo);
            }
            break;

            // 유탄 발사기는 발사 시 탄피가 나오지 않음
            //case "011":
            //case "012":
            //for(int i = 0; i < 20; ++i)
            //{
            //    GameObject ammo = Instantiate(dic["bomb_Ammo"]);
            //    ammo.AddComponent<Rigidbody>();
            //    Push("bomb_Ammo", ammo);
            //}
            //break;
        }

        int max = weaponManager.m_SubWeaponObject.GetComponent<HitScan>().m_Max_Clip_Value;

        for(int i = 0; i < 100; i++)
        {
            GameObject ammo = Instantiate(dic["pistol_Ammo"]);
            AutoBulletDestroying a = ammo.AddComponent<AutoBulletDestroying>();
            a.time = 5f;
            a.destroyingWaitTime = new WaitForSeconds(a.time);
            a.name = "pistol_Ammo";

            Push("pistol_Ammo", ammo);
        }

        for(int i = 0; i < 20; i++)
        {
            GameObject mag1 = Instantiate(dic["rifle_Mag"]);
            if(mag1.GetComponent<Rigidbody>() == null)
            {
                mag1.AddComponent<Rigidbody>();
            }
            AutoObjectDestroying a = mag1.AddComponent<AutoObjectDestroying>();
            a.time = 5f;
            a.name = "rifle_Mag";
            a.destroyingWaitTime = new WaitForSeconds(a.time);

            GameObject mag2 = Instantiate(dic["pistol_Mag"]);
            mag2.AddComponent<Rigidbody>();
            AutoObjectDestroying b = mag2.AddComponent<AutoObjectDestroying>();
            b.time = 5f;
            b.name = "pistol_Mag";
            b.destroyingWaitTime = new WaitForSeconds(b.time);

            Push("rifle_Mag", mag1);
            Push("pistol_Mag", mag2);
        }

    }

    public void Push(string key, GameObject p_obj)
    {
        m_list[key].Enqueue(p_obj);

        // 사격 후 떨어지는 탄피들에게 리지드바디를 넣는다.
        if(p_obj.GetComponent<Rigidbody>() == null)
        {
            p_obj.AddComponent<Rigidbody>();
        }

        p_obj.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        // MagazinePool 오브젝트의 자식으로 넣는다.
        p_obj.transform.SetParent(this.transform);
        // false...
        p_obj.SetActive(false);
    }


    public GameObject Pop(string key)
    {
        // 꺼낼려 하는데 없는 경우 새로 만든다.
        if(m_list[key].Count == 0)
        {
            GameObject ammo = Instantiate(dic[key]);
            Push(key, ammo);

            AutoObjectDestroying b = ammo.AddComponent<AutoObjectDestroying>();
            b.time = 5f;
            b.name = key;
            b.destroyingWaitTime = new WaitForSeconds(b.time);
        }

        // 오브젝트를 꺼낸다.   
        GameObject t_obj = m_list[key].Dequeue();
        t_obj.SetActive(true);
        t_obj.transform.SetParent(null);

        return t_obj;
    }
}
