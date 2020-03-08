using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BackgroundMusicController : MonoBehaviour
{

    [Range(0,1)]
    private float _volume = 1;

    [SerializeField]
    private AudioSource _sad;
    [SerializeField]
    private AudioSource _fun;
    // Start is called before the first frame update

    private bool _isPlaying = false;
    private bool _killAgain = false;

    private bool _isFading = false;

    [SerializeField]
    private float _funPlayTime = 0;

    DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> fading;

    private void PlayFun()
    {
        _isFading = true;
        fading = _sad.DOFade(0,1f).OnComplete(()=>{
            _sad.Pause();
            _fun.volume = 0;
            fading = _fun.DOFade(_volume*0.4f,1f).OnComplete(()=>{            
                _isFading = false;
            });
            _fun.Play();
        });
    }
    private void PlaySad()
    {
        _isFading = true;
        fading =_fun.DOFade(0,1f).OnComplete(()=>{
            _isPlaying = false;
            _fun.Stop();
            _sad.volume = 0;
            _sad.UnPause();
            fading = _sad.DOFade(_volume,1f).OnComplete(()=>{            
                _isFading = false;
            });
        });
    }


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            _volume-=0.1f;
            if(_isFading)
            {
                fading.endValue *= _volume/(_volume+0.1f);
            }
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            _volume += 0.1f;
            if (_isFading)
            {
                if(Mathf.Approximately(_volume,0.1f))
                    fading.endValue = _volume;
                else
                    fading.endValue *= _volume / (_volume - 0.1f);
            }
        }
        _volume = Mathf.Clamp01(_volume);
        if (_funPlayTime > 0)
        {
            if(!_isPlaying)
            {
                PlayFun();
                _isPlaying = true;
            }
            _funPlayTime-=Time.deltaTime;
        }
        else if(_funPlayTime<=0)
        {
            if(_isPlaying)
                PlaySad();
        }

        if(!_isFading)
        {
            if (!_isPlaying)
            {
                _sad.volume = _volume;
            }
            if (_isPlaying)
            {
                _fun.volume = 0.4f * _volume;
            }
        }

    }

    public void PlayFunAndReturn()
    {
        _funPlayTime = 7;
    }

}
