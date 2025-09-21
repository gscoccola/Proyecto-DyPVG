using UnityEngine;

[CreateAssetMenu(menuName = "Dog Type", fileName = "New Dog SO")]

public class DogSO : ScriptableObject
{
    [Header("Parameters")]
    public string DogName;

    [Header("Visibility")]
    public Vector3 PathOffset;
    public Color PathEndColor;
    public Color PathStartColor;

    [Header("Pathing")]
    public TileType[] TraversableTiles  = new TileType[] { TileType.Walkable };
    public int MaxDistance;

    [Header("Distractions")]
    public bool SeesDistractions;
    public float DistractionDetectionDist;

}
