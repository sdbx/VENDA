﻿using UnityEngine;
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

    delegate void UrlRequestCallback(string s);

    private void Awake()
    {
        StartCoroutine(GetRequest("https://vendagame.com/version.txt", (ver) =>
        {
            if(ver.Trim()!="v1.0.1")
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
        _cameraEffect = Camera.main.GetComponent<CameraEffect>();
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
        Application.targetFrameRate = 70;

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

        socket.On("userData", (string data) =>
        {
            var userData = JsonConvert.DeserializeObject<CharacterData>(data);
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
                player.KillSomeone();
            }
            else
            {
                if(characterList.ContainsKey(by_))
                    characterList[by_].KillSomeone();
            }
            
        });
    
    }

    void Update()
    {
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
