using UnityEngine;

// This ScriptableObject defines the parameters for a specific type of dog

[CreateAssetMenu(menuName = "Dog Type", fileName = "New Dog SO")]

public class DogSO : ScriptableObject, IPathParametersSO
{
    [Header("Parameters")]
    [SerializeField] private string _dogName;
    [SerializeField] private DogType _type;

    [Header("Path Visibility")]
    [SerializeField] private Vector3 _pathOffset;
    [SerializeField] private Color _pathEndColor;
    [SerializeField] private Color _pathStartColor;

    [Header("Chip Sprites")]
    [SerializeField] private Sprite[] _chipSprites;

    [Header("Pathing")]
    [SerializeField] private TileType[] _traversableTiles  = new TileType[] { TileType.Walkable };
    [SerializeField] private int _maxDistance;

    [Header("Distractions")]
    [SerializeField] private bool _seesDistractions;
    [SerializeField] private float _distractionDetectionDist;

    [Header("SFX")]
    [SerializeField] private AudioClip _selectedSFX;
    [SerializeField] private AudioClip _actionSFX;

    public string DogName => _dogName;
    public DogType Type => _type;

    public int MaxDistance => _maxDistance;
    public Vector3 PathOffset => _pathOffset;
    public Color PathStartColor => _pathStartColor;
    public Color PathEndColor => _pathEndColor;
    public Sprite[] ChipSprites => _chipSprites;
    public TileType[] TraversableTiles => _traversableTiles;
    public bool SeesDistractions => _seesDistractions;
    public float DistractionDetectionDist => _distractionDetectionDist;

    public AudioClip SelectedSFX => _selectedSFX;
    public AudioClip ActionSFX => _actionSFX;

}

public enum DogType
{
    Agile,
    Bully,
    Water,
}
