using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;
using Google.Protobuf;

public class Character : MonoBehaviour
{

    [SerializeField]
    private ProgressBar hpbar;
    [SerializeField]
    private SocketManager _socketManager;
    public bool isMe = false;
    [SerializeField]
    private DeadBody _deadBody;
    [SerializeField]
    private Text _nameText;

    //attack collision
    [SerializeField]
    private AttackCollision _rightAttackCollision;
    [SerializeField]
    private AttackCollision _leftAttackCollision;
    [SerializeField]
    private AttackCollision _upAttackCollision;
    [SerializeField]
    private AttackCollision _downAttackCollision;

    //cha balance settings
    [SerializeField]
    private float _shieldTime = 2;

    //users info
    public string _name= "";
    public int _id;
    [SerializeField]
    private float _maxHp = 100;
    [SerializeField]
    private float _hp = 100;
    [SerializeField]
    private float _maxCooltime = 10;
    [SerializeField]
    private float _cooltime = 0;
    [SerializeField]
    private ProgressBar _coolTimeBar;
    private bool _isCooltimeIncreased = false;

    [SerializeField]
    private CharacterAnimationAndSound _characterAnimationAndSound;

    private float _speed = 0;

    private bool _defense = false;
    private bool _facingRight = false;


    [SerializeField]
    private Transform _characterTransform;

    private Rigidbody2D _rigidbody;
    private Transform _transform;

    private CameraEffect _cameraEffect;

    private List<Character> _hittedEnemy = new List<Character>();

    [SerializeField]
    private float _maxDistanceToListen = 5;

    [SerializeField]
    private Transform[] _spawnPoints;


    private float _volume;
    public bool _dead{get;private set;} = false;

    [SerializeField]
    private DeadHeadOnPlayer _deadBodyOnPlayer;

    private int _lastHitter;

    private IEnumerator RespawnC()
    {
        yield return new WaitForSeconds(5);
        Respawn();
    }

    private void Respawn()
    {
        SetHp(_maxHp);
        _facingRight = false;
        _cooltime = 0;
        _isCooltimeIncreased = false;
        _speed = 0;
        _defense = false;
        _dead = false;
        _lastHitter = -1;
        gameObject.transform.position = GetRandomSpawnPoint();
        gameObject.SetActive(true);
    }

    private void SetHp(float hp)
    {
        _hp = Mathf.Clamp(hp,0,100);
        hpbar.setValue(_hp/_maxHp);
    }

