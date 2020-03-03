using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ProgressBar : MonoBehaviour
{
    [SerializeField]
    private Transform _bar;
    [SerializeField]
    private float _smooth = 0.25f;
    [SerializeField]
    private bool _invisibleWhenFull;

    public void setValue(float v)
    {
        v = Mathf.Clamp01(v);
        if(_invisibleWhenFull)
        {
            if (Mathf.Approximately(v,1))
                gameObject.SetActive(false);
            else
                gameObject.SetActive(true);
        }

        _bar.DOScaleX(v,_smooth);
    }
}
