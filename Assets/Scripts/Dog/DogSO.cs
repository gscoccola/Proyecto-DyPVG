using UnityEngine;

// This ScriptableObject defines the parameters for a specific type of dog

[CreateAssetMenu(menuName = "Dog Type", fileName = "New Dog SO")]

public class DogSO : ScriptableObject, IPathParametersSO
{
    [Header("Parameters")]
    [SerializeField] private string _dogName;

    [Header("Visibility")]
    [SerializeField] private Vector3 _pathOffset;
    [SerializeField] private Color _pathEndColor;
    [SerializeField] private Color _pathStartColor;

    [Header("Pathing")]
    [SerializeField] private TileType[] _traversableTiles  = new TileType[] { TileType.Walkable };
    [SerializeField] private int _maxDistance;

    [Header("Distractions")]
    [SerializeField] private bool _seesDistractions;
    [SerializeField] private float _distractionDetectionDist;

    public string DogName => _dogName;
    public int MaxDistance => _maxDistance;
    public Vector3 PathOffset => _pathOffset;
    public Color PathStartColor => _pathStartColor;
    public Color PathEndColor => _pathEndColor;
    public TileType[] TraversableTiles => _traversableTiles;
    public bool SeesDistractions => _seesDistractions;
    public float DistractionDetectionDist => _distractionDetectionDist;

}
