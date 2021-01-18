using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Player;

public class SprayingBomb : MonoBehaviour
{

    public Vector3 start;
    public Vector3 destination;

    public float journeyTime = 0f;

    public GameObject effect;

    Vector3 center;
    Vector3 RelCenter;
    Vector3 aimRelCenter;
    private float startTime;

    public Explosion explosion;

    public AudioSource audioSource;
    public AudioSource exaudio;
    bool moveEnd = false;

    float rotationX = 0f;

    public WaitForSeconds stime;


    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        //explosion = SkillManager.Instance.Explosion.GetComponent<Explosion>();
        //exaudio = explosion.GetComponent<AudioSource>();
    }

    public IEnumerator BombExplosion()
    {
        yield return stime;

        effect = SkillManager.Instance.GetObject();
        effect.transform.position = transform.position;
        explosion = effect.GetComponent<Explosion>();

        StartCoroutine(explosion.switchOnOff());
        explosion.start = new Vector3(transform.position.x, transform.position.y + explosion.BoxSize.y * 0.5f, transform.position.z);
        //Instantiate(effect, transform.position, effect.transform.rotation);

        this.gameObject.SetActive(false);
        transform.SetParent(SkillManager.Instance.transform);

        moveEnd = false;
        rotationX = 0f;
    }


    public void setCenter()
    {
        startTime = Time.time;
        center = (start + destination) * 0.5f;
        center.y -= 4.0f;
        RelCenter = start - center;
        aimRelCenter = destination - center;

        float tm = Vector3.Distance(start, destination) * 0.1f;

        journeyTime = tm;
    }


    private void Update()
    {
        if(!moveEnd)
        {

            float fracComplete = (Time.time - startTime) / journeyTime;
            transform.position = Vector3.Slerp(RelCenter, aimRelCenter, fracComplete);
            transform.position += center;

            float randomRotationX = Random.Range(10f, 30f);

            transform.Rotate(0f, 0f, rotationX + randomRotationX);

            if(transform.position == destination)
            {
                moveEnd = true;
                SoundManager.instance.PlaySFX(audioSource, "THUD_Dark_03_Short_mono");
            }
        }
    }
}
