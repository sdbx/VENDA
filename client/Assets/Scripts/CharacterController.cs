using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterController : MonoBehaviour
{
    [SerializeField]
    private float _speed = 0.3f;
    [SerializeField]
    private float _jumpPower = 0.5f;

    [SerializeField]
    private JumpChecker _groundChecker;

    private Rigidbody2D _rigidbody;
    private Transform _transform;
    private Character _character;


    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _transform = GetComponent<Transform>();
        _character = GetComponent<Character>();
    }

    void Update()
    {
        KeyInput();
    }

    void KeyInput()
    {

        //char attack

        //right attack
        if (Input.GetKey("d"))
        {
            _character.AttackRight();
        }

        //left attack
        if (Input.GetKey("a"))
        {
            _character.AttackLeft();
        }

        //up attack
        if (Input.GetKey("w"))
        {
            _character.AttackUp();
        }

        //down attack
        if (Input.GetKey("s"))
        {
            _character.AttackDown();
        }



        //char movement
        if (Input.GetKey(KeyCode.RightArrow))
        {
            MoveX(_speed);
        }
        
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            MoveX(-_speed);
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            //Jump
            if(_groundChecker.isGrounded)
            {
                Jump(_jumpPower);
                _groundChecker.isGrounded = false;
            }

        }
    }

    void Jump(float power)
    {
        _rigidbody.velocity = new Vector2(0,power);
    }

    void MoveX(float x)
    {
        transform.Translate(Vector3.right * x * Time.deltaTime, Space.World);
    }

}
