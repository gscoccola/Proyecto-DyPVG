using UnityEngine;

// Interface for all ScriptableObjects that define parameters for path drawing and visualization.

public interface IPathParametersSO
{
    public int MaxDistance { get; }
    public Vector3 PathOffset { get; }
    public Color PathStartColor { get; }
    public Color PathEndColor { get; }
    public TileType[] TraversableTiles  { get; }
    public AudioClip SelectedSFX  { get; }
}
