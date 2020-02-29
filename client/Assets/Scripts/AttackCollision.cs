using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackCollision : MonoBehaviour
{
    List<Character> characters = new List<Character>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.tag == "Enemy")
            characters.Add(other.GetComponent<Character>());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.tag == "Enemy")
            characters.Remove(other.GetComponent<Character>());
    }

    public List<Character> GetHittedChars()
    {
        return characters;
    }
}
