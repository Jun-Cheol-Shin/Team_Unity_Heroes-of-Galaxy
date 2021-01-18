using UnityEngine;

namespace Player
{
    public class Player_WeaponAnimation : MonoBehaviour
    {

        // 캐릭터 등, 허벅지 무기차는 곳
        public GameObject Pose_MainWeapon;
        public GameObject Pose_SubWeapon;

        // 오른손 1번,2번
        public GameObject type1;
        public GameObject type2;


        Player_Movement playermovement;
        AllyController allyController;
        public WeaponManager weaponManager;


        Vector3 originMainPose;
        Vector3 originMainRot;

        Vector3 originSubPose;
        Vector3 originSubRot;

        public GameObject Bomb;

        GameObject main_Mag;
        GameObject sub_Mag;

        GameObject[] detatch_mag;
        GameObject[] attach_mag;

        WEAPON_STATE pre_state;

        private void Start()
        {
            playermovement = GetComponent<Player_Movement>();
            allyController = GetComponent<AllyController>();
            weaponManager = GetComponent<WeaponManager>();

            originMainPose = weaponManager.m_MainWeaponObject.transform.localPosition;
            originMainRot = weaponManager.m_MainWeaponObject.transform.localRotation.eulerAngles;

            originSubPose = weaponManager.m_SubWeaponObject.transform.localPosition;
            originSubRot = weaponManager.m_SubWeaponObject.transform.localRotation.eulerAngles;


            if (playermovement != null)
            {
                switch (playermovement.p_Wstate)
                {
                    case WEAPON_STATE.MAIN_WEAPON:
                        weaponManager.m_SubWeaponObject.transform.SetParent(Pose_SubWeapon.transform);
                        weaponManager.m_SubWeaponObject.transform.localPosition = Vector3.zero;
                        weaponManager.m_SubWeaponObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
                        break;

                    case WEAPON_STATE.SUB_WEAPON:
                        weaponManager.m_MainWeaponObject.transform.SetParent(Pose_MainWeapon.transform);
                        weaponManager.m_MainWeaponObject.transform.localPosition = Vector3.zero;
                        weaponManager.m_MainWeaponObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
                        break;
                }
            }

            else if (allyController != null)
            {
                if (allyController.weaponType == WeaponType.MainWeapon)
                {
                    weaponManager.m_SubWeaponObject.transform.SetParent(Pose_SubWeapon.transform);
                    weaponManager.m_SubWeaponObject.transform.localPosition = Vector3.zero;
                    weaponManager.m_SubWeaponObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
                }
                else
                {
                    weaponManager.m_MainWeaponObject.transform.SetParent(Pose_MainWeapon.transform);
                    weaponManager.m_MainWeaponObject.transform.localPosition = Vector3.zero;
                    weaponManager.m_MainWeaponObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
                }
            }

            //weaponManager.m_MainWeaponObject.SetActive(true);
            //weaponManager.m_SubWeaponObject.SetActive(true);

            Bomb = Resources.Load("Character/Skill/Bomb") as GameObject;
            int count;

            if (weaponManager.m_MainWeapon == "006" || weaponManager.m_MainWeapon == "008")
            {
                count = weaponManager.m_MainWeaponObject.transform.childCount - 2;
                main_Mag = weaponManager.m_MainWeaponObject.transform.GetChild(count).gameObject;
            }

            else
            {
                count = weaponManager.m_MainWeaponObject.transform.childCount - 1;
                main_Mag = weaponManager.m_MainWeaponObject.transform.GetChild(count).gameObject;
            }

            count = weaponManager.m_SubWeaponObject.transform.childCount - 1;
            sub_Mag = weaponManager.m_SubWeaponObject.transform.GetChild(count).gameObject;

            SettingMag();
        }

        void SettingMag()
        {

            switch (weaponManager.m_MainWeapon)
            {
                case "001":
                    ReloadType = 0;
                    attach_mag = new GameObject[2];
                    a1 = main_Mag.transform.localPosition;
                    b1 = new Vector3(a1.x, a1.y, 0.35f);
                    break;
                case "002":
                    ReloadType = 0;
                    attach_mag = new GameObject[2];
                    break;

                case "011":
                case "012":
                    ReloadType = 6;
                    detatch_mag = new GameObject[6];
                    attach_mag = new GameObject[6];
                    break;

                default:
                    ReloadType = 1;
                    detatch_mag = new GameObject[1];
                    attach_mag = new GameObject[1];
                    break;
            }
        }

