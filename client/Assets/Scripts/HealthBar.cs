using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    GameObject _bar;
    float maxScale;

    private void Start()
    {
        maxScale = _bar.transform.localScale.x;
    }

    public void setValue(float v)
    {
        var currentScale =  _bar.transform.localScale; 
        _bar.transform.localScale = new Vector3(maxScale*v,currentScale.y,currentScale.z);
    }
}
