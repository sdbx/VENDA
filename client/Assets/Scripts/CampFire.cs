using System.Collections.Generic;
using UnityEngine;

public class CampFire : MonoBehaviour
{
    [SerializeField]
    private float _cool = 0;
    private float _maxCool = 2;

    private Character _character;
    private void OnTriggerEnter2D(Collider2D other)
    {
        var cha = other.GetComponent<Character>();
        if (cha&&cha.isMe)
            _character=cha;
    }


    private void Update()
    {
        if(!_character)
            return;
        if (_cool > _maxCool)
        {
            _cool = 0;
            _character.Heal(9);
        }
        else _cool += Time.deltaTime;
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        var cha = other.GetComponent<Character>();
        if (cha && cha.isMe)
            _character = null;
    }
}