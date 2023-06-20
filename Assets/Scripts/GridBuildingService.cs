using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class GridBuildingService : MonoBehaviour
{
    public static GridBuildingService Instance;
    
    private static Dictionary<TileType, TileBase> _tileBases = new ();

    [SerializeField] private GridLayout _gridLayout;
    [SerializeField] private Tilemap _mainTilemap;
    [SerializeField] private Tilemap _tempTilemap;

    [SerializeField] private string _tilePath = @"Tiles\";

    private Tower _temp;
    private Vector3 _prevPosition;

    public void Initialize(Tower tower)
    {
        _temp = Instantiate(tower, Vector3.zero, quaternion.identity).GetComponent<Tower>();
    }
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        _tileBases.Add(TileType.Empty, null);
        _tileBases.Add(TileType.White, Resources.Load<TileBase>(_tilePath + "white"));
        _tileBases.Add(TileType.Green, Resources.Load<TileBase>(_tilePath + "green"));
        _tileBases.Add(TileType.Red, Resources.Load<TileBase>(_tilePath + "red"));
    }

    private void Update()
    {
        if (!_temp)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject(0))
            {
                return;
            }

            if (!_temp.Placed)
            {
                var touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var cellPosition = _gridLayout.LocalToCell(touchPosition);

                if (_prevPosition != cellPosition)
                {
                    _temp.transform.localPosition = _gridLayout.CellToLocalInterpolated(cellPosition
                        + new Vector3(.5f, .5f, 0f));
                    _prevPosition = cellPosition;
                }
            }
        }
    }

    private TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        var tiles = new TileBase[GetAreaSize(area)];
        var count = 0;

        foreach (var vector in area.allPositionsWithin)
        {
            var position = new Vector3Int(vector.x, vector.y, 0);
            tiles[count] = tilemap.GetTile(position);
            count++;
        }

        return tiles;
    }

    private void SetTilesBlock(BoundsInt area, TileType type, Tilemap tilemap)
    {
        var size = GetAreaSize(area);
        var tiles = new TileBase[size];
        FillTiles(tiles, type);
    }

    private void FillTiles(TileBase[] tiles, TileType type)
    {
        for (var i = 0; i < tiles.Length; i++)
        {
            tiles[i] = _tileBases[type];
        }
    }

    private int GetAreaSize(BoundsInt area)
    {
        var size = area.size.x * area.size.y * area.size.z;
        return size;
    }
}