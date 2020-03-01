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
    private float _dashJumpPower = 0.5f;
    [SerializeField]
    private float _dashPower = 0.5f;

    [SerializeField]
    private JumpChecker _groundChecker;

    private Rigidbody2D _rigidbody;
    private Transform _transform;
    private Character _character;

    private int _dashPoint = 0;

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

        if (Input.GetKey(KeyCode.DownArrow))
        {
            _character.Defense();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //Jump
            if(_groundChecker.isGrounded)
            {
                Jump(_jumpPower);
                _dashPoint = 2;
                _groundChecker.isGrounded = false;
            } else {
                if (Input.GetKey(KeyCode.RightArrow)) {
                    Dash(_dashJumpPower,_dashPower);
                } else if (Input.GetKey(KeyCode.LeftArrow)) {
                    Dash(_dashJumpPower,-_dashPower);
                }
            }

        }
    }

    void FixedUpdate() {
       
    }

    void Jump(float power)
    {
        _rigidbody.AddForce(new Vector2(0,power), ForceMode2D.Impulse);
    }

    void Dash(float power, float frontPower)
    {
        if (_dashPoint > 0) {
            _rigidbody.velocity = new Vector2(frontPower,power);
            _dashPoint -= 1;
        }
    }

    void MoveX(float x)
    {
        _rigidbody.AddForce(new Vector2(x*Time.deltaTime,0), ForceMode2D.Impulse);
    }
}
