using UnityEngine;
using UnityEngine.UI;

public class KillCounter : MonoBehaviour 
{
    [SerializeField]
    private Text _killText;

    private int _count;


    public void Plus()
    {
        _count ++;
        UpdateCount();
    }

    public void Minus()
    {
        _count --;
        UpdateCount();
    }

    public void ResetCount()
    {
        _count = 0;
        UpdateCount();
    }

    private void UpdateCount()
    {
        _killText.text = _count + " KILL";
    }
}