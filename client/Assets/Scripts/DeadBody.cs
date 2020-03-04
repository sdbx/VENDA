using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadBody : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D[] _parts;

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(30f);
        Destroy(gameObject);
    }

    void Start()
    {
        Timer();
        foreach(var part in _parts)
        part.AddForce(Random.insideUnitCircle*Random.Range(200,300));
    }
}
