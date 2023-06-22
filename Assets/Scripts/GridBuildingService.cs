using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class GridBuildingService : MonoBehaviour
{
    public static GridBuildingService Instance;
    
    [SerializeField] private GridLayout _gridLayout;
    [SerializeField] private Tilemap _mainTilemap;
    [SerializeField] private Tilemap _tempTilemap;
    [SerializeField] private string _tilePath = @"Tiles\MarkupTiles\";

    private readonly Dictionary<TileType, TileBase> _tileBases = new();

    private Building _currentBuilding;
    private Vector3 _previousPosition;
    private BoundsInt _previousArea;

    [UsedImplicitly]
    public void Initialize(Building building)
    {
        _currentBuilding = Instantiate(building, Vector3.zero, Quaternion.identity);
        FollowBuilding();
    }
    
    public Vector3Int GetCellPosition(Vector3 objectPosition)
    {
        return _gridLayout.LocalToCell(objectPosition);
    }
    
    public bool CanTakeArea(BoundsInt area)
    {
        var baseArray = GetTilesBlock(area, _tempTilemap);
        return baseArray.All(tiles => tiles == _tileBases[TileType.White]);
    }
    
    public void TakeArea(BoundsInt area)
    {
        SetTilesBlock(area, TileType.Green, _mainTilemap);
        SetTilesBlock(area, TileType.Red, _tempTilemap);
    }
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        
        Instance = this;
    }

    private void Start()
    {
        _tileBases.Add(TileType.Empty, null);
        _tileBases.Add(TileType.White, Resources.Load<TileBase>(_tilePath + "white"));
        _tileBases.Add(TileType.Green, Resources.Load<TileBase>(_tilePath + "green"));
        _tileBases.Add(TileType.Red, Resources.Load<TileBase>(_tilePath + "red"));
    }

    private TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        var tiles = new TileBase[GetAreaSize(area)];
        var counter = 0;
        
        foreach (var v in area.allPositionsWithin)
        {
            var position = new Vector3Int(v.x, v.y, 0);
            tiles[counter] = tilemap.GetTile(position);
            counter++;
        }

        return tiles;
    }

    private void SetTilesBlock(BoundsInt area, TileType type, Tilemap tilemap)
    {
        var size = GetAreaSize(area);
        var tiles = new TileBase[size];
        FillTiles(tiles, type);
        tilemap.SetTilesBlock(area, tiles);
    }

    private void FillTiles(TileBase[] tiles, TileType type)
    {
        for (var i = 0; i < tiles.Length; i++)
        {
            tiles[i] = _tileBases[type];
        }
    }

    private void Update()
    {
        if (!_currentBuilding)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject(0))
            {
                return;
            }

            if (!_currentBuilding.Placed)
            {
                var touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var cellPosition = _gridLayout.LocalToCell(touchPosition);

                if (_previousPosition != cellPosition)
                {
                    _currentBuilding.transform.localPosition = _gridLayout.CellToLocalInterpolated(cellPosition
                        + new Vector3(.5f, .5f, 0f));
                    _previousPosition = cellPosition;
                    FollowBuilding();
                }
            }
        }
        
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            if (_currentBuilding.CanBePlaced())
            {
                _currentBuilding.Place();
            }
        }
        
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearArea();
            Destroy(_currentBuilding.gameObject);
        }
    }

    private void ClearArea()
    {
        var toClear = new TileBase[GetAreaSize(_previousArea)];
        FillTiles(toClear, TileType.Empty);
        _tempTilemap.SetTilesBlock(_previousArea, toClear);
    }

    private void FollowBuilding()
    {
        ClearArea();

        _currentBuilding.ChangeAreaPosition(_gridLayout.WorldToCell(_currentBuilding.gameObject.transform.position));
        var buildingArea = _currentBuilding.Area;
        var tileBases = GetTilesBlock(buildingArea, _tempTilemap);
        var tiles = new TileBase[tileBases.Length];
        
        for (var i = 0; i < tileBases.Length; i++)
        {
            if (tileBases[i] == _tileBases[TileType.White])
            {
                tiles[i] = _tileBases[TileType.Green];
            }
            else
            {
                FillTiles(tiles, TileType.Red);
                break;
            }
        }
        
        _tempTilemap.SetTilesBlock(buildingArea, tiles);
        _previousArea = buildingArea;
    }

    private int GetAreaSize(BoundsInt area)
    {
        var size = area.size.x * area.size.y * area.size.z;
        return size;
    }
}