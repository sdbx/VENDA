using System.Collections.Generic;
using UnityEngine;

public class DeadBodyOnPlayer : MonoBehaviour 
{
    [SerializeField]
    private Rigidbody2D _prefab;

   
    private List<Rigidbody2D> _bodies = new List<Rigidbody2D>();
    private Rigidbody2D _lastBody = null;

    private Rigidbody2D _firstBody = null;

    private Rigidbody2D _charRb;
    
    [SerializeField]
    private float _maxDistance = 0.5f;

    public void ResetBodies()
    {
        _lastBody = null;
        foreach(var body in _bodies)
            Destroy(body.gameObject);
        _bodies.Clear();
    }

    public void Add(Rigidbody2D charb)
    {
        _charRb = charb;
        if(!_lastBody)
            _lastBody = charb;
        _lastBody = Instantiate(_prefab, _lastBody.transform.position, Quaternion.Euler(0, 0, 0));
        _bodies.Add(_lastBody);
    }

    private void FixedUpdate() 
    {
        if(!_lastBody)
            return;
        
        for(int i = 0;i<_bodies.Count;i++)
        {
            if(i==0)
            {
                Follow(_charRb,_bodies[0]);
                continue;
            }
            Follow(_bodies[i-1],_bodies[i]);
        }

    }

    private void Follow(Rigidbody2D target,Rigidbody2D follower)
    {
        var _distance = Vector2.Distance(target.position,follower.position);

        if(_distance>_maxDistance)
        {
            var delta = -target.position+follower.position;
            var angle = Mathf.Atan2(delta.y,delta.x);
            //Debug.DrawRay(_charRb.position,delta);
            follower.position = target.position+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*_maxDistance;
            follower.velocity = target.velocity/2;
        }
    }
}