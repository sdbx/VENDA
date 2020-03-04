using UnityEngine;
using socket.io;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using Amguna;

public class SocketManager : MonoBehaviour
{
    [SerializeField]
    private Character prefab;

    private Dictionary<string, string> keylist = new Dictionary<string, string>();
    private bool connected = false;
    public Character player;
    [SerializeField]
    private string serverUrl = "59.11.136.225:5858";
    [SerializeField]
    private MapManager _mapManager;
    public EventSocket socket;
    private string id;
    public Dictionary<string, Character> characterList = new Dictionary<string, Character>();
    [SerializeField]
    private CameraEffect _cameraEffect;

    private void Awake()
    {
        socket = new EventSocket(serverUrl);
        _cameraEffect = Camera.main.GetComponent<CameraEffect>();
    }

    void Start()
    {
        
        Application.targetFrameRate = 70;


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
            cha.SetData(userData);
        });

        socket.On("info", (string data) => {
            id = (string)JObject.Parse(data)["id"];
            var name_ = (string)JObject.Parse(data)["name"];
            player.setIdAndName(id,name_);
            player.gameObject.SetActive(true);
            
        });

        socket.On("hit",(string data)=>{
            int dmg = (int)JObject.Parse(data)["dmg"];
            string hitter = (string)JObject.Parse(data)["id"];
            player.GetDmg(dmg,hitter);
            Debug.Log(data);
        });

        socket.On("animate",(string data)=>{
            string id_ = (string)JObject.Parse(data)["id"];
            int animeId = (int)JObject.Parse(data)["animeId"];
            if (id_ != id) {
                characterList[id_].PlayAnimation((aniType)animeId);
            }
        });

        socket.On("delPlayer",(string data)=>{
            string id_ = (string)JObject.Parse(data)["id"];
            Destroy(characterList[id_].gameObject);
            characterList.Remove(id_);
        });

        socket.On("ping",(string data)=>{
            socket.EmitJson("pong", data);
        });
        
        socket.On("death",(string data)=>{
            string id_ = (string)JObject.Parse(data)["id"];
            string by_ = (string)JObject.Parse(data)["by"];
            Character deathChar;
            if(id == id_)
                deathChar = player;
            else
                deathChar = characterList[id_];

            deathChar.DGim();
            if(by_ == id)
            {
                _cameraEffect.Shake(0.5f);
                player.Heal(30);
            }
            characterList.Remove(id_);
            Destroy(deathChar.gameObject);
            socket.Disconnect();
        });
    }

    void Update()
    {
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
