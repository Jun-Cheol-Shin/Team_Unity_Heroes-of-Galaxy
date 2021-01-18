using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoBulletDestroying : AutoObjectDestroying
{

    private void OnEnable()
    {
        StartCoroutine(Destroying());
        //transform.rotation = Quaternion.Euler(Random.rotation.x, 0, 0);
        if(objRigidbody != null)
        {
            objRigidbody.AddForce(player.right * 100f);
        }
    }


    private void OnDisable()
    {
        StopCoroutine(Destroying());
    }



}
