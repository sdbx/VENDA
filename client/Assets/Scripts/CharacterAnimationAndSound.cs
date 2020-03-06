using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using DG.Tweening;

public class CharacterAnimationAndSound : MonoBehaviour
{

    [SerializeField]
    private AudioSource _audioSource;
    [SerializeField]
    private AudioClip _dash;
    [SerializeField]
    private float _dashVolume;
    [SerializeField]
    private AudioClip _attack;
    [SerializeField]
    private float _attackVolume;
    [SerializeField]
    private AudioClip _blood;
    [SerializeField]
    private float _bloodVolume;
    [SerializeField]
    private AudioClip _cooltime;
    [SerializeField]
    private float _cooltimeVolume;
    [SerializeField]
    private AudioClip _defenseOn;
    [SerializeField]
    private float _defenseOnVolume;
    [SerializeField]
    private AudioClip _defenseOff;
    [SerializeField]
    private float _defenseOffVolume;
    [SerializeField]
    private AudioClip _defenseSuccess;
    [SerializeField]
    private float _defenseSuccessVolume;

    //dash
    [SerializeField]
    private ParticleSystem _dashEffect;

     //blood   
    [SerializeField]
    private SingleUseParticle _bloodParticle;

    //defense
    [SerializeField]
    private SingleUseParticle _shieldPop;
    [SerializeField]
    private ParticleSystem _guardingEffect;
    [SerializeField]
    private SingleUseParticle _guardSuccessEffect;
    [SerializeField]
    private Light2D _guardLight;

    //aniamtor
    [SerializeField]
    private Animator _animator;

    private float _volume = 1f;

    public void SetVolume(float volume)
    {
        _volume = volume;
    }

    //walking
    public void Walk(float speed)
    {
        _animator.SetFloat("speed",speed);
    }

    //attack
    public void PlayAttackFront()
    {
        _animator.SetTrigger("front_attack");
    }
    public void PlayAttackBack()
    {
        _animator.SetTrigger("back_attack");
    }
    public void PlayAttackUp()
    {
        _animator.SetTrigger("up_attack");
    }
    public void PlayAttackDown()
    {
        _animator.SetTrigger("down_attack");
    }

    public void PlayAttackSound()
    {
        PlaySound(_attack,_attackVolume);
    }
    public void PlayCooltimeSound()
    {
        PlaySound(_cooltime,_cooltimeVolume);
    }

    //dash
    public void PlayDash()
    {
        _dashEffect.Play();
        PlaySound(_dash,_dashVolume);
    }

    public void PlayBloodEffect()
    {
        _bloodParticle.DuplicateAndPlay();
        PlaySound(_blood,_bloodVolume);
    }

    //defense
    public void PlayDefenseStartEffect()
    {
        _shieldPop.DuplicateAndPlay();
        _guardingEffect.Play();
        DOTween.To(()=>_guardLight.intensity, x=> _guardLight.intensity = x, 1, 0.25f);
        PlaySound(_defenseOn,_defenseOnVolume);
    }
    public void PlayDefenseExitEffect()
    {
        _shieldPop.DuplicateAndPlay();
        _guardingEffect.Stop();
        DOTween.To(()=>_guardLight.intensity, x=> _guardLight.intensity = x, 0, 0.25f);
        PlaySound(_defenseOff,_defenseOffVolume);
    }
    public void PlayDefenseSuccess()
    {
        _guardSuccessEffect.DuplicateAndPlay();
        PlaySound(_defenseSuccess,_defenseSuccessVolume);
    }

    public void PlaySound(AudioClip audio,float volume)
    {
        _audioSource.PlayOneShot(audio,volume*_volume);
    }

    public void PlayAnimation(aniType aniType)
    {
        switch ((int)aniType)
        {
            case 0:
                PlayAttackFront();
                break;
            case 1:
                PlayAttackBack();
                break;
            case 2:
                PlayAttackDown();
                break;
            case 3:
                PlayAttackUp();
                break;
            //방어 모션
            case 4:
                PlayDefenseStartEffect();
                break;
            case 5:
                PlayDefenseExitEffect();
                break;
            //가로 대쉬
            case 6:
                PlayDash();
                break;
            //피 이펙트
            case 7:
                PlayBloodEffect();
                break;
            case 8:
                PlayDefenseSuccess();
                break;
        }
    }
}


public enum aniType
{
    FrontAttack,
    BackAttack,
    DownAttack,
    UpAttack,
    DefenseStart,
    DefenseExit,
    Dash,
    Blood,
    DefenseSuccess,
}