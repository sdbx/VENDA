using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixRotation : MonoBehaviour
{

    void FixedUpdate()
    {
        gameObject.transform.rotation = Quaternion.Euler(0,0,0);
    }
}
