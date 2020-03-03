using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attacks : MonoBehaviour
{
    [SerializeField]
    private Character _character;

    public void AttackFront()
    {
        _character.AttackFront();
    }

    public void AttackBack()
    {
           _character.AttackBack();
    }

    public void AttackDown()
    {
        _character.AttackDown();
    }

    public void AttackUp()
    {
        _character.AttackUp();
    }

    public void AttackEnd()
    {
        _character.AttackEnemyEnd();
    }
}
