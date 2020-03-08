using System.Collections.Generic;
using UnityEngine;

public class CampFire : MonoBehaviour
{

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
        _character.Heal(0.3f*Time.deltaTime);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        var cha = other.GetComponent<Character>();
        if (cha && cha.isMe)
            _character = null;
    }
}