using Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class PlayableSkill : MonoBehaviour
{

    [Header("스킬사용가능")]
    [SerializeField]
    public bool enable;

    // 쿨타임
    protected WaitForSeconds c_time;
    // 스킬 시전 시간
    protected WaitForSeconds s_time;

    public float cooltimeCheck;
    public float cooltime;
    public float skilltime;

    public GameObject effect;

    protected Animator anim;
    protected Player_Movement state;
    protected Player_WeaponAnimation weapon;
    protected WeaponManager weaponManager;
    public CharacterAttributes attributes;
    public CameraFollow cameraFollow;


    private void Start()
    {
        attributes = GameManager.instance.player.GetComponent<CharacterAttributes>();
    }

    public virtual void Shot() { }

    protected virtual IEnumerator ActivatingSkill()
    {
        yield return null;
    }
        
    protected IEnumerator StateChange(string name)
    {
        while(!anim.GetCurrentAnimatorStateInfo(0).IsTag(name))
        {
            //Debug.Log("아직 애니메이션으로 안감");
            yield return null;
        }

        while(anim.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1f)
        {
            //Debug.Log("애니메이션 실행 중");
            if(!anim.GetCurrentAnimatorStateInfo(0).IsTag(name))
            {
                //Debug.Log("이 애니메이션이 아니라서 탈출");
                break;
            }
            yield return null;
        }

        state.p_state = STATE.IDLE;
        state.p_Astate = AIM_STATE.NONE;
        yield return null;
    }


    protected virtual IEnumerator WaitSkillCooltime()
    {
        UIManager.Instance.UsePlayerSkill(this);

        if (attributes == null)
        {
            attributes = GameManager.instance.player.GetComponent<CharacterAttributes>();
        }

        if (attributes.SearchPerk(2007) != null)
        {
            attributes.AddBuff("Switch ON", attributes.gameObject);
        }

        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            cooltimeCheck += 0.1f;

            if (cooltimeCheck >= cooltime)
            {
                enable = true;
                break;
            }
        }
        cooltimeCheck = 0f;

        yield return null;
    }
}
