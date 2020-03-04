using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
    private SingleUseParticle _horizontalDashParticle;
    [SerializeField]
    private SingleUseParticle _downDashParticle;
    [SerializeField]

    //blood
    private SingleUseParticle _bloodParticle;

    //defense
    [SerializeField]
    private SingleUseParticle _shieldPop;
    [SerializeField]
    private ParticleSystem _guardingEffect;
    [SerializeField]
    private SingleUseParticle _guardSuccessEffect;

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
    public void PlayDashHorizontal()
    {
        _horizontalDashParticle.DuplicateAndPlay();
        PlaySound(_dash,_dashVolume);
    }
    public void PlayDashDown()
    {
        _downDashParticle.DuplicateAndPlay();
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
        PlaySound(_defenseOn,_defenseOnVolume);
    }
    public void PlayDefenseExitEffect()
    {
        _shieldPop.DuplicateAndPlay();
        _guardingEffect.Stop();
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
                PlayDashHorizontal();
                break;
            //세로대쉬
            case 7:
                PlayDashDown();
                break;
            //피 이펙트
            case 8:
                PlayBloodEffect();
                break;
            case 9:
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
    DashHorizontal,
    DashDown,
    Blood,
    DefenseSuccess,
}