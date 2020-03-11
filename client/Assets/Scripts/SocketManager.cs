using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using Amguna;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;
using Google.Protobuf;

static class EventByte {
    public const byte Info = 0x01;
    public const byte UserData = 0x02;
    public const byte MyData = 0x03;
    public const byte Hit = 0x04;
    public const byte Death = 0x05;
    public const byte Animate = 0x06;

    // public const byte Disconnected = 0x07;

    public const byte Connected = 0xF1;

    public const byte Disconnected = 0xF2;
    
}

public class SocketManager : MonoBehaviour
{
    [SerializeField]
    Fps fps;

    [SerializeField]
    private Character prefab;

    private Dictionary<string, string> keylist = new Dictionary<string, string>();
    private bool connected = false;
    public Character player;
    [SerializeField]
    private MapManager _mapManager;
    public EventSocket socket = null;
    private int id;
    public Dictionary<int, Character> characterList = new Dictionary<int, Character>();
    [SerializeField]
    private CameraEffect _cameraEffect;

    [SerializeField]
    private BackgroundMusicController _backgroundMusic;
    [SerializeField]
    private KillCounter _killCounter;


    private string _preSettedName = "";

    [SerializeField]
    private bool _connectToTestServer;
    [SerializeField]
    private string _testServerIP; 

    delegate void UrlRequestCallback(string s);


    private float _pingTime;

    private float _maxPing;
    private float _pingtimer;

    private void Awake()
    {
        _cameraEffect = Camera.main.GetComponent<CameraEffect>();
        if (_connectToTestServer)
        {
            socket = new EventSocket(_testServerIP);
            StartServer();
            return;
        }
        StartCoroutine(GetRequest("https://vendagame.com/version.txt", (ver) =>
        {
            if (ver.Trim() != Application.version)
            {
                SceneManager.LoadScene("Version");
                return;
            }
            StartCoroutine(GetRequest("https://vendagame.com/server.txt", (url) =>
            {
                socket = new EventSocket(url.Trim());
                StartServer();
            }));
        }));
    }
    
    IEnumerator GetRequest(string uri,UrlRequestCallback callback)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else
            {
                callback(webRequest.downloadHandler.text);
            }
        }
    }

    void StartServer()
    {
        Application.targetFrameRate = 60;

        var nameSetter = GameObject.FindGameObjectWithTag("NickNameSetter");
        if(nameSetter)
        {
            _preSettedName = nameSetter.GetComponent<NicknameSetter>()._settedName;
            Destroy(nameSetter);
        }

        socket.OnConnect += (id) =>
        {
            Debug.Log("connected!!!");
            connected = true;
        };

        socket.On(EventByte.UserData, (data) =>
        {
            var time = Time.time;
            var ping  = (time-_pingTime)*1000;
            if(_maxPing<ping)
                _maxPing = ping;

            if(_pingtimer>5)
            {
                _pingtimer = 0;
                _maxPing = 0;
            }
            else _pingtimer+=Time.deltaTime;
            _pingTime = time;
        
            fps._maxPing = _maxPing;

            var userDatas = UserDataEvent.Parser.ParseFrom(data);
            foreach(var user in userDatas.Users)
            {
                ProcessUserdata(user);
            }
        });

        socket.On(EventByte.Info, (byte[] data) =>
        {
            var info = InfoEvent.Parser.ParseFrom(data);
            id = info.Id;
            var name_ = info.Name;
            Debug.Log("START:"+name_);
            if (_preSettedName != "")
                name_ = _preSettedName;

            player.setIdAndName(id, name_);
            player.gameObject.SetActive(true);
        });

        socket.On(EventByte.Hit, (byte[] data) =>
        {
            var hit = HitEvent.Parser.ParseFrom(data);
            var dmg = (int)hit.Dmg;
            var hitter = hit.Id;
            player.GetDmg(dmg, hitter);
        });

        socket.On(EventByte.Animate, (byte[] data) =>
        {
            var animate = AnimateEvent.Parser.ParseFrom(data);
            var id_ = animate.Id;
            var animeId = animate.Anime;
            if (id_ != id)
            {
                characterList[id_].PlayAnimation((aniType)animeId);
            }
        });

        socket.OnDisconnect += id_ =>
        {
            Destroy(characterList[id_].gameObject);
            characterList.Remove(id_);
        };

        socket.On(EventByte.Death, (byte[] data) =>
        {
            var death = DeathEvent.Parser.ParseFrom(data);
            int id_ = death.Id;
            int by_ = death.By;
            Character deathChar;
            if (id == id_) {
                 _killCounter.ResetCount();
                 deathChar = player;
            }  
            else
            {
                deathChar = characterList[id_];
                deathChar.DGim();
            }


            if (by_ == id)
            {
                _backgroundMusic.PlayFunAndReturn();
                _cameraEffect.Shake(0.5f);
                _killCounter.Plus();
                player.KillSomeone(deathChar._name);
                
            }
            else
            {
                if(by_!=null&&characterList.ContainsKey(by_))
                    characterList[by_].KillSomeone(deathChar._name);
            }
            
        });
    
    }

    void ProcessUserdata(CharacterData userData)
    {
        if (userData.Id == id)
            return;
        if (!characterList.ContainsKey(userData.Id))
        {
            var newChar = Instantiate<Character>(prefab);
            characterList.Add(userData.Id, newChar);
        }

        Character cha = characterList[userData.Id];
        if (userData.Dead)
        {
            cha.gameObject.SetActive(false);
        }
        cha.SetData(userData, player.transform.position);
    }
    void Update()
    {
        if(socket!=null) {
            socket.Update();
        }
            

        if (connected)
        {
            socket.Emit(EventByte.MyData, new MyDataEvent { Data = player.GetData() }.ToByteArray());
        }
    }

    void OnApplicationQuit()
    {
        socket.Disconnect();
    }
}
