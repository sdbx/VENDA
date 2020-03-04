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
    private Transform _feetPos;
    [SerializeField]
    private float _groundCheckRadius;
    [SerializeField]
    private LayerMask whatIsGround;


    private Rigidbody2D _rigidbody;
    private Transform _transform;
    private Character _character;

    private int _dashPoint = 0;

    private int _xMoveDir = 0;
    private Dir _dashDir = Dir.None;
    
    private bool _jump = false;
    
    //dash key double press check
    private KeyCode _prevKey;
    private float _prevKeyTime;
    [SerializeField]
    private float _doublePressTimeLimit = 0.5f;

    [SerializeField]
    private float _minGroundCheckTime = 3;
    [SerializeField]
    private float _groundCheckTime = 0;

    [SerializeField]
    private bool _isGrounded = false;

    [SerializeField]
    private bool _groundChecked = false;

    
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _transform = GetComponent<Transform>();
        _character = GetComponent<Character>();
    }

    void Update()
    {
        var checkGround = CheckIsGround();
        if(!_isGrounded&&checkGround)
        {
            if(_groundCheckTime<_minGroundCheckTime)
            {
                _groundCheckTime+=Time.deltaTime;
            }
            else
            {
                _groundCheckTime = 0;
                _isGrounded = true;
                _groundChecked = true;
            }
           
        }
        else
        {
            _groundCheckTime = 0;
        }
        if(!_groundChecked && checkGround)
        {
            _isGrounded = true;
            _groundChecked = true;
        }
        else if(!checkGround){
            _isGrounded = false;
            _groundChecked = false;
        }

        KeyInput();
    }

    void FixedUpdate()
    {

        if(_xMoveDir!=0)
        {
            MoveX(_speed*_xMoveDir);
            _xMoveDir = 0;
        }

        if (_jump)
        {
            if (_isGrounded)
            {
                Jump(_jumpPower/2);  
                _dashPoint = 2; 
                _isGrounded = false;
            }
            _jump = false;
            return;
        }

        if(_dashDir!=Dir.None&&_isGrounded)
        {
            Jump(_jumpPower);
            _dashPoint = 2;
            //_dashDir = Dir.None;
            return;
        }
        
        switch(_dashDir)
        {
            case Dir.Right:
                Dash(_dashJumpPower, _dashPower, true);
                break;
            case Dir.Left:
                Dash(_dashJumpPower, -_dashPower, true);
                break;
            case Dir.Up:
                Dash(_dashJumpPower + _dashPower, 0, false);
                break;
        }
        _dashDir = Dir.None;
    }

    void KeyInput()
    {
        //right attack
        if (Input.GetKey("d"))
        {
            _character.StartRightAttack();
        }

        //left attack
        if (Input.GetKey("a"))
        {
            _character.StartLeftAttack();
        }

        //up attack
        if (Input.GetKey("w"))
        {
            _character.StartUpAttack();
        }

        //down attack
        if (Input.GetKey("s"))
        {
            _character.StartDownAttack();
        }

        //char movement
        //dash keys
        switch(GetDashKeyInput(KeyCode.RightArrow))
        {
            case dashKeyInputType.Single:
            _xMoveDir = 1;
            break;
            case dashKeyInputType.Double:
            _dashDir = Dir.Right;
            break;
        }
        
        switch(GetDashKeyInput(KeyCode.LeftArrow))
        {
            case dashKeyInputType.Single:
            _xMoveDir = -1;
            break;
            case dashKeyInputType.Double:
            _dashDir = Dir.Left;
            break;
        }

        switch(GetDashKeyInput(KeyCode.UpArrow))
        {
            case dashKeyInputType.Single:
            _jump = true;
            break;
            case dashKeyInputType.Double:
            _dashDir = Dir.Up;
            break;
        }

        //defense key
        if (Input.GetKey(KeyCode.DownArrow))
        {
            _character.StartDefense();
        }
    }

    private dashKeyInputType GetDashKeyInput(KeyCode keyCode)
    {
        if(!Input.GetKey(keyCode))
            return dashKeyInputType.None;

        if(Input.GetKeyDown(keyCode))
        {
            if (CheckDoublePress(keyCode))
            {
                return dashKeyInputType.Double;
            }
            _prevKey = keyCode;
            _prevKeyTime = Time.time;
            
        }
        return dashKeyInputType.Single;
        
    }
    
    private bool CheckDoublePress(KeyCode keyCode)
    {
        return (_prevKey == keyCode) && (Time.time - _prevKeyTime<_doublePressTimeLimit);
    }

    void Jump(float power)
    {
        _rigidbody.AddForce(new Vector2(0,power), ForceMode2D.Impulse);
    }

    void Dash(float power, float frontPower,bool isHorizontal)
    {
        if (_dashPoint > 0) {
            _rigidbody.velocity = new Vector2(frontPower,power);
            _dashPoint -= 1;
            _character.Update();

            if(isHorizontal)
            {
                _character.SendAnimeAndPlay(aniType.DashHorizontal);
            }
            else
            {
                _character.SendAnimeAndPlay(aniType.DashDown);
            }        
        }
    }

    void MoveX(float x)
    {
        if(_isGrounded)
        {
            _rigidbody.velocity = new Vector2(x/10,_rigidbody.velocity.y);
        }
        else
        {
            _rigidbody.AddForce(new Vector2(x*Time.deltaTime,0), ForceMode2D.Impulse);
        }
    }

    private bool CheckIsGround()
    {
        return Physics2D.OverlapCircle(_feetPos.position,_groundCheckRadius,whatIsGround);
    }

}



enum dashKeyInputType
{
    None,
    Single,
    Double,
}


