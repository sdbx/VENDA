using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

///<summary>
///this struct is used to show condition of key-binding-box.
///</summary>
public struct Key
{
    public int index { get; private set; }
    public int itemIndex { get; private set; }

    public Key(int index, int itemIndex)
    {
        this.index = index;
        this.itemIndex = itemIndex;
    }
}
///<summary>
///this class manages a specific layer.
///</summary>
public class MapEditor : MonoBehaviour
{
    public string layerName;
    public Coroutine layerCoroutine;
    public Tilemap tilemap;

    [SerializeField] private Camera mainCamera = null;
    [SerializeField] private int layerIndex = 0;
    [SerializeField] private MapEditor[] editors = null;

    [SerializeField] private RectTransform TileSelector = null;
    [SerializeField] private RectTransform mapPos = null;
    [SerializeField] private Transform grid = null;

    [SerializeField] private Tilemap mapViewMap = null;

    [SerializeField] private TileBase[] blockTiles = null;
    [SerializeField] private TileBase[] playerTiles = null;
    [SerializeField] private TileBase[] portalTiles = null;

    [SerializeField] private Sprite[] blockSlots = null;
    [SerializeField] private Sprite[] playerSlots = null;
    [SerializeField] private Sprite[] portalSlots = null;

    [SerializeField] private Image mouseTile = null;
    [SerializeField] private Image[] keySlot = null;
    [SerializeField] private MapManager mapManager = null;
    [SerializeField] private GameObject mapViewWindow = null;

    [SerializeField] private CanvasGroup keySlotAlphaManager = null;
    [SerializeField] private CanvasGroup itemKindsAlphaManger = null;
    [SerializeField] private CanvasGroup layerAlphaManager = null;
    [SerializeField] private Text itemKind = null;
    [SerializeField] private Text currentLayer = null;
    [SerializeField] private string[] itemKinds = null;

    private readonly List<TileBase[]> tileGroups = new List<TileBase[]>();
    private readonly List<Sprite[]> slotGroups = new List<Sprite[]>();

    private Sprite[] slots = null;
    private TileBase[] tiles = null;
    
    private readonly int[] indexes = new int[3];
    private int index = 0;
    private int itemIndex = 0;
    
    private Vector3 offset;

    private readonly Key[] keys = new Key[5] { new Key(-1, -1), new Key(-1, -1), new Key(-1, -1), new Key(-1, -1), new Key(-1, -1) };
    private readonly Vector3Int[] playerPoses = new Vector3Int[5]
    { new Vector3Int(-1, -1, 0), new Vector3Int(-1, -1, 0), new Vector3Int(-1, -1, 0), new Vector3Int(-1, -1, 0), new Vector3Int(-1, -1, 0) };

    private Coroutine keySlotCoroutine;
    private Coroutine itemKindsCoroutine;

    private void Awake()
    {
        tileGroups.Add(blockTiles);
        tileGroups.Add(playerTiles);
        tileGroups.Add(portalTiles);
        slotGroups.Add(blockSlots);
        slotGroups.Add(playerSlots);
        slotGroups.Add(portalSlots);
        tiles = (TileBase[])tileGroups[itemIndex].Clone();
        slots = (Sprite[])slotGroups[itemIndex].Clone();
    }
    private void Update()
    {
        var mousePos = MouseUtil.ToIndexPos(MouseUtil.GetWorld(mainCamera));
        TileSelector.anchoredPosition = new Vector2(50 * (int)(mousePos.x + 0.5f) - 775, 50 * (int)(mousePos.y) - 425);

        if (tilemap.gameObject.activeSelf) {
            if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl)) ChangeMouseTile();
            else ChangeItemKind();

            if (Input.GetKey(KeyCode.Mouse0)) SetTile();
            if (Input.GetKey(KeyCode.Mouse1)) UnsetTile();

            if (Input.GetKeyDown(KeyCode.S) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                CreateMapFile(editors);
            }

