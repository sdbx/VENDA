using UnityEngine;
using UnityEngine.UI;

public class DeadHead : MonoBehaviour 
{
    [SerializeField]
    private Text headName;

    public void SetName(string name)
    {
        headName.text = name;
    }   
}