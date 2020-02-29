using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

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

    private Rigidbody2D _rigidbody;
    private Transform _transform;
    private CharacterAnimation _characterAnimation;


    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _transform = GetComponent<Transform>();
        _characterAnimation = GetComponent<CharacterAnimation>();
    }

    void Update()
    {
        //자신일 경우에만 쿨타임 적용
        if (isMe && _coolTime < _maxCooltime)
        {
            _coolTime += Time.deltaTime;
        }
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
            case 1:
                break;
        }
    }


    //내가 공격한거 - 연산까지
    //상대가 공격한거 - 모션만
    public void AttackRight()
    {

        if (isMe && _coolTime>=_maxCooltime)
        {   
            _coolTime = 0;
            AttackEnemy(_rightAttackCollision);
    
        }
        _characterAnimation.AttackRight();
    }
    public void AttackLeft()
    {
        if (isMe && _coolTime>=_maxCooltime)
        {   
            _coolTime = 0;
            AttackEnemy(_leftAttackCollision);
        }
        _characterAnimation.AttackLeft();
    }
    public void AttackUp()
    {
        if (isMe && _coolTime>=_maxCooltime)
        {   
            _coolTime = 0;
            AttackEnemy(_upAttackCollision);
        }
        _characterAnimation.AttackUp();
    }
    public void AttackDown()
    {
        if (isMe && _coolTime>=_maxCooltime)
        {   
            _coolTime = 0;
            AttackEnemy(_downAttackCollision);
        }
        _characterAnimation.AttackDown();
    }

    public void AttackEnemy(AttackCollision attackCollision)
    {
        var chars = attackCollision.GetHittedChars();//맞은놈들 캐릭터임
        foreach (var cha in chars)
        {
            Debug.Log(cha._id);
            _socketManager.socket.EmitJson("hit", JsonConvert.SerializeObject(new { target = cha._id, dmg = 10 }));
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
        return new CharacterData(_id, _hp, pos.x, pos.y);
    }

    public void SetData(CharacterData characterData)
    {
        _id = characterData.id;
        SetPos(characterData.x, characterData.y);
        _hp = characterData.hp;
    }
}


public struct CharacterData
{
    public CharacterData(string id, int hp, float x, float y)
    {
        this.id = id;
        this.hp = hp;
        this.x = x;
        this.y = y;
    }
    public string id;
    public int hp;
    public float x;
    public float y;
}