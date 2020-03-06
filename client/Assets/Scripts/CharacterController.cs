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
    private List<Transform> _feetPoses;
    Vector3 _feetleftOffset;
    Vector3 _feetrightOffset;
    Vector3 _feetCenterOffset;
    [SerializeField]
    float _checkRectSize;
    
    [SerializeField]
    private float _groundCheckDistance;
    [SerializeField]
    private LayerMask whatIsGround;


    private Rigidbody2D _rigidbody;
    private Transform _transform;
    private Character _character;

    [SerializeField]
    private int _dashPoint = 0;

    private int _xMoveDir = 0;
    private Dir _dashDir = Dir.None;
    private Dir _arrowDir = Dir.None;
    
    private bool _jump = false;

    [SerializeField]
    private float _maxClimbAngle;

    
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


    [SerializeField]
    private Vector3 _currentGroundVector;

    [SerializeField]
    private CapsuleCollider2D _charCollider;
    
    [SerializeField]
    private float _BothFeetCheckDistance = 0.6f;
    private bool _BothFeetOnGround;




    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _transform = GetComponent<Transform>();
        _character = GetComponent<Character>();

        var halfwidth = _charCollider.size.x/2;
        var halfheight = _charCollider.size.y/2;

        _feetleftOffset = new Vector2(-halfwidth+_charCollider.offset.x,-halfheight+_charCollider.offset.y);
        _feetrightOffset = new Vector2(halfwidth+_charCollider.offset.x,-halfheight+_charCollider.offset.y);
        _feetCenterOffset = new Vector3(_charCollider.offset.x,-halfheight+_charCollider.offset.y);
    }

    void Update()
    {
        var ray = Physics2D.CircleCast((Vector2)(_charCollider.transform.position)+_charCollider.offset,_charCollider.size.x/2,Vector2.down,_groundCheckDistance,whatIsGround);
        UpdateGroundAngle(ray);
        var checkGround = CheckIsGround(ray);
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

        if(_isGrounded)
            _dashPoint = 2;

        KeyInput();
    }

    void FixedUpdate()
    {

        if(_xMoveDir!=0)
        {
            MoveX(_speed*_xMoveDir);
            _xMoveDir = 0;
        }


        if(_isGrounded)
        {
            //fromGround
            switch (_dashDir)
            {
                case Dir.Right:
                    Dash(_dashPower/2, _dashPower);
                    break;
                case Dir.Left:
                    Dash(_dashPower/2, -_dashPower);
                    break;
                case Dir.Up:
                    Dash(_dashJumpPower+_dashPower, 0);
                    break;
            }
        }
        else if(_dashPoint == 2)
        {
            switch (_dashDir)
            {
                case Dir.Right:
                    Dash(_dashPower, _dashPower);
                    break;
                case Dir.Left:
                    Dash(_dashPower, -_dashPower);
                    break;
                case Dir.Up:
                    Dash(_dashJumpPower+_dashPower, 0);
                    break;
            }
        }
        else if(_dashPoint == 1)
        {
            switch (_dashDir)
            {
                case Dir.Right:
                    Dash(_dashJumpPower, _dashPower);
                    break;
                case Dir.Left:
                    Dash(_dashJumpPower, -_dashPower);
                    break;
                case Dir.Up:
                    Dash(_dashPower, 0);
                    break;
            }
        }
        
        if (_jump)
        {
            if (_isGrounded)
            {
                Jump(_jumpPower / 2);
                _isGrounded = false;
            }
            _jump = false;
            return;
        }


        _dashDir = Dir.None;
    }

    void KeyInput()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            _arrowDir = Dir.Right;
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            _arrowDir = Dir.Left;
        }
        else if (Input.GetKey(KeyCode.UpArrow))
        {
           _arrowDir = Dir.Up;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
           _arrowDir = Dir.Down;
        }
        else
        {
            _arrowDir = Dir.None;
        }



        //dash
        if (Input.GetKeyDown("s"))
        {
            _dashDir = _arrowDir;
            if (_arrowDir == Dir.None)
            {
                if(_isGrounded)
                    _jump = true;
                else
                    _dashDir = Dir.Up;
            }
        }
        else if (Input.GetKeyDown("w")||Input.GetKeyDown(KeyCode.Space))
        {
            _jump = true;
        }
        else if (Input.GetKeyDown("a"))
        {
            switch (_arrowDir)
            {
                case Dir.Right:
                    _character.StartRightAttack();
                    break;
                case Dir.Left:
                    _character.StartLeftAttack();
                    break;
                case Dir.Up:
                    _character.StartUpAttack();
                    break;
                case Dir.Down:
                    _character.StartDownAttack();
                    break;
            }
        }



        if (_arrowDir == Dir.Right)
            _xMoveDir = 1;
        else if (_arrowDir == Dir.Left)
            _xMoveDir = -1;


        
        _arrowDir = Dir.None;
        //char movement
        //dash keys

        //defense key
        if (Input.GetKeyDown(KeyCode.D))
        {
            _character.StartDefense();
        }
    }

    private dashKeyInputType GetArrowKeyInput(KeyCode keyCode)
    {
        // if(!Input.GetKey(keyCode))
        //     return dashKeyInputType.None;

        // if(Input.GetKeyDown(keyCode))
        // {
        //     if (CheckDoublePress(keyCode))
        //     {
        //         return dashKeyInputType.Double;
        //     }
        //     _prevKey = keyCode;
        //     _prevKeyTime = Time.time;
            
        // }
        // return dashKeyInputType.Single;

        if(!Input.GetKey(keyCode))
            return dashKeyInputType.None;

        if(Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("dash");
            return dashKeyInputType.Double;
            
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

    void Dash(float power, float frontPower)
    {
        if (_dashPoint > 0) {
            _rigidbody.velocity = new Vector2(frontPower,power);
            _dashPoint -= 1;
            _character.Update();

            _character.SendAnimeAndPlay(aniType.Dash);
        }
    }

    void MoveX(float x)
    {
        if (_isGrounded)
        {       
           _rigidbody.velocity = _currentGroundVector*x/10;
        }
        else
        {
            _rigidbody.AddForce(new Vector2(x*Time.deltaTime,0), ForceMode2D.Impulse);
        }
    }

    private void UpdateGroundAngle(RaycastHit2D ray)
    {
        if(!ray)
        {
            _currentGroundVector = Vector2.right;
            return;
        }
        _currentGroundVector = new Vector3(ray.normal.y,-ray.normal.x);
        
    }

    private bool CheckIsGround(RaycastHit2D ray)
    {
        if(!ray)
        {
            return false;
        }
        var angle = Mathf.Acos( Vector2.Dot(ray.normal,Vector2.up))*Mathf.Rad2Deg;
        return Mathf.Abs(angle)<_maxClimbAngle;
    }
    private bool RaycastGround(Vector3 pos,float distance)
    {
        var ray = Physics2D.Raycast(pos, transform.TransformDirection(Vector2.down), distance, whatIsGround);
        Debug.DrawRay(pos, Vector2.down * ray.distance, Color.yellow);
        return ray;
    }
}

enum dashKeyInputType
{
    None,
    Single,
    Double,
}


