using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System;

public class Character : MonoBehaviour
{

    [SerializeField]
    private HealthBar hpbar;
    [SerializeField]
    private SocketManager _socketManager;
    public bool isMe = false;

    //attack collision
    [SerializeField]
    private AttackCollision _rightAttackCollision;
    [SerializeField]
    private AttackCollision _leftAttackCollision;
    [SerializeField]
    private AttackCollision _upAttackCollision;
    [SerializeField]
    private AttackCollision _downAttackCollision;


    //users info
    [SerializeField]
    private string _id;
    [SerializeField]
    private int _maxHp = 100;
    [SerializeField]
    private int _hp = 100;
    [SerializeField]
    private float _maxCooltime = 10;
    [SerializeField]
    private float _coolTime = 0;

    private float _speed = 0;

    private bool _defense = false;
    private bool _facingRight = false;

    private Rigidbody2D _rigidbody;
    private Transform _transform;

    private Animator _animator;


    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _transform = GetComponent<Transform>();
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        //자신일 경우에만 쿨타임 적용
        if (isMe && _coolTime < _maxCooltime)
        {
            _coolTime += Time.deltaTime;
        }
        
        if (isMe) {
            if (Math.Abs(_rigidbody.velocity.x) >= 0.1)
                _facingRight = _rigidbody.velocity.x > 0;
            _speed = Math.Abs(_rigidbody.velocity.x);
        }
        _animator.SetFloat("speed", _speed);
        var localScale = transform.localScale;
        localScale.x = _facingRight ? 0.25f : -0.25f;
        transform.localScale = localScale;
    }

    //서버에서 좌표 적용
    public void SetPos(float x, float y)
    {
        _rigidbody.position = new Vector2(x, y);
        
    }

    public void PlayAnimation(int aniType)
    {
        //enum 추가, 타입별 애니메이션 재생 -> _characterAnimation.~~~
        switch (aniType)
        {
            case 0:
                _animator.SetTrigger("front_attack");
                break;
            case 1:
                _animator.SetTrigger("back_attack");
                break;
            case 2:
                _animator.SetTrigger("down_attack");
                break;
            case 3:
                _animator.SetTrigger("up_attack");
                break;
            case 4:
                _animator.SetTrigger("defense");
                break;
        }
    }
    
    public void SendAnime(int aniType) {
        _socketManager.socket.EmitJson("animate", JsonConvert.SerializeObject(new { id = _id, animeId = aniType }));
    }

    //내가 공격한거 - 연산까지
    //상대가 공격한거 - 모션만
    public void AttackRight()
    {

        if (isMe && _coolTime>=_maxCooltime)
        {   
            _coolTime = 0;
            if (_facingRight) {
                _animator.SetTrigger("front_attack");
                SendAnime(0);
            } else {
                _animator.SetTrigger("back_attack");
                SendAnime(1);
            }
            // _characterAnimation.AttackRight();
        }
    }
    public void AttackLeft()
    {
        if (isMe && _coolTime>=_maxCooltime)
        {   
            _coolTime = 0;
            SendAnime(1);
            if (!_facingRight) {
                _animator.SetTrigger("front_attack");
                SendAnime(0);
            } else {
                _animator.SetTrigger("back_attack");
                SendAnime(1);
            }
            // _characterAnimation.AttackLeft();
        }
    }
    
    public void AttackUp()
    {
        if (isMe && _coolTime>=_maxCooltime)
        {   
            _coolTime = 0;
            _animator.SetTrigger("up_attack");
            SendAnime(3);
            // _characterAnimation.AttackUp();
        }
        
    }
    public void AttackDown()
    {
        if (isMe && _coolTime>=_maxCooltime)
        {   
            _coolTime = 0;
            _animator.SetTrigger("down_attack");
            SendAnime(2);
            // _characterAnimation.AttackDown();
        }
    }

    public void FrontAttack()
    {
        if (isMe) {
            AttackEnemy(_rightAttackCollision);
        }
    }

    public void BackAttack()
    {
        if (isMe) {
            AttackEnemy(_leftAttackCollision);
        }
    }

    public void DownAttack()
    {
        if (isMe) {
            AttackEnemy(_downAttackCollision);
        }
    }

    public void UpAttack()
    {
        if (isMe) {
            AttackEnemy(_upAttackCollision);
        }
    }

    public void Defense()
    {
         if (isMe && _coolTime>=_maxCooltime)
        {   
            _coolTime = 0;
            _animator.SetTrigger("defense");
            SendAnime(4);
            _defense = true;
            // _characterAnimation.AttackUp();
        }
    }

    public void DefenseExit()
    {
        if (isMe) {
            _defense = false;
        }
    }


    public void AttackEnemy(AttackCollision attackCollision)
    {
        var chars = attackCollision.GetHittedChars();//맞은놈들 캐릭터임
        foreach (var cha in chars)
        {
            Debug.Log(cha._id);
            if (_socketManager.characterList[cha._id]._defense) {
                _animator.SetTrigger("exit_attack");
                _coolTime = _maxCooltime * 3;
            } else {
                _socketManager.socket.EmitJson("hit", JsonConvert.SerializeObject(new { target = cha._id, dmg = 30 }));
            } 
        }
    }

    public void GetDmg(int dmg)
    {
        _hp -= dmg;
        hpbar.setValue((float)_hp/(float)_maxHp);
        if(_hp<0 && isMe)
        {
            _rigidbody.velocity = new Vector2(0,50);
        }
    }

    public void setId(string id)
    {
        _id = id;
    }

    public CharacterData GetData()
    {
        var pos = transform.position;
        return new CharacterData(_id, _hp, _maxHp, pos.x, pos.y, _facingRight, _speed, _defense);
    }

    public void SetData(CharacterData characterData)
    {
        _id = characterData.id;
        SetPos(characterData.x, characterData.y);
        _hp = characterData.hp;
        _maxHp = characterData.maxhp;
        hpbar.setValue((float)_hp/(float)_maxHp);
        _speed = characterData.speed;
        _facingRight = characterData.facingRight;
        _defense = characterData.defense;
    }
}


public struct CharacterData
{
    public CharacterData(string id, int hp, int maxhp, float x, float y, bool facingRight, float speed, bool defense)
    {
        this.id = id;
        this.hp = hp;
        this.maxhp = maxhp;
        this.x = x;
        this.y = y;
        this.facingRight = facingRight;
        this.speed = speed;
        this.defense = defense;
    }
    public string id;
    public int hp;
    public int maxhp;
    public float x;
    public float y;
    public bool facingRight;
    public float speed;
    public bool defense;
}