using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreItem : MonoBehaviour
{
    public int score;
    public ParticleSystem destroyParticles;
    MeshRenderer rend;
    Collider col;
    private void Start()
    {
        rend = GetComponent<MeshRenderer>();
        col = GetComponent<Collider>();
    }

    //collecting score item
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Score.scr.IncreaseScore(score);
            StartCoroutine(DestroyAnim());

            col.enabled = false;
        }
    }

    private void Update()
    {
        //idle rotation
        transform.Rotate(0, 45 * Time.deltaTime, 0);
    }
    IEnumerator DestroyAnim()
    {
        //anim parameters
        float needH = 1.5f;
        float flySpeed = 2;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale / 3;
        //randomizing rotation direction
        float[] rotSpeed = new float[3];
        for (int num = 0; num < rotSpeed.Length; num++)
        {
            rotSpeed[num] = Random.Range(45, 90);
            if (Random.Range(0, 2) == 1)
            {
                rotSpeed[num] = -rotSpeed[num];
            }
        }

        float startH = transform.position.y;
        float h = 0;
        do//animation
        {
            float coef = h / needH;
            float change = flySpeed * Time.deltaTime;
            h += change;
            transform.position += new Vector3(0, change, 0);
            transform.Rotate(new Vector3(rotSpeed[0], rotSpeed[1], rotSpeed[2]) * Time.deltaTime);
            transform.localScale = Vector3.Lerp(startScale, endScale, coef);
            yield return new WaitForEndOfFrame();
        } while (h < needH);
        //particle emit at the end of animation
        rend.enabled = false;
        transform.rotation = Quaternion.identity;
        destroyParticles.Play();
        yield return new WaitForSeconds(destroyParticles.main.duration + destroyParticles.main.startLifetime.constantMax);
        //destroy when finished
        Destroy(gameObject);
    }
}