    private Vector2 GetRandomSpawnPoint()
    {
        return _spawnPoints[new System.Random().Next(0,_spawnPoints.Length-1)].position;
    }

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        if(isMe)
        {
            _cameraEffect = Camera.main.GetComponent<CameraEffect>();
            SetPos(GetRandomSpawnPoint());
        }
        
    }

    public void Update()
    {
        //자신일 경우에만 쿨타임 적용
        if (isMe && _cooltime < _maxCooltime)
        {
            _cooltime += Time.deltaTime / (_isCooltimeIncreased?3:1) ;
            _coolTimeBar.setValue(_cooltime / _maxCooltime);
            if (_cooltime >= _maxCooltime)
                _characterAnimationAndSound.PlayCooltimeSound();
        }
        else 
        {
            _isCooltimeIncreased = false;
        }

        if (isMe)
        {
            if (Math.Abs(_rigidbody.velocity.x) >= 0.1)
                _facingRight = _rigidbody.velocity.x > 0;
            _speed = Math.Abs(_rigidbody.velocity.x);

            Heal(-0.5f*Time.deltaTime);
            if(_hp<=0)
            {
                _socketManager.socket.Emit(EventByte.Death, new DeathEvent { By = _lastHitter, Id = _id }.ToByteArray());
                Debug.Log(_lastHitter);
                DGim();
            }
        }
        _characterAnimationAndSound.Walk(_speed);
        var rotation = _characterTransform.rotation;
        rotation.y = _facingRight ? 0 : 180;
        _characterTransform.rotation = rotation;
    }

    //서버에서 좌표 적용
    public void SetPos(float x, float y)
    {
        _rigidbody.position = new Vector2(x, y);
    }
    public void SetPos(Vector2 pos)
    {
        _rigidbody.position = pos;
    }


    private IEnumerator SetLastHitter(int hitter)
    {
        _lastHitter = hitter;
        yield return new WaitForSeconds(30f);
        if(hitter==_lastHitter)
        {
            _lastHitter = -1;
        }
    }


    //CharacterAnimation 다시 추가 해서 별도로 다시 만들것 
    public void PlayAnimation(aniType aniType)
    {
        _characterAnimationAndSound.PlayAnimation(aniType);
    }

    public void SendAnimeAndPlay(aniType aniType)
    {
        _socketManager.socket.Emit(EventByte.Animate, new AnimateEvent { Id = _id, Anime = (int)aniType }.ToByteArray());
        PlayAnimation(aniType);
    }

    //startAttacking
    public void StartFrontAttack()
    {
        if (_cooltime >= _maxCooltime)
        {
            _cooltime = 0;

            SendAnimeAndPlay(aniType.FrontAttack);

            // _characterAnimation.AttackRight();
        }
    }
    public void StartBackAttack()
    {
        if (_cooltime >= _maxCooltime)
        {
            _cooltime = 0;

            SendAnimeAndPlay(aniType.BackAttack);
            // _characterAnimation.AttackLeft();
        }
    }

    public void StartUpAttack()
    {
        if (_cooltime >= _maxCooltime)
        {
            _cooltime = 0;
            SendAnimeAndPlay(aniType.UpAttack);
            // _characterAnimation.AttackUp();
        }

    }
    public void StartDownAttack()
    {
        if (_cooltime >= _maxCooltime)
        {
            _cooltime = 0;
            SendAnimeAndPlay(aniType.DownAttack);
            // _characterAnimation.AttackDown();
        }
    }

    public void StartDefense()
    {
        if (_cooltime >= _maxCooltime)
        {
            _cooltime = 0;
            StartCoroutine(DefenseXSencods(_shieldTime));
        }
        
    }

    private IEnumerator DefenseXSencods(float x)
    {
        SendAnimeAndPlay(aniType.DefenseStart);
        _defense = true;
        yield return new WaitForSeconds(x);
        DefenseExit();
    }

    public void DefenseExit()
    {
        if (isMe)
        {
            SendAnimeAndPlay(aniType.DefenseExit);
            _defense = false;
        }
    }

    //damaging
    public void AttackFront()
    {
        if (isMe)
            AttackEnemy(_rightAttackCollision);
    }

    public void AttackBack()
    {
        if (isMe)
            AttackEnemy(_leftAttackCollision);
    }

    public void AttackDown()
    {
        if (isMe)
        {
            AttackEnemy(_downAttackCollision);
        }
    }

    public void AttackUp()
    {
        if (isMe)
        {
            AttackEnemy(_upAttackCollision);
        }
    }

    public void AttackEnemy(AttackCollision attackCollision)
    {
        var newEnemys = attackCollision.GetHittedChars();
        var currentEnemys = newEnemys.Except(_hittedEnemy).ToList();
        _hittedEnemy.AddRange(newEnemys);
        AttackEnemyCheck(currentEnemys);
    }

    public void AttackEnemyEnd()
    {
        _hittedEnemy.Clear();
    }

    public void AttackEnemyCheck(List<Character> hittedEnemy)
    {
        //맞은놈들 캐릭터
        if (hittedEnemy.Count != 0)
            _cameraEffect.Shake(0.1f);

        foreach (var enemy in hittedEnemy)
        {
            //Debug.Log(enemy._id);
            if (_socketManager.characterList[enemy._id]._defense)
            {
                SendAnimeAndPlay(aniType.DefenseSuccess);
                _isCooltimeIncreased = true;
            }
            else
            {
                _socketManager.socket.Emit(EventByte.Hit, new HitEvent { Target = enemy._id, Dmg = 30, Id = _id}.ToByteArray());
            }
        }
    }

    public void KillSomeone(string diedName)
    {
        _deadBodyOnPlayer.Add(_rigidbody,diedName);
        Debug.Log(_name);
        if(isMe)
            Heal(100);
    }

    public void GetDmg(int dmg,int hitter)
    {
        SetHp(_hp-dmg);
        StartCoroutine(SetLastHitter(hitter));
        if (_hp <= 0)
        {
            //뒤짐처리
            if(isMe)
            {
                Debug.Log("killer:"+hitter);
                _socketManager.socket.Emit(EventByte.Death, new DeathEvent { By = hitter, Id = _id }.ToByteArray());
                DGim();
            }
            return;
        }

        SendAnimeAndPlay(aniType.Blood);
        _cameraEffect.Shake(0.1f);
        _cameraEffect.ShowBloodOnScreen();
    }

    public void Heal(float amount)
    {
        SetHp(_hp+amount);
    }

    public void DGim()
    {
        var deadBody = Instantiate<DeadBody>(_deadBody, transform.position, transform.rotation);
        deadBody.DestroyBody(_volume);
        gameObject.SetActive(false);
        if(isMe)
            _socketManager.StartCoroutine(RespawnC());
        _dead = true;
        _deadBodyOnPlayer.ResetBodies();
    }

    public void setIdAndName(int id, string name)
    {
        _id = id;
        _name = name;
        _nameText.text = name;
    }

    public CharacterData GetData()
    {
        var pos = transform.position;
        return new CharacterData {
            Id = _id,
            Name = _name,
            Hp = _hp,
            MaxHp = _maxHp,
            X = pos.x,
            Y = pos.y,
            FacingRight = _facingRight,
            Speed = _speed,
            Defense = _defense,
            Dead = _dead
        };
    }

    public void SetData(CharacterData characterData, Vector2 myPos)
    {
        if(characterData.Dead)
        {
            gameObject.transform.position = new Vector2(characterData.X,characterData.Y);
            return;
        }

        _id = characterData.Id;
        SetPos(characterData.X, characterData.Y);
        _maxHp = characterData.MaxHp;
        SetHp(characterData.Hp);
        _speed = characterData.Speed;
        _facingRight = characterData.FacingRight;
        _defense = characterData.Defense;
        if (_name == "")
        {
            _name = characterData.Name;
            _nameText.text = _name;
        }
        _volume = Vector2.Distance(myPos, transform.position);
        _dead = characterData.Dead;
        if (!isMe)
            SetVolumeWithDistance(_volume);

        gameObject.SetActive(!_dead);
    }

    public void SetVolumeWithDistance(float distance)
    {
        _characterAnimationAndSound.SetVolume(1-distance/_maxDistanceToListen);
    }
}