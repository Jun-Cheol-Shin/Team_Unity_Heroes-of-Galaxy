using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class AutoObjectDestroying : MonoBehaviour
{
    public float time;
    public WaitForSeconds destroyingWaitTime;
    public string name;

    protected Rigidbody objRigidbody;
    protected Transform player;

    private void Awake()
    {
        objRigidbody = GetComponent<Rigidbody>();
        player = SkillManager.Instance.player.transform;
    }

    private void OnEnable()
    {
        StartCoroutine(Destroying());
        transform.rotation = Quaternion.Euler(0, Random.rotation.y, 0);
        if(objRigidbody != null)
        {
            objRigidbody.AddForce(-player.right * 50f);
        }
    }

    private void OnDisable()
    {
        StopCoroutine(Destroying());
    }

    protected IEnumerator Destroying()
    {
        yield return destroyingWaitTime;

        MagazinePool.Instance.Push(name, this.gameObject);
    }
}
