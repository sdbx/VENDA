using UnityEngine;

public class ObjectFollower : MonoBehaviour 
{
    [SerializeField]
    private Transform _target;
    [SerializeField]
    private float _smooth = 0.125f;
    [SerializeField]
    private Vector3 _offset = new Vector3(0, 0, -10);
    
    void FixedUpdate()
    {
        if(_smooth == 0)
        {
            transform.position = _offset + _target.position;
            return;
        }
        Vector3 smoothPos = Vector2.Lerp(transform.position, _target.position, _smooth);
        transform.position = _offset + smoothPos;
    }
}