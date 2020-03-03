using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadBody : MonoBehaviour
{
    private Rigidbody2D _rd;
    private void Awake() 
    {
        _rd = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        _rd.AddForce(Random.insideUnitCircle*Random.Range(200,300));
    }
}
