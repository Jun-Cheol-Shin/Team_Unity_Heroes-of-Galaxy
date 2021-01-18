using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class S_SpecterBullet : PlayableSkill
{

    bool ct;

    public GameObject target;


    public float skillDist = 10f;
    public int shot;
    WaitForSeconds Delay;

    [SerializeField]
    Collider[] colls;


    private void Start()
    {
        attributes = GameManager.instance.player.GetComponent<CharacterAttributes>();
        weaponManager = GetComponent<WeaponManager>();
        state = GetComponent<Player_Movement>();
        Delay = new WaitForSeconds(0.05f);
        c_time = new WaitForSeconds(cooltime);
        target = null;
        enable = true;
        ct = true;
    }

    void LookingTarget()
    {
        if(colls.Length > 0)
        {
            int random = Random.Range(0, colls.Length);
            if(colls[random] == null)
            {
                int reset_rand = random;
                while(reset_rand == random)
                {
                    reset_rand = Random.Range(0, colls.Length);
                }

                target = colls[reset_rand].gameObject;
            }
            else
            {
                target = colls[random].gameObject;
            }
        }
    }

    public void Kill()
    {
        ++shot;
    }


    [ContextMenu("[스킬 발동]")]
    public override void Shot()
    {
        base.Shot();

        if (enable)
        {
            StartCoroutine(ActivatingSkill());
        }
    }

    protected override IEnumerator ActivatingSkill()
    {
        // 스택 수정해야함
        ct = false;
        enable = false;

        while(shot > 0)
        {
            LookingTarget();
            state.Shoot();
            --shot;
            GameObject Bullet = SkillManager.Instance.GetObject();
            SpecterBullet bullet = Bullet.GetComponent<SpecterBullet>();
            SoundManager.instance.PlaySFX(bullet.GetComponent<AudioSource>(), 
                "BLASTER_Complex_Fire_Trigger_Powerful_Release_stereo");
            // 총구로 변경..
            switch(state.p_Wstate)
            {
                case WEAPON_STATE.MAIN_WEAPON:
                bullet.character = weaponManager.m_MainWeaponObject.transform.GetChild(0).gameObject;
                break;

                case WEAPON_STATE.SUB_WEAPON:
                bullet.character = weaponManager.m_SubWeaponObject.transform.GetChild(0).gameObject;
                break;
            }

            bullet.Destination = target;
            bullet.Initialized();

            yield return Delay;
        }

        StartCoroutine(WaitSkillCooltime());
        yield return null;
    }


    private void Update()
    {
        if(ct)
        {
            colls = Physics.OverlapSphere(transform.position, skillDist, LayerMask.GetMask("Enemy"));

            if(colls.Length > 0 && shot > 0)
            {
                enable = true;
            }

            else
            {
                enable = false;
            }
        }
    }

    protected override IEnumerator WaitSkillCooltime()
    {
        if (attributes.SearchPerk(2007) != null)
        {
            attributes.AddBuff("Switch ON", attributes.gameObject);
        }

        yield return c_time;

        ct = true;

        yield return null;
    }

}
