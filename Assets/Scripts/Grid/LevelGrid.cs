using System.Collections.Generic;
using System.Linq;
using KwaaktjePathfinder2D;
using UnityEngine;
using UnityEngine.Tilemaps;

// This class generates a grid representation of the level based on a Tilemap and provides pathfinding functionality.
public class LevelGrid : Singleton<LevelGrid>
{
    [Header("References")]
    [SerializeField] private Tilemap _wallTilemap;
    [SerializeField] private Tilemap _jumpableTilemap;
    [SerializeField] private Tilemap _waterTilemap;


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
        _cellSizeInverse = 1f / _cellSize;
        _bounds = _wallTilemap.cellBounds;
        TileBase[] wallTiles = _wallTilemap.GetTilesBlock(_bounds);
        TileBase[] jumpableTiles = _jumpableTilemap == null ? null : _jumpableTilemap.GetTilesBlock(_bounds);
        _tileTypeGrid = new TileType[_bounds.size.x, _bounds.size.y];

        for (int x = 0; x < _bounds.size.x; x++)
        {
            for (int y = 0; y < _bounds.size.y; y++)
            {
                TileBase wallTile = wallTiles[x + y * _bounds.size.x];
                TileBase jumpableTile = _jumpableTilemap == null ? null : jumpableTiles[x + y * _bounds.size.x];
                if (wallTile != null)
                {
                    _tileTypeGrid[x, y] = TileType.Wall;
                }
                else if (jumpableTile != null)
                {
                    _tileTypeGrid[x, y] = TileType.Jumpable;
                }
                else
                {
                    _tileTypeGrid[x, y] = TileType.Walkable;
                }
            }
        }
    }

    public List<Vector2Int> CalculatePath(TileType[] traversableTileTypes, Vector2Int origin, Vector2Int target,
        bool excludePoint = false, Vector2Int excludedPoint = new Vector2Int())
    {
        Dictionary<Vector2Int, float> traversableTilemap = new Dictionary<Vector2Int, float>();
        for (int x = 0; x < _bounds.size.x; x++)
        {
            for (int y = 0; y < _bounds.size.y; y++)
            {
                if (x== excludedPoint.x && y == excludedPoint.y && excludePoint) continue;
                if (traversableTileTypes.Contains(_tileTypeGrid[x, y])) traversableTilemap[new Vector2Int(x,y)] = 1f;
            }
        }
        Pathfinder2DResult result = new Pathfinder2D(traversableTilemap, NodeConnectionType.RectangleNoDiagonals).FindPath(origin, target);
        result.Path.Reverse();
        return result.Path;
    }

    public List<Vector2Int> CalculatePath(TileType[] traversableTileTypes, Vector3 origin, Vector3 target,
        bool excludePoint = false,  Vector2Int excludedPoint = new Vector2Int())
    {
        return CalculatePath(traversableTileTypes, WorldToGridPos(origin), WorldToGridPos(target), excludePoint, excludedPoint);
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
    Water,
}