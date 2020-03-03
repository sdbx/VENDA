using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraEffect : MonoBehaviour
{
    [SerializeField]
    Camera _camera;
    float _shake = 0;
    [SerializeField]
    float _shakeAmount = 0.5f;
    [SerializeField]
    CanvasGroup _effectImage;


    void Update()
    {
        if (_shake > 0)
        {
            var randomVector = Random.insideUnitCircle * _shakeAmount;

            var pos = _camera.transform.position;
            pos.x+=randomVector.x;
            pos.y+=randomVector.y;
            _camera.transform.position = pos;
            _shake -= Time.deltaTime;
        }
        else
        {
            _shake = 0.0f;
        }
    }

    public void Shake(float seconds)
    {
        _shake = seconds;
    }

    public void ShowBloodOnScreen()
    {
        _effectImage.DOFade(0.5f,1).OnComplete(()=>{
            _effectImage.DOFade(0,1);
        });
    }

}
