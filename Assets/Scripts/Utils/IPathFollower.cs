using UnityEngine;
using System.Collections.Generic;

// Interface for all the classes that can follow a path drawn by the PathDrawer.
// They must define which tiles they can traverse and implement how they handle the drawn path
// given to them by the path drawer.

public interface IPathFollower
{
    public TileType[] TraversableTiles { get; set; }

    public void SetDrawnPath(List<Vector2Int> path);

}
