using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadBody : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D[] _parts;
    [SerializeField]
    private AudioSource _audio;

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(30f);
        Destroy(gameObject);
    }

    public void DestroyBody(float volume)
    {
        _audio.Play();
        StartCoroutine(Timer());
        foreach(var part in _parts)
        part.AddForce(Random.insideUnitCircle*Random.Range(200,300));
    }
}