        GameObject grenade = null;
        void GenarateGrenade()
        {
            if (grenade == null)
            {
                grenade = Instantiate(Bomb, playermovement.RightHand.transform);
                grenade.transform.localPosition = Vector3.zero;
                grenade.transform.localRotation = Quaternion.Euler(Vector3.zero);
            }
            else
            {
                grenade.SetActive(true);
            }
        }
        public void DestroyGrenade()
        {
            grenade.SetActive(false);
        }

        public void ResetMain()
        {

            //Debug.Log(weaponManager.m_CurrentWeapon);


            weaponManager.m_MainWeaponObject.transform.SetParent(Pose_MainWeapon.transform);
            weaponManager.m_MainWeaponObject.transform.localPosition = Vector3.zero;
            weaponManager.m_MainWeaponObject.transform.localRotation = Quaternion.Euler(Vector3.zero);


            //weaponManager.m_CurrentWeapon.transform.SetParent(Pose_MainWeapon.transform);
            //  weaponManager.m_CurrentWeapon.transform.localPosition = Vector3.zero;
            //  weaponManager.m_CurrentWeapon.transform.localRotation = Quaternion.Euler(Vector3.zero);



            weaponManager.WeaponOff();

            // Debug.Log(weaponManager.m_CurrentWeapon);
        }

        public void SetMain()
        {
            if (playermovement != null)
            {
                playermovement.p_Wstate = WEAPON_STATE.MAIN_WEAPON;
            }

            else if (allyController != null)
            {
                allyController.weaponType = WeaponType.MainWeapon;
            }

            weaponManager.MainWeaponSwap();

            weaponManager.m_MainWeaponObject.transform.SetParent(type1.transform);
            weaponManager.m_MainWeaponObject.transform.localPosition = originMainPose;
            weaponManager.m_MainWeaponObject.transform.localRotation = Quaternion.Euler(originMainRot);
            SettingMag();
        }

        public void SetSub()
        {
            if (playermovement != null)
            {
                playermovement.p_Wstate = WEAPON_STATE.SUB_WEAPON;
            }

            else if (allyController != null)
            {
                allyController.weaponType = WeaponType.SubWeapon;
            }

            weaponManager.SubWeaponSwap();

            //Debug.Log(weaponManager.m_CurrentWeapon);

            weaponManager.m_SubWeaponObject.transform.SetParent(type2.transform);
            weaponManager.m_SubWeaponObject.transform.localPosition = originSubPose;
            weaponManager.m_SubWeaponObject.transform.localRotation = Quaternion.Euler(originSubRot);

            //weaponManager.m_CurrentWeapon.transform.SetParent(type2.transform);
            //weaponManager.m_CurrentWeapon.transform.localPosition = originSubPose;
            //weaponManager.m_CurrentWeapon.transform.localRotation = Quaternion.Euler(originSubRot);

            //Debug.Log(weaponManager.m_CurrentWeapon);
            SettingMag();
        }

        public void ResetSub()
        {

            weaponManager.m_SubWeaponObject.transform.SetParent(Pose_SubWeapon.transform);
            weaponManager.m_SubWeaponObject.transform.localPosition = Vector3.zero;
            weaponManager.m_SubWeaponObject.transform.localRotation = Quaternion.Euler(Vector3.zero);

            //weaponManager.m_CurrentWeapon.transform.SetParent(Pose_SubWeapon.transform);
            //weaponManager.m_CurrentWeapon.transform.localPosition = Vector3.zero;
            //weaponManager.m_CurrentWeapon.transform.localRotation = Quaternion.Euler(Vector3.zero);

            weaponManager.WeaponOff();
        }