            if (Input.GetKeyDown(KeyCode.Q)) AdjustKeySlot(0);
            if (Input.GetKeyDown(KeyCode.W)) AdjustKeySlot(1);
            if (Input.GetKeyDown(KeyCode.E)) AdjustKeySlot(2);
            if (Input.GetKeyDown(KeyCode.R)) AdjustKeySlot(3);
            if (Input.GetKeyDown(KeyCode.T)) AdjustKeySlot(4);

            if (Input.GetKeyDown(KeyCode.Tab)) ChangeLayer();
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M))
        {
            mapViewWindow.SetActive(!mapViewWindow.activeSelf);
            grid.gameObject.SetActive(!grid.gameObject.activeSelf);
            mouseTile.gameObject.SetActive(!mouseTile.gameObject.activeSelf);
        }

        MapMove();
    }
    private void ChangeLayer()
    {
        editors[(layerIndex + 1) % editors.Length].enabled = true;
        enabled = false;
        currentLayer.text = editors[(layerIndex + 1) % editors.Length].layerName;
        if (layerCoroutine != null) editors[(layerIndex + editors.Length - 1) % editors.Length].StopCoroutine(editors[(layerIndex + editors.Length - 1) % editors.Length].layerCoroutine);
        layerCoroutine = StartCoroutine(ShowLayerWindow());
    }
    private void ChangeItemKind()
    {
        var wheelAxis = GetWheelAxis();

        if (wheelAxis != 0)
        {
            itemIndex = (itemIndex + wheelAxis + itemKinds.Length) % itemKinds.Length;
            itemKind.text = itemKinds[itemIndex];
            StopAllCoroutines();
            if (itemKindsCoroutine != null) StopCoroutine(itemKindsCoroutine);
            itemKindsCoroutine = StartCoroutine(ShowItemKinds());

            index = indexes[itemIndex];
            tiles = (TileBase[])tileGroups[itemIndex].Clone();
            slots = (Sprite[])slotGroups[itemIndex].Clone();

            ChangeMouseTile(false);
        }
    }
    private void MapMove() 
    {
        if (Input.GetKey(KeyCode.LeftArrow)) grid.localPosition += Vector3.right * 0.56f; 
        if (Input.GetKey(KeyCode.RightArrow)) grid.localPosition += Vector3.left * 0.56f;         
        if (Input.GetKey(KeyCode.DownArrow)) grid.localPosition += Vector3.up * 0.56f; 
        if (Input.GetKey(KeyCode.UpArrow)) grid.localPosition += Vector3.down * 0.56f;

        grid.localPosition = new Vector3(Mathf.Clamp(grid.localPosition.x, -17.8f - 8.89f, -8.89f), Mathf.Clamp(grid.localPosition.y, -15.08f, -5.08f));

        offset = MouseUtil.ToIndexPos(grid.localPosition + new Vector3(8.89f, 5.08f)) - new Vector3(15.4f, 8.6f);
        offset = new Vector3(Mathf.Round(offset.x), Mathf.Round(offset.y), 0);
        mapPos.anchoredPosition = -offset * 28.9f + new Vector3(705, 400);
    }
    private void AdjustKeySlot(int keyCode)
    {
        if (keys[keyCode].index != -1)
        {
            if (keys[keyCode].index == index && keys[keyCode].itemIndex == itemIndex)
            {
                UnregistKey(keyCode);
                return;
            }

            index = keys[keyCode].index;
            itemIndex = keys[keyCode].itemIndex;
            slots = (Sprite[])slotGroups[itemIndex].Clone();
            tiles = (TileBase[])tileGroups[itemIndex].Clone();
            mouseTile.sprite = slots[index];
            if (keySlotCoroutine != null) StopCoroutine(keySlotCoroutine);
            keySlotCoroutine = StartCoroutine(ShowKeySlot());
        }
        else
        {
            RegistKey(keyCode);
        }
    }
    private void RegistKey(int keyCode)
    {
        keySlot[keyCode].sprite = slots[index];
        keySlot[keyCode].color = Color.white;
        keys[keyCode] = new Key(index, itemIndex);
        if (keySlotCoroutine != null) StopCoroutine(keySlotCoroutine);
        keySlotCoroutine = StartCoroutine(ShowKeySlot());
    }
    private void UnregistKey(int keyCode)
    {
        keySlot[keyCode].color = new Color(1, 1, 1, 0);
        keys[keyCode] = new Key(-1, -1);
        if (keySlotCoroutine != null) StopCoroutine(keySlotCoroutine);
        keySlotCoroutine = StartCoroutine(ShowKeySlot());
    }
    private IEnumerator ShowKeySlot()
    {
        for (var i = 0; i <= 60; i++) {
            keySlotAlphaManager.alpha = (float)i * (60 - i) / 900;
            yield return null;
        }
    }
    private IEnumerator ShowItemKinds()
    {
        var i = 0;
        if (itemKindsAlphaManger.alpha > 0.001f) i = 30;

        for (; i <= 60; i++)
        {
            itemKindsAlphaManger.alpha = (float)i * (60 - i) / 900;
            yield return null;
        }
    }
    private IEnumerator ShowLayerWindow()
    {
        for (var i = 0; i <= 60; i++)
        {
            layerAlphaManager.alpha = (float)i * (60 - i) / 900;
            yield return null;
        }
    }
    private void CreateMapFile(MapEditor[] editors)
    {
        var cache = CreateCache();
        var file = File.Create($"Assets/Resources/{cache}.txt");
        var writer = new StreamWriter(file);
        foreach (var editor in editors) writer.Write(mapManager.CreateCode(editor.tilemap, (0, 0, 65, 33)) + "@");
        writer.Close();
        file.Close();
    }
    private string CreateCache()
    {
        var cache = "";
        for (var i = 0; i < 10; i++) cache += (char)(Random.Range(0, 26) + 'a');
        return cache;
    }
    private void SetTile()
    {
        var pos = new Vector3Int((int)(TileSelector.anchoredPosition.x + 775) / 50 - (int)offset.x, (int)(TileSelector.anchoredPosition.y - offset.y + 425) / 50 - (int)offset.y, 0);
        var offset3Int = new Vector3Int((int)offset.x, (int)offset.y, 0);
        var screenPos = pos + offset3Int;

        if (screenPos.x >= 0 && screenPos.x <= 31 && screenPos.y >= 0 && screenPos.y <= 17) {
            tilemap.SetTile(pos, tiles[index]);
            mapViewMap.SetTile(pos, tiles[index]);

            if (itemKinds[itemIndex] == "Player" && pos != playerPoses[index])
            {
                tilemap.SetTile(playerPoses[index], null);
                mapViewMap.SetTile(playerPoses[index], null);
                playerPoses[index] = pos;
            }
        }
    }
    private void UnsetTile()
    {
        var pos = new Vector3Int((int)(TileSelector.anchoredPosition.x + 775) / 50 - (int)offset.x, (int)(TileSelector.anchoredPosition.y - offset.y + 425) / 50 - (int)offset.y, 0);
        var offset3Int = new Vector3Int((int)offset.x, (int)offset.y, 0);
        var screenPos = pos + offset3Int;

        if (screenPos.x >= 0 && screenPos.x <= 31 && screenPos.y >= 0 && screenPos.y <= 17)
        {
            tilemap.SetTile(pos, null);
            mapViewMap.SetTile(pos, null);
        }
    }
    private void ChangeMouseTile(bool indexChange = true)
    {
        if (indexChange) index = (index + GetWheelAxis() + slots.Length) % slots.Length;
        mouseTile.sprite = slots[index];
        indexes[itemIndex] = index;
    }
    private int GetWheelAxis()
    {
        var wheelAxis = Input.GetAxis("Mouse ScrollWheel");

        if (wheelAxis > 0.01f) return 1;
        else if (wheelAxis > -0.01f) return 0;
        else return -1;
    }
}

public static class MouseUtil
{
    private static Plane floorPlane = new Plane(Vector3.up, Vector3.zero);

    public static Vector3 GetWorld(Camera camera)
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        floorPlane.Raycast(ray, out var distance);
        return ray.GetPoint(distance);
    }

    public static Vector3 ToIndexPos(Vector3 worldPos)
    {
        return new Vector3(worldPos.x + 8.61f, worldPos.y + 4.8f) / 0.56f;
    }
}