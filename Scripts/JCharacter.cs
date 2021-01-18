using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;
using Generics.Dynamics;

public class JCharacter
{
    // characterController
    public float cc_slopeLimit;
    public float cc_stepOffset;
    public float cc_skinWidth;
    public float cc_minMoveDistance;
    public Vector3 cc_center;
    public float cc_radius;
    public float cc_height;

    // Player_Movement 코드
    public Vector3 p_playerpos;
    public Vector3 p_playerrot;
    public float p_smooth;
    //public bool p_isDead;
    public bool p_onGravityField;
    public float p_rollDistance;
    public float p_rollReduction;
    public float p_jumpPower;
    public float p_swapDelay;
    public float p_walkSpeed;
    public float p_jogSpeed;
    public float p_runSpeed;
    public float p_meleeDistance;
    public float p_rotateTurnSpeed;

    public Vector3 p_pivot;
    public Vector3 p_target;

    // Player_IK
    public float ik_smoothDamp;

    // CharacterAttibute
    public string c_name;
    public int c_maxHealth;
    public int c_health;
    public int c_maxShield;
    public int c_shield;
    public int c_shieldRecoverValue;
    public float c_shieldRecoverStart;
    public float c_shieldRecoverInterval;
    public string c_faceSprite;

    // WeaponAnimation
    //..

    // WeaponManager
    public string w_mainWeapon;
    public string w_subWeapon;
    public string[] w_specialWeapon;

    // Granade
    public int g_currentClip;
    public float g_throwPower;
    public float g_explosionTime;
    public float g_explosionDistance;
    public float g_throwRate;
    public int g_damage;
    public int g_bounceCount;
    public bool g_isThrow;
    public bool g_isSticky;
    public bool g_isAir;
    public bool g_isPenetrate;
    public bool g_isStun;
    public bool g_isRadiation;
    public bool g_isSlow;
    public bool g_isDouble;
    public bool g_isAttraction;
    public bool g_isWeakness;
    public bool g_isAI;

    // GrenadeManager
    public bool gm_isEnemy;
    public bool gm_isFriend;


    // Camera_Follow
    public float cf_Sensitivity;
    public float cf_ClampAngle;
    //public float cf_CrouchGain;

    // Camera_Collision
    public float cc_smooth;
    public Vector3 cc_idle;
    public Vector3 cc_zoom;
    public Vector3 cc_aiming;

    // SubCameraPosition
    public Vector3 sc_pos;

    // Particles
    public string bleeding;
    public Vector3 bleeding_pos;
    public string stun;
    public Vector3 stun_pos;
    public string radiation;
    public Vector3 radiation_pos;
    public string burn;
    public Vector3 burn_pos;



    public void SetCharacter(GameObject obj)
    {
        CharacterController cc = obj.GetComponent<CharacterController>();

        cc_slopeLimit = cc.slopeLimit;
        cc_stepOffset = cc.stepOffset;
        cc_skinWidth = cc.skinWidth;
        cc_minMoveDistance = cc.minMoveDistance;
        cc_center = cc.center;
        cc_radius = cc.radius;
        cc_height = cc.height;

        Player_Movement player = obj.GetComponent<Player_Movement>();
        p_smooth = player.smooth;
        //p_isDead = player.isDead;
        p_onGravityField = player.onGravityField;
        //p_isSwap = player.isSwap;
        p_rollDistance = player.rollDistance;
        p_rollReduction = player.rollReduction;
        p_jumpPower = player.JumpPower;
        p_swapDelay = player.SwapDelay;
        p_walkSpeed = player.walkSpeed;
        p_jogSpeed = player.jogSpeed;
        p_runSpeed = player.runSpeed;
        p_meleeDistance = player.MeleeDistance;
        p_rotateTurnSpeed = player.RotateturnSpeed;

        Player_IK handIK = obj.GetComponent<Player_IK>();
        ik_smoothDamp = handIK.smoothDamp;

        CharacterAttributes attribute = obj.GetComponent<CharacterAttributes>();
        c_name = attribute.name;
        c_maxHealth = attribute.maxHealth;
        c_health = attribute.health;
        c_maxShield = attribute.maxShield;
        c_shield = attribute.shield;
        c_shieldRecoverValue = attribute.shieldRecoverValue;
        c_shieldRecoverStart = attribute.shieldRecoverStartTime;
        c_shieldRecoverInterval = attribute.shieldRecoverIntervalTime;
        c_faceSprite = attribute.faceSprite.name;

        WeaponManager weapon = obj.GetComponent<WeaponManager>();

        w_mainWeapon = weapon.m_MainWeapon;
        w_subWeapon = weapon.m_SubWeapon;
        w_specialWeapon = new string[weapon.m_SpecialWeapon.Count];

        for(int i=0; i < weapon.m_SpecialWeapon.Count; i++)
        {
            w_specialWeapon[i] = weapon.m_SpecialWeapon[i];
        }
    }
}
