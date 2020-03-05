using UnityEngine;
using UnityEngine.UI;

public class NicknameSetter : MonoBehaviour 
{
    public string _settedName = "";
    [SerializeField]
    private InputField _inputField;
    [SerializeField]
    private MainMenu _mainMenu;
    
    private bool _isSetting = false;

    private void Awake() 
    {
        DontDestroyOnLoad(gameObject);
    }


    private void Update()
    {
        if(!_isSetting)
            return;

        _inputField.enabled = true;
        _inputField.Select();
        _inputField.ActivateInputField();
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SetName();
        }
    }

    public void SetName()
    {
        _isSetting = false;
        _settedName = _inputField.text;
        _mainMenu.CamToMain();
        _inputField.enabled = false;
    }

    public void ActiveSetting()
    {
        _isSetting = true;
    }
}