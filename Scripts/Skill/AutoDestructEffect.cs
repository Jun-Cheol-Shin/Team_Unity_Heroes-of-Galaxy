using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestructEffect : MonoBehaviour
{

    public bool OnlyDeactivate;


    private void OnEnable()
    {
        StartCoroutine(CheckIfAlive());
    }


    IEnumerator CheckIfAlive()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();

        while(true && ps != null)
        {
            yield return new WaitForSeconds(0.1f);

            if(!ps.IsAlive(true))
            {
                if(OnlyDeactivate)
                {
                    this.gameObject.SetActive(false);
                }

                else
                {
                    Destroy(this.gameObject);
                }

                break;
            }
        }

    }
}
