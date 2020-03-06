using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MainScreenCamera : MonoBehaviour
{
    [SerializeField]
    private Transform _parent;
    private Camera _camera;

    [SerializeField]
    private float _zOffset;

    
    // Start is called before the first frame update
    void Start()
    {
        _camera = GetComponent<Camera>();
        SetCamPivot(1,0);
    }

    public void SetCamPivotWithNoMovement(float x,float y)
    {
        var tempPos = _camera.transform.localPosition - new Vector3(0,0,_zOffset);
        SetCamPivot(x,y);
        _parent.position += tempPos - (_camera.transform.localPosition - new Vector3(0,0,_zOffset));
    }

    public void SetCamPivot(float x,float y)
    {
        float halfFieldOfView = _camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
        float halfHeightAtDepth = 10 * Mathf.Tan(halfFieldOfView);
        float halfWidthAtDepth = _camera.aspect * halfHeightAtDepth;

        var campos = new Vector3(-x*halfWidthAtDepth, -y*halfHeightAtDepth, 0);
        _camera.transform.localPosition = campos + new Vector3(0,0,_zOffset);
    }

    public void MoveToPos(Vector3 pos)
    {
        _parent.DOMove(pos,1f).SetEase(Ease.OutSine);
    }
}