        void SetSpecial()
        {
            if (playermovement != null)
            {
                switch (playermovement.p_Wstate)
                {
                    case WEAPON_STATE.MAIN_WEAPON:
                        pre_state = WEAPON_STATE.MAIN_WEAPON;
                        ResetMain();
                        break;

                    case WEAPON_STATE.SUB_WEAPON:
                        pre_state = WEAPON_STATE.SUB_WEAPON;
                        ResetSub();
                        break;
                }
                playermovement.p_Wstate = WEAPON_STATE.SPECIAL_WEAPON;
            }

            if (allyController != null)
            {
                switch (allyController.weaponType)
                {
                    case WeaponType.MainWeapon:
                        pre_state = WEAPON_STATE.MAIN_WEAPON;
                        weaponManager.m_MainWeaponObject.SetActive(false);
                        ResetMain();
                        break;

                    case WeaponType.SubWeapon:
                        pre_state = WEAPON_STATE.SUB_WEAPON;
                        weaponManager.m_SubWeaponObject.SetActive(false);
                        ResetSub();
                        break;
                }
                allyController.weaponType = WeaponType.SpecialWeapon;
            }
            // 바꿔야댐
            weaponManager.SpecialWeaponSwap(weaponManager.m_SpecialWeapon[0]);
            SettingMag();
        }

        void ResetSpecial()
        {
            weaponManager.m_SpecialWeaponObject.SetActive(false);

            switch (pre_state)
            {
                case WEAPON_STATE.SUB_WEAPON:
                    weaponManager.SubWeaponSwap();
                    weaponManager.m_SubWeaponObject.SetActive(true);
                    SetSub();
                    break;

                case WEAPON_STATE.MAIN_WEAPON:
                    weaponManager.MainWeaponSwap();
                    weaponManager.m_MainWeaponObject.SetActive(true);
                    SetMain();
                    break;
            }
        }

        GameObject mag;
        string weapon = "";
        int ReloadType;

        public Vector3 RandomSphereInPoint(Vector3 pos)
        {
            Vector3 getPoint = Random.onUnitSphere;
            getPoint.y = 0f;

            float r = Random.Range(0f, 0.3f);

            return (getPoint * r) + pos;
        }


        void MagazineAnimation(string state)
        {
            if (playermovement == null)
            {
                return;
            }

            if (playermovement.p_Wstate == WEAPON_STATE.MAIN_WEAPON)
            {
                weapon = "rifle_Mag";
                mag = main_Mag;
            }
            else
            {
                weapon = "pistol_Mag";
                mag = sub_Mag;
            }

            ReloadMethod(state);
        }


        void SetUpdate(string state)
        {
            Updatestate = state;
        }

        string Updatestate;
        Quaternion a = Quaternion.Euler(new Vector3(0, 0, 0));
        Quaternion b = Quaternion.Euler(new Vector3(50, 0, 0));
        Vector3 a1;
        Vector3 b1;
        void ReloadMethod(string state)
        {

            Updatestate = state;
            switch (state)
            {
                case "detatch":
                    for (int i = 0; i < ReloadType; i++)
                    {
                        detatch_mag[i] = MagazinePool.Instance.Pop(weapon);
                        detatch_mag[i].transform.SetParent(playermovement.LeftHand.transform);
                        detatch_mag[i].transform.localPosition = Vector3.zero;
                    }

                    if (ReloadType == 1)
                    {
                        mag.SetActive(false);
                    }
                    break;

                case "drop":
                    for (int i = 0; i < ReloadType; i++)
                    {
                        detatch_mag[i].transform.SetParent(null);
                        detatch_mag[i].GetComponent<Rigidbody>().useGravity = true;
                        while (i > 0)
                        {
                            int count = 0;
                            detatch_mag[i].transform.localPosition = RandomSphereInPoint(detatch_mag[i].transform.localPosition);
                            for (int j = 0; j < i; j++)
                            {
                                if (Vector3.Distance(detatch_mag[j].transform.localPosition, detatch_mag[i].transform.localPosition) > 0.15f)
                                {
                                    ++count;
                                }
                            }

                            if (count == i)
                            {
                                break;
                            }
                        }
                    }
                    break;

                case "refill":
                    attach_mag[0] = MagazinePool.Instance.Pop(weapon);
                    attach_mag[0].transform.SetParent(playermovement.LeftHand.transform);
                    attach_mag[0].transform.localPosition = Vector3.zero;
                    break;

                case "attach":
                    if (ReloadType == 1)
                    {
                        mag.SetActive(true);
                    }
                    MagazinePool.Instance.Push(weapon, attach_mag[0]);
                    break;
            }
        }

