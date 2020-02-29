using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    GameObject _bar;

    private void Start()
    {
    }

    public void setValue(float v)
    {
        var currentScale =  _bar.transform.localScale; 
        _bar.transform.localScale = new Vector3(v,currentScale.y,currentScale.z);
    }
}
