using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleUseParticle : MonoBehaviour
{
    private ParticleSystem _particleSystem;
    private bool _isPlayed;

    void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void Update() 
    {
        if(_isPlayed&&!_particleSystem.isPlaying)
            Destroy(gameObject);
    }

    public void Play()
    {
        _particleSystem.Play();
        _isPlayed = true;
    }

    // public void DuplicateAndPlay(Quaternion roatation)
    // {
    //     Instantiate<SingleUseParticle>(this,transform.position,roatation).Play();
    // }
    public void DuplicateAndPlay()
    {
        Instantiate<SingleUseParticle>(this,transform.position,transform.rotation).Play();
    }

}
