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

    public void CamToSetName()
    {
        _mainCam.SetCamPivotWithNoMovement(0,0);
        _mainCam.MoveToPos(_setnameCamPos.position);
    }

    public void CamToMain()
    {
        _mainCam.SetCamPivotWithNoMovement(1,0);
        _mainCam.MoveToPos(_mainCamPos.position);
    }

    public void CamToCredit()
    {
        _mainCam.SetCamPivotWithNoMovement(-1,-1);
        _mainCam.MoveToPos(_creditCamPos.position);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(_gameScene);
    }
}