using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    [SerializeField]
    GameObject up;
    [SerializeField]
    GameObject left;
    [SerializeField]
    GameObject right;
    [SerializeField]
    GameObject down;

    private IEnumerator asdf(GameObject gm)
    {
        gm.SetActive(true);   
        yield return new WaitForSeconds(1.0f);
        gm.SetActive(false);   
    }

    public void Walk()
    {

    }
    public void AttackRight()
    {
        StartCoroutine(asdf(right));
    }

    public void AttackLeft()
    {
        StartCoroutine(asdf(left));
    }
    public void AttackUp()
    {
        StartCoroutine(asdf(up));
    }
    public void AttackDown()
    {
        StartCoroutine(asdf(down));
    }
    public void Jump()
    {

    }
    public void JumpEnd()
    {

    }
    public void Falling()
    {

    }
    public void Protect()
    {

    }
}
