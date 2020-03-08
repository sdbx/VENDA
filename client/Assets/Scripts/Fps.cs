
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fps : MonoBehaviour
{
    [Range(1, 100)]
    public int fFont_Size;
    [Range(0, 1)]
    public float Red, Green, Blue;
    public Texture2D tex;
    float deltaTime = 0.0f;

    [SerializeField]
    float _width = 10;
    [SerializeField]
    float _height = 10;


    public float _ping = 0;
    public float _maxPing = 0;

    bool on = false;

    private void Start()
    {
        fFont_Size = fFont_Size == 0 ? 50 : fFont_Size;
    }

    void Update()
    {  
        if(Input.GetKeyDown(KeyCode.Escape))
            on = !on;
        if(!on)
            return;
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        if (!on)
            return;

        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0} fps {1} ping {2} maxping", (int)fps,(int)_ping,(int)_maxPing);
        var style = new GUIStyle();
        style.fontSize = fFont_Size;
        //style.normal.background = tex;
        style.normal.textColor = new Color(Red,Green,Blue,1);
        var content = new GUIContent(text, "This is a fps");
        
        GUI.DrawTexture(new Rect(0,0,_width,_height),tex);
        GUI.Label(new Rect(0,0,100,50),text,style);
    }
}