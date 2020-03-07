using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour 
{
    [SerializeField]
    private MainScreenCamera _mainCam;
    [SerializeField]
    private Transform _mainCamPos;
    [SerializeField]
    private Transform _setnameCamPos;
    [SerializeField]
    private Transform _creditCamPos;

    [SerializeField]
    private string _gameScene;

    [SerializeField]
    private AudioSource _startMusic;    

    [SerializeField]
    private AudioSource _creditMusic;

    [SerializeField]
    private GameObject _creditText;
    public void CamToSetName()
    {
        _mainCam.SetCamPivotWithNoMovement(0,0);
        _mainCam.MoveToPos(_setnameCamPos.position);
    }

    public void CamToMain()
    {
        _mainCam.SetCamPivotWithNoMovement(1,0);
        _mainCam.MoveToPos(_mainCamPos.position);
        if (!_startMusic.isPlaying)
            _startMusic.Play();
        _creditMusic.Stop();
    }

    public void CamToCredit()
    {
        _mainCam.SetCamPivotWithNoMovement(-1,-1);
        _mainCam.MoveToPos(_creditCamPos.position);
        _startMusic.Stop();
        _creditMusic.Play();
        _creditText.GetComponent<Animator>().Play("CreditAnimation", -1, 0); 
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(_gameScene);
    }
}