using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using Amguna;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;

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
    private string id;
    public Dictionary<string, Character> characterList = new Dictionary<string, Character>();
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

        socket.On("connected", (s) =>
        {
            connected = true;
        });

        socket.On("reconnect", (s) =>
        {
            Debug.Log("Hello, Again! ");
        });

        socket.On("userData", (data) =>
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

            var userDatas = JsonConvert.DeserializeObject<Dictionary<string,CharacterData>>(data);
            foreach(var userData in userDatas)
            {
                ProcessUserdata(userData.Value);
            }
        });

        socket.On("info", (string data) =>
        {

            id = (string)JObject.Parse(data)["id"];
            var name_ = (string)JObject.Parse(data)["name"];
            Debug.Log("START:"+name_);
            if (_preSettedName != "")
                name_ = _preSettedName;

            player.setIdAndName(id, name_);
            player.gameObject.SetActive(true);
        });

        socket.On("hit", (string data) =>
        {
            int dmg = (int)JObject.Parse(data)["dmg"];
            string hitter = (string)JObject.Parse(data)["id"];
            player.GetDmg(dmg, hitter);
            if(hitter == id)
            {
                player.Heal(5);
            }
            
        });

        socket.On("animate", (string data) =>
        {
            string id_ = (string)JObject.Parse(data)["id"];
            int animeId = (int)JObject.Parse(data)["animeId"];
            if (id_ != id)
            {
                characterList[id_].PlayAnimation((aniType)animeId);
            }
        });

        socket.On("delPlayer", (string data) =>
        {
            string id_ = (string)JObject.Parse(data)["id"];
            Destroy(characterList[id_].gameObject);
            characterList.Remove(id_);
        });

        socket.On("ping", (string data) =>
        {
            socket.Emit("pong", data);
        });

        socket.On("death", (string data) =>
        {
            Debug.Log(data);
            string id_ = (string)JObject.Parse(data)["id"];
            string by_ = (string)JObject.Parse(data)["by"];
            Character deathChar;
            if (id == id_) {
                 _killCounter.ResetCount();
                 return;
            }  
            else
                deathChar = characterList[id_];


            deathChar.DGim();
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
        if (userData.id == id)
            return;


        if (!characterList.ContainsKey(userData.id))
        {
            var newChar = Instantiate<Character>(prefab);
            characterList.Add(userData.id, newChar);
        }

        Character cha = characterList[userData.id];
        if (userData.dead)
        {
            cha.gameObject.SetActive(false);
        }
        cha.SetData(userData, player.transform.position);
    }
    void Update()
    {
        Debug.Log((int)(Time.deltaTime*1000));
        if(socket!=null)
            socket.Update();

        if (connected)
        {
            socket.Emit("myData", JsonConvert.SerializeObject(player.GetData()));
        }
    }

    void OnApplicationQuit()
    {
        socket.Disconnect();
    }
}
