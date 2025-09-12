using System.Collections.Generic;
using System.Linq;
using KwaaktjePathfinder2D;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGrid : Singleton<LevelGrid>
{
    [Header("References")]
    [SerializeField] private Tilemap _wallTilemap;


    private TileType[,] _tileTypeGrid;
    private BoundsInt _bounds;
    private float _cellSize;
    private float _cellSizeInverse;

    private new void Awake()
    {
        base.Awake();
        GenerateMovementGrid();
    }

    private void GenerateMovementGrid()
    {
        _cellSize = _wallTilemap.cellSize.x;
        Debug.Log(_cellSize);
        _cellSizeInverse = 1f / _cellSize;
        _bounds = _wallTilemap.cellBounds;
        TileBase[] allTiles = _wallTilemap.GetTilesBlock(_bounds);
        _tileTypeGrid = new TileType[_bounds.size.x, _bounds.size.y];

        for (int x = 0; x < _bounds.size.x; x++)
        {
            for (int y = 0; y < _bounds.size.y; y++)
            {
                TileBase tile = allTiles[x + y * _bounds.size.x];
                if (tile != null)
                {
                    _tileTypeGrid[x, y] = TileType.Wall;
                }
                else
                {
                    _tileTypeGrid[x, y] = TileType.Walkable;
                }
            }
        }
    }

    public List<Vector2Int> CalculatePath(TileType[] traversableTileTypes, Vector2Int origin, Vector2Int target)
    {
        Dictionary<Vector2Int, float> traversableTilemap = new Dictionary<Vector2Int, float>();
        for (int x = 0; x < _bounds.size.x; x++)
        {
            for (int y = 0; y < _bounds.size.y; y++)
            {
                if (traversableTileTypes.Contains(_tileTypeGrid[x, y])) traversableTilemap[new Vector2Int(x,y)] = 1f;
            }
        }
        List<Vector2Int> path = new Pathfinder2D(traversableTilemap, NodeConnectionType.RectangleNoDiagonals).FindPath(origin, target).Path;
        //path.Add(target);
        return path;
    }

    public List<Vector2Int> CalculatePath(TileType[] traversableTileTypes, Vector3 origin, Vector3 target)
    {
        return CalculatePath(traversableTileTypes, WorldToGridPos(origin), WorldToGridPos(target));
    }

    public Vector2Int WorldToGridPos(Vector3 worldPosition)
    {
        return new Vector2Int(Mathf.RoundToInt(worldPosition.x *_cellSizeInverse - _bounds.x * _cellSizeInverse),
            Mathf.RoundToInt(worldPosition.y - _bounds.y * _cellSizeInverse));
    }

    public Vector3 GridToWorldPos(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * _cellSize + _bounds.x,
            gridPosition.y * _cellSize + _bounds.y, 0f);
    }

    public Vector3 SnapToGrid(Vector3 worldPos)
    {
        return GridToWorldPos(WorldToGridPos(worldPos));
    }
}

public enum TileType
{
    Walkable,
    Wall,
    Jumpable,
}