        public void LeftHandUpdate()
        {
            if (playermovement == null)
            {
                return;
            }

            if (playermovement.p_Astate == AIM_STATE.RELOAD)
            {
                switch(Updatestate)
                {
                    case "detatch":
                        if(weaponManager.m_MainWeapon == "002")
                        {
                            main_Mag.transform.localRotation = Quaternion.Lerp(main_Mag.transform.localRotation, b, Time.deltaTime * 20f);
                        }

                        else if(weaponManager.m_MainWeapon == "001")
                        {
                            main_Mag.transform.localPosition = Vector3.Lerp(main_Mag.transform.localPosition, b1, Time.deltaTime * 20f);
                        }

                        break;
                    case "drop":
                        if (weaponManager.m_MainWeapon == "002")
                        {
                            main_Mag.transform.localRotation = Quaternion.Lerp(main_Mag.transform.localRotation, b, Time.deltaTime * 20f);
                        }

                        else if (weaponManager.m_MainWeapon == "001")
                        {
                            main_Mag.transform.localPosition = Vector3.Lerp(main_Mag.transform.localPosition, b1, Time.deltaTime * 20f);
                        }
                        break;
                    case "refill":
                        if (weaponManager.m_MainWeapon == "002")
                        {
                            main_Mag.transform.localRotation = Quaternion.Lerp(main_Mag.transform.localRotation, b, Time.deltaTime * 20f);
                        }

                        else if (weaponManager.m_MainWeapon == "001")
                        {
                            main_Mag.transform.localPosition = Vector3.Lerp(main_Mag.transform.localPosition, b1, Time.deltaTime * 20f);
                        }

                        attach_mag[0].transform.localPosition = Vector3.zero;
                        attach_mag[0].transform.localRotation = Quaternion.Euler(Vector3.zero);
                        break;
                    case "attach":
                        if (weaponManager.m_MainWeapon == "002")
                        {
                            main_Mag.transform.localRotation = Quaternion.Lerp(main_Mag.transform.localRotation, a, Time.deltaTime * 20f);

                            if(Vector3.Distance(main_Mag.transform.localRotation.eulerAngles, a.eulerAngles) < 0.05f)
                            {
                                Updatestate = "";
                            }
                        }

                        else if (weaponManager.m_MainWeapon == "001")
                        {
                            main_Mag.transform.localPosition = Vector3.Lerp(main_Mag.transform.localPosition, a1, Time.deltaTime * 20f);

                            if (Vector3.Distance(main_Mag.transform.localPosition, a1) < 0.05f)
                            {
                                Updatestate = "";
                            }
                        }
                        break;
                }

                if (playermovement.p_state == STATE.ROLL)
                {
                    if (weaponManager.m_MainWeapon == "002")
                    {
                        main_Mag.transform.localRotation = a;
                    }

                    else if (weaponManager.m_MainWeapon == "001")
                    {
                        main_Mag.transform.localPosition = a1;
                    }
                }
            }
        }

        void ResetAllHand()
        {
            allyController.ally_wState = AllyController.Ally_WeaponState.SKILL2HAND;/*
            if (!characterIK.rh_switch && !characterIK.lh_switch)
            {
                already = true;
            }

            characterIK.rh_switch = false;
            characterIK.lh_switch = false;*/
        }

        void WeaponShoot()
        {
            allyController.skill_S.Attack();
        }

        void EndAIGrenade()
        {
            if (allyController)
                allyController.EndGrenade();
        }

        void SetAllHand()
        {
            allyController.ally_wState = AllyController.Ally_WeaponState.AIMING;/*
            if (already)
            {
                already = false;
            }

            else
            {
                characterIK.rh_switch = true;
                characterIK.lh_switch = true;
            }*/
        }

        void EndSpecial()
        {
            SetAllHand();
            allyController.EndSkill();
        }

        //void SetRightHand()
        //{
        //    if(already_r)
        //    {
        //        already_r = false;
        //    }
        //    else
        //    {
        //        characterIK.rh_switch = true;
        //    }
        //}

        //void SetLeftHand()
        //{
        //    if(already_l)
        //    {
        //        already_l = false;
        //    }
        //    else
        //    {
        //        characterIK.lh_switch = true;
        //    }
        //}

        //void ResetRightHand()
        //{
        //    if(!characterIK.rh_switch)
        //    {
        //        already_r = true;
        //    }
        //    characterIK.rh_switch = false;
        //}

        //void ResetLeftHand()
        //{
        //    if(!characterIK.lh_switch)
        //    {
        //        already_l = true;
        //    }
        //    characterIK.lh_switch = false;
        //}


    }
